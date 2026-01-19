#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace PereViader.ManualReserialization
{
    public delegate void ReserializeDelegate<in T>(T instance, Metadata metadata);
    
    public static class Reserializer
    {
        /// <summary>
        /// Finds all the assets in the project project where the type is used and applies the delegate to them
        /// </summary>
        public static void Reserialize<T>(ReserializeDelegate<T> action)
        {
            if (Application.isPlaying)
            {
                throw new InvalidOperationException("Stop Playing before trying to reserialize");
            }

            AssetDatabase.SaveAssets();

            var type = typeof(T);

            if (typeof(Component).IsAssignableFrom(type))
            {
                var method = typeof(ComponentReserializer).GetMethod(nameof(ComponentReserializer.Reserialize));
                var generic = method!.MakeGenericMethod(type);
                generic.Invoke(null, new object[] { action });
                return;
            }

            if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                var method = typeof(ScriptableObjectReserializer).GetMethod(nameof(ScriptableObjectReserializer.Reserialize));
                var generic = method!.MakeGenericMethod(type);
                generic.Invoke(null, new object[] { action });
                return;
            }

            if (ReserializeReflectionUtils.IsTypeReserializable(type))
            {
                SerializedClassReserializer.Reserialize<T>(action);
                return;
            }

            Debug.LogError($"The type {typeof(T).FullName} can not be reserialized");
        }
    }
}
#endif
