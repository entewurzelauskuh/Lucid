using System.Collections.Generic;
using Lucid.Core;
using Lucid.Runtime;
using UnityEngine;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// Generates a cube's walls, floor and ceiling from its connector mask.
    /// Boxes only: a doorway is an absence of wall segments rather than a hole
    /// cut in a mesh, which keeps the output deterministic and therefore
    /// re-buildable without churn.
    /// </summary>
    public static class ShellBuilder
    {
        /// <summary>Builds the shell under <paramref name="parent"/>.</summary>
        public static void Build(Transform parent, CubeSpec spec)
        {
            ShellSpec shell = spec.Shell;
            float t = CubeGeometry.Thickness(shell);
            float halfIn = CubeGeometry.InteriorHalf(shell);
            float height = CubeGeometry.InteriorHeight(shell);
            FaceMask mask = CubeSpecMapping.ToMask(spec.Connectors);

            foreach (Face face in new[] { Face.North, Face.East, Face.South, Face.West })
            {
                Wall(parent, face, halfIn, height, Faces.Has(mask, face), shell);
            }

            if (!shell.OpenFloor) Slab(parent, "floor", -t, 0f, Faces.Has(mask, Face.Down), shell.Materials.Floor);
            if (!shell.OpenCeiling) Slab(parent, "ceiling", height, CubeGeometry.Size,
                Faces.Has(mask, Face.Up), shell.Materials.Ceiling);
        }

        /// <summary>
        /// One wall, as up to three segments around the doorway. North and
        /// south run the interior width; east and west run the full footprint,
        /// so the four meet at the corners without overlapping.
        /// </summary>
        static void Wall(Transform parent, Face face, float halfIn, float height, bool doorway, ShellSpec shell)
        {
            bool alongX = face == Face.North || face == Face.South;
            float outer = face == Face.North || face == Face.East ? CubeGeometry.Half : -CubeGeometry.Half;
            float inner = face == Face.North || face == Face.East ? halfIn : -halfIn;
            float spanHalf = alongX ? halfIn : CubeGeometry.Half;

            string name = face.ToString().ToLowerInvariant();
            string material = shell.Materials.Wall;

            if (!doorway)
            {
                Box(parent, $"wall_{name}", Span(alongX, -spanHalf, spanHalf, inner, outer, 0f, height), material);
                return;
            }

            float doorHalf = CubeGeometry.DoorWidth / 2f;

            Box(parent, $"wall_{name}_a",
                Span(alongX, -spanHalf, -doorHalf, inner, outer, 0f, height), material);
            Box(parent, $"wall_{name}_b",
                Span(alongX, doorHalf, spanHalf, inner, outer, 0f, height), material);

            // The lintel. Absent when the room is no taller than the doorway.
            if (height > CubeGeometry.DoorHeight)
            {
                Box(parent, $"wall_{name}_lintel",
                    Span(alongX, -doorHalf, doorHalf, inner, outer, CubeGeometry.DoorHeight, height), material);
            }

            Frame(parent, name, alongX, inner, doorHalf, shell);
        }

        /// <summary>
        /// The trim standing proud of the opening. Its job is to read as
        /// architecture rather than as a hole punched in a wall, and to give a
        /// skin somewhere to put a different material from the wall itself.
        /// </summary>
        /// <remarks>
        /// The three non-`none` styles generate the same geometry in M0.3 and
        /// differ only by material role. They diverge when skins land (M1.10),
        /// which is the first point at which "arch" and "industrial" can look
        /// like anything.
        /// </remarks>
        static void Frame(Transform parent, string name, bool alongX, float inner,
                          float doorHalf, ShellSpec shell)
        {
            if (shell.DoorFrame == DoorFrameStyle.None) return;

            const float depth = 0.15f;
            const float width = 0.2f;
            float material0 = inner;
            float material1 = inner + (inner > 0 ? -depth : depth);

            string trim = shell.Materials.Trim ?? shell.Materials.Wall;
            float top = CubeGeometry.DoorHeight;

            Box(parent, $"frame_{name}_a",
                Span(alongX, -doorHalf - width, -doorHalf, material0, material1, 0f, top + width), trim);
            Box(parent, $"frame_{name}_b",
                Span(alongX, doorHalf, doorHalf + width, material0, material1, 0f, top + width), trim);
            Box(parent, $"frame_{name}_head",
                Span(alongX, -doorHalf, doorHalf, material0, material1, top, top + width), trim);
        }

        /// <summary>Floor or ceiling, as a ring of four boxes when it is pierced.</summary>
        static void Slab(Transform parent, string name, float bottom, float top, bool hole, string material)
        {
            float full = CubeGeometry.Half;
            if (!hole)
            {
                Box(parent, name, new Bounds3(-full, full, bottom, top, -full, full), material);
                return;
            }

            float h = CubeGeometry.VerticalHole / 2f;
            Box(parent, name + "_west", new Bounds3(-full, -h, bottom, top, -full, full), material);
            Box(parent, name + "_east", new Bounds3(h, full, bottom, top, -full, full), material);
            Box(parent, name + "_south", new Bounds3(-h, h, bottom, top, -full, -h), material);
            Box(parent, name + "_north", new Bounds3(-h, h, bottom, top, h, full), material);
        }

        /// <summary>A wall segment, expressed along whichever axis the wall runs.</summary>
        static Bounds3 Span(bool alongX, float from, float to, float inner, float outer, float low, float high)
        {
            float a = Mathf.Min(inner, outer), b = Mathf.Max(inner, outer);
            return alongX
                ? new Bounds3(from, to, low, high, a, b)
                : new Bounds3(a, b, low, high, from, to);
        }

        static void Box(Transform parent, string name, Bounds3 bounds, string material)
        {
            if (bounds.IsEmpty) return;

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = bounds.Centre;
            go.transform.localScale = bounds.Size;

            // The skin resolves the role later; the builder only records it.
            var role = go.AddComponent<MaterialRole>();
            role.Role = material;
        }

        /// <summary>An axis-aligned box in cube-local metres.</summary>
        readonly struct Bounds3
        {
            readonly float _x0, _x1, _y0, _y1, _z0, _z1;

            public Bounds3(float x0, float x1, float y0, float y1, float z0, float z1)
            {
                _x0 = Mathf.Min(x0, x1); _x1 = Mathf.Max(x0, x1);
                _y0 = Mathf.Min(y0, y1); _y1 = Mathf.Max(y0, y1);
                _z0 = Mathf.Min(z0, z1); _z1 = Mathf.Max(z0, z1);
            }

            public Vector3 Size => new Vector3(_x1 - _x0, _y1 - _y0, _z1 - _z0);

            public Vector3 Centre =>
                new Vector3((_x0 + _x1) / 2f, (_y0 + _y1) / 2f, (_z0 + _z1) / 2f);

            /// <summary>A zero-width segment is skipped rather than built flat.</summary>
            public bool IsEmpty
            {
                get
                {
                    Vector3 s = Size;
                    return s.x <= 1e-4f || s.y <= 1e-4f || s.z <= 1e-4f;
                }
            }
        }
    }
}
