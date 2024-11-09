#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


namespace ManualReserialization
{
    public static class SerializedClassReserializer
    {
        public static void Reserialize<T>(ReserializeDelegate<T> action, string[] reserializePaths)
        {
            DoReserialize<T>(action, reserializePaths);
            DoReserialize<T[]>((array, metadata) => { foreach (var obj in array) { action.Invoke(obj, metadata); } }, reserializePaths);
            DoReserialize<List<T>>((list, metadata) => { foreach (var obj in list) { action.Invoke(obj, metadata); } }, reserializePaths);

            //TODO: Missing. This will not reserialize in places where Type T is beeig serialized using SerializeReference and another type
        }

        private static void DoReserialize<T>(ReserializeDelegate<T> action, string[] reserializePaths)
        {
            ReserializePrefabMonoBehavioursNested<T>(action, reserializePaths);
            ReserializeSceneMonoBehavioursNested<T>(action, reserializePaths);
            ReserializeScriptableObjectsNested<T>(action);
        }

        private static void ReserializePrefabMonoBehavioursNested<T>(ReserializeDelegate<T> action, string[] reserializePaths)
        {
            var typeActions = GetNestedTypeActions<MonoBehaviour, T>();

            foreach (var typeAction in typeActions)
            {
                var prefabs = AssetDatabaseUtils.GetAllPrefabsWithComponent(
                    typeAction.type);

                foreach (var prefab in prefabs)
                {
                    var components = prefab.GetComponentsInChildren(typeAction.type);
                    foreach (var component in components)
                    {
                        foreach (var nestedApperence in typeAction.nestedApperences)
                        {
                            if (!ReserializeReflectionUtils.ShouldApplyReserialize(
                                component,
                                typeAction.type,
                                nestedApperence.path,
                                reserializePaths))
                            {
                                continue;
                            }

                            var instance = ReflectionUtils.GetNestedObjectInitializing(nestedApperence.fieldInfos, component);
                            try
                            {
                                action.Invoke((T)instance, new GameObjectPrefabMetadata(prefab, component));
                                EditorUtility.SetDirty(component);
                                AssetDatabase.SaveAssets();
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                            }
                        }
                    }
                }
            }
        }

        private static void ReserializeSceneMonoBehavioursNested<T>(ReserializeDelegate<T> action, string[] reserializePaths)
        {
            var typeActions = GetNestedTypeActions<MonoBehaviour, T>();

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

                foreach (var typeAction in typeActions)
                {
                    var components = UnityEngine.Object.FindObjectsOfType(typeAction.type);

                    foreach (var component in components)
                    {
                        foreach (var nestedApperence in typeAction.nestedApperences)
                        {
                            if (!ReserializeReflectionUtils.ShouldApplyReserialize(
                                component,
                                typeAction.type,
                                nestedApperence.path,
                                reserializePaths))
                            {
                                continue;
                            }

                            var instance = ReflectionUtils.GetNestedObjectInitializing(nestedApperence.fieldInfos, component);
                            try
                            {
                                action.Invoke((T)instance, new SceneMetadata(scenePath, (Component)component));
                                EditorUtility.SetDirty(component);
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                            }
                        }
                    }
                }

                EditorSceneManager.SaveOpenScenes();
            }

            AssetDatabase.SaveAssets();
        }

        private static List<(Type type, List<(List<FieldInfo> fieldInfos, string path)> nestedApperences)> GetNestedTypeActions<TBase, TLook>()
        {
            return ReserializeReflectionUtils.GetAllAssignableTypesWithNestedMemberOfType(
                typeof(TBase),
                typeof(TLook))
                .Select<Type, (Type, List<(List<FieldInfo>, string)>)>(x =>
                {
                    List<List<FieldInfo>> paths = ReserializeReflectionUtils.GetFieldInfoPathOfNestedTypeApperencesInType(
                        x,
                        typeof(TLook));

                    return (x, paths.Select(y => (y, string.Join(".", y.Select(z => z.Name)))).ToList());
                })
                .ToList();
        }

        private static void ReserializeScriptableObjectsNested<T>(ReserializeDelegate<T> action)
        {
            var typeActions = GetNestedTypeActions<ScriptableObject, T>();

            foreach (var typeAction in typeActions)
            {
                var scriptableObjects = AssetDatabaseUtils.GetAllAssetsOfType(typeAction.type)
                    .Cast<ScriptableObject>();
                
                foreach (var scriptableObject in scriptableObjects)
                {
                    foreach (var nestedAppearance in typeAction.nestedApperences)
                    {
                        //No need to filter scriptable objects because they don't have variants

                        var instance = ReflectionUtils.GetNestedObjectInitializing(
                            nestedAppearance.fieldInfos,
                            scriptableObject);

                        try
                        {
                            action.Invoke((T)instance, new ScriptableObjectMetadata(scriptableObject));
                            EditorUtility.SetDirty(scriptableObject);
                            AssetDatabase.SaveAssets();
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }
                }
            }
        }
    }
}
#endif
