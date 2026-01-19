using PereViader.ManualReserialization.Tests.TestClasses;
using NUnit.Framework;
using UnityEditor;

namespace PereViader.ManualReserialization.Tests
{

    [TestFixture]
    public class TestComponentReserializer
    {
        [Test]
        public void TestReserializeMonoBehaviourWithPublicToFindPrefab()
        {
            using DeleteAssetsTearDown deleteAssetsTearDown = new();
            var asset1 = deleteAssetsTearDown.CreatePrefabWithComponent<MonoBehaviourWithPublicToFind>("asset1");
            var asset2 = deleteAssetsTearDown.CreatePrefabWithComponent<MonoBehaviourWithPublicToFind>("asset2");

            ComponentReserializer.Reserialize<MonoBehaviourWithPublicToFind>((x, m) =>
            {
                x.toFind.newValue = x.toFind.previousValue + 1;
            });

            asset1.Invoke(x => Assert.That(x.toFind.newValue, Is.EqualTo(1)));
            asset2.Invoke(x => Assert.That(x.toFind.newValue, Is.EqualTo(1)));
        }

        [Test]
        public void TestReserializeMonoBehaviourWithDoubleNestedToFindPrefab()
        {
            using DeleteAssetsTearDown deleteAssetsTearDown = new();
            var asset1 = deleteAssetsTearDown.CreatePrefabWithComponent<MonoBehaviourWithDoubleNestedToFind>("asset1");
            var asset2 = deleteAssetsTearDown.CreatePrefabWithComponent<MonoBehaviourWithDoubleNestedToFind>("asset2");

            ComponentReserializer.Reserialize<MonoBehaviourWithDoubleNestedToFind>((x, m) =>
            {
                x.doubleNestedToFind.toFind.toFind.newValue = x.doubleNestedToFind.toFind.toFind.previousValue + 1;
            });

            asset1.Invoke(x => Assert.That(x.doubleNestedToFind.toFind.toFind.newValue, Is.EqualTo(1)));
            asset2.Invoke(x => Assert.That(x.doubleNestedToFind.toFind.toFind.newValue, Is.EqualTo(1)));
        }

        [Test]
        public void TestReserializeMonoBehaviourWithPublicToFindScene()
        {
            using DeleteAssetsTearDown deleteAssetsTearDown = new();
            var asset1 = deleteAssetsTearDown.CreateSceneWithGameObjectComponent<MonoBehaviourWithPublicToFind>("asset1");
            var asset2 = deleteAssetsTearDown.CreateSceneWithGameObjectComponent<MonoBehaviourWithPublicToFind>("asset2");

            ComponentReserializer.Reserialize<MonoBehaviourWithPublicToFind>((x, m) =>
            {
                x.toFind.newValue = x.toFind.previousValue + 1;
            });

            asset1.Invoke(x => Assert.That(x.toFind.newValue, Is.EqualTo(1)));
            asset2.Invoke(x => Assert.That(x.toFind.newValue, Is.EqualTo(1)));
        }

        [Test]
        public void TestReserializeMonoBehaviourWithDoubleNestedToFindScene()
        {
            using DeleteAssetsTearDown deleteAssetsTearDown = new();
            var asset1 = deleteAssetsTearDown.CreateSceneWithGameObjectComponent<MonoBehaviourWithDoubleNestedToFind>("asset1");
            var asset2 = deleteAssetsTearDown.CreateSceneWithGameObjectComponent<MonoBehaviourWithDoubleNestedToFind>("asset2");

            ComponentReserializer.Reserialize<MonoBehaviourWithDoubleNestedToFind>((x, m) =>
            {
                x.doubleNestedToFind.toFind.toFind.newValue = x.doubleNestedToFind.toFind.toFind.previousValue + 1;
            });

            asset1.Invoke(x => Assert.That(x.doubleNestedToFind.toFind.toFind.newValue, Is.EqualTo(1)));
            asset2.Invoke(x => Assert.That(x.doubleNestedToFind.toFind.toFind.newValue, Is.EqualTo(1)));
        }

        [Test]
        public void TestReserializeMonoBehaviourWithPublicToFindPrefabVariantUnchanged()
        {
            using DeleteAssetsTearDown deleteAssetsTearDown = new();
            var asset = deleteAssetsTearDown.CreatePrefabAndVariantWithComponent<MonoBehaviourWithPublicToFind>("asset1", x => { });

            ComponentReserializer.Reserialize<MonoBehaviourWithPublicToFind>((x, m) =>
            {
                x.toFind.newValue = x.toFind.previousValue + 1;
            });

            asset.Invoke((prefab, variant) =>
            {
                Assert.That(PrefabUtility.GetPropertyModifications(variant), Has.None.Matches<PropertyModification>(x => x.propertyPath.Contains("toFind.found")));
                Assert.That(prefab.toFind.newValue, Is.EqualTo(1));
                Assert.That(variant.toFind.newValue, Is.EqualTo(1));
            });
        }

        [Test]
        public void TestReserializeMonoBehaviourWithPublicToFindPrefabVariantChanged()
        {
            using DeleteAssetsTearDown deleteAssetsTearDown = new();
            var asset = deleteAssetsTearDown.CreatePrefabAndVariantWithComponent<MonoBehaviourWithPublicToFind>("asset", 
                x => x.toFind.previousValue = 5
                );

            ComponentReserializer.Reserialize<MonoBehaviourWithPublicToFind>((x, m) =>
            {
                x.toFind.newValue = x.toFind.previousValue + 1;
            });

            asset.Invoke((prefab, variant) =>
            {
                Assert.That(PrefabUtility.GetPropertyModifications(variant), Has.Exactly(1).Matches<PropertyModification>(x => x.propertyPath.Contains("toFind.newValue")));
                Assert.That(prefab.toFind.newValue, Is.EqualTo(1));
                Assert.That(variant.toFind.newValue, Is.EqualTo(6));
            });
        }
    }
}
