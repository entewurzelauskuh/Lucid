using Lucid.Core;
using Lucid.Editor.Cubes;
using Lucid.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Lucid.Tests.EditMode.Cubes
{
    /// <summary>
    /// The comparison the builder uses to decide whether to write. It has to be
    /// insensitive to what Unity randomises and sensitive to everything the
    /// builder sets, or the builder either churns or goes blind.
    /// </summary>
    public sealed class CubeEquivalenceTests
    {
        GameObject _a, _b;

        [TearDown]
        public void TearDown()
        {
            if (_a != null) Object.DestroyImmediate(_a);
            if (_b != null) Object.DestroyImmediate(_b);
        }

        static GameObject Cube(string role = "wall", Vector3? scale = null, string childName = "piece")
        {
            var root = new GameObject("cube");
            var child = GameObject.CreatePrimitive(PrimitiveType.Cube);
            child.name = childName;
            child.transform.SetParent(root.transform, false);
            child.transform.localScale = scale ?? Vector3.one;
            child.AddComponent<MaterialRole>().Role = role;
            return root;
        }

        [Test]
        public void TwoIdenticallyBuiltCubesMatch()
        {
            _a = Cube();
            _b = Cube();
            Assert.That(CubeEquivalence.Matches(_a, _b), Is.True);
        }

        [Test]
        public void ChildOrderDoesNotMatter()
        {
            // The builder's emission order is not part of the cube, and Unity
            // reorders on load anyway.
            _a = new GameObject("cube");
            _b = new GameObject("cube");
            foreach (string n in new[] { "alpha", "beta" }) Child(_a, n);
            foreach (string n in new[] { "beta", "alpha" }) Child(_b, n);

            Assert.That(CubeEquivalence.Matches(_a, _b), Is.True);
        }

        [Test]
        public void AMissingCubeIsNeverAMatch()
        {
            _a = Cube();
            Assert.That(CubeEquivalence.Matches(_a, null), Is.False);
        }

        [Test]
        public void GeometryDifferencesAreNoticed()
        {
            _a = Cube();
            _b = Cube(scale: new Vector3(2f, 1f, 1f));
            Assert.That(CubeEquivalence.Matches(_a, _b), Is.False);
        }

        [Test]
        public void MaterialRolesAreNoticed()
        {
            _a = Cube(role: "wall");
            _b = Cube(role: "metal");
            Assert.That(CubeEquivalence.Matches(_a, _b), Is.False);
        }

        [Test]
        public void RenamedPiecesAreNoticed()
        {
            _a = Cube(childName: "wall_north");
            _b = Cube(childName: "wall_south");
            Assert.That(CubeEquivalence.Matches(_a, _b), Is.False);
        }

        [Test]
        public void ConnectorAndDoorStateAreNoticed()
        {
            _a = new GameObject("cube");
            _b = new GameObject("cube");

            Connector ca = Socket(_a, Face.North, doorway: true);
            Connector cb = Socket(_b, Face.North, doorway: false);

            Assert.That(CubeEquivalence.Matches(_a, _b), Is.False, "a doorway is not a wall");

            cb.Configure(Face.North, true, cb.Door);
            Assert.That(CubeEquivalence.Matches(_a, _b), Is.True);

            cb.Door.SetState(ConnectorState.Solid);
            Assert.That(CubeEquivalence.Matches(_a, _b), Is.False, "door state counts too");
        }

        [Test]
        public void AnExtraChildIsNoticed()
        {
            _a = Cube();
            _b = Cube();
            Child(_b, "extra");
            Assert.That(CubeEquivalence.Matches(_a, _b), Is.False);
        }

        static void Child(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
        }

        static Connector Socket(GameObject parent, Face face, bool doorway)
        {
            var socket = new GameObject(face.ToString());
            socket.transform.SetParent(parent.transform, false);

            var doorGo = new GameObject("FogDoor");
            doorGo.transform.SetParent(socket.transform, false);
            var door = doorGo.AddComponent<FogDoor>();

            var connector = socket.AddComponent<Connector>();
            connector.Configure(face, doorway, door);
            return connector;
        }
    }
}
