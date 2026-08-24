using System.Linq;
using Lucid.Core;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Core
{
    /// <summary>docs/CORE-API.md §12, "Derive".</summary>
    public sealed class DeriverTests
    {
        [Test]
        public void TheStartCubesDoorIsTheFirstExit()
        {
            (Lattice l, Derived d, CubeRegistry _) = TestLattice.Fresh();

            var door = new ConnectorRef(l.Start, Face.North);
            Assert.That(d.StateOf(door), Is.EqualTo(ConnectorState.Exit));
            Assert.That(d.ExitDepth, Is.EqualTo(0));
            Assert.That(d.Exits, Is.EqualTo(new[] { door }));
        }

        [Test]
        public void DepthCountsHopsAlongALine()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();

            // Start -> (0,1,0) -> (0,2,0), a straight corridor north.
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Straight);
            (l, d) = TestLattice.Place(l, d, reg, new Coord(0, 1, 0), Face.North, TestLattice.Straight);

            Assert.That(d.DepthOf(new Coord(0, 0, 0)), Is.EqualTo(0));
            Assert.That(d.DepthOf(new Coord(0, 1, 0)), Is.EqualTo(1));
            Assert.That(d.DepthOf(new Coord(0, 2, 0)), Is.EqualTo(2));
        }

        [Test]
        public void AttachingClearsTheMistOnBothSides()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Straight);

            Assert.That(d.StateOf(new ConnectorRef(l.Start, Face.North)), Is.EqualTo(ConnectorState.Attached));
            Assert.That(d.StateOf(new ConnectorRef(new Coord(0, 1, 0), Face.South)),
                Is.EqualTo(ConnectorState.Attached));
        }

        [Test]
        public void TheExitMovesToTheDeepestFogDoor()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Straight);

            Assert.That(d.ExitDepth, Is.EqualTo(1));
            Assert.That(d.Exits, Is.EqualTo(new[] { new ConnectorRef(new Coord(0, 1, 0), Face.North) }));
        }

        [Test]
        public void DepthCrossesLayers()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();

            // Start -> drop cube -> the cube below it.
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Drop, Rotation.R180);
            (l, d) = TestLattice.Place(l, d, reg, new Coord(0, 1, 0), Face.Down, TestLattice.Ladder);

            Assert.That(d.DepthOf(new Coord(0, 1, -1)), Is.EqualTo(2));
        }

        [Test]
        public void TiesProduceSeveralExitsAtOnce()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();

            // A T at depth 1 leaves two fog doors on the same cube.
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Tee, Rotation.R180);

            Assert.That(d.ExitDepth, Is.EqualTo(1));
            Assert.That(d.Exits.Count, Is.EqualTo(2), "both remaining doors of the deepest cube are exits");
            Assert.That(d.Exits.All(e => e.Cube == new Coord(0, 1, 0)));
        }

        [Test]
        public void ExploringSolidifiesFogButNeverTheExit()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Tee, Rotation.R0);

            // Give the dream a deeper cube so the T's doors are no longer exits.
            (l, d) = TestLattice.Place(l, d, reg, new Coord(0, 1, 0), Face.East, TestLattice.Straight, Rotation.R90);
            Assert.That(d.ExitDepth, Is.EqualTo(2));

            (l, d) = TestLattice.Explore(l, d, reg, new Coord(0, 1, 0));

            Assert.That(d.StateOf(new ConnectorRef(new Coord(0, 1, 0), Face.North)),
                Is.EqualTo(ConnectorState.Solid), "the T's unused door condensed into wall");
            Assert.That(l.IsExplored(new Coord(0, 1, 0)), Is.True);
        }

        [Test]
        public void ExitDoorsSurviveExploration()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Straight);

            var exit = new ConnectorRef(new Coord(0, 1, 0), Face.North);
            Assert.That(d.StateOf(exit), Is.EqualTo(ConnectorState.Exit));

            (l, d) = TestLattice.Explore(l, d, reg, new Coord(0, 1, 0));

            Assert.That(d.StateOf(exit), Is.EqualTo(ConnectorState.Exit),
                "an exit never solidifies, or the dream could be sealed");
        }

        [Test]
        public void HysteresisHoldsTheLightThenReleasesIt()
        {
            var settings = new RoundSettings(ExitHysteresis: 2);
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh(settings);

            // Depth 1 with two spare doors, so the previous exits keep their fog.
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Tee, Rotation.R0, settings);
            int held = d.ExitDepth;

            // A cube at depth 2 does not beat depth 1 by 2, so the exit stays put.
            (l, d) = TestLattice.Place(l, d, reg, new Coord(0, 1, 0), Face.East,
                TestLattice.Straight, Rotation.R90, settings);
            Assert.That(d.ExitDepth, Is.EqualTo(held), "hysteresis should have held the exit at depth 1");

            // Depth 3 clears the threshold and the light moves.
            (l, d) = TestLattice.Place(l, d, reg, new Coord(1, 1, 0), Face.East,
                TestLattice.Straight, Rotation.R90, settings);
            Assert.That(d.ExitDepth, Is.EqualTo(3));
        }

        [Test]
        public void HysteresisIsOffByDefault()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Tee, Rotation.R0);
            (l, d) = TestLattice.Place(l, d, reg, new Coord(0, 1, 0), Face.East, TestLattice.Straight, Rotation.R90);

            Assert.That(d.ExitDepth, Is.EqualTo(2), "without hysteresis the exit follows the deepest cube");
        }

        [Test]
        public void TheHashIgnoresInsertionOrder()
        {
            // Two dreams with the same cubes reached by different build orders
            // must hash identically, or the netcode's sync check is worthless.
            (Lattice a, Derived da, CubeRegistry rega) = TestLattice.Fresh();
            (a, da) = TestLattice.Place(a, da, rega, a.Start, Face.North, TestLattice.Cross, Rotation.R0);
            (a, da) = TestLattice.Place(a, da, rega, new Coord(0, 1, 0), Face.East, TestLattice.Straight, Rotation.R90);
            (a, da) = TestLattice.Place(a, da, rega, new Coord(0, 1, 0), Face.West, TestLattice.Straight, Rotation.R90);

            (Lattice b, Derived db, CubeRegistry regb) = TestLattice.Fresh();
            (b, db) = TestLattice.Place(b, db, regb, b.Start, Face.North, TestLattice.Cross, Rotation.R0);
            (b, db) = TestLattice.Place(b, db, regb, new Coord(0, 1, 0), Face.West, TestLattice.Straight, Rotation.R90);
            (b, db) = TestLattice.Place(b, db, regb, new Coord(0, 1, 0), Face.East, TestLattice.Straight, Rotation.R90);

            Assert.That(da.Hash, Is.EqualTo(db.Hash));
        }

        [Test]
        public void TheHashChangesWhenTheDreamDoes()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            ulong before = d.Hash;

            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Straight);
            Assert.That(d.Hash, Is.Not.EqualTo(before));

            // Exploration only shows up in the hash when a fog door actually
            // condenses. A T with a spare door, made shallow by a deeper cube,
            // gives it something to solidify.
            (l, d) = TestLattice.Place(l, d, reg, new Coord(0, 1, 0), Face.North, TestLattice.Tee, Rotation.R0);
            (l, d) = TestLattice.Place(l, d, reg, new Coord(0, 2, 0), Face.East, TestLattice.Straight, Rotation.R90);

            ulong placed = d.Hash;
            (l, d) = TestLattice.Explore(l, d, reg, new Coord(0, 2, 0));
            Assert.That(d.Hash, Is.Not.EqualTo(placed),
                "a fog door condensing into wall changes connector state, so it changes the hash");
        }

        [Test]
        public void DerivingTwiceGivesTheSameAnswer()
        {
            (Lattice l, Derived d, CubeRegistry reg) = TestLattice.Fresh();
            (l, d) = TestLattice.Place(l, d, reg, l.Start, Face.North, TestLattice.Tee, Rotation.R180);

            Derived again = Deriver.Derive(l, reg);
            Assert.That(again.Hash, Is.EqualTo(d.Hash));
            Assert.That(again.ExitDepth, Is.EqualTo(d.ExitDepth));
        }
    }
}
