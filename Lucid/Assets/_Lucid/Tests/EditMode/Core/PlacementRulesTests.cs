using Lucid.Core;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Core
{
    /// <summary>docs/CORE-API.md §12, groups "Fit" and "Frontier".</summary>
    public sealed class PlacementRulesTests
    {
        // ---- Fit -----------------------------------------------------------

        [Test]
        public void ACubeMustHaveADoorFacingTheOneItGrowsFrom()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();

            // A corner rotated so its doors are North and East has no South face
            // to meet the start cube's North door.
            PlaceVerdict v = TestLattice.Validate(l, d, reg, l.Start, Face.North,
                TestLattice.Corner, Rotation.R0);

            Assert.That(v.Error, Is.EqualTo(PlaceError.DoesNotFit));
        }

        [Test]
        public void WallMeetingWallIsFine()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Tee, Rotation.R0);

            // (1,1,0) sits beside the start cube. Its South face is a wall and
            // the start cube's East face is a wall, so they may touch.
            PlaceVerdict v = TestLattice.Validate(l, d, reg, new Coord(0, 1, 0), Face.East,
                TestLattice.Straight, Rotation.R90);

            Assert.That(v.IsOk, Is.True, $"expected a legal placement, got {v}");
        }

        /// <summary>
        /// An L of corridor with a gap at (1,2,0), bounded by a door on
        /// (0,2,0)'s east face and another on (1,1,0)'s north face. Anything
        /// dropped into that gap has to satisfy both at once.
        /// </summary>
        static (Lattice, Derived, CubeRegistry) CornerGap()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Tee, Rotation.R0);
            (l, d) = TestLattice.Place(l, d, reg, new Coord(0, 1, 0), Face.East, TestLattice.Corner, Rotation.R270);
            (l, d) = TestLattice.Place(l, d, reg, new Coord(0, 1, 0), Face.North, TestLattice.Corner, Rotation.R90);
            return (l, d, reg);
        }

        [Test]
        public void ADoorwayMayNotOpenIntoAWall()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Tee, Rotation.R0);
            (l, d) = TestLattice.Place(l, d, reg, new Coord(0, 1, 0), Face.East, TestLattice.Straight, Rotation.R90);
            (l, d) = TestLattice.Place(l, d, reg, new Coord(0, 1, 0), Face.North, TestLattice.Corner, Rotation.R90);

            // A corner with South and West doors at (1,2,0): its West door meets
            // (0,2,0) correctly, but its South door would open into the blank
            // north wall of the straight at (1,1,0).
            PlaceVerdict v = TestLattice.Validate(l, d, reg, new Coord(0, 2, 0), Face.East,
                TestLattice.Corner, Rotation.R180);

            Assert.That(v.Error, Is.EqualTo(PlaceError.DoesNotFit));
        }

        [Test]
        public void FitIsCheckedAgainstEveryNeighbourNotJustTheTarget()
        {
            (Lattice l, Derived d, CubeRegistry reg) = CornerGap();

            // A straight satisfies the door it is built on and ignores the one
            // below it, leaving (1,1,0)'s north doorway facing a wall.
            PlaceVerdict straight = TestLattice.Validate(l, d, reg, new Coord(0, 2, 0), Face.East,
                TestLattice.Straight, Rotation.R90);
            Assert.That(straight.Error, Is.EqualTo(PlaceError.DoesNotFit),
                "the second contact is unmet, even though the target door is satisfied");

            // A T with South, West and North doors answers both neighbours and
            // still leaves itself a fog door to the north.
            PlaceVerdict tee = TestLattice.Validate(l, d, reg, new Coord(0, 2, 0), Face.East,
                TestLattice.Tee, Rotation.R180);
            Assert.That(tee.IsOk, Is.True, $"expected a two-sided fit to be legal, got {tee}");
        }

        // ---- Frontier ------------------------------------------------------

        [Test]
        public void PlacingIntoEmptySpaceIsRejected()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();

            PlaceVerdict v = TestLattice.Validate(l, d, reg, new Coord(5, 5, 0), Face.North,
                TestLattice.Straight);

            Assert.That(v.Error, Is.EqualTo(PlaceError.NotADoor));
        }

        [Test]
        public void PlacingOnAWallIsRejected()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();

            // The start cube has one door, facing north.
            PlaceVerdict v = TestLattice.Validate(l, d, reg, l.Start, Face.East, TestLattice.Straight);

            Assert.That(v.Error, Is.EqualTo(PlaceError.NotADoor));
        }

        [Test]
        public void PlacingOnAnAttachedDoorIsRejected()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Straight);

            PlaceVerdict v = TestLattice.Validate(l, d, reg, l.Start, Face.North, TestLattice.Straight);

            Assert.That(v.Error, Is.EqualTo(PlaceError.NotADoor), "that doorway is already a passage");
        }

        [Test]
        public void PlacingOnASolidDoorIsRejected()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Tee, Rotation.R0);
            (l, d) = TestLattice.Place(l, d, reg, new Coord(0, 1, 0), Face.East, TestLattice.Straight, Rotation.R90);

            // The T's North door is now plain fog, and exploring the cube
            // condenses it into wall for good.
            (l, d) = TestLattice.Explore(l, d, reg, new Coord(0, 1, 0));
            Assert.That(d.StateOf(new ConnectorRef(new Coord(0, 1, 0), Face.North)),
                Is.EqualTo(ConnectorState.Solid));

            PlaceVerdict v = TestLattice.Validate(l, d, reg, new Coord(0, 1, 0), Face.North,
                TestLattice.Straight);

            Assert.That(v.Error, Is.EqualTo(PlaceError.DoorIsSolid));
        }

        [Test]
        public void PlacingOnAnExitIsLegalAndMovesTheExitDeeper()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();

            var startDoor = new ConnectorRef(l.Start, Face.North);
            Assert.That(d.StateOf(startDoor), Is.EqualTo(ConnectorState.Exit));

            PlaceVerdict v = TestLattice.Validate(l, d, reg, l.Start, Face.North, TestLattice.Straight);
            Assert.That(v.IsOk, Is.True, $"building on the exit is how the dream grows, got {v}");

            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Straight);
            Assert.That(d.ExitDepth, Is.EqualTo(1));
            Assert.That(d.StateOf(startDoor), Is.EqualTo(ConnectorState.Attached));
        }

        // ---- The other refusals --------------------------------------------

        [Test]
        public void AnUnknownTypeIsRejected()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            Assert.That(TestLattice.Validate(l, d, reg, l.Start, Face.North, "core.nope").Error,
                Is.EqualTo(PlaceError.UnknownType));
        }

        [Test]
        public void TheStartCubeIsNotAPlaceableType()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            Assert.That(TestLattice.Validate(l, d, reg, l.Start, Face.North, TestLattice.Start).Error,
                Is.EqualTo(PlaceError.UnknownType), "there is exactly one bedroom");
        }

        [Test]
        public void TheDreamHasEdges()
        {
            var settings = new RoundSettings(Limits: new Limits(FootprintHalf: 1, LayerMin: 0, LayerMax: 0));
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh(settings);
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Straight, Rotation.R0, settings);

            // (0,2,0) is outside a footprint of one.
            PlaceVerdict v = TestLattice.Validate(l, d, reg, new Coord(0, 1, 0), Face.North,
                TestLattice.Straight, Rotation.R0, settings);

            Assert.That(v.Error, Is.EqualTo(PlaceError.OutOfBounds));
        }

        [Test]
        public void ACubeYouCannotAffordIsRejected()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();

            PlaceVerdict broke = TestLattice.Validate(l, d, reg, l.Start, Face.North,
                TestLattice.Straight, Rotation.R0, budget: new Budget(0, 0));
            Assert.That(broke.Error, Is.EqualTo(PlaceError.NotEnoughBudget));

            PlaceVerdict flush = TestLattice.Validate(l, d, reg, l.Start, Face.North,
                TestLattice.Straight, Rotation.R0, budget: new Budget(5, 0));
            Assert.That(flush.IsOk, Is.True);
        }

        [Test]
        public void ValidationNeverChangesAnything()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            int cubes = l.Cubes.Count;
            ulong hash = d.Hash;
            var budget = new Budget(10, 0);

            TestLattice.Validate(l, d, reg, l.Start, Face.North, TestLattice.Straight,
                Rotation.R0, budget: budget);

            Assert.That(l.Cubes.Count, Is.EqualTo(cubes));
            Assert.That(d.Hash, Is.EqualTo(hash));
            Assert.That(budget.Points, Is.EqualTo(10), "the host spends, not the rules");
        }
    }
}
