#if UNITY_EDITOR
using UnityEngine;

namespace ManualReserialization.Tests.TestClasses
{
    public class MonoBehaviourWithPrivateSerializedToFind : MonoBehaviourTest
    {
        [SerializeField]
        private ToFind toFind;
    }
}
#endif
