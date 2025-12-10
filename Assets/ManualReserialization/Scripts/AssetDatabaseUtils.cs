#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PereViader.ManualReserialization
{
    public static class AssetDatabaseUtils
    {
        /// <summary>
        /// Lazily gets all prefab asset paths for prefabs that have a specific component type
        /// anywhere in their hierarchy, sorted so that base prefab paths come before their variants.
        /// GameObjects are loaded only transiently during checking/sorting or finally by the caller.
        /// </summary>
        /// <param name="type">The Component type to search for.</param>
        /// <returns>An IEnumerable of asset paths (strings) for qualifying prefabs, sorted by dependency.</returns>
        public static IEnumerable<GameObject> GetAllPrefabsWithComponentSortedByVariant(Type type)
        {
            var pathsWithComponent = GetAllPrefabPathsWithComponent(type);
            
            var dependencyCache = new Dictionary<string, string>();

            Func<string, string> getDependencyPath = (currentPath) =>
            {
                if (dependencyCache.TryGetValue(currentPath, out var cachedDependencyPath))
                {
                    return cachedDependencyPath;
                }

                string resultDependencyPath = null;
                GameObject currentGO = AssetDatabase.LoadAssetAtPath<GameObject>(currentPath);
                if (currentGO is not null)
                {
                    GameObject dependencyGO = PrefabUtility.GetCorrespondingObjectFromSource(currentGO);
                    if (dependencyGO is not null)
                    {
                        resultDependencyPath = AssetDatabase.GetAssetPath(dependencyGO);
                        if (string.IsNullOrEmpty(resultDependencyPath)) {
                            // Should not happen for valid source objects, but handle defensively
                            Debug.LogWarning($"Could not get asset path for dependency of {currentPath}");
                            resultDependencyPath = null; // Treat as root if path is invalid
                        }
                    }
                }
                else {
                     Debug.LogWarning($"Could not load GameObject at path '{currentPath}' during dependency check.");
                }

                // Store in cache (even if null)
                dependencyCache[currentPath] = resultDependencyPath;
                return resultDependencyPath;
            };

            foreach (var path in PerformBreadthFirstSort(pathsWithComponent, getDependencyPath))
            {
                yield return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }

        /// <summary>
        /// Lazily finds the asset paths of all prefabs that contain a given component type.
        /// Loads GameObjects only temporarily to check for the component.
        /// </summary>
        /// <param name="type">The Component type to search for.</param>
        /// <returns>An IEnumerable of asset paths (strings).</returns>
        private static IEnumerable<string> GetAllPrefabPathsWithComponent(Type type)
        {
            var guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) 
                {
                    // Skip if path is invalid
                    continue;
                }
                
                if (path.StartsWith("Packages/"))
                {
                    continue;
                }

                var rootGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (rootGameObject is null)
                {
                    // Silently skip if prefab asset is broken/unloadable
                    continue;
                }

                if (rootGameObject.GetComponentInChildren(type, true) is not null)
                {
                    yield return path;
                }
            }
        }
        
        private static IEnumerable<T> PerformBreadthFirstSort<T>(IEnumerable<T> items, Func<T, T> getDependency) where T : class // Changed name slightly for clarity
        {
            var uniqueItems = new HashSet<T>(items);
            if (uniqueItems.Count == 0)
            {
                yield break;
            }

            var queue = new Queue<T>(uniqueItems);
            var visited = new HashSet<T>(uniqueItems.Count);

            while (queue.Count > 0)
            {
                var currentItem = queue.Dequeue();
                var dependency = getDependency(currentItem);

                if (dependency is not null && uniqueItems.Contains(dependency) && !visited.Contains(dependency))
                {
                    queue.Enqueue(currentItem);
                    continue;
                }
                
                if (visited.Add(currentItem))
                {
                    yield return currentItem;
                }
            }
        }

        public static IEnumerable<string> GetAllScenePaths()
        {
            var guids = AssetDatabase.FindAssets("t:Scene");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.StartsWith("Packages/"))
                {
                    continue;
                }
                yield return path;
            }
        }

        public static IEnumerable<UnityEngine.Object> GetAllAssetsOfType(Type type)
        {
            var guids = AssetDatabase.FindAssets("t:" + type.Name);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.StartsWith("Packages/"))
                {
                    continue;
                }
                //Maybe this will have problems with assets that have more than one object at the same path (?)
                yield return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            }
        }
    }
}
#endif
