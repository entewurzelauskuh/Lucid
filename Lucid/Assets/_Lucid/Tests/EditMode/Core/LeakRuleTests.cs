using Lucid.Core;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Core
{
    /// <summary>
    /// docs/CORE-API.md §12, group "Leak". The rule that stops the Nightmare
    /// winning by sealing the dream instead of outlasting the Sleepers.
    /// </summary>
    public sealed class LeakRuleTests
    {
        /// <summary>
        /// A square of corridor whose last placement would close the loop and
        /// leave no fog door anywhere.
        /// </summary>
        static (Lattice, Derived, CubeRegistry) AlmostClosedLoop()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Tee, Rotation.R0);
            (l, d) = TestLattice.Place(l, d, reg, new Coord(0, 1, 0), Face.East, TestLattice.Corner, Rotation.R270);
            (l, d) = TestLattice.Place(l, d, reg, new Coord(1, 1, 0), Face.North, TestLattice.Corner, Rotation.R180);
            return (l, d, reg);
        }

        [Test]
        public void ClosingTheLastFogDoorIsRejected()
        {
            (Lattice l, Derived d, CubeRegistry reg) = AlmostClosedLoop();

            // A corner with East and South doors at (0,2,0) meets (1,2,0) to its
            // east and (0,1,0) to its south, consuming both remaining fog doors
            // and creating none.
            PlaceVerdict v = TestLattice.Validate(l, d, reg, new Coord(0, 1, 0), Face.North,
                TestLattice.Corner, Rotation.R90);

            Assert.That(v.Error, Is.EqualTo(PlaceError.WouldTrap),
                "the Nightmare can never seal the dream (docs/SPEC.md §7)");
        }

        [Test]
        public void SealingIsRefusedEvenWithNobodyInTheDream()
        {
            // Invariant 4 has two halves: an exit must exist at all, and every
            // Sleeper must reach one. An empty dream still may not be sealed.
            (Lattice l, Derived d, CubeRegistry reg) = AlmostClosedLoop();

            PlaceVerdict v = TestLattice.Validate(l, d, reg, new Coord(0, 1, 0), Face.North,
                TestLattice.Corner, Rotation.R90, sleepers: new SleeperState[0]);

            Assert.That(v.Error, Is.EqualTo(PlaceError.WouldTrap));
        }

        /// <summary>
        /// A ledge with a pit below it: the Sleeper in the pit cannot climb out,
        /// so the exit has to stay down there with them.
        /// </summary>
        static (Lattice, Derived, CubeRegistry) PitBelowALedge()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Ledge, Rotation.R180);
            (l, d) = TestLattice.Place(l, d, reg, new Coord(0, 1, 0), Face.Down, TestLattice.Pit, Rotation.R0);
            return (l, d, reg);
        }

        [Test]
        public void ADropKeepsTheExitBeyondIt()
        {
            (Lattice l, Derived d, CubeRegistry reg) = PitBelowALedge();

            Assert.That(d.DepthOf(new Coord(0, 1, -1)), Is.EqualTo(2));
            Assert.That(d.ExitDepth, Is.EqualTo(2), "the deepest fog door is in the pit");
            Assert.That(d.StateOf(new ConnectorRef(new Coord(0, 1, -1), Face.North)),
                Is.EqualTo(ConnectorState.Exit));
        }

        [Test]
        public void YouCannotClimbOutOfAPit()
        {
            (Lattice l, Derived d, CubeRegistry reg) = PitBelowALedge();

            var pit = new Coord(0, 1, -1);
            Assert.That(Traversal.Reachable(l, reg, d, pit), Is.EquivalentTo(new[] { pit }),
                "the pit is not climbable, so its only exit is its own fog door");

            // From above, the drop is one-way but perfectly passable.
            Assert.That(Traversal.Reachable(l, reg, d, l.Start), Does.Contain(pit));
        }

        [Test]
        public void StrandingASleeperBelowADropIsRejectedAndNamesThem()
        {
            (Lattice l, Derived d, CubeRegistry reg) = PitBelowALedge();
            SleeperState[] sleepers = { TestLattice.Sleeper(2, new Coord(0, 1, -1)) };

            // Grow the branch above the drop until it would be strictly deeper
            // than the pit, which would move the light out of the Sleeper's reach.
            (l, d) = TestLattice.Place(l, d, reg, new Coord(0, 1, 0), Face.West, TestLattice.Straight, Rotation.R90);
            Assert.That(d.ExitDepth, Is.EqualTo(2), "a tie still leaves an exit in the pit");

            PlaceVerdict v = TestLattice.Validate(l, d, reg, new Coord(-1, 1, 0), Face.West,
                TestLattice.Straight, Rotation.R90, sleepers: sleepers);

            Assert.That(v.Error, Is.EqualTo(PlaceError.WouldTrap));
            Assert.That(v.TrappedSleeper, Is.EqualTo(2), "the verdict names who would be stranded");
        }

        [Test]
        public void TheSamePlacementIsFineForASleeperWhoIsNotInTheDream()
        {
            (Lattice l, Derived d, CubeRegistry reg) = PitBelowALedge();
            (l, d) = TestLattice.Place(l, d, reg, new Coord(0, 1, 0), Face.West, TestLattice.Straight, Rotation.R90);

            foreach (SleeperStatus status in new[]
                     { SleeperStatus.Awake, SleeperStatus.Consumed, SleeperStatus.Disconnected })
            {
                SleeperState[] sleepers = { TestLattice.Sleeper(2, new Coord(0, 1, -1), status) };
                PlaceVerdict v = TestLattice.Validate(l, d, reg, new Coord(-1, 1, 0), Face.West,
                    TestLattice.Straight, Rotation.R90, sleepers: sleepers);

                Assert.That(v.IsOk, Is.True, $"a {status} Sleeper is not the leak rule's concern, got {v}");
            }
        }

        [Test]
        public void ASleeperWhoCanStillWalkToAnExitIsNotTrapped()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Straight);

            SleeperState[] sleepers = { TestLattice.Sleeper(0, l.Start) };
            PlaceVerdict v = TestLattice.Validate(l, d, reg, new Coord(0, 1, 0), Face.North,
                TestLattice.Straight, Rotation.R0, sleepers: sleepers);

            Assert.That(v.IsOk, Is.True, $"the corridor still leads somewhere, got {v}");
        }
    }
}
