#if UNITY_EDITOR
namespace PereViader.ManualReserialization.Tests.TestClasses
{
    public class ScriptableObjectWithPublicNestedToFind : ScriptableObjectTest
    {
        public SerializedNestedToFind nestedToFind;
    }
}
#endif
