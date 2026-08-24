using System;
using System.IO;
using Lucid.Core;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Core
{
    /// <summary>docs/CORE-API.md §12, "Log".</summary>
    public sealed class EventLogTests
    {
        [Test]
        public void AppendInsistsOnTheNextSequenceNumber()
        {
            var log = new EventLog();
            Assert.That(log.NextSeq, Is.EqualTo(0));

            log.Append(new CubePlaced(0, new Coord(0, 1, 0), TestLattice.Straight, Rotation.R0, null));
            Assert.That(log.NextSeq, Is.EqualTo(1));

            // A gap would mean a lost event, which the wire does not allow
            // (docs/NETCODE.md §5).
            Assert.That(() => log.Append(new CubeExplored(5, new Coord(0, 1, 0), 0)),
                Throws.ArgumentException);
        }

        [Test]
        public void BinaryRoundTripPreservesEveryEvent()
        {
            var log = new EventLog();
            log.Append(new CubePlaced(0, new Coord(0, 1, 0), TestLattice.Straight, Rotation.R90, "skin.bedroom"));
            log.Append(new CubeExplored(1, new Coord(0, 1, 0), 2));
            log.Append(new CubePlaced(2, new Coord(0, 2, -1), TestLattice.Drop, Rotation.R180, null));

            EventLog read;
            using (var ms = new MemoryStream())
            {
                log.Write(ms);
                ms.Position = 0;
                read = EventLog.Read(ms);
            }

            Assert.That(read.Events.Count, Is.EqualTo(3));
            Assert.That(read.Events, Is.EqualTo(log.Events), "records compare by value, so this is a deep check");
        }

        [Test]
        public void ReadRejectsSomethingThatIsNotALog()
        {
            using (var ms = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }))
                Assert.That(() => EventLog.Read(ms), Throws.TypeOf<InvalidDataException>());
        }

        [Test]
        public void ReplayingAScriptedGameReproducesTheLiveHash()
        {
            // The whole netcode sync check rests on this: a client that applies
            // the same events derives the same dream (docs/NETCODE.md §5).
            (Lattice live, Derived derived, CubeRegistry reg) = TestLattice.Fresh();
            var log = new EventLog();
            var rng = new DeterministicWalk();

            for (int step = 0; step < 50; step++)
            {
                if (derived.Exits.Count == 0) break;
                ConnectorRef door = derived.Exits[rng.Next(derived.Exits.Count)];

                if (step % 7 == 6 && door.Cube != live.Start)
                {
                    log.Append(new CubeExplored(log.NextSeq, door.Cube, 0));
                    (live, derived) = TestLattice.Explore(live, derived, reg, door.Cube);
                    continue;
                }

                // A straight corridor rotated to meet the door it grows from.
                Rotation rot = door.Face == Face.East || door.Face == Face.West
                    ? Rotation.R90 : Rotation.R0;
                Coord at = door.Cube.Offset(door.Face);
                log.Append(new CubePlaced(log.NextSeq, at, TestLattice.Straight, rot, null));
                (live, derived) = TestLattice.Place(live, derived, reg, door.Cube, door.Face,
                    TestLattice.Straight, rot);
            }

            Assert.That(log.Events.Count, Is.GreaterThan(20), "the walk should have built a real dream");

            (Lattice replayed, Derived replayedDerived) =
                EventLog.Replay(log, TestLattice.Registry(), TestLattice.Start, Rotation.R0, new RoundSettings());

            Assert.That(replayedDerived.Hash, Is.EqualTo(derived.Hash));
            Assert.That(replayed.Cubes.Count, Is.EqualTo(live.Cubes.Count));
            Assert.That(replayedDerived.ExitDepth, Is.EqualTo(derived.ExitDepth));
        }

        [Test]
        public void ReplayOfAnEmptyLogIsJustTheStartCube()
        {
            (Lattice lattice, Derived derived) = EventLog.Replay(
                new EventLog(), TestLattice.Registry(), TestLattice.Start, Rotation.R0, new RoundSettings());

            Assert.That(lattice.Cubes.Count, Is.EqualTo(1));
            Assert.That(derived.ExitDepth, Is.EqualTo(0));
        }

        [Test]
        public void ReplaySurvivesTheFileFormat()
        {
            (Lattice live, Derived derived, CubeRegistry reg) = TestLattice.Fresh();
            var log = new EventLog();

            log.Append(new CubePlaced(0, new Coord(0, 1, 0), TestLattice.Tee, Rotation.R180, null));
            (live, derived) = TestLattice.Place(live, derived, reg, live.Start, Face.North,
                TestLattice.Tee, Rotation.R180);
            log.Append(new CubeExplored(1, new Coord(0, 1, 0), 0));
            (live, derived) = TestLattice.Explore(live, derived, reg, new Coord(0, 1, 0));

            EventLog fromDisk;
            using (var ms = new MemoryStream())
            {
                log.Write(ms);
                ms.Position = 0;
                fromDisk = EventLog.Read(ms);
            }

            (Lattice _, Derived replayed) = EventLog.Replay(
                fromDisk, TestLattice.Registry(), TestLattice.Start, Rotation.R0, new RoundSettings());

            Assert.That(replayed.Hash, Is.EqualTo(derived.Hash),
                "a .lucidlog attached to a bug report must rebuild the reporter's dream exactly");
        }

        /// <summary>
        /// A fixed sequence, so a failure is always the same failure. Core
        /// forbids Random for exactly this reason.
        /// </summary>
        sealed class DeterministicWalk
        {
            uint _state = 0x9E3779B9;

            public int Next(int exclusiveMax)
            {
                unchecked
                {
                    _state ^= _state << 13;
                    _state ^= _state >> 17;
                    _state ^= _state << 5;
                }
                return (int)(_state % (uint)exclusiveMax);
            }
        }
    }
}
