#if UNITY_EDITOR
namespace ManualReserialization.Tests.TestClasses
{
    public class MonoBehaviourWithNonSerializedNestedToFind : MonoBehaviourTest
    {
        public NonSerializedNestedToFind nestedToFind;
    }
}
#endif
