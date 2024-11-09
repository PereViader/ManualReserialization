#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


namespace ManualReserialization
{
    public static class MonoBehaviourReserializer
    {
        public static void Reserialize<T>(Action<T> action, string[] reserializePaths) where T : MonoBehaviour
        {
            ReserializePrefabs<T>(action, reserializePaths);
            ReserializeScenes<T>(action, reserializePaths);
        }

        private static void ReserializePrefabs<T>(Action<T> action, string[] reserializePaths) where T : MonoBehaviour
        {
            try
            {
                AssetDatabase.StartAssetEditing();

                var prefabs = AssetDatabaseUtils.GetAllPrefabsWithComponent(typeof(T));
                foreach (var prefab in prefabs)
                {
                    var components = prefab.GetComponentsInChildren<T>();
                    foreach (var component in components)
                    {
                        if (!ReserializeReflectionUtils.ShouldApplyReserialize(
                                component,
                                typeof(T),
                                path: string.Empty,
                                reserializePaths))
                        {
                            continue;
                        }

                        try
                        {
                            action.Invoke(component);
                            EditorUtility.SetDirty(component);
                            AssetDatabase.SaveAssets();
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e, prefab);
                        }
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        private static void ReserializeScenes<T>(Action<T> action, string[] reserializePaths) where T : MonoBehaviour
        {
            foreach (var scenePath in AssetDatabaseUtils.GetAllScenePaths())
            {
                try
                {
                    EditorSceneManager.OpenScene(scenePath);
                }
                catch
                {
                    continue;
                }

                var components = UnityEngine.Object.FindObjectsOfType<T>();
                foreach (var component in components)
                {
                    if (!ReserializeReflectionUtils.ShouldApplyReserialize(
                        component,
                        typeof(T),
                        path: string.Empty,
                        reserializePaths))
                    {
                        continue;
                    }

                    action.Invoke(component);
                    EditorUtility.SetDirty(component);
                }

                EditorSceneManager.SaveOpenScenes();
            }

            AssetDatabase.SaveAssets();
        }
    }
}
#endif
