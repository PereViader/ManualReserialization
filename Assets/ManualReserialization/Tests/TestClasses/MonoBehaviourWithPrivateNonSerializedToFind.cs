#if UNITY_EDITOR
namespace PereViader.ManualReserialization.Tests.TestClasses
{
    public class MonoBehaviourWithPrivateNonSerializedToFind : MonoBehaviourTest
    {
        private ToFind toFind;
    }
}
#endif
