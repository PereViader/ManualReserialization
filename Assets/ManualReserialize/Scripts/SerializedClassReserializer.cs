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
        public static void Reserialize<T>(Action<T> action, string[] reserializePaths)
        {
            DoReserialize<T>(action, reserializePaths);
            DoReserialize<T[]>(array => { foreach (var obj in array) { action.Invoke(obj); } }, reserializePaths);
            DoReserialize<List<T>>(list => { foreach (var obj in list) { action.Invoke(obj); } }, reserializePaths);

            //TODO: Missing. This will not reserialize in places where Type T is beeig serialized using SerializeReference and another type
        }

        private static void DoReserialize<T>(Action<T> action, string[] reserializePaths)
        {
            ReserializePrefabMonoBehavioursNested<T>(action, reserializePaths);
            ReserializeSceneMonoBehavioursNested<T>(action, reserializePaths);
            ReserializeScriptableObjectsNested<T>(action);
        }

        private static void ReserializePrefabMonoBehavioursNested<T>(Action<T> action, string[] reserializePaths)
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
                            action.Invoke((T)instance);
                        }
                        EditorUtility.SetDirty(component);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }

        private static void ReserializeSceneMonoBehavioursNested<T>(Action<T> action, string[] reserializePaths)
        {
            var typeActions = GetNestedTypeActions<MonoBehaviour, T>();

            foreach (var scenePath in AssetDatabaseUtils.GetAllScenePaths())
            {
                EditorSceneManager.OpenScene(scenePath);

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
                            action.Invoke((T)instance);
                        }

                        EditorUtility.SetDirty(component);
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

        private static void ReserializeScriptableObjectsNested<T>(Action<T> action)
        {
            var typeActions = GetNestedTypeActions<ScriptableObject, T>();

            foreach (var typeAction in typeActions)
            {
                var assets = AssetDatabaseUtils.GetAllAssetsOfType(typeAction.type);
                foreach (var asset in assets)
                {
                    foreach (var nestedApperence in typeAction.nestedApperences)
                    {
                        //No need to filter scriptable objects because they don't have variants

                        var instance = ReflectionUtils.GetNestedObjectInitializing(
                            nestedApperence.fieldInfos,
                            asset);

                        action.Invoke((T)instance);
                    }
                    EditorUtility.SetDirty(asset);
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
}
#endif
