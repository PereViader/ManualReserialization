#if UNITY_EDITOR
using System;

namespace ManualReserialization.Tests.TestClasses
{
    [Serializable]
    public class SerializedNestedToFind
    {
        public ToFind toFind;
    }
}
#endif
