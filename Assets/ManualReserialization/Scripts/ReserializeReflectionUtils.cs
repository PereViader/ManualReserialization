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

        public static List<FieldInfo[]> GetFieldInfoPathOfNestedTypeApperencesInType(Type baseType, Type toFind)
        {
            return ReflectionUtils.GetFieldInfoPathOfNestedTypeAppearencesInType(
                baseType,
                toFind,
                IsFieldReserializable);
        }

        public static bool IsTypeReserializable(Type type)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return false;
            }

            if (type.GetCustomAttribute<SerializableAttribute>() is not null)
            {
                return true;
            }

            return typeof(ISerializationCallbackReceiver).IsAssignableFrom(type);
        }

        public static bool IsFieldReserializable(FieldInfo fieldInfo)
        {
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
            
            if (fieldInfo.GetCustomAttribute<NonSerializedAttribute>() is not null)
            {
                return false;
            }

            if (!(fieldInfo.IsPublic || fieldInfo.GetCustomAttribute<SerializeField>() is not null))
            {
                return false;
            }

            return IsTypeReserializable(fieldInfo.FieldType);
        }
    }
}
#endif
