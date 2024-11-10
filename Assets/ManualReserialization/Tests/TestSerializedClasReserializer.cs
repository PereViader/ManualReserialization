using PereViader.ManualReserialization.Tests.TestClasses;
using NUnit.Framework;

namespace PereViader.ManualReserialization.Tests
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

            SerializedClassReserializer.Reserialize<ToFind>((x, metadata) =>
            {
                x.newValue = x.previousValue + 1;
            });

            scriptableObject.Invoke(x =>
            {
                Assert.That(x.doubleNestedToFind.toFind.toFind.newValue, Is.EqualTo(1));
            });
        }

        [Test]
        public void TestSerializedClassReserializerPrefabChanged()
        {
            var asset = deleteAssetsTearDown.CreatePrefabAndVariantWithComponent<MonoBehaviourWithDoubleNestedToFind>(
                "asset",
                x => x.doubleNestedToFind.toFind.toFind.previousValue = 5);

            SerializedClassReserializer.Reserialize<ToFind>((x, metadata) =>
            {
                x.newValue = x.previousValue + 1;
            });

            asset.Invoke((prefab, variant) =>
            {
                Assert.That(prefab.doubleNestedToFind.toFind.toFind.newValue, Is.EqualTo(1));
                Assert.That(variant.doubleNestedToFind.toFind.toFind.newValue, Is.EqualTo(6));
            });
        }

        [Test]
        public void TestSerializedClassReserializerPrefabUnchanged()
        {
            var asset = deleteAssetsTearDown.CreatePrefabAndVariantWithComponent<MonoBehaviourWithDoubleNestedToFind>(
                "asset",
                x => { });

            SerializedClassReserializer.Reserialize<ToFind>((x, metadata) =>
            {
                x.newValue = x.previousValue + 1;
            });

            asset.Invoke((prefab, variant) =>
            {
                Assert.That(prefab.doubleNestedToFind.toFind.toFind.newValue, Is.EqualTo(1));
                Assert.That(variant.doubleNestedToFind.toFind.toFind.newValue, Is.EqualTo(1));
            });
        }
    }
}
