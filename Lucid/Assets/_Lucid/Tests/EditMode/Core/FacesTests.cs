using System.Collections.Generic;
using System.Linq;
using Lucid.Core;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Core
{
    /// <summary>docs/CORE-API.md §12, "Faces".</summary>
    public sealed class FacesTests
    {
        [Test]
        public void OppositePairsEveryFace()
        {
            Assert.That(Faces.Opposite(Face.North), Is.EqualTo(Face.South));
            Assert.That(Faces.Opposite(Face.East), Is.EqualTo(Face.West));
            Assert.That(Faces.Opposite(Face.Up), Is.EqualTo(Face.Down));

            foreach (Face f in Faces.All)
                Assert.That(Faces.Opposite(Faces.Opposite(f)), Is.EqualTo(f), $"{f} is not its own double opposite");
        }

        [Test]
        public void OffsetMatchesTheAxisConvention()
        {
            Assert.That(Faces.Offset(Face.North), Is.EqualTo(new Coord(0, 1, 0)));
            Assert.That(Faces.Offset(Face.East), Is.EqualTo(new Coord(1, 0, 0)));
            Assert.That(Faces.Offset(Face.South), Is.EqualTo(new Coord(0, -1, 0)));
            Assert.That(Faces.Offset(Face.West), Is.EqualTo(new Coord(-1, 0, 0)));
            Assert.That(Faces.Offset(Face.Up), Is.EqualTo(new Coord(0, 0, 1)));
            Assert.That(Faces.Offset(Face.Down), Is.EqualTo(new Coord(0, 0, -1)));
        }

        [Test]
        public void SteppingThroughAFaceAndBackReturnsHome()
        {
            var c = new Coord(3, -2, 1);
            foreach (Face f in Faces.All)
                Assert.That(c.Offset(f).Offset(Faces.Opposite(f)), Is.EqualTo(c));
        }

        [Test]
        public void RotationTurnsClockwiseSeenFromAbove()
        {
            Assert.That(Faces.Rotate(Face.North, Rotation.R90), Is.EqualTo(Face.East));
            Assert.That(Faces.Rotate(Face.East, Rotation.R90), Is.EqualTo(Face.South));
            Assert.That(Faces.Rotate(Face.South, Rotation.R90), Is.EqualTo(Face.West));
            Assert.That(Faces.Rotate(Face.West, Rotation.R90), Is.EqualTo(Face.North));
        }

        [Test]
        public void RotationLeavesUpAndDownAlone()
        {
            foreach (Rotation r in new[] { Rotation.R0, Rotation.R90, Rotation.R180, Rotation.R270 })
            {
                Assert.That(Faces.Rotate(Face.Up, r), Is.EqualTo(Face.Up));
                Assert.That(Faces.Rotate(Face.Down, r), Is.EqualTo(Face.Down));
            }
        }

        [Test]
        public void FourNinetyDegreeTurnsAreIdentity()
        {
            foreach (Face f in Faces.All)
            {
                Face turned = f;
                for (int i = 0; i < 4; i++) turned = Faces.Rotate(turned, Rotation.R90);
                Assert.That(turned, Is.EqualTo(f), $"{f} did not return after four turns");
            }
        }

        [Test]
        public void MaskRotationMovesEveryHorizontalFaceTogether()
        {
            FaceMask tee = FaceMask.North | FaceMask.East | FaceMask.South;
            Assert.That(Faces.Rotate(tee, Rotation.R90),
                Is.EqualTo(FaceMask.East | FaceMask.South | FaceMask.West));
        }

        [Test]
        public void MaskRotationRoundTripsThroughAllFourRotations()
        {
            FaceMask mixed = FaceMask.North | FaceMask.East | FaceMask.Up | FaceMask.Down;
            FaceMask turned = mixed;
            for (int i = 0; i < 4; i++) turned = Faces.Rotate(turned, Rotation.R90);
            Assert.That(turned, Is.EqualTo(mixed));
        }

        [Test]
        public void RotatingByR180IsTwoNinetyDegreeTurns()
        {
            FaceMask m = FaceMask.North | FaceMask.West | FaceMask.Down;
            Assert.That(Faces.Rotate(m, Rotation.R180),
                Is.EqualTo(Faces.Rotate(Faces.Rotate(m, Rotation.R90), Rotation.R90)));
        }

        [Test]
        public void OfEnumeratesInEnumOrder()
        {
            FaceMask all = FaceMask.North | FaceMask.East | FaceMask.South
                         | FaceMask.West | FaceMask.Up | FaceMask.Down;
            Assert.That(Faces.Of(all).ToArray(), Is.EqualTo(Faces.All));
            Assert.That(Faces.Of(FaceMask.None).Any(), Is.False);
        }

        [Test]
        public void CoordOrderingSortsByLayerThenNorthThenEast()
        {
            var coords = new List<Coord>
            {
                new Coord(1, 0, 1), new Coord(0, 0, 0), new Coord(0, 1, 0), new Coord(1, 0, 0),
            };
            coords.Sort(Coord.Ordering);
            Assert.That(coords, Is.EqualTo(new[]
            {
                new Coord(0, 0, 0), new Coord(1, 0, 0), new Coord(0, 1, 0), new Coord(1, 0, 1),
            }));
        }

        [Test]
        public void CoordsAreValuesNotReferences()
        {
            Assert.That(new Coord(1, 2, 3), Is.EqualTo(new Coord(1, 2, 3)));
            Assert.That(new Coord(1, 2, 3).GetHashCode(), Is.EqualTo(new Coord(1, 2, 3).GetHashCode()));
            Assert.That(new Coord(1, 2, 3) == new Coord(1, 2, 3), Is.True);
            Assert.That(new Coord(1, 2, 3) != new Coord(3, 2, 1), Is.True);
        }
    }
}
