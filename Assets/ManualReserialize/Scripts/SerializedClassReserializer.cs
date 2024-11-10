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
        public static void Reserialize<T>(ReserializeDelegate<T> action)
        {
            DoReserialize<T>(action);
            DoReserialize<T[]>((array, metadata) => { foreach (var obj in array) { action.Invoke(obj, metadata); } });
            DoReserialize<List<T>>((list, metadata) => { foreach (var obj in list) { action.Invoke(obj, metadata); } });

            //TODO: Missing. This will not reserialize in places where Type T is beeig serialized using SerializeReference and another type
        }

        private static void DoReserialize<T>(ReserializeDelegate<T> action)
        {
            ReserializePrefabMonoBehavioursNested<T>(action);
            ReserializeSceneMonoBehavioursNested<T>(action);
            ReserializeScriptableObjectsNested<T>(action);
        }

        private static void ReserializePrefabMonoBehavioursNested<T>(ReserializeDelegate<T> action)
        {
            foreach (var typeAction in GetNestedTypeActions<MonoBehaviour, T>())
            {
                var prefabs = AssetDatabaseUtils.GetAllPrefabsWithComponentSortedByVariant(
                    typeAction.type);

                foreach (var prefab in prefabs)
                {
                    var components = prefab.GetComponentsInChildren(typeAction.type);
                    foreach (var component in components)
                    {
                        foreach (var nestedAppearence in typeAction.nestedApperences)
                        {
                            var instance = ReflectionUtils.GetNestedObjectInitializing(nestedAppearence, component);
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

        private static void ReserializeSceneMonoBehavioursNested<T>(ReserializeDelegate<T> action)
        {
            var typeActions = GetNestedTypeActions<MonoBehaviour, T>().ToArray();

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
                            var instance = ReflectionUtils.GetNestedObjectInitializing(nestedApperence, component);
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

        private static IEnumerable<(Type type, List<FieldInfo[]> nestedApperences)> GetNestedTypeActions<TBase, TLook>()
        {
            return ReserializeReflectionUtils.GetAllAssignableTypesWithNestedMemberOfType(
                typeof(TBase),
                typeof(TLook))
                .Select<Type, (Type, List<FieldInfo[]>)>(x =>
                {
                    List<FieldInfo[]> paths = ReserializeReflectionUtils.GetFieldInfoPathOfNestedTypeApperencesInType(
                        x,
                        typeof(TLook));

                    return (x, paths);
                });
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
                            nestedAppearance,
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
