using System;
using System.Collections.Generic;

namespace Lucid.Core
{
    /// <summary>Face arithmetic: opposites, unit steps and rotation.</summary>
    public static class Faces
    {
        /// <summary>Every face, in enum order. Iteration order feeds the hash.</summary>
        public static readonly Face[] All =
        {
            Face.North, Face.East, Face.South, Face.West, Face.Up, Face.Down,
        };

        public static Face Opposite(Face f)
        {
            switch (f)
            {
                case Face.North: return Face.South;
                case Face.East: return Face.West;
                case Face.South: return Face.North;
                case Face.West: return Face.East;
                case Face.Up: return Face.Down;
                case Face.Down: return Face.Up;
                default: throw new ArgumentOutOfRangeException(nameof(f), f, "not a face");
            }
        }

        /// <summary>The unit step through a face.</summary>
        public static Coord Offset(Face f)
        {
            switch (f)
            {
                case Face.North: return new Coord(0, 1, 0);
                case Face.East: return new Coord(1, 0, 0);
                case Face.South: return new Coord(0, -1, 0);
                case Face.West: return new Coord(-1, 0, 0);
                case Face.Up: return new Coord(0, 0, 1);
                case Face.Down: return new Coord(0, 0, -1);
                default: throw new ArgumentOutOfRangeException(nameof(f), f, "not a face");
            }
        }

        public static bool IsVertical(Face f) => f == Face.Up || f == Face.Down;

        /// <summary>Rotate a face clockwise seen from above. Up and Down are fixed.</summary>
        public static Face Rotate(Face f, Rotation r)
        {
            if (IsVertical(f)) return f;
            int steps = (int)r;
            return (Face)(((int)f + steps) & 3);   // North..West occupy 0..3
        }

        public static FaceMask Rotate(FaceMask m, Rotation r)
        {
            if (r == Rotation.R0) return m;

            FaceMask result = m & (FaceMask.Up | FaceMask.Down);
            for (int i = 0; i < 4; i++)
            {
                var f = (Face)i;
                if ((m & ToMask(f)) != 0) result |= ToMask(Rotate(f, r));
            }
            return result;
        }

        public static FaceMask ToMask(Face f) => (FaceMask)(1 << (int)f);

        public static bool Has(FaceMask m, Face f) => (m & ToMask(f)) != 0;

        /// <summary>The faces set in a mask, in enum order.</summary>
        public static IEnumerable<Face> Of(FaceMask m)
        {
            foreach (Face f in All)
            {
                if (Has(m, f)) yield return f;
            }
        }

        public static int Count(FaceMask m)
        {
            int n = 0;
            foreach (Face f in All)
            {
                if (Has(m, f)) n++;
            }
            return n;
        }
    }
}
