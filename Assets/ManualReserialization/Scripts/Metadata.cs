#if UNITY_EDITOR
using UnityEngine;

namespace PereViader.ManualReserialization
{
    public enum MetadataType
    {
        GameObjectPrefab,
        ScriptableObject,
        Scene
    }
    
    public abstract class Metadata
    {
        public abstract MetadataType MetadataType { get; }
    }
    
    public sealed class GameObjectPrefabMetadata : Metadata
    {
        public override MetadataType MetadataType => MetadataType.GameObjectPrefab;
        public GameObject GameObjectPrefab { get; }
        public Component ParentComponent { get; }

        public GameObjectPrefabMetadata(GameObject gameObjectPrefab, Component parentComponent)
        {
            GameObjectPrefab = gameObjectPrefab;
            ParentComponent = parentComponent;
        }
    }

    public sealed class ScriptableObjectMetadata : Metadata
    {
        public override MetadataType MetadataType => MetadataType.ScriptableObject;
        public ScriptableObject ScriptableObject { get; }

        public ScriptableObjectMetadata(ScriptableObject scriptableObject)
        {
            ScriptableObject = scriptableObject;
        }
    }
    
    public sealed class SceneMetadata : Metadata
    {
        public override MetadataType MetadataType => MetadataType.Scene;
        public string ScenePath { get; }
        public Component ParentComponent { get; }

        public SceneMetadata(string scenePath, Component parentComponent)
        {
            ScenePath = scenePath;
            ParentComponent = parentComponent;
        }
    }
}
#endif