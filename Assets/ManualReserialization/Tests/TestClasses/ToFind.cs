#if UNITY_EDITOR
using System;

namespace PereViader.ManualReserialization.Tests.TestClasses
{
    [Serializable]
    public class ToFind
    {
        public int previousValue = 0;
        public int newValue;
    }
}
#endif
