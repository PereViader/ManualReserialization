#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ManualReserialization
{
    public static class AssetDatabaseUtils
    {
        /// <summary>
        /// Gets all the prefabs that have some component and sorts them first so the root prefabs come before the variants
        /// </summary>
        public static IEnumerable<GameObject> GetAllPrefabsWithComponentSortedByVariant(Type type)
        {
            var prefabs = GetAllPrefabsWithComponent(type);
            return PerformBreadthFirstSearch(prefabs, PrefabUtility.GetCorrespondingObjectFromSource);
        }
        
        /// <summary>
        /// Takes some Enumerable and sorts it so using a breath first search approach so that dependencies of each element must always come before dependants
        /// </summary>
        private static IEnumerable<T> PerformBreadthFirstSearch<T>(IEnumerable<T> elements, Func<T, T> getDependency) where T : class
        {
            var queue = new Queue<T>(elements);
            var visited = new HashSet<T>(queue.Count);

            while (queue.Count > 0)
            {
                var currentElement = queue.Dequeue();
                var dependency = getDependency(currentElement);
                if (dependency != null && !visited.Contains(dependency))
                {
                    queue.Enqueue(currentElement);
                    continue;
                }
                
                visited.Add(currentElement);
                yield return currentElement;
            }
        }
        
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

                    var component = gameObject.GetComponentInChildren(type);
                    if (component != null)
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
