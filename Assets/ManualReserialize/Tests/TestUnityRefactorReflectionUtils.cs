using ManualReserialization.Tests.TestClasses;
using NUnit.Framework;
using System;
using System.Reflection;
using UnityEngine;

namespace ManualReserialization.Tests
{
    [TestFixture]
    public class TestUnityRefactorReflectionUtils
    {
        [Test]
        public void TestGetAllAssignableTypesWithNestedMemberOfType()
        {
            var types = ReserializeReflectionUtils.GetAllAssignableTypesWithNestedMemberOfType(
                typeof(MonoBehaviourTest),
                typeof(ToFind));

            Assert.That(types, Is.EquivalentTo(new[]
            {
                typeof(MonoBehaviourWithPrivateSerializedToFind),
                typeof(MonoBehaviourWithPublicToFind),
                typeof(MonoBehaviourWithNestedToFind),
                typeof(MonoBehaviourWithDoubleNestedToFind),
            }));
        }

        public class TrueShouldInspectFieldClass
        {
            public ToFind publicToFind;
            [SerializeField] public ToFind serializedPublicToFind;
            [SerializeField] private ToFind serializedPrivateToFind;
            [SerializeField] protected ToFind serializedProtectedToFind;
        }

#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0044 // Add readonly modifier
        public class FalseShouldInspectFieldClass
        {
            private ToFind privateToFind;
            protected ToFind protectedToFind;
            public static ToFind publicStaticToFind;
            private static ToFind privateStaticToFind;
            protected static ToFind protectedStaticToFind;
            public readonly ToFind publicReadonlyToFind;
            private readonly ToFind privateReadonlyToFind;
            protected readonly ToFind protectedReadonlyToFind;
            public const ToFind publicConstToFind = null;
            private const ToFind privateConstToFind = null;
            protected const ToFind protectedConstToFind = null;
            [NonSerialized] public ToFind nonSerializedPublicToFind;
            [NonSerialized] private ToFind nonSerializedPrivateToFind;
            [NonSerialized] protected ToFind nonSerializedProtectedToFind;
        }
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members

        [Test]
        public void TestShouldInspectField()
        {
            foreach (FieldInfo field in typeof(TrueShouldInspectFieldClass).GetFields())
            {
                Assert.That(ReserializeReflectionUtils.IsFieldReserializable(field), Is.True, () => field.Name + "should be true");
            }

            foreach (FieldInfo field in typeof(FalseShouldInspectFieldClass).GetFields())
            {
                Assert.That(ReserializeReflectionUtils.IsFieldReserializable(field), Is.False, () => field.Name + "should be false");
            }
        }
    }
}
