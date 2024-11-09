using ManualReserialization.Tests.TestClasses;
using NUnit.Framework;

namespace ManualReserialization.Tests
{
    [TestFixture]
    public class TestScriptableObjectUnityReserializeUtils
    {
        private readonly DeleteAssetsTearDown deleteAssetsTearDown = new DeleteAssetsTearDown();

        [TearDown]
        public void Teardown()
        {
            deleteAssetsTearDown.TearDown();
        }

        [Test]
        public void TestReserializeScriptableObjectWithPublicToFind()
        {
            var asset1 = deleteAssetsTearDown.CreateScriptableObject<ScriptableObjectWithPublicToFind>();
            var asset2 = deleteAssetsTearDown.CreateScriptableObject<ScriptableObjectWithPublicToFind>();

            ScriptableObjectReserializer.Reserialize<ScriptableObjectWithPublicToFind>((x, metadata) => x.toFind.found++);

            asset1.Invoke(x => Assert.That(x.toFind.found, Is.EqualTo(1)));
            asset2.Invoke(x => Assert.That(x.toFind.found, Is.EqualTo(1)));
        }

        [Test]
        public void TestReserializeScriptableObjectWithPublicNestedToFind()
        {
            var asset1 = deleteAssetsTearDown.CreateScriptableObject<ScriptableObjectWithPublicNestedToFind>();
            var asset2 = deleteAssetsTearDown.CreateScriptableObject<ScriptableObjectWithPublicNestedToFind>();

            ScriptableObjectReserializer.Reserialize<ScriptableObjectWithPublicNestedToFind>((x, metadata) => x.nestedToFind.toFind.found++);

            asset1.Invoke(x => Assert.That(x.nestedToFind.toFind.found, Is.EqualTo(1)));
            asset2.Invoke(x => Assert.That(x.nestedToFind.toFind.found, Is.EqualTo(1)));
        }

        [Test]
        public void TestReserializeScriptableObjectWithPublicSerializedDoubleNestedToFind()
        {
            var asset1 = deleteAssetsTearDown.CreateScriptableObject<ScriptableObjectWithPublicSerializedDoubleNestedToFind>();
            var asset2 = deleteAssetsTearDown.CreateScriptableObject<ScriptableObjectWithPublicSerializedDoubleNestedToFind>();

            ScriptableObjectReserializer.Reserialize<ScriptableObjectWithPublicSerializedDoubleNestedToFind>((x, metadata) => x.doubleNestedToFind.toFind.toFind.found++);

            asset1.Invoke(x => Assert.That(x.doubleNestedToFind.toFind.toFind.found, Is.EqualTo(1)));
            asset2.Invoke(x => Assert.That(x.doubleNestedToFind.toFind.toFind.found, Is.EqualTo(1)));
        }
    }
}
