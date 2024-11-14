#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace PereViader.ManualReserialization
{
    public static class ReserializeReflectionUtils
    {
        public static IEnumerable<Type> GetAllAssignableTypesWithNestedMemberOfType(Type assignableType, Type memberType)
        {
            return ReflectionUtils.GetAllAssignableTypesWithNestedMemberOfType(
                assignableType,
                memberType,
                IsFieldReserializable
                );
        }

        public static List<FieldInfo[]> GetFieldInfoPathOfNestedTypeAppearencesInType(Type baseType, Type toFind)
        {
            return ReflectionUtils.GetFieldInfoPathOfNestedTypeAppearencesInType(
                baseType,
                toFind,
                IsFieldReserializable);
        }

        public static bool IsFieldReserializable(FieldInfo fieldInfo)
        {
            //In case we missed some rule https://docs.unity3d.com/Manual/script-Serialization.html

            if (!IsFieldSerialized(fieldInfo))
            {
                return false;
            }

            return IsTypeSerializedAndNotAnEngineObject(fieldInfo.FieldType);
        }

        public static bool IsTypeSerializedAndNotAnEngineObject(Type type)
        {
            if (type.IsAssignableFrom(typeof(UnityEngine.Object)))
            {
                return false;
            }

            return type.GetCustomAttribute<SerializableAttribute>() != null;
        }

        public static bool IsFieldSerialized(FieldInfo fieldInfo)
        {
            if (fieldInfo.GetCustomAttribute<NonSerializedAttribute>() != null)
            {
                return false;
            }

            if (fieldInfo.IsStatic)
            {
                return false;
            }

            if (fieldInfo.IsLiteral)
            {
                return false;
            }

            if (fieldInfo.IsInitOnly)
            {
                return false;
            }

            if (fieldInfo.IsPublic)
            {
                return true;
            }

            if (fieldInfo.GetCustomAttribute<SerializeField>() != null)
            {
                return true;
            }

            return false;
        }
    }
}
#endif
