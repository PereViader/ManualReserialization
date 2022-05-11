#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace ManualReserialization
{
    public static class ScriptableObjectReserializer
    {
        public static void Reserialize<T>(Action<T> action) where T : ScriptableObject
        {
            foreach (var asset in AssetDatabaseUtils.GetAllAssetsOfType(typeof(T)))
            {
                action.Invoke((T)asset);
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
#endif
