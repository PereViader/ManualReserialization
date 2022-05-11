#if UNITY_EDITOR
namespace ManualReserialization.Tests.TestClasses
{
    public class MonoBehaviourWithPrivateNonSerializedToFind : MonoBehaviourTest
    {
        private ToFind toFind;
    }
}
#endif
