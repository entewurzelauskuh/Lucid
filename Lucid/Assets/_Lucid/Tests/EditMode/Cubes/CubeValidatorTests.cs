using System.Linq;
using Lucid.Core;
using Lucid.Editor.Cubes;
using Lucid.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Lucid.Tests.EditMode.Cubes
{
    /// <summary>
    /// The validator's four M0.3 rules (docs/WORKPLAN.md §4). Each test breaks
    /// one thing in an otherwise valid cube, so a failure names the rule rather
    /// than the fixture.
    /// </summary>
    public sealed class CubeValidatorTests
    {
        GameObject _cube;

        [TearDown]
        public void TearDown()
        {
            if (_cube != null) Object.DestroyImmediate(_cube);
        }

        static CubeSpec Spec(string json = null) =>
            CubeSpecReader.Read(json ?? SpecFixtures.Straight).Spec;

        /// <summary>A cube that passes every rule, for tests to break one of.</summary>
        GameObject Valid(CubeSpec spec)
        {
            _cube = new GameObject("straight");

            Transform shell = Child(_cube.transform, "Shell");
            ShellBuilder.Build(shell, spec);
            foreach (MeshFilter f in shell.GetComponentsInChildren<MeshFilter>())
            {
                if (f.GetComponent<Collider>() == null) f.gameObject.AddComponent<BoxCollider>();
            }

            Transform sockets = Child(_cube.transform, "Sockets");
            FaceMask mask = CubeSpecMapping.ToMask(spec.Connectors);
            foreach (Face face in Faces.All)
            {
                Transform socket = Child(sockets, face.ToString());
                socket.localPosition = CubeGeometry.Centre(face);

                Transform doorGo = Child(socket, "FogDoor");
                var door = doorGo.gameObject.AddComponent<FogDoor>();
                door.Configure(face);
                socket.gameObject.AddComponent<Connector>().Configure(face, Faces.Has(mask, face), door);
            }

            return _cube;
        }

        static Transform Child(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        static void AssertBlames(ValidationReport report, string rule)
        {
            Assert.That(report.Ok, Is.False, "expected the cube to be rejected");
            Assert.That(report.Problems.Select(p => p.Rule), Has.Some.EqualTo(rule),
                report.Describe());
        }

        [Test]
        public void AWellFormedCubePasses()
        {
            CubeSpec spec = Spec();
            ValidationReport report = CubeValidator.Validate(Valid(spec), spec, "Assets/scratch");

            Assert.That(report.Ok, Is.True, report.Describe());
            Assert.That(report.Triangles, Is.GreaterThan(0));
            Assert.That(report.Cube, Is.EqualTo("core.straight"));
        }

        [Test]
        public void GeometryOutsideTheCubeIsRejected()
        {
            CubeSpec spec = Spec();
            GameObject cube = Valid(spec);

            GameObject stray = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stray.name = "stray";
            stray.transform.SetParent(cube.transform.Find("Shell"), false);
            stray.transform.localPosition = new Vector3(6f, 1f, 0f);

            ValidationReport report = CubeValidator.Validate(cube, spec, "Assets/scratch");
            AssertBlames(report, "bounds");
            Assert.That(report.Problems[0].Message, Does.Contain("stray"));
        }

        [Test]
        public void TheFloorSlabBelowTheOriginIsNotOutOfBounds()
        {
            // The cube owns y in [-t, 8-t], not [0, 8]: the walkable surface is
            // the origin plane (docs/CUBE-SPEC.md §1). A naive bounds check
            // would fail every cube in the repository.
            CubeSpec spec = Spec();
            ValidationReport report = CubeValidator.Validate(Valid(spec), spec, "Assets/scratch");

            Assert.That(report.Problems.Select(p => p.Rule), Has.None.EqualTo("bounds"),
                report.Describe());
        }

        [Test]
        public void ASocketOffItsStandardPositionIsRejected()
        {
            CubeSpec spec = Spec();
            GameObject cube = Valid(spec);
            cube.transform.Find("Sockets/North").localPosition += new Vector3(0.5f, 0f, 0f);

            AssertBlames(CubeValidator.Validate(cube, spec, "Assets/scratch"), "connectors");
        }

        [Test]
        public void AConnectorWithoutAFogDoorIsRejected()
        {
            CubeSpec spec = Spec();
            GameObject cube = Valid(spec);
            Transform north = cube.transform.Find("Sockets/North");
            north.GetComponent<Connector>().Configure(Face.North, true, null);

            AssertBlames(CubeValidator.Validate(cube, spec, "Assets/scratch"), "connectors");
        }

        [Test]
        public void FewerThanTwoDoorwaysIsRejected()
        {
            // The rule that stops a placement ever sealing the dream
            // (docs/SPEC.md §7).
            CubeSpec spec = Spec();
            GameObject cube = Valid(spec);
            Transform south = cube.transform.Find("Sockets/South");
            var connector = south.GetComponent<Connector>();
            connector.Configure(Face.South, false, connector.Door);

            AssertBlames(CubeValidator.Validate(cube, spec, "Assets/scratch"), "connectors");
        }

        [Test]
        public void TheStartCubeIsAllowedItsSingleDoorway()
        {
            string json = SpecFixtures.Minimal
                .Replace("\"category\": \"connector\"", "\"category\": \"start\"")
                .Replace("\"cost\": 1", "\"cost\": 0")
                .Replace("[\"north\", \"south\"]", "[\"north\"]");
            CubeSpec spec = Spec(json);

            ValidationReport report = CubeValidator.Validate(Valid(spec), spec, "Assets/scratch");
            Assert.That(report.Problems.Select(p => p.Rule), Has.None.EqualTo("connectors"),
                report.Describe());
        }

        [Test]
        public void AShellPieceWithNoColliderIsRejected()
        {
            // A shell with meshes and no colliders looks right in a preview and
            // is walked straight through.
            CubeSpec spec = Spec();
            GameObject cube = Valid(spec);
            Object.DestroyImmediate(
                cube.transform.Find("Shell").GetComponentInChildren<Collider>());

            AssertBlames(CubeValidator.Validate(cube, spec, "Assets/scratch"), "collision");
        }

        [Test]
        public void AnEmptyShellIsRejected()
        {
            CubeSpec spec = Spec();
            var cube = new GameObject("empty");
            _cube = cube;
            Child(cube.transform, "Shell");
            Child(cube.transform, "Sockets");

            ValidationReport report = CubeValidator.Validate(cube, spec, "Assets/scratch");
            Assert.That(report.Problems.Select(p => p.Rule), Has.Some.EqualTo("collision"));
        }

        [Test]
        public void TheReportSerialisesToJson()
        {
            // The pipeline's caller reads this file and acts on it
            // (docs/SPEC.md §17), so its shape is part of the contract.
            CubeSpec spec = Spec();
            ValidationReport report = CubeValidator.Validate(Valid(spec), spec, "Assets/scratch");
            string json = report.ToJson();

            Assert.That(json, Does.Contain("\"cube\": \"core.straight\""));
            Assert.That(json, Does.Contain("\"ok\": true"));
            Assert.That(json, Does.Contain("\"problems\": []"));
            Assert.That(json, Does.Contain("\"triangles\""));
        }
    }
}
