#if UNITY_EDITOR
namespace ManualReserialization.Tests.TestClasses
{
    public class ScriptableObjectWithPublicNestedToFind : ScriptableObjectTest
    {
        public SerializedNestedToFind nestedToFind;
    }
}
#endif
