using Lucid.Core;
using UnityEngine;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// The cube frame from docs/CUBE-SPEC.md §1, in one place so the builder
    /// and the validator cannot drift apart.
    /// </summary>
    /// <remarks>
    /// Cube-local coordinates: origin at the centre of the floor, x east,
    /// y up, z north, metres. The cube spans x, z in [-4, 4] and y in [0, 8].
    /// Note this is *not* Core's frame — Core's z is the layer and its y is
    /// north (docs/CORE-API.md §1). The swap lives in <see cref="Centre"/> and
    /// nowhere else.
    /// </remarks>
    public static class CubeGeometry
    {
        public const float Size = 8f;
        public const float Half = Size / 2f;

        /// <summary>Doorways are 2.5 m wide and 3 m high, at floor level.</summary>
        public const float DoorWidth = 2.5f;
        public const float DoorHeight = 3f;

        /// <summary>A vertical connector is a 2.5 m square hole.</summary>
        public const float VerticalHole = 2.5f;

        public const float DefaultThickness = 0.3f;

        /// <summary>The centre of a face, at floor level.</summary>
        public static Vector3 Centre(Face face)
        {
            switch (face)
            {
                case Face.North: return new Vector3(0f, 0f, Half);
                case Face.East: return new Vector3(Half, 0f, 0f);
                case Face.South: return new Vector3(0f, 0f, -Half);
                case Face.West: return new Vector3(-Half, 0f, 0f);
                case Face.Up: return new Vector3(0f, Size, 0f);
                case Face.Down: return new Vector3(0f, 0f, 0f);
                default: return Vector3.zero;
            }
        }

        /// <summary>Which way a face looks, from inside the cube.</summary>
        public static Vector3 Outward(Face face)
        {
            switch (face)
            {
                case Face.North: return Vector3.forward;
                case Face.East: return Vector3.right;
                case Face.South: return Vector3.back;
                case Face.West: return Vector3.left;
                case Face.Up: return Vector3.up;
                case Face.Down: return Vector3.down;
                default: return Vector3.zero;
            }
        }

        /// <summary>
        /// How far the interior reaches horizontally. A narrower room does not
        /// thin the walls, it makes them thicker: the cube stays solid out to
        /// its bounds, which is what "a corridor inside an 8 m cube" means
        /// (docs/CUBE-SPEC.md §3).
        /// </summary>
        public static float InteriorHalf(ShellSpec shell)
        {
            float width = shell?.Interior?.Width ?? (Size - Thickness(shell) * 2f);
            return width / 2f;
        }

        /// <summary>The height of the room, from the floor surface to the ceiling.</summary>
        public static float InteriorHeight(ShellSpec shell) =>
            shell?.Interior?.Height ?? (Size - Thickness(shell) * 2f);

        public static float Thickness(ShellSpec shell) => shell?.Thickness ?? DefaultThickness;

        /// <summary>
        /// Where the ceiling slab stops. One thickness below the top, so that
        /// the cube stacked above can put its floor slab in the gap rather than
        /// in the same volume.
        /// </summary>
        public static float CeilingTop(ShellSpec shell) => Size - Thickness(shell);

        /// <summary>
        /// The widest interior that still leaves a wall on each side. The schema
        /// permits a width of 8, which would leave none at all.
        /// </summary>
        public static float MaxInteriorWidth(ShellSpec shell) => Size - Thickness(shell) * 2f;

        /// <summary>
        /// The tallest interior that still leaves a ceiling. Above this there is
        /// no room between the interior and where the slab has to stop.
        /// </summary>
        public static float MaxInteriorHeight(ShellSpec shell) => CeilingTop(shell) - Thickness(shell);
    }
}
