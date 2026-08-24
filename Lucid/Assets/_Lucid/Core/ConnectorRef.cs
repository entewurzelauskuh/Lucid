using System;
using System.Collections.Generic;

namespace Lucid.Core
{
    /// <summary>
    /// One doorway, seen from one cube. The same physical passage between two
    /// attached cubes is two ConnectorRefs, one per side.
    /// </summary>
    public readonly struct ConnectorRef : IEquatable<ConnectorRef>
    {
        public Coord Cube { get; }
        public Face Face { get; }

        public ConnectorRef(Coord cube, Face face)
        {
            Cube = cube;
            Face = face;
        }

        /// <summary>The same doorway seen from the neighbouring cube.</summary>
        public ConnectorRef Flip() => new ConnectorRef(Cube.Offset(Face), Faces.Opposite(Face));

        /// <summary>Sort order: by cube in (Z, Y, X), then by face in enum order.</summary>
        public static IComparer<ConnectorRef> Ordering { get; } = new CubeThenFaceComparer();

        sealed class CubeThenFaceComparer : IComparer<ConnectorRef>
        {
            public int Compare(ConnectorRef a, ConnectorRef b)
            {
                int byCube = Coord.Ordering.Compare(a.Cube, b.Cube);
                return byCube != 0 ? byCube : ((int)a.Face).CompareTo((int)b.Face);
            }
        }

        public bool Equals(ConnectorRef other) => Cube.Equals(other.Cube) && Face == other.Face;
        public override bool Equals(object obj) => obj is ConnectorRef other && Equals(other);

        public override int GetHashCode()
        {
            unchecked { return (Cube.GetHashCode() * 31) + (int)Face; }
        }

        public static bool operator ==(ConnectorRef a, ConnectorRef b) => a.Equals(b);
        public static bool operator !=(ConnectorRef a, ConnectorRef b) => !a.Equals(b);
        public override string ToString() => $"{Cube}.{Face}";
    }
}
