#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ManualReserialization
{
    public static class AssetDatabaseUtils
    {
        public static IEnumerable<GameObject> GetAllPrefabsWithComponent(Type type)
        {
            var guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var assetsAtPath = AssetDatabase.LoadAllAssetsAtPath(path);

                foreach (var asset in assetsAtPath)
                {
                    if (!(asset is GameObject gameObject))
                    {
                        continue;
                    }

                    var components = gameObject.GetComponentsInChildren(type);
                    if (components.Length > 0)
                    {
                        yield return gameObject;
                        continue;
                    }
                }
            }
        }

        public static IEnumerable<string> GetAllScenePaths()
        {
            var guids = AssetDatabase.FindAssets("t:Scene");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                yield return path;
            }
        }

        public static IEnumerable<UnityEngine.Object> GetAllAssetsOfType(Type type)
        {
            var guids = AssetDatabase.FindAssets("t:" + type.Name);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                //Maybe this will have problems with assets that have more than one object at the same path (?)
                yield return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            }
        }
    }
}
#endif
