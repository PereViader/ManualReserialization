#if UNITY_EDITOR
using UnityEngine;

namespace PereViader.ManualReserialization.Tests.TestClasses
{
    public class MonoBehaviourWithPrivateSerializedToFind : MonoBehaviourTest
    {
        [SerializeField]
        private ToFind toFind;
    }
}
#endif
