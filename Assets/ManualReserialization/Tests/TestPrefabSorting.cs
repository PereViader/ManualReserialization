using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PereViader.ManualReserialization.Tests.TestClasses;
using UnityEditor;
using UnityEngine;

namespace PereViader.ManualReserialization.Tests
{
    [TestFixture]
    public class TestPrefabSorting
    {
        [Test]
        [Timeout(5000)] // timeout to prevent the test runner from hanging indefinitely
        public void GetAllPrefabsWithComponentSortedByVariant_HandlesVariantWhoseRootLacksComponent_DoesNotLoopInfinitely()
        {
            using DeleteAssetsTearDown deleteAssetsTearDown = new();
            deleteAssetsTearDown.CreateVariantWhereRootLacksComponent<MonoBehaviourWithPublicToFind>("InfiniteLoopTest");
            
            List<GameObject> sortedPrefabs = AssetDatabaseUtils.GetAllPrefabsWithComponentSortedByVariant(typeof(MonoBehaviourWithPublicToFind)).ToList();
            
            Assert.IsNotNull(sortedPrefabs, "Resulting list should not be null.");
            Assert.AreEqual(1, sortedPrefabs.Count, "Should only find the variant prefab.");
            Assert.IsTrue(sortedPrefabs[0].name.EndsWith("_Variant"), "The found prefab should be the variant.");

            Assert.IsTrue(PrefabUtility.IsPartOfVariantPrefab(sortedPrefabs[0]), "The found object should be a variant prefab.");
            Assert.IsNotNull(PrefabUtility.GetCorrespondingObjectFromSource(sortedPrefabs[0]), "Variant should have a source.");
            Assert.IsNull(PrefabUtility.GetCorrespondingObjectFromSource(sortedPrefabs[0]).GetComponent<MonoBehaviourWithPublicToFind>(), "Variant's source should not have the component.");
        }
    }
}