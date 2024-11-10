#if UNITY_EDITOR
using System;

namespace PereViader.ManualReserialization.Tests.TestClasses
{
    [Serializable]
    public class SerializedNestedToFind
    {
        public ToFind toFind;
    }
}
#endif
