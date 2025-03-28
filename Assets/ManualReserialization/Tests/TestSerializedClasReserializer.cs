using PereViader.ManualReserialization.Tests.TestClasses;
using NUnit.Framework;
using UnityEditor;

namespace PereViader.ManualReserialization.Tests
{
    [TestFixture]
    public class TestSerializedClasReserializer
    {
        [Test]
        public void TestSerializedClassReserializerScriptableObject()
        {
            using DeleteAssetsTearDown deleteAssetsTearDown = new();
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
            using DeleteAssetsTearDown deleteAssetsTearDown = new();
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
            using DeleteAssetsTearDown deleteAssetsTearDown = new();
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
