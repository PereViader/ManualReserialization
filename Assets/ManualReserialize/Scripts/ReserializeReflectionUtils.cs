#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace ManualReserialization
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

        public static List<List<FieldInfo>> GetFieldInfoPathOfNestedTypeApperencesInType(Type baseType, Type toFind)
        {
            return ReflectionUtils.GetFieldInfoPathOfNestedTypeApperencesInType(
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

        public static bool ShouldApplyReserialize(UnityEngine.Object obj, Type type, string path, string[] reserializePaths)
        {
            if (IsObjectFromRegularGameObject(obj))
            {
                return true;
            }

            if (IsObjectFromRegularPrefabOnProject(obj))
            {
                return true;
            }

            //Object is one of these: prefab instance on scene / prefab variant on project / prefab variant on scene
            //All these options have modifications over the parent prefab we can check

            if (DoesObjectHaveMatchingModification(obj, type, path, reserializePaths))
            {
                return true;
            }

            return false;
        }

        private static bool DoesObjectHaveMatchingModification(UnityEngine.Object obj, Type type, string path, string[] reserializePaths)
        {
            var propertyModifications = PrefabUtility.GetPropertyModifications(obj);
            if (propertyModifications == null)
            {
                return false;
            }

            foreach (var propertyModification in propertyModifications)
            {
                if (propertyModification.target.GetType() != type)
                {
                    continue;
                }

                if (PropertyPathUtils.DoesPropertyModificationMatchAnyPath(
                    propertyModification.propertyPath,
                    path,
                    reserializePaths))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsObjectFromRegularPrefabOnProject(UnityEngine.Object obj)
        {
            return !PrefabUtility.IsPartOfPrefabInstance(obj) && !PrefabUtility.IsPartOfVariantPrefab(obj);
        }

        private static bool IsObjectFromRegularGameObject(UnityEngine.Object obj)
        {
            return !PrefabUtility.IsPartOfAnyPrefab(obj);
        }
    }
}
#endif
