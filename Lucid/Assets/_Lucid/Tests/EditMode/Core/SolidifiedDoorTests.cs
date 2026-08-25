using Lucid.Core;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Core
{
    /// <summary>
    /// A door a Sleeper condensed into wall is wall forever (docs/SPEC.md §7:
    /// "Nightmare may attach: never again"), and invariant 3 says no door is
    /// both Solidified and Attached. Building on the door is already refused;
    /// these cover reaching the cell behind it from another side.
    /// </summary>
    public sealed class SolidifiedDoorTests
    {
        /// <summary>
        /// A T whose north door is solidified, with a corridor running round
        /// the outside to the cell on the far side of that wall.
        /// </summary>
        static (Lattice, Derived, CubeRegistry) SolidDoorWithAWayAround()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Tee, Rotation.R0);
            (l, d) = TestLattice.Place(l, d, reg, new Coord(0, 1, 0), Face.East, TestLattice.Cross, Rotation.R0);

            // The T's north door is fog at depth 1 while the cross is deeper, so
            // exploring the T condenses it into wall.
            (l, d) = TestLattice.Explore(l, d, reg, new Coord(0, 1, 0));

            (l, d) = TestLattice.Place(l, d, reg, new Coord(1, 1, 0), Face.North, TestLattice.Corner, Rotation.R180);
            return (l, d, reg);
        }

        [Test]
        public void TheSolidifiedDoorIsReallySolid()
        {
            (Lattice l, Derived d, CubeRegistry reg) = SolidDoorWithAWayAround();

            var sealedDoor = new ConnectorRef(new Coord(0, 1, 0), Face.North);
            Assert.That(l.IsSolidified(sealedDoor), Is.True);
            Assert.That(d.StateOf(sealedDoor), Is.EqualTo(ConnectorState.Solid));
        }

        [Test]
        public void ACubeMayNotOpenADoorOntoASolidifiedWall()
        {
            (Lattice l, Derived d, CubeRegistry reg) = SolidDoorWithAWayAround();

            // A corner with East and South doors at (0,2,0): its South door would
            // face the T's sealed north wall and re-open it.
            PlaceVerdict v = TestLattice.Validate(l, d, reg, new Coord(1, 2, 0), Face.West,
                TestLattice.Corner, Rotation.R90);

            Assert.That(v.Error, Is.EqualTo(PlaceError.DoesNotFit),
                "a condensed door is wall; a doorway may not open onto it");
        }

        [Test]
        public void AWallMayAbutASolidifiedWall()
        {
            (Lattice l, Derived d, CubeRegistry reg) = SolidDoorWithAWayAround();

            // A straight running east-west at (0,2,0) presents a blank south face
            // to the sealed door, which is a legal wall-to-wall contact.
            PlaceVerdict v = TestLattice.Validate(l, d, reg, new Coord(1, 2, 0), Face.West,
                TestLattice.Straight, Rotation.R90);

            Assert.That(v.Ok, Is.True, $"wall against sealed wall is the correct neighbour, got {v}");
        }

        [Test]
        public void ASolidifiedDoorNeverBecomesAPassage()
        {
            (Lattice l, Derived d, CubeRegistry reg) = SolidDoorWithAWayAround();
            (l, d) = TestLattice.Place(l, d, reg, new Coord(1, 2, 0), Face.West,
                TestLattice.Straight, Rotation.R90);

            var sealedDoor = new ConnectorRef(new Coord(0, 1, 0), Face.North);

            // Invariant 3 (docs/CORE-API.md §11).
            Assert.That(l.IsSolidified(sealedDoor), Is.True);
            Assert.That(d.StateOf(sealedDoor), Is.Not.EqualTo(ConnectorState.Attached),
                "no door is both Solidified and Attached");
        }
    }
}
