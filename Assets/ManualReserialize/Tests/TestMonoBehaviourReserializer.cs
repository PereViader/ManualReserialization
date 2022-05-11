using ManualReserialization.Tests.TestClasses;
using NUnit.Framework;
using UnityEditor;

namespace ManualReserialization.Tests
{

    [TestFixture]
    public class TestMonoBehaviourReserializer
    {
        private readonly DeleteAssetsTearDown deleteAssetsTearDown = new DeleteAssetsTearDown();

        [TearDown]
        public void TearDown()
        {
            deleteAssetsTearDown.TearDown();
        }

        [Test]
        public void TestReserializeMonoBehaviourWithPublicToFindPrefab()
        {
            var asset1 = deleteAssetsTearDown.CreatePrefabWithComponent<MonoBehaviourWithPublicToFind>("asset1");
            var asset2 = deleteAssetsTearDown.CreatePrefabWithComponent<MonoBehaviourWithPublicToFind>("asset2");

            MonoBehaviourReserializer.Reserialize<MonoBehaviourWithPublicToFind>(x => x.toFind.found++, new[] { "toFind.found" });

            asset1.Invoke(x => Assert.That(x.toFind.found, Is.EqualTo(1)));
            asset2.Invoke(x => Assert.That(x.toFind.found, Is.EqualTo(1)));
        }

        [Test]
        public void TestReserializeMonoBehaviourWithDoubleNestedToFindPrefab()
        {
            var asset1 = deleteAssetsTearDown.CreatePrefabWithComponent<MonoBehaviourWithDoubleNestedToFind>("asset1");
            var asset2 = deleteAssetsTearDown.CreatePrefabWithComponent<MonoBehaviourWithDoubleNestedToFind>("asset2");

            MonoBehaviourReserializer.Reserialize<MonoBehaviourWithDoubleNestedToFind>(x => x.doubleNestedToFind.toFind.toFind.found++, new[] { "doubleNestedToFind.toFind.toFind.found" });

            asset1.Invoke(x => Assert.That(x.doubleNestedToFind.toFind.toFind.found, Is.EqualTo(1)));
            asset2.Invoke(x => Assert.That(x.doubleNestedToFind.toFind.toFind.found, Is.EqualTo(1)));
        }

        [Test]
        public void TestReserializeMonoBehaviourWithPublicToFindScene()
        {
            var asset1 = deleteAssetsTearDown.CreateSceneWithGameObjectComponent<MonoBehaviourWithPublicToFind>("asset1");
            var asset2 = deleteAssetsTearDown.CreateSceneWithGameObjectComponent<MonoBehaviourWithPublicToFind>("asset2");

            MonoBehaviourReserializer.Reserialize<MonoBehaviourWithPublicToFind>(x => x.toFind.found++, new[] { "toFind.found" });

            asset1.Invoke(x => Assert.That(x.toFind.found, Is.EqualTo(1)));
            asset2.Invoke(x => Assert.That(x.toFind.found, Is.EqualTo(1)));
        }

        [Test]
        public void TestReserializeMonoBehaviourWithDoubleNestedToFindScene()
        {
            var asset1 = deleteAssetsTearDown.CreateSceneWithGameObjectComponent<MonoBehaviourWithDoubleNestedToFind>("asset1");
            var asset2 = deleteAssetsTearDown.CreateSceneWithGameObjectComponent<MonoBehaviourWithDoubleNestedToFind>("asset2");

            MonoBehaviourReserializer.Reserialize<MonoBehaviourWithDoubleNestedToFind>(x => x.doubleNestedToFind.toFind.toFind.found++, new[] { "doubleNestedToFind.toFind.toFind.found" });

            asset1.Invoke(x => Assert.That(x.doubleNestedToFind.toFind.toFind.found, Is.EqualTo(1)));
            asset2.Invoke(x => Assert.That(x.doubleNestedToFind.toFind.toFind.found, Is.EqualTo(1)));
        }

        [Test]
        public void TestReserializeMonoBehaviourWithPublicToFindPrefabVariantUnchanged()
        {
            var asset = deleteAssetsTearDown.CreatePrefabAndVariantWithComponent<MonoBehaviourWithPublicToFind>("asset1", x => { });
            var executionCount = 0;

            MonoBehaviourReserializer.Reserialize<MonoBehaviourWithPublicToFind>(
                x => { executionCount++; x.toFind.found++; },
                new[] { "toFind.found" });

            asset.Invoke((prefab, variant) =>
            {
                Assert.That(PrefabUtility.GetPropertyModifications(variant), Has.None.Matches<PropertyModification>(x => x.propertyPath.Contains("toFind.found")));
                Assert.That(prefab.toFind.found, Is.EqualTo(1));
                Assert.That(variant.toFind.found, Is.EqualTo(1));
                Assert.That(executionCount, Is.EqualTo(1));
            });
        }

        [Test]
        public void TestReserializeMonoBehaviourWithPublicToFindPrefabVariantChanged()
        {
            var asset = deleteAssetsTearDown.CreatePrefabAndVariantWithComponent<MonoBehaviourWithPublicToFind>("asset", x => x.toFind.found = 5);
            var executionCount = 0;

            MonoBehaviourReserializer.Reserialize<MonoBehaviourWithPublicToFind>(x =>
                {
                    executionCount++; x.toFind.found++;
                },
                new[] { "toFind.found" });

            asset.Invoke((prefab, variant) =>
            {
                Assert.That(PrefabUtility.GetPropertyModifications(variant), Has.Exactly(1).Matches<PropertyModification>(x => x.propertyPath.Contains("toFind.found")));
                Assert.That(prefab.toFind.found, Is.EqualTo(1));
                Assert.That(variant.toFind.found, Is.EqualTo(6));
                Assert.That(executionCount, Is.EqualTo(2));
            });
        }
    }
}
