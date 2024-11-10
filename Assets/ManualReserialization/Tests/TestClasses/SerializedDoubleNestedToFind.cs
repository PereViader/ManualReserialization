#if UNITY_EDITOR
using System;

namespace PereViader.ManualReserialization.Tests.TestClasses
{
    [Serializable]
    public class SerializedDoubleNestedToFind
    {
        public SerializedNestedToFind toFind;
    }
}
#endif
