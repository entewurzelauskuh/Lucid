using System.IO;
using Lucid.Core;
using Lucid.Runtime;
using UnityEditor;
using UnityEngine;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// Generates `CubeTemplate.prefab`. Every cube is built from this, so the
    /// six sockets sit at the standard positions once, here, rather than being
    /// re-derived per cube.
    /// </summary>
    /// <remarks>
    /// The template is generated rather than authored because prefab YAML is
    /// never hand-edited (CLAUDE.md rule 4). Rebuilding it after a change is
    /// one menu item, and then one `tools/build-cube.sh &lt;pack&gt;` to bring
    /// every cube back into line.
    /// </remarks>
    public static class CubeTemplateBuilder
    {
        public const string TemplatePath = "Assets/_Lucid/Templates/CubeTemplate.prefab";

        [MenuItem("Lucid/Rebuild Cube Template")]
        public static void Rebuild()
        {
            string path = Build();
            Debug.Log($"Lucid: rebuilt {path}");
        }

        /// <summary>
        /// Whether the committed template still matches what this code would
        /// generate. Without this, changing a socket position and rebuilding a
        /// pack would quietly produce every cube from the stale template.
        /// </summary>
        public static bool IsStale()
        {
            var onDisk = AssetDatabase.LoadAssetAtPath<GameObject>(TemplatePath);
            if (onDisk == null) return true;

            var fresh = new GameObject("CubeTemplate");
            try
            {
                Populate(fresh.transform);
                return !CubeEquivalence.Matches(fresh, onDisk);
            }
            finally
            {
                Object.DestroyImmediate(fresh);
            }
        }

        /// <summary>Writes the template and returns its asset path.</summary>
        public static string Build()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(TemplatePath));

            var root = new GameObject("CubeTemplate");
            try
            {
                Populate(root.transform);
                PrefabUtility.SaveAsPrefabAsset(root, TemplatePath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }

            AssetDatabase.ImportAsset(TemplatePath);
            return TemplatePath;
        }

        /// <summary>Everything the template is, in one place, so IsStale can rebuild it.</summary>
        static void Populate(Transform root)
        {
            {
                // The bounds the cube must stay inside, carried on the root so
                // the validator and anyone opening the prefab can see it
                // (#47, docs/SPEC.md §17). It occupies the 8 m the cube owns in
                // the lattice: the floor slab hangs below the origin and the
                // ceiling stops the same distance short of the top.
                var bounds = root.gameObject.AddComponent<CubeBounds>();
                bounds.SetExtent(CubeGeometry.Size, CubeGeometry.DefaultThickness);

                NewChild(root, "Shell");
                NewChild(root, "Interior");
                NewChild(root, "Logic");

                Transform nav = NewChild(root, "Nav");
                nav.gameObject.AddComponent<Unity.AI.Navigation.NavMeshSurface>();

                Transform sockets = NewChild(root, "Sockets");
                foreach (Face face in Faces.All) Socket(sockets, face);
            }
        }

        /// <summary>
        /// One socket per face, doorway or not. A walled face still carries its
        /// FogDoor so that a cube type's connector mask is the only thing
        /// deciding what is passable.
        /// </summary>
        static void Socket(Transform parent, Face face)
        {
            Transform socket = NewChild(parent, face.ToString());
            socket.localPosition = CubeGeometry.Centre(face);
            socket.localRotation = Quaternion.LookRotation(CubeGeometry.Outward(face), Vector3.up);

            Transform doorTransform = NewChild(socket, "FogDoor");
            var door = doorTransform.gameObject.AddComponent<FogDoor>();
            door.Configure(face);

            var connector = socket.gameObject.AddComponent<Connector>();
            connector.Configure(face, false, door);
        }

        static Transform NewChild(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }
    }
}
