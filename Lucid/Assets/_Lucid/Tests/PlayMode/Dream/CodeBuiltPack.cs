using System.Collections.Generic;
using Lucid.Core;
using Lucid.Runtime;
using UnityEngine;

namespace Lucid.Tests.PlayMode.Dream
{
    /// <summary>
    /// A DreamPack built in code, shaped like the ones <c>CubeTemplateBuilder</c>
    /// makes, for tests that need cubes standing without loading the real
    /// pack — the dream's own tests, and the netcode's two ends.
    /// </summary>
    public static class CodeBuiltPack
    {
        public static DreamPack Create(List<UnityEngine.Object> assets, List<GameObject> spawned,
            params (string id, FaceMask doors, CubeCategory category, bool climbable)[] cubes)
        {
            var pack = ScriptableObject.CreateInstance<DreamPack>();
            pack.Configure("test");
            foreach ((string id, FaceMask doors, CubeCategory category, bool climbable) c in cubes)
                pack.AddOrReplace(Definition(assets, spawned, c.id, c.doors, c.category, c.climbable));
            assets.Add(pack);
            return pack;
        }

        /// <summary>
        /// A cube shaped like the ones <c>CubeTemplateBuilder</c> makes: a
        /// socket on every face carrying a FogDoor, walled faces included, and
        /// a floor to stand on.
        /// </summary>
        public static GameObject CubePrefab(List<GameObject> spawned, string id, FaceMask doorways)
        {
            var body = new GameObject($"prefab {id}");

            // Parked far below, so the source object's own colliders never take
            // part in a test. Instantiate is given an explicit pose, so where
            // the source sits does not reach the clone.
            body.transform.position = new Vector3(0f, -10000f, 0f);

            // The template's own nodes (docs/SPEC.md §17). Interior is here so
            // that the tests take the same path a real cube does — the entry
            // volume must not adopt it.
            foreach (string node in new[] { "Shell", "Interior", "Logic" })
            {
                var child = new GameObject(node);
                child.transform.SetParent(body.transform, false);
            }

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "floor";
            floor.transform.SetParent(body.transform, false);
            floor.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(CubeMetrics.Size, 1f, CubeMetrics.Size);

            foreach (Face face in Faces.All)
            {
                var socket = new GameObject(face.ToString());
                socket.transform.SetParent(body.transform, false);
                socket.transform.localPosition = SocketCentre(face);
                socket.transform.localRotation =
                    Quaternion.LookRotation(DreamSpace.Direction(face), Vector3.up);

                var doorObject = new GameObject("FogDoor");
                doorObject.transform.SetParent(socket.transform, false);
                var door = doorObject.AddComponent<FogDoor>();
                door.Configure(face);

                socket.AddComponent<Connector>()
                    .Configure(face, Faces.Has(doorways, face), door);
            }

            spawned.Add(body);
            return body;
        }

        /// <summary>Where a socket sits, mirroring <c>CubeGeometry.Centre</c>.</summary>
        static Vector3 SocketCentre(Face face)
        {
            switch (face)
            {
                case Face.North: return new Vector3(0f, 0f, CubeMetrics.Half);
                case Face.East: return new Vector3(CubeMetrics.Half, 0f, 0f);
                case Face.South: return new Vector3(0f, 0f, -CubeMetrics.Half);
                case Face.West: return new Vector3(-CubeMetrics.Half, 0f, 0f);
                case Face.Up: return new Vector3(0f, CubeMetrics.Size, 0f);
                default: return Vector3.zero;
            }
        }

        public static CubeDefinition Definition(List<UnityEngine.Object> assets, List<GameObject> spawned,
            string id, FaceMask doorways, CubeCategory category, bool climbable = false)
        {
            var d = ScriptableObject.CreateInstance<CubeDefinition>();
            d.Configure(id, "test", $"display name of {id}", category, doorways,
                climbable, 1, CubePrefab(spawned, id, doorways), new[] { "*" });
            assets.Add(d);
            return d;
        }

    }
}
