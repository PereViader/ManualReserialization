#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ManualReserialization
{
    public static class ReflectionUtils
    {
        public static IEnumerable<Type> GetAllAssignableTypesWithNestedMemberOfType(
            Type assignableType,
            Type memberType,
            Predicate<FieldInfo> checkField)
        {
            foreach (var type in GetAllAssignableTypesForType(assignableType))
            {
                if (DoesTypeHaveNestedMemberOfType(type, memberType, checkField))
                {
                    yield return type;
                }
            }
        }

        private static bool DoesTypeHaveNestedMemberOfType(
            Type type,
            Type memberType,
            Predicate<FieldInfo> checkField)
        {
            var seenTypes = new HashSet<Type>();
            var toInspect = new Stack<Type>();
            toInspect.Push(type);

            while (toInspect.Count > 0)
            {
                Type currentType = toInspect.Pop();

                if (!seenTypes.Add(currentType))
                {
                    continue;
                }

                var fieldInfos = currentType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (var fieldInfo in fieldInfos)
                {
                    if (!checkField.Invoke(fieldInfo))
                    {
                        continue;
                    }

                    var fieldType = fieldInfo.FieldType;
                    if (fieldType.Equals(memberType))
                    {
                        return true;
                    }

                    toInspect.Push(fieldType);
                }
            }

            return false;
        }

        public static List<List<FieldInfo>> GetFieldInfoPathOfNestedTypeApperencesInType(
            Type baseType,
            Type toFind,
            Predicate<FieldInfo> checkField)
        {
            var nestedApperences = new List<List<FieldInfo>>();
            var fieldChain = new Stack<FieldInfo>();
            Stack<(FieldInfo field, int depth)> toInspect = new Stack<(FieldInfo, int)>();

            foreach (var field in baseType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                toInspect.Push((field, 1));
            }


            while (toInspect.Count > 0)
            {
                (FieldInfo fieldInfo, int depth) = toInspect.Pop();

                if (!checkField.Invoke(fieldInfo))
                {
                    continue;
                }

                while (depth <= fieldChain.Count)
                {
                    fieldChain.Pop();
                }

                fieldChain.Push(fieldInfo);

                var fieldType = fieldInfo.FieldType;
                if (fieldType.Equals(toFind))
                {
                    var fieldChainCopy = fieldChain.Reverse().ToList();
                    nestedApperences.Add(fieldChainCopy);
                    continue;
                }

                var nestedFields = fieldInfo.FieldType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (var nestedField in nestedFields)
                {
                    toInspect.Push((nestedField, depth + 1));
                }
            }

            return nestedApperences;
        }

        public static IEnumerable<Type> GetAllAssignableTypesForType(Type type)
        {
            foreach (var typeToCheck in GetAllTypes())
            {
                if (type.IsAssignableFrom(typeToCheck))
                {
                    yield return typeToCheck;
                }
            }
        }

        public static IEnumerable<Type> GetAllTypes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    yield return type;
                }
            }
        }

        public static object GetNestedObjectInitializing(List<FieldInfo> fieldInfos, object obj)
        {
            var current = obj;
            foreach (var field in fieldInfos)
            {
                var fieldValue = field.GetValue(current);
                if (current == null)
                {
                    fieldValue = Activator.CreateInstance(field.FieldType);
                    field.SetValue(current, fieldValue);
                }

                current = fieldValue;
            }
            return current;
        }
    }
}
#endif
