using System;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Hutan.Tests
{
    public sealed class RitualStateTests
    {
        private GameObject _fixture;
        private Component _inventory;
        private static Type GameType(string name)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType("Hutan.Ritual." + name);
                if (type != null) return type;
            }
            throw new InvalidOperationException("Missing ritual type: " + name);
        }
        private static object Call(Component component, string method, params object[] arguments)
        {
            return component.GetType().GetMethod(method).Invoke(component, arguments);
        }
        private static object Read(Component component, string property)
        {
            return component.GetType().GetProperty(property).GetValue(component);
        }
        [SetUp] public void Setup()
        {
            _fixture = new GameObject("Ritual test fixture");
            _inventory = _fixture.AddComponent(GameType("IncenseInventory"));
            Call(_inventory, "ResetSession");
        }
        [TearDown] public void Cleanup() { if (_fixture != null) UnityEngine.Object.DestroyImmediate(_fixture); }

        [Test] public void InventoryCapacityIsExactlyOne()
        {
            Assert.That(Call(_inventory, "TryTakeIncense"), Is.EqualTo(true));
            Assert.That(Call(_inventory, "TryTakeIncense"), Is.EqualTo(false));
            Assert.That(Read(_inventory, "HasIncense"), Is.EqualTo(true));
            Assert.That(Call(_inventory, "TryPlaceIncense"), Is.EqualTo(true));
            Assert.That(Call(_inventory, "TryPlaceIncense"), Is.EqualTo(false));
        }
        [Test] public void FullHandsDoNotConsumePickup()
        {
            var pickup = _fixture.AddComponent(GameType("IncensePickup"));
            Call(_inventory, "TryTakeIncense");
            Call(pickup, "Interact", _inventory);
            Assert.That(Read(pickup, "Collected"), Is.EqualTo(false));
            Assert.That(_fixture.activeSelf, Is.True);
        }
        [Test] public void FilledStoneDoesNotConsumeSecondIncense()
        {
            var stone = _fixture.AddComponent(GameType("RitualStone"));
            Call(stone, "Initialize", new object[] { null });
            Call(_inventory, "TryTakeIncense");
            Call(stone, "Interact", _inventory);
            Assert.That(Read(stone, "Filled"), Is.EqualTo(true));
            Assert.That(Read(_inventory, "HasIncense"), Is.EqualTo(false));
            Call(_inventory, "TryTakeIncense");
            Call(stone, "Interact", _inventory);
            Assert.That(Read(_inventory, "HasIncense"), Is.EqualTo(true));
        }
        [Test] public void EmptyStoneDoesNotFillWithoutIncense()
        {
            var stone = _fixture.AddComponent(GameType("RitualStone"));
            Call(stone, "Initialize", new object[] { null });
            Call(stone, "Interact", _inventory);
            Assert.That(Read(stone, "Filled"), Is.EqualTo(false));
        }
        [Test] public void ResetClearsIncenseAndTirta()
        {
            Assert.That(Call(_inventory, "TryTakeTirta"), Is.EqualTo(true));
            Assert.That(Call(_inventory, "TryTakeTirta"), Is.EqualTo(false));
            Call(_inventory, "TryTakeIncense");
            Call(_inventory, "ResetSession");
            Assert.That(Read(_inventory, "HasTirta"), Is.EqualTo(false));
            Assert.That(Read(_inventory, "HasIncense"), Is.EqualTo(false));
        }
        private static Component SceneSpawner()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
                foreach (var root in SceneManager.GetSceneAt(i).GetRootGameObjects())
                {
                    var component = root.GetComponent(GameType("RitualSpawner"));
                    if (component != null) return component;
                }
            Assert.Ignore("Open Hutan.unity to run terrain layout tests.");
            return null;
        }
        [Test] public void HundredSeedsProduceFiveSeparatedLocationsAndSevenRingSlots()
        {
            var spawner = SceneSpawner();
            var centers = new System.Collections.Generic.HashSet<Vector3>();
            for (int seed = 0; seed < 100; seed++)
            {
                object[] args = { seed, null, 0f };
                Assert.That(Call(spawner, "TryCreateLayout", args), Is.EqualTo(true), "seed " + seed);
                var layout = (Vector3[])args[1]; var angle = (float)args[2];
                Assert.That(layout.Length, Is.EqualTo(5)); centers.Add(layout[0]);
                for (int i = 0; i < 5; i++)
                    for (int j = i + 1; j < 5; j++)
                        Assert.That(Vector2.Distance(new Vector2(layout[i].x, layout[i].z), new Vector2(layout[j].x, layout[j].z)), Is.GreaterThanOrEqualTo(7.99f));
                for (int slot = 0; slot < 7; slot++)
                {
                    var p = (Vector3)Call(spawner, "RingPoint", layout[0], angle, slot);
                    Assert.That(Vector2.Distance(new Vector2(p.x, p.z), new Vector2(layout[0].x, layout[0].z)), Is.EqualTo(3.2f).Within(.01f));
                }
            }
            Assert.That(centers.Count, Is.GreaterThan(1));
        }
        [Test] public void ValidFallbackWorksWhenRandomAttemptsAreZero()
        {
            var spawner = SceneSpawner(); var so = new SerializedObject(spawner);
            int original = so.FindProperty("_attempts").intValue;
            try
            {
                so.FindProperty("_attempts").intValue = 0; so.ApplyModifiedPropertiesWithoutUndo();
                object[] args = { 42, null, 0f };
                Assert.That(Call(spawner, "TryCreateLayout", args), Is.EqualTo(true));
                Assert.That(((Vector3[])args[1]).Length, Is.EqualTo(5));
            }
            finally { so.Update(); so.FindProperty("_attempts").intValue = original; so.ApplyModifiedPropertiesWithoutUndo(); }
        }
    }
}
