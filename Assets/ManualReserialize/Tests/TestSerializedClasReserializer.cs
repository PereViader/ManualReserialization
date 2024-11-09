using ManualReserialization.Tests.TestClasses;
using NUnit.Framework;

namespace ManualReserialization.Tests
{
    [TestFixture]
    public class TestSerializedClasReserializer
    {
        private readonly DeleteAssetsTearDown deleteAssetsTearDown = new DeleteAssetsTearDown();

        [TearDown]
        public void TearDown()
        {
            deleteAssetsTearDown.TearDown();
        }

        [Test]
        public void TestSerializedClassReserializerScriptableObject()
        {
            var scriptableObject = deleteAssetsTearDown.CreateScriptableObject<ScriptableObjectWithPublicSerializedDoubleNestedToFind>();
            var executionCount = 0;

            SerializedClassReserializer.Reserialize<ToFind>((x, metadata) => { executionCount++; x.found++; }, new[] { "found" });

            scriptableObject.Invoke(x =>
            {
                Assert.That(x.doubleNestedToFind.toFind.toFind.found, Is.EqualTo(1));
                Assert.That(executionCount, Is.EqualTo(1));
            });
        }

        [Test]
        public void TestSerializedClassReserializerPrefabChanged()
        {
            var asset = deleteAssetsTearDown.CreatePrefabAndVariantWithComponent<MonoBehaviourWithDoubleNestedToFind>(
                "asset",
                x => x.doubleNestedToFind.toFind.toFind.found = 5);
            var executionCount = 0;

            SerializedClassReserializer.Reserialize<ToFind>((x, metadata) => { executionCount++; x.found++; }, new[] { "found" });

            asset.Invoke((prefab, variant) =>
            {
                Assert.That(prefab.doubleNestedToFind.toFind.toFind.found, Is.EqualTo(1));
                Assert.That(variant.doubleNestedToFind.toFind.toFind.found, Is.EqualTo(6));
                Assert.That(executionCount, Is.EqualTo(2));
            });
        }

        [Test]
        public void TestSerializedClassReserializerPrefabUnchanged()
        {
            var asset = deleteAssetsTearDown.CreatePrefabAndVariantWithComponent<MonoBehaviourWithDoubleNestedToFind>(
                "asset",
                x => { });
            var executionCount = 0;

            SerializedClassReserializer.Reserialize<ToFind>((x, metadata) => { executionCount++; x.found++; }, new[] { "found" });

            asset.Invoke((prefab, variant) =>
            {
                Assert.That(prefab.doubleNestedToFind.toFind.toFind.found, Is.EqualTo(1));
                Assert.That(variant.doubleNestedToFind.toFind.toFind.found, Is.EqualTo(1));
                Assert.That(executionCount, Is.EqualTo(1));
            });
        }
    }
}
