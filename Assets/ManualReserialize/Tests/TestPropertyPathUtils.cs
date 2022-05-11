using NUnit.Framework;

namespace ManualReserialization.Tests
{
    [TestFixture]
    public class TestPropertyPathUtils
    {
        [Test]
        [TestCase("theClass.A", "theClass", new[] { "B", "A" }, ExpectedResult = true)]
        [TestCase("theClass.A.XX.Y", "theClass.A", new[] { "B", "XX.Y" }, ExpectedResult = true)]
        [TestCase("theClass.A", "theClass", new[] { "B" }, ExpectedResult = false)]
        [TestCase("theClass.AA.X", "theClass", new[] { "B", "AA" }, ExpectedResult = false)]
        public bool TestDoesPropertyModificationMatchAnyPath(string propertyPath, string initialPath, string[] reserializePaths)
        {
            return PropertyPathUtils.DoesPropertyModificationMatchAnyPath(propertyPath, initialPath, reserializePaths);
        }
    }
}
