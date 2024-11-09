#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace ManualReserialization
{
    public static class ScriptableObjectReserializer
    {
        public static void Reserialize<T>(ReserializeDelegate<T> action) where T : ScriptableObject
        {
            foreach (var asset in AssetDatabaseUtils.GetAllAssetsOfType(typeof(T)))
            {
                var scriptableObject = (T)asset;
                action.Invoke(scriptableObject, new ScriptableObjectMetadata(scriptableObject));
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
#endif
