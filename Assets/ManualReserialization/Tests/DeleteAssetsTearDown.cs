using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PereViader.ManualReserialization.Tests
{
    public class DeleteAssetsTearDown : IDisposable
    {
        private readonly List<string> assetsToDestroy = new List<string>();

        public Action<Action<T>> CreateScriptableObject<T>() where T : ScriptableObject
        {
            var path = AssetDatabase.GenerateUniqueAssetPath("Assets/TestAsset.asset");
            assetsToDestroy.Add(path);

            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);

            return x =>
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                x.Invoke(asset);
            };
        }

        public Action<Action<T>> CreatePrefabWithComponent<T>(string name) where T : MonoBehaviour
        {
            var path = AssetDatabase.GenerateUniqueAssetPath("Assets/TestPrefabAsset.prefab");
            assetsToDestroy.Add(path);

            var gameObject = new GameObject(name);
            var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(
                gameObject,
                path,
                InteractionMode.AutomatedAction);
            GameObject.DestroyImmediate(gameObject);
            var component = prefab.AddComponent<T>();

            //We need to set the asset as dirty and save the database to initialize
            //the serialized values of the component in the prefab
            EditorUtility.SetDirty(component);
            AssetDatabase.SaveAssets();

            return x =>
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var component = asset.GetComponent<T>();
                x.Invoke(component);
            };
        }

        /// <summary>
        /// Creates a prefab and a prefab variant on the asset database and queues them for removal on teardown
        /// </summary>
        /// <typeparam name="T">The component to add to the gameobjects</typeparam>
        /// <param name="name">Name to identify the objects</param>
        /// <param name="alterVariant">Action where to change the prefab variant</param>
        /// <returns>Action where the first element is the prefab and the second is the variant</returns>
        public Action<Action<T, T>> CreatePrefabAndVariantWithComponent<T>(string name, Action<T> alterVariant)
            where T : MonoBehaviour
        {
            var pathPrefab = AssetDatabase.GenerateUniqueAssetPath("Assets/TestPrefabAsset.prefab");
            var pathVariant = AssetDatabase.GenerateUniqueAssetPath("Assets/TestPrefabVariantAsset.prefab");
            assetsToDestroy.Add(pathVariant);
            assetsToDestroy.Add(pathPrefab);

            var gameObject = new GameObject(name);
            var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(
                gameObject,
                pathPrefab,
                InteractionMode.AutomatedAction);
            var component = prefab.AddComponent<T>();

            //We need to set the asset as dirty and save the database to initialize
            //the serialized values of the component in the prefab
            EditorUtility.SetDirty(component);
            AssetDatabase.SaveAssets();

            var prefabVariant = PrefabUtility.SaveAsPrefabAsset(gameObject, pathVariant);
            var variantComponent = prefabVariant.GetComponent<T>();
            alterVariant.Invoke(variantComponent);
            EditorUtility.SetDirty(variantComponent);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return x =>
            {
                var assetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pathPrefab);
                var componentPrefab = assetPrefab.GetComponent<T>();

                var assetVariant = AssetDatabase.LoadAssetAtPath<GameObject>(pathVariant);
                var componentVariant = assetVariant.GetComponent<T>();

                x.Invoke(componentPrefab, componentVariant);
            };
        }

        public Action<Action<T>> CreateSceneWithGameObjectComponent<T>(string name) where T : MonoBehaviour
        {
            var path = AssetDatabase.GenerateUniqueAssetPath("Assets/TestScene.unity");
            assetsToDestroy.Add(path);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var gameObject = new GameObject(name);
            var component = gameObject.AddComponent<T>();
            SceneManager.MoveGameObjectToScene(gameObject, scene);

            if (!EditorSceneManager.SaveScene(scene, path))
            {
                throw new System.Exception("Could not save scene");
            }

            //Clear the scene by opening a new empty one
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            return x =>
            {
                EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                var scene = EditorSceneManager.GetSceneByPath(path);
                var sceneGameObject = scene.GetRootGameObjects()[0];
                var sceneComponent = sceneGameObject.GetComponent<T>();
                x.Invoke(sceneComponent);

                //Clear the scene by opening a new empty one
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            };
        }

        public (string rootPath, string variantPath) CreateVariantWhereRootLacksComponent<T>(string baseName)
            where T : MonoBehaviour
        {
            var rootPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/{baseName}_Root.prefab");
            var variantPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/{baseName}_Variant.prefab");
            // Ensure these paths are registered for deletion in TearDown
            assetsToDestroy.Add(variantPath);
            assetsToDestroy.Add(rootPath);
            
            GameObject rootGo = null;
            GameObject variantGo = null; // Instance used to create the variant asset
            GameObject rootPrefabAsset = null;
            GameObject variantPrefabAsset = null;

            try
            {
                // 1. Create the root GameObject (without the component)
                rootGo = new GameObject($"{baseName}_Root");

                // --- Use SaveAsPrefabAssetAndConnect for the root ---
                // This creates the asset and connects rootGo to it.
                // It also handles destroying rootGo automatically on success.
                rootPrefabAsset = PrefabUtility.SaveAsPrefabAssetAndConnect(
                    rootGo,
                    rootPath,
                    InteractionMode.AutomatedAction);

                if (rootPrefabAsset == null)
                {
                    // If rootGo wasn't destroyed automatically due to failure, clean it up.
                    if (rootGo != null) GameObject.DestroyImmediate(rootGo);
                    throw new Exception($"Failed to save root prefab at {rootPath}");
                }
                // Note: rootGo is likely destroyed or invalid after SaveAsPrefabAssetAndConnect succeeds.
                // We work with rootPrefabAsset asset from now on.

                // 2. Create the variant instance based on the saved root prefab *asset*
                variantGo = (GameObject)PrefabUtility.InstantiatePrefab(rootPrefabAsset);
                if (variantGo == null) throw new Exception("Failed to instantiate prefab asset for variant creation.");
                variantGo.name = $"{baseName}_Variant";

                // 3. Add the component ONLY to the variant instance
                var component = variantGo.AddComponent<T>();
                // --- Mark the instance dirty after adding component ---
                EditorUtility.SetDirty(component);
                EditorUtility.SetDirty(variantGo);

                // 4. Save the modified variant instance as a new prefab asset (the variant)
                //    Use SaveAsPrefabAsset, we don't want to connect variantGo back.
                variantPrefabAsset = PrefabUtility.SaveAsPrefabAsset(variantGo, variantPath);
                if (variantPrefabAsset == null) throw new Exception($"Failed to save variant prefab at {variantPath}");
                // --- Mark the new variant asset dirty ---
                EditorUtility.SetDirty(variantPrefabAsset);

                // 5. Save assets *after* both prefabs are created and potentially dirtied
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // --- Verification (Optional but recommended) ---
                // Reload assets fresh from disk to ensure links are correct after save/refresh
                var loadedRoot = AssetDatabase.LoadAssetAtPath<GameObject>(rootPath);
                var loadedVariant = AssetDatabase.LoadAssetAtPath<GameObject>(variantPath);
                if (loadedRoot == null || loadedVariant == null)
                    throw new Exception("Failed to load created prefabs after save.");
                if (loadedRoot.GetComponent<T>() != null)
                    throw new Exception("Test Setup Error: Root prefab incorrectly has the component.");
                if (loadedVariant.GetComponent<T>() == null)
                    throw new Exception("Test Setup Error: Variant prefab lacks the component.");

                var sourceOfVariant = PrefabUtility.GetCorrespondingObjectFromSource(loadedVariant);
                if (sourceOfVariant == null)
                    throw new Exception("Test Setup Error: Variant link to source is null after save.");
                if (sourceOfVariant.GetInstanceID() != loadedRoot.GetInstanceID())
                    throw new Exception(
                        $"Test Setup Error: Variant points to the wrong source. Expected {loadedRoot.name}, got {sourceOfVariant.name}");

                return (rootPath, variantPath);
            }
            finally
            {
                // Clean up the temporary scene instance used for the variant
                // rootGo should have been handled by SaveAsPrefabAssetAndConnect
                if (variantGo != null)
                {
                    GameObject.DestroyImmediate(variantGo);
                }

                // Belt-and-suspenders: Ensure rootGo is gone if SaveAsPrefabAssetAndConnect failed early
                if (rootGo != null && rootPrefabAsset == null)
                {
                    GameObject.DestroyImmediate(rootGo);
                }
            }
        }

        public void Dispose()
        {
            AssetDatabase.Refresh();
            foreach (var path in assetsToDestroy)
            {
                if (path == null)
                {
                    continue;
                }

                if (!AssetDatabase.DeleteAsset(path))
                {
                    Debug.LogError($"Could not delete asset with path {path}");
                }
            }

            assetsToDestroy.Clear();
        }
    }
}
