using System.Collections.Generic;
using System.Reflection;
using Lucid.Core;
using Lucid.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Lucid.Tests.EditMode.Cubes
{
    /// <summary>
    /// The generated content types. `DreamPack.AddOrReplace` is what #47's
    /// "rebuilding changes nothing on disk" rests on, so its behaviour is
    /// pinned here rather than discovered there.
    /// </summary>
    public sealed class RuntimeCubeTypesTests
    {
        static CubeDefinition Cube(string id, FaceMask connectors = FaceMask.North | FaceMask.South,
                                   CubeCategory category = CubeCategory.Connector, int cost = 1)
        {
            var def = ScriptableObject.CreateInstance<CubeDefinition>();
            def.Configure(id, "core", id, category, connectors, false, cost, null, new[] { "*" });
            return def;
        }

        [Test]
        public void ACubeDefinitionBecomesTheRulesEnginesView()
        {
            CubeDefinition def = Cube("core.straight");
            CubeType type = def.ToCubeType();

            Assert.That(type.Id, Is.EqualTo("core.straight"));
            Assert.That(type.Connectors, Is.EqualTo(FaceMask.North | FaceMask.South));
            Assert.That(type.Category, Is.EqualTo(CubeCategory.Connector));
            Assert.That(type.Cost, Is.EqualTo(1));
        }

        [Test]
        public void APackRegistersEveryCubeWithTheRules()
        {
            var pack = ScriptableObject.CreateInstance<DreamPack>();
            pack.Configure("core");
            pack.AddOrReplace(Cube("core.straight"));
            pack.AddOrReplace(Cube("core.corner", FaceMask.North | FaceMask.East));

            var registry = new CubeRegistry();
            pack.RegisterAll(registry);

            Assert.That(registry.All.Count, Is.EqualTo(2));
            Assert.That(registry.Contains("core.straight"), Is.True);
            Assert.That(registry.Contains("core.corner"), Is.True);
        }

        [Test]
        public void ThePackStaysSortedByIdWhateverOrderCubesArriveIn()
        {
            // Registration order is the wire's type index order
            // (docs/NETCODE.md §4, §5), so it cannot depend on the order the
            // builder happened to walk the folder in.
            var pack = ScriptableObject.CreateInstance<DreamPack>();
            pack.AddOrReplace(Cube("core.tee"));
            pack.AddOrReplace(Cube("core.corner"));
            pack.AddOrReplace(Cube("core.straight"));

            Assert.That(new[] { pack.Cubes[0].Id, pack.Cubes[1].Id, pack.Cubes[2].Id },
                Is.EqualTo(new[] { "core.corner", "core.straight", "core.tee" }));
        }

        [Test]
        public void ReplacingACubeKeepsThePackTheSameSize()
        {
            var pack = ScriptableObject.CreateInstance<DreamPack>();
            pack.AddOrReplace(Cube("core.straight"));

            Assert.That(pack.AddOrReplace(Cube("core.straight", cost: 2)), Is.True,
                "a different asset for the same id is a change");
            Assert.That(pack.Cubes.Count, Is.EqualTo(1), "replaced, not appended");
            Assert.That(pack.Cubes[0].Cost, Is.EqualTo(2));
        }

        [Test]
        public void AddingTheSameAssetTwiceReportsNoChange()
        {
            // This is what makes rebuilding a pack idempotent (#47).
            var pack = ScriptableObject.CreateInstance<DreamPack>();
            CubeDefinition straight = Cube("core.straight");

            Assert.That(pack.AddOrReplace(straight), Is.True);
            Assert.That(pack.AddOrReplace(straight), Is.False, "nothing about the asset changed");
        }

        [Test]
        public void ReorderingOrDroppingANullCountsAsAChange()
        {
            // The return value gates saving the asset, so a re-sort that reports
            // "nothing happened" loses the order — and the order is the wire's
            // type index order. AddOrReplace always sorts, so the only way to
            // reach an out-of-order list is the one that produces it in
            // practice: a hand-edited or older asset deserialised from disk.
            var pack = ScriptableObject.CreateInstance<DreamPack>();
            CubeDefinition tee = Cube("core.tee");
            CubeDefinition corner = Cube("core.corner");

            SetCubes(pack, tee, null, corner);   // unsorted, and carrying a null

            Assert.That(pack.AddOrReplace(tee), Is.True,
                "the null was dropped and the order restored, so the asset changed");
            Assert.That(pack.Cubes.Count, Is.EqualTo(2));
            Assert.That(pack.Cubes[0].Id, Is.EqualTo("core.corner"));

            Assert.That(pack.AddOrReplace(tee), Is.False, "now it really is unchanged");
        }

        /// <summary>
        /// Writes the serialised list directly. Nothing in the public surface
        /// can produce an unsorted pack, but deserialising one can.
        /// </summary>
        static void SetCubes(DreamPack pack, params CubeDefinition[] cubes)
        {
            var field = typeof(DreamPack).GetField(
                "_cubes", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "DreamPack._cubes was renamed; update this test");
            field.SetValue(pack, new List<CubeDefinition>(cubes));
        }

        [Test]
        public void ANullCubeIsIgnored()
        {
            var pack = ScriptableObject.CreateInstance<DreamPack>();
            Assert.That(pack.AddOrReplace(null), Is.False);
            Assert.That(pack.Cubes, Is.Empty);
        }

        [Test]
        public void AFogDoorReportsWhatIsPassable()
        {
            // docs/SPEC.md §7's table: Attached and Exit are passable, Fog and
            // Solid are not, and only Exit wakes a Sleeper.
            // A door each, because these are properties of a state rather than
            // of a history. Walking one door through all four would assert a
            // sequence §7 forbids — Solid and Attached are both terminal — and
            // that is now refused rather than quietly recorded.
            var go = new GameObject("door");
            try
            {
                FogDoor Door(ConnectorState state)
                {
                    var d = new GameObject(state.ToString()).AddComponent<FogDoor>();
                    d.transform.SetParent(go.transform, false);
                    d.Initialise(state);
                    return d;
                }

                Assert.That(Door(ConnectorState.Fog).IsPassable, Is.False);
                Assert.That(Door(ConnectorState.Fog).IsExit, Is.False);

                Assert.That(Door(ConnectorState.Solid).IsPassable, Is.False);
                Assert.That(Door(ConnectorState.Solid).IsExit, Is.False);

                Assert.That(Door(ConnectorState.Attached).IsPassable, Is.True);
                Assert.That(Door(ConnectorState.Attached).IsExit, Is.False);

                Assert.That(Door(ConnectorState.Exit).IsPassable, Is.True);
                Assert.That(Door(ConnectorState.Exit).IsExit, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
