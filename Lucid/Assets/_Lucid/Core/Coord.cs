using System;
using System.Collections.Generic;

namespace Lucid.Core
{
    /// <summary>
    /// A cube address: x east, y north, z layer up. The start cube is (0,0,0).
    /// Core never knows about metres; the runtime maps a coord to a Unity
    /// position as (X*8, Z*8, Y*8) (docs/CORE-API.md §1).
    /// </summary>
    /// <remarks>
    /// Hand-written value semantics rather than a record struct: Unity 6
    /// compiles at C# 9, where record structs do not exist. See
    /// docs/DECISIONS.md.
    /// </remarks>
    public readonly struct Coord : IEquatable<Coord>
    {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public Coord(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>The neighbouring cube through <paramref name="f"/>.</summary>
        public Coord Offset(Face f)
        {
            Coord step = Faces.Offset(f);
            return new Coord(X + step.X, Y + step.Y, Z + step.Z);
        }

        /// <summary>
        /// Sort order (Z, Y, X). Every iteration that feeds the hash uses it,
        /// so two machines walk the lattice in the same sequence.
        /// </summary>
        public static IComparer<Coord> Ordering { get; } = new ZyxComparer();

        sealed class ZyxComparer : IComparer<Coord>
        {
            public int Compare(Coord a, Coord b)
            {
                if (a.Z != b.Z) return a.Z.CompareTo(b.Z);
                if (a.Y != b.Y) return a.Y.CompareTo(b.Y);
                return a.X.CompareTo(b.X);
            }
        }

        public bool Equals(Coord other) => X == other.X && Y == other.Y && Z == other.Z;

        public override bool Equals(object obj) => obj is Coord other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = (h * 31) + X;
                h = (h * 31) + Y;
                h = (h * 31) + Z;
                return h;
            }
        }

        public static bool operator ==(Coord a, Coord b) => a.Equals(b);

        public static bool operator !=(Coord a, Coord b) => !a.Equals(b);

        public void Deconstruct(out int x, out int y, out int z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public override string ToString() => $"({X}, {Y}, {Z})";
    }
}
