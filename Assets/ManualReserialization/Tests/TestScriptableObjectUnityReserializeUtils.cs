﻿using PereViader.ManualReserialization.Tests.TestClasses;
using NUnit.Framework;

namespace PereViader.ManualReserialization.Tests
{
    [TestFixture]
    public class TestScriptableObjectUnityReserializeUtils
    {
        [Test]
        public void TestReserializeScriptableObjectWithPublicToFind()
        {
            using DeleteAssetsTearDown deleteAssetsTearDown = new();
            var asset1 = deleteAssetsTearDown.CreateScriptableObject<ScriptableObjectWithPublicToFind>();
            var asset2 = deleteAssetsTearDown.CreateScriptableObject<ScriptableObjectWithPublicToFind>();

            ScriptableObjectReserializer.Reserialize<ScriptableObjectWithPublicToFind>((x, metadata) =>
            {
                x.toFind.newValue = x.toFind.previousValue + 1;
            });

            asset1.Invoke(x => Assert.That(x.toFind.newValue, Is.EqualTo(1)));
            asset2.Invoke(x => Assert.That(x.toFind.newValue, Is.EqualTo(1)));
        }

        [Test]
        public void TestReserializeScriptableObjectWithPublicNestedToFind()
        {
            using DeleteAssetsTearDown deleteAssetsTearDown = new();
            var asset1 = deleteAssetsTearDown.CreateScriptableObject<ScriptableObjectWithPublicNestedToFind>();
            var asset2 = deleteAssetsTearDown.CreateScriptableObject<ScriptableObjectWithPublicNestedToFind>();

            ScriptableObjectReserializer.Reserialize<ScriptableObjectWithPublicNestedToFind>((x, metadata) =>
            {
                x.nestedToFind.toFind.newValue = x.nestedToFind.toFind.previousValue + 1;
            });

            asset1.Invoke(x => Assert.That(x.nestedToFind.toFind.newValue, Is.EqualTo(1)));
            asset2.Invoke(x => Assert.That(x.nestedToFind.toFind.newValue, Is.EqualTo(1)));
        }

        [Test]
        public void TestReserializeScriptableObjectWithPublicSerializedDoubleNestedToFind()
        {
            using DeleteAssetsTearDown deleteAssetsTearDown = new();
            var asset1 = deleteAssetsTearDown.CreateScriptableObject<ScriptableObjectWithPublicSerializedDoubleNestedToFind>();
            var asset2 = deleteAssetsTearDown.CreateScriptableObject<ScriptableObjectWithPublicSerializedDoubleNestedToFind>();

            ScriptableObjectReserializer.Reserialize<ScriptableObjectWithPublicSerializedDoubleNestedToFind>((x, metadata) =>
            {
                x.doubleNestedToFind.toFind.toFind.newValue = x.doubleNestedToFind.toFind.toFind.previousValue + 1;
            });

            asset1.Invoke(x => Assert.That(x.doubleNestedToFind.toFind.toFind.newValue, Is.EqualTo(1)));
            asset2.Invoke(x => Assert.That(x.doubleNestedToFind.toFind.toFind.newValue, Is.EqualTo(1)));
        }
    }
}
