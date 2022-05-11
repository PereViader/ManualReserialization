using ManualReserialization.Tests.TestClasses;
using NUnit.Framework;

namespace ManualReserialization.Tests
{
    [TestFixture]
    public class TestReflectionUtils
    {
        [Test]
        public void TestGetAllAssignableTypesForType()
        {
            var types = ReflectionUtils.GetAllAssignableTypesForType(typeof(MonoBehaviourTest));

            Assert.That(types, Is.EquivalentTo(new[] {
                typeof(MonoBehaviourTest),
                typeof(MonoBehaviourWithPrivateSerializedToFind),
                typeof(MonoBehaviourWithPrivateNonSerializedToFind),
                typeof(MonoBehaviourWithPublicToFind),
                typeof(MonoBehaviourWithNestedToFind),
                typeof(MonoBehaviourWithNonSerializedNestedToFind),
                typeof(MonoBehaviourWithDoubleNestedToFind),
            }));
        }

        [Test]
        public void TestGetAllAssignableTypesWithNestedMemberOfType()
        {
            var types = ReflectionUtils.GetAllAssignableTypesWithNestedMemberOfType(
                typeof(MonoBehaviourTest),
                typeof(ToFind),
                _ => true);

            Assert.That(types, Is.EquivalentTo(new[]
            {
                typeof(MonoBehaviourWithPrivateSerializedToFind),
                typeof(MonoBehaviourWithPrivateNonSerializedToFind),
                typeof(MonoBehaviourWithPublicToFind),
                typeof(MonoBehaviourWithNestedToFind),
                typeof(MonoBehaviourWithNonSerializedNestedToFind),
                typeof(MonoBehaviourWithDoubleNestedToFind),
            }));
        }
    }
}
