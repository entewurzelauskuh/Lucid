using Lucid.Core;
using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// The one place the lattice's axes meet Unity's (CLAUDE.md, Conventions).
    /// </summary>
    /// <remarks>
    /// Core counts in cubes on a right-handed grid where z is the layer;
    /// Unity measures metres and calls up y. So the mapping is a swap and a
    /// scale, <c>(X·8, Z·8, Y·8)</c>, and it lives in Runtime alone — Core may
    /// not reference UnityEngine (CLAUDE.md rule 3), and nothing else needs to
    /// know which of the two frames it is holding.
    /// </remarks>
    public static class DreamSpace
    {
        /// <summary>
        /// Where a cube's prefab is planted: the centre of its floor, which is
        /// the origin <c>CubeBuilder</c> builds every cube around.
        /// </summary>
        public static Vector3 Origin(Coord c) => new Vector3(
            c.X * CubeMetrics.Size,
            c.Z * CubeMetrics.Size,
            c.Y * CubeMetrics.Size);

        /// <summary>The middle of a cube's interior, eight metres of it.</summary>
        public static Vector3 Centre(Coord c) => Origin(c) + new Vector3(0f, CubeMetrics.Half, 0f);

        /// <summary>
        /// Which cube a world point falls in. The x and z arms round because a
        /// cube straddles its origin on those axes, and the y arm floors
        /// because a cube stands on its origin.
        /// </summary>
        public static Coord CoordAt(Vector3 world) => new Coord(
            Mathf.RoundToInt(world.x / CubeMetrics.Size),
            Mathf.RoundToInt(world.z / CubeMetrics.Size),
            Mathf.FloorToInt(world.y / CubeMetrics.Size));

        /// <summary>
        /// A cube's rotation about the vertical. <see cref="Rotation"/> counts
        /// quarter turns clockwise from above, and so does Unity's y euler:
        /// both send North to East.
        /// </summary>
        public static Quaternion Orientation(Rotation r) => Quaternion.Euler(0f, 90f * (int)r, 0f);

        /// <summary>Which way a face looks, in world space.</summary>
        public static Vector3 Direction(Face f)
        {
            switch (f)
            {
                case Face.North: return Vector3.forward;
                case Face.East: return Vector3.right;
                case Face.South: return Vector3.back;
                case Face.West: return Vector3.left;
                case Face.Up: return Vector3.up;
                case Face.Down: return Vector3.down;
                default: return Vector3.forward;
            }
        }
    }
}
