#if UNITY_EDITOR
using System;

namespace ManualReserialization.Tests.TestClasses
{
    [Serializable]
    public class SerializedDoubleNestedToFind
    {
        public SerializedNestedToFind toFind;
    }
}
#endif
