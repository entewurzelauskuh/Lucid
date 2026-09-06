using Lucid.Core;
using Lucid.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Lucid.Tests.EditMode.Dream
{
    /// <summary>
    /// The lattice's axes against Unity's. Core calls z the layer and Unity
    /// calls y up, so every one of these would pass just as happily with two
    /// of the three arms swapped — which is why they are checked against the
    /// cube geometry the builder actually produces rather than against each
    /// other.
    /// </summary>
    public sealed class DreamSpaceTests
    {
        [Test]
        public void TheLayerAxisBecomesUp()
        {
            // (x east, y north, z layer) -> (x, up, forward) · 8.
            Assert.That(DreamSpace.Origin(new Coord(1, 0, 0)), Is.EqualTo(new Vector3(8f, 0f, 0f)));
            Assert.That(DreamSpace.Origin(new Coord(0, 1, 0)), Is.EqualTo(new Vector3(0f, 0f, 8f)));
            Assert.That(DreamSpace.Origin(new Coord(0, 0, 1)), Is.EqualTo(new Vector3(0f, 8f, 0f)));
        }

        [Test]
        public void ACubeStandsOnItsOriginAndStraddlesItSideways()
        {
            // CubeBuilder puts Down at y 0 and Up at y 8, and North at +z half
            // — so the origin is the floor's centre, not the cube's.
            Assert.That(DreamSpace.Origin(new Coord(0, 0, 0)), Is.EqualTo(Vector3.zero));
            Assert.That(DreamSpace.Centre(new Coord(0, 0, 0)), Is.EqualTo(new Vector3(0f, 4f, 0f)));
        }

        [Test]
        public void CoordAtInvertsOriginForEveryCorner()
        {
            foreach (Coord c in new[]
                     {
                         new Coord(0, 0, 0), new Coord(1, -2, 3), new Coord(-4, 5, -6),
                     })
            {
                Assert.That(DreamSpace.CoordAt(DreamSpace.Origin(c)), Is.EqualTo(c), $"origin {c}");
                Assert.That(DreamSpace.CoordAt(DreamSpace.Centre(c)), Is.EqualTo(c), $"centre {c}");
            }
        }

        [Test]
        public void APointJustInsideACubeBelongsToIt()
        {
            var c = new Coord(0, 0, 0);

            // The two horizontal arms straddle, so ±3.9 is still home and the
            // vertical arm runs 0 to 8 rather than -4 to 4. Rounding all three
            // the same way put every cube's ceiling in the layer above it.
            Assert.That(DreamSpace.CoordAt(new Vector3(3.9f, 0.1f, 3.9f)), Is.EqualTo(c));
            Assert.That(DreamSpace.CoordAt(new Vector3(-3.9f, 7.9f, -3.9f)), Is.EqualTo(c));
            Assert.That(DreamSpace.CoordAt(new Vector3(0f, 8.1f, 0f)), Is.EqualTo(new Coord(0, 0, 1)));
            Assert.That(DreamSpace.CoordAt(new Vector3(4.1f, 0f, 0f)), Is.EqualTo(new Coord(1, 0, 0)));
        }

        [Test]
        public void AQuarterTurnSendsNorthToEast()
        {
            // Rotation is clockwise seen from above (docs/CORE-API.md §1), and
            // so is Unity's y euler. If the two disagreed, a rotated cube's
            // doors would be wired to the wall opposite.
            Vector3 turned = DreamSpace.Orientation(Rotation.R90) * DreamSpace.Direction(Face.North);
            Assert.That(turned.x, Is.EqualTo(1f).Within(1e-5f));
            Assert.That(turned.z, Is.EqualTo(0f).Within(1e-5f));

            Assert.That(DreamSpace.Direction(Faces.Rotate(Face.North, Rotation.R90)),
                Is.EqualTo(DreamSpace.Direction(Face.East)));
        }

        [Test]
        public void EveryFaceLooksTheWayItsOffsetPoints()
        {
            foreach (Face f in Faces.All)
            {
                Coord step = Faces.Offset(f);
                Vector3 expected = DreamSpace.Origin(new Coord(step.X, step.Y, step.Z));
                Assert.That(DreamSpace.Direction(f) * CubeMetrics.Size, Is.EqualTo(expected), f.ToString());
            }
        }
    }
}
