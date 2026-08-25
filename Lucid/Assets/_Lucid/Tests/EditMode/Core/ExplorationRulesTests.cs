using Lucid.Core;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Core
{
    /// <summary>docs/CORE-API.md §12, group "Explore".</summary>
    public sealed class ExplorationRulesTests
    {
        [Test]
        public void ReportingACubeThatDoesNotExistIsIgnored()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            Assert.That(Rules.ValidateExplore(TestLattice.Context(l, d, reg), new Coord(9, 9, 0)),
                Is.EqualTo(ExploreError.NoCube));
        }

        [Test]
        public void TheStartCubeIsExempt()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            Assert.That(Rules.ValidateExplore(TestLattice.Context(l, d, reg), l.Start),
                Is.EqualTo(ExploreError.StartCube),
                "the bedroom is never explored, or its door could solidify and seal the dream");
        }

        [Test]
        public void ExplorationIsIdempotent()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Straight);

            Assert.That(Rules.ValidateExplore(TestLattice.Context(l, d, reg), new Coord(0, 1, 0)),
                Is.EqualTo(ExploreError.None));

            (l, d) = TestLattice.Explore(l, d, reg, new Coord(0, 1, 0));

            Assert.That(Rules.ValidateExplore(TestLattice.Context(l, d, reg), new Coord(0, 1, 0)),
                Is.EqualTo(ExploreError.AlreadyExplored),
                "reported twice by two dreams; the host drops the second quietly");
        }

        [Test]
        public void ExploreThenPlaceLeavesTheDoorSolid()
        {
            // The host serialises events, so a race resolves by arrival order
            // (docs/CORE-API.md §6). Exploration first closes the door.
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Tee, Rotation.R0);
            (l, d) = TestLattice.Place(l, d, reg, new Coord(0, 1, 0), Face.East, TestLattice.Straight, Rotation.R90);

            (l, d) = TestLattice.Explore(l, d, reg, new Coord(0, 1, 0));

            PlaceVerdict v = TestLattice.Validate(l, d, reg, new Coord(0, 1, 0), Face.North,
                TestLattice.Straight);
            Assert.That(v.Error, Is.EqualTo(PlaceError.DoorIsSolid));
        }

        [Test]
        public void PlaceThenExploreLeavesTheDoorAttached()
        {
            // The same race the other way round: the cube got there first, so
            // exploration finds a passage rather than mist.
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Tee, Rotation.R0);
            (l, d) = TestLattice.Place(l, d, reg, new Coord(0, 1, 0), Face.East, TestLattice.Straight, Rotation.R90);
            (l, d) = TestLattice.Place(l, d, reg, new Coord(0, 1, 0), Face.North, TestLattice.Straight);

            (l, d) = TestLattice.Explore(l, d, reg, new Coord(0, 1, 0));

            var door = new ConnectorRef(new Coord(0, 1, 0), Face.North);
            Assert.That(d.StateOf(door), Is.EqualTo(ConnectorState.Attached),
                "exploration had nothing to solidify on that face");

            // The state alone proves little: Deriver reports Attached from the
            // neighbour existing, so this would hold even if ApplyExplore had
            // wrongly added the door to Solidified. Check the set itself.
            Assert.That(l.IsSolidified(door), Is.False,
                "a passage must never end up in Solidified (invariant 3)");
        }
    }
}
