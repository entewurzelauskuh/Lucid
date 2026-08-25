using System.Linq;
using Lucid.Editor.Cubes;
using NUnit.Framework;
using UnityEngine;

namespace Lucid.Tests.EditMode.Cubes
{
    /// <summary>
    /// Generated shell geometry. A doorway is the absence of wall segments, so
    /// what these check is mostly where the walls *stop*.
    /// </summary>
    public sealed class ShellBuilderTests
    {
        GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.DestroyImmediate(_root);
        }

        Transform Build(string json)
        {
            CubeSpecResult read = CubeSpecReader.Read(json);
            Assert.That(read.Ok, Is.True, read.Describe());

            _root = new GameObject("shell");
            ShellBuilder.Build(_root.transform, read.Spec);
            return _root.transform;
        }

        static Bounds WorldBounds(Transform t)
        {
            Renderer[] rs = t.GetComponentsInChildren<Renderer>();
            var b = new Bounds(rs[0].bounds.center, Vector3.zero);
            foreach (Renderer r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        /// <summary>Whether any shell piece occupies a point.</summary>
        static bool Solid(Transform shell, Vector3 point)
        {
            foreach (Renderer r in shell.GetComponentsInChildren<Renderer>())
            {
                if (r.bounds.Contains(point)) return true;
            }
            return false;
        }

        [Test]
        public void ADoorwayIsOpenAndTheWallBesideItIsNot()
        {
            Transform shell = Build(SpecFixtures.Straight);

            // The doorway is 2.5 m wide and 3 m high, centred on the face.
            Assert.That(Solid(shell, new Vector3(0f, 1.5f, 3.0f)), Is.False, "middle of the north doorway");
            Assert.That(Solid(shell, new Vector3(1.2f, 1.5f, 3.0f)), Is.False, "just inside its edge");
            Assert.That(Solid(shell, new Vector3(1.4f, 1.5f, 3.0f)), Is.True, "just outside its edge");
            Assert.That(Solid(shell, new Vector3(0f, 3.5f, 3.0f)), Is.True, "above the lintel");
        }

        [Test]
        public void AWalledFaceIsSolidAllTheWayAcross()
        {
            Transform shell = Build(SpecFixtures.Straight);

            // A straight runs north to south, so east and west are walls.
            Assert.That(Solid(shell, new Vector3(3.0f, 1.5f, 0f)), Is.True, "middle of the east wall");
            Assert.That(Solid(shell, new Vector3(-3.0f, 1.5f, 0f)), Is.True, "middle of the west wall");
        }

        [Test]
        public void TheFloorHangsBelowTheOriginSoTheDoorwayIsNotStepped()
        {
            // docs/DECISIONS.md: the walkable surface is the origin plane, and
            // a doorway occupies y in [0, 3] exactly as §1 states.
            Transform shell = Build(SpecFixtures.Straight);

            Assert.That(Solid(shell, new Vector3(0f, -0.15f, 0f)), Is.True, "floor slab is below y=0");
            Assert.That(Solid(shell, new Vector3(0f, 0.1f, 0f)), Is.False, "nothing standing on the floor");
        }

        [Test]
        public void TheShellStaysInsideTheCube()
        {
            Bounds b = WorldBounds(Build(SpecFixtures.Straight));

            Assert.That(b.min.x, Is.GreaterThanOrEqualTo(-4f - 1e-3f));
            Assert.That(b.max.x, Is.LessThanOrEqualTo(4f + 1e-3f));
            Assert.That(b.min.z, Is.GreaterThanOrEqualTo(-4f - 1e-3f));
            Assert.That(b.max.z, Is.LessThanOrEqualTo(4f + 1e-3f));
            Assert.That(b.max.y, Is.LessThanOrEqualTo(8f + 1e-3f));
            Assert.That(b.min.y, Is.GreaterThanOrEqualTo(-CubeGeometry.DefaultThickness - 1e-3f),
                "only the floor slab may sit below the origin");
        }

        [Test]
        public void EveryPieceCarriesTheRoleTheSpecAskedFor()
        {
            Transform shell = Build(SpecFixtures.Straight);
            var roles = shell.GetComponentsInChildren<Lucid.Runtime.MaterialRole>();

            Assert.That(roles, Is.Not.Empty);
            Assert.That(roles.All(r => !string.IsNullOrEmpty(r.Role)), Is.True);
            Assert.That(roles.Any(r => r.Role == "wall"), Is.True);
            Assert.That(roles.Any(r => r.Role == "floor"), Is.True);
            Assert.That(roles.Any(r => r.Role == "trim"), Is.True, "the door frame uses the trim role");
        }

        [Test]
        public void ADoorFrameIsGeneratedPerDoorwayAndCanBeTurnedOff()
        {
            Transform framed = Build(SpecFixtures.Straight);
            int frames = framed.GetComponentsInChildren<Transform>()
                .Count(t => t.name.StartsWith("frame_"));
            Assert.That(frames, Is.EqualTo(6), "three pieces for each of two doorways");

            Object.DestroyImmediate(_root);
            _root = null;

            Transform bare = Build(SpecFixtures.Straight.Replace(
                @"""doorFrame"": ""plain""", @"""doorFrame"": ""none"""));
            Assert.That(bare.GetComponentsInChildren<Transform>()
                .Any(t => t.name.StartsWith("frame_")), Is.False);
        }

        [Test]
        public void AVerticalConnectorPiercesTheSlabItPassesThrough()
        {
            string drop = SpecFixtures.Minimal
                .Replace(@"[""north"", ""south""]", @"[""north"", ""down""]");
            Transform shell = Build(drop);

            Assert.That(Solid(shell, new Vector3(0f, -0.15f, 0f)), Is.False,
                "the hole is open at the centre of the floor");
            Assert.That(Solid(shell, new Vector3(3.5f, -0.15f, 0f)), Is.True,
                "and the slab survives outside it");
        }

        [Test]
        public void AnOpenFloorGeneratesNoSlabAtAll()
        {
            string pit = SpecFixtures.Replacing(
                @"""ceiling"": ""ceiling"" } }",
                @"""ceiling"": ""ceiling"" }, ""openFloor"": true }");
            Transform shell = Build(pit);

            Assert.That(shell.GetComponentsInChildren<Transform>()
                .Any(t => t.name.StartsWith("floor")), Is.False);
        }
    }
}
