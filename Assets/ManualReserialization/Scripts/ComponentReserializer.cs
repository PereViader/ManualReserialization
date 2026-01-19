#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


namespace PereViader.ManualReserialization
{
    public static class ComponentReserializer
    {
        public static void Reserialize<T>(ReserializeDelegate<T> action) where T : Component
        {
            ReserializePrefabs<T>(action);
            ReserializeScenes<T>(action);
        }

        private static void ReserializePrefabs<T>(ReserializeDelegate<T> action) where T : Component
        {
            var prefabs = AssetDatabaseUtils.GetAllPrefabsWithComponentSortedByVariant(typeof(T));
            foreach (var prefab in prefabs)
            {
                var components = prefab.GetComponentsInChildren<T>();
                foreach (var component in components)
                {
                    try
                    {
                        action.Invoke(component, new GameObjectPrefabMetadata(prefab, component));
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e, prefab);
                    }
                }
                EditorUtility.SetDirty(prefab);
            }
            
            AssetDatabase.SaveAssets();
        }

        private static void ReserializeScenes<T>(ReserializeDelegate<T> action) where T : Component
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
                    action.Invoke(component, new SceneMetadata(scenePath, component));
                    EditorUtility.SetDirty(component);
                }

                EditorSceneManager.SaveOpenScenes();
            }

            AssetDatabase.SaveAssets();
        }
    }
}
#endif
