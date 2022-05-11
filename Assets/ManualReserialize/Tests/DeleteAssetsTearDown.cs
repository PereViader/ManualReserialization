using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ManualReserialization.Tests
{
    public class DeleteAssetsTearDown
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
        public Action<Action<T, T>> CreatePrefabAndVariantWithComponent<T>(string name, Action<T> alterVariant) where T : MonoBehaviour
        {
            var pathPrefab = AssetDatabase.GenerateUniqueAssetPath("Assets/TestPrefabAsset.prefab");
            var pathVariant = AssetDatabase.GenerateUniqueAssetPath("Assets/TestPrefabVariantAsset.prefab");
            assetsToDestroy.Add(pathPrefab);
            assetsToDestroy.Add(pathVariant);

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

        public void TearDown()
        {
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
