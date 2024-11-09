﻿#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace ManualReserialization
{
    public delegate void ReserializeDelegate<T>(T instance, Metadata metadata);
    
    public static class Reserializer
    {
        /// <summary>
        /// Finds all the assets in the project project where the type is used and applies the delegate to them
        /// </summary>
        public static void Reserialize<T>(ReserializeDelegate<T> action, params string[] reserializePaths)
        {
            if (Application.isPlaying)
            {
                throw new InvalidOperationException("Stop Playing before trying to reserialize");
            }

            AssetDatabase.SaveAssets();

            var type = typeof(T);

            if (typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                var method = typeof(MonoBehaviourReserializer).GetMethod(nameof(MonoBehaviourReserializer.Reserialize));
                var generic = method.MakeGenericMethod(type);
                generic.Invoke(null, new object[] { action, reserializePaths });
                return;
            }

            if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                var method = typeof(ScriptableObjectReserializer).GetMethod(nameof(ScriptableObjectReserializer.Reserialize));
                var generic = method.MakeGenericMethod(type);
                generic.Invoke(null, new object[] { action });
                return;
            }

            if (ReserializeReflectionUtils.IsTypeSerializedAndNotAnEngineObject(type))
            {
                SerializedClassReserializer.Reserialize<T>(action, reserializePaths);
                return;
            }

            Debug.LogError("The type is not valid to be reserialized");
        }
    }
}
#endif
