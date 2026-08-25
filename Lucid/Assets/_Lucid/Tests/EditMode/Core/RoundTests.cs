using System.Collections.Generic;
using Lucid.Core;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Core
{
    /// <summary>docs/CORE-API.md §12, group "Round".</summary>
    public sealed class RoundTests
    {
        static Round NewRound(RoundSettings s = null, int sleepers = 1)
        {
            var players = new List<PlayerId>();
            for (int i = 0; i < sleepers; i++) players.Add(new PlayerId(100 + i));
            return new Round(s ?? new RoundSettings(), TestLattice.Registry(),
                TestLattice.Start, Rotation.R0, players);
        }

        /// <summary>Advance past the head start so Sleepers can wake.</summary>
        static Round Running(RoundSettings s = null, int sleepers = 1)
        {
            Round r = NewRound(s, sleepers);
            r.Advance(r.Settings.HeadStartMs);
            return r;
        }

        [Test]
        public void ARoundStartsInTheHeadStartWithEverybodyInTheirBedroom()
        {
            Round r = NewRound(sleepers: 2);

            Assert.That(r.Phase, Is.EqualTo(Phase.HeadStart));
            Assert.That(r.ClockMs, Is.Zero);
            Assert.That(r.IsOver, Is.False);
            Assert.That(r.Sleepers.Count, Is.EqualTo(2));
            foreach (SleeperState s in r.Sleepers)
            {
                Assert.That(s.Status, Is.EqualTo(SleeperStatus.InDream));
                Assert.That(s.Cube, Is.EqualTo(r.Lattice.Start));
                Assert.That(s.Lives, Is.EqualTo(r.Settings.Lives));
            }
        }

        [Test]
        public void TheMistDropsWhenTheHeadStartEnds()
        {
            Round r = NewRound();
            r.Advance(r.Settings.HeadStartMs - 1);
            Assert.That(r.Phase, Is.EqualTo(Phase.HeadStart));

            r.Advance(1);
            Assert.That(r.Phase, Is.EqualTo(Phase.Running));
        }

        [Test]
        public void TheHeadStartBlocksWaking()
        {
            Round r = NewRound();
            var startDoor = new ConnectorRef(r.Lattice.Start, Face.North);
            Assert.That(r.Derived.StateOf(startDoor), Is.EqualTo(ConnectorState.Exit));

            Assert.That(r.TryWake(0, startDoor), Is.EqualTo(WakeVerdict.HeadStart),
                "the start door is misted until the countdown ends");
            Assert.That(r.Sleepers[0].Status, Is.EqualTo(SleeperStatus.InDream));
        }

        [Test]
        public void TouchingAWhiteDoorWakesYou()
        {
            Round r = Running();
            var startDoor = new ConnectorRef(r.Lattice.Start, Face.North);

            Assert.That(r.TryWake(0, startDoor), Is.EqualTo(WakeVerdict.Woke));
            Assert.That(r.Sleepers[0].Status, Is.EqualTo(SleeperStatus.Awake));
            Assert.That(r.Sleepers[0].WokeAtMs, Is.EqualTo(r.ClockMs));
            Assert.That(r.IsOver, Is.True, "the last Sleeper woke");
        }

        [Test]
        public void ADoorThatIsNoLongerAnExitDoesNotWakeYou()
        {
            // A placement beat the report here (docs/SPEC.md §14).
            Round r = Running();
            var startDoor = new ConnectorRef(r.Lattice.Start, Face.North);
            r.TryPlace(new PlaceRequest(startDoor, TestLattice.Straight, Rotation.R0, null));

            Assert.That(r.TryWake(0, startDoor), Is.EqualTo(WakeVerdict.NotAnExit));
            Assert.That(r.Sleepers[0].Status, Is.EqualTo(SleeperStatus.InDream));
        }

        [Test]
        public void AnUnknownOrAlreadyWokenSleeperCannotWake()
        {
            Round r = Running();
            var startDoor = new ConnectorRef(r.Lattice.Start, Face.North);

            Assert.That(r.TryWake(7, startDoor), Is.EqualTo(WakeVerdict.NotInDream));
            Assert.That(r.TryWake(0, startDoor), Is.EqualTo(WakeVerdict.Woke));
            Assert.That(r.TryWake(0, startDoor), Is.EqualTo(WakeVerdict.NotInDream), "already awake");
        }

        [Test]
        public void LivesCountDownAndTheLastOneConsumes()
        {
            Round r = Running(new RoundSettings(Lives: 2));

            // Walk the Sleeper away from the start cube first, or "respawned at
            // Start" is true before the death and the assertion proves nothing.
            r.TryPlace(new PlaceRequest(new ConnectorRef(r.Lattice.Start, Face.North),
                TestLattice.Straight, Rotation.R0, null));
            r.UpdateSleeperCube(0, new Coord(0, 1, 0));
            Assert.That(r.Sleepers[0].Cube, Is.EqualTo(new Coord(0, 1, 0)), "fixture precondition");

            Assert.That(r.ReportDeath(0), Is.EqualTo(new DeathVerdict(DeathOutcome.LostLife, 1)));
            Assert.That(r.Sleepers[0].Status, Is.EqualTo(SleeperStatus.InDream));
            Assert.That(r.Sleepers[0].Cube, Is.EqualTo(r.Lattice.Start), "respawn is in the bedroom");

            Assert.That(r.ReportDeath(0), Is.EqualTo(new DeathVerdict(DeathOutcome.Consumed, 0)));
            Assert.That(r.Sleepers[0].Status, Is.EqualTo(SleeperStatus.Consumed));
            Assert.That(r.IsOver, Is.True);

            Assert.That(r.ReportDeath(0).Outcome, Is.EqualTo(DeathOutcome.Ignored),
                "a death arriving after the consume is ordinary on a 10 Hz link");
        }

        [Test]
        public void DawnConsumesWhoeverIsLeft()
        {
            Round r = Running(sleepers: 2);
            r.TryWake(0, new ConnectorRef(r.Lattice.Start, Face.North));

            r.Advance(r.Settings.RoundLengthMs);

            Assert.That(r.Phase, Is.EqualTo(Phase.Dawn));
            Assert.That(r.Sleepers[0].Status, Is.EqualTo(SleeperStatus.Awake), "waking already happened");
            Assert.That(r.Sleepers[1].Status, Is.EqualTo(SleeperStatus.Consumed));
            Assert.That(r.IsOver, Is.True);
        }

        [Test]
        public void TheClockStopsAtDawn()
        {
            Round r = Running();
            r.Advance(r.Settings.RoundLengthMs);
            int atDawn = r.ClockMs;
            int points = r.Budget.Points;

            r.Advance(60_000);

            Assert.That(r.ClockMs, Is.EqualTo(atDawn));
            Assert.That(r.Budget.Points, Is.EqualTo(points), "the trickle stops with the round");
        }

        [Test]
        public void BothEndingsAreReachable()
        {
            Round woke = Running();
            woke.TryWake(0, new ConnectorRef(woke.Lattice.Start, Face.North));
            Assert.That(woke.IsOver, Is.True);
            Assert.That(woke.Sleepers[0].Status, Is.EqualTo(SleeperStatus.Awake));

            Round eaten = Running();
            eaten.Advance(eaten.Settings.RoundLengthMs);
            Assert.That(eaten.IsOver, Is.True);
            Assert.That(eaten.Sleepers[0].Status, Is.EqualTo(SleeperStatus.Consumed));
        }

        [Test]
        public void APlacementSpendsBudgetAndAppendsToTheLog()
        {
            Round r = Running();
            int before = r.Budget.Points;
            int cost = r.Registry.Get(TestLattice.Straight).Cost;

            PlaceVerdict v = r.TryPlace(new PlaceRequest(
                new ConnectorRef(r.Lattice.Start, Face.North), TestLattice.Straight, Rotation.R0, null));

            Assert.That(v.Ok, Is.True, $"expected a legal placement, got {v}");
            Assert.That(r.Budget.Points, Is.EqualTo(before - cost));
            Assert.That(r.Log.Events.Count, Is.EqualTo(1));
            Assert.That(r.Log.Events[0], Is.TypeOf<CubePlaced>());
            Assert.That(((CubePlaced)r.Log.Events[0]).Cube, Is.EqualTo(new Coord(0, 1, 0)));
        }

        [Test]
        public void ARefusedPlacementCostsNothingAndLogsNothing()
        {
            Round r = Running();
            int before = r.Budget.Points;

            PlaceVerdict v = r.TryPlace(new PlaceRequest(
                new ConnectorRef(r.Lattice.Start, Face.East), TestLattice.Straight, Rotation.R0, null));

            Assert.That(v.Error, Is.EqualTo(PlaceError.NotADoor));
            Assert.That(r.Budget.Points, Is.EqualTo(before));
            Assert.That(r.Log.Events, Is.Empty);
        }

        [Test]
        public void ExplorationIsLoggedOnceAndOnlyOnce()
        {
            Round r = Running(sleepers: 2);
            r.TryPlace(new PlaceRequest(
                new ConnectorRef(r.Lattice.Start, Face.North), TestLattice.Straight, Rotation.R0, null));

            Assert.That(r.TryExplore(0, new Coord(0, 1, 0)), Is.EqualTo(ExploreError.None));
            Assert.That(r.Log.Events.Count, Is.EqualTo(2));

            // A second dream reports the same cube; the host drops it silently.
            Assert.That(r.TryExplore(1, new Coord(0, 1, 0)), Is.EqualTo(ExploreError.AlreadyExplored));
            Assert.That(r.Log.Events.Count, Is.EqualTo(2), "nothing was appended");
        }

        [Test]
        public void ReplayingTheRoundsOwnLogReproducesItsHash()
        {
            // The round is the host, and this is the property the netcode's
            // per-event HashReport check rests on (docs/NETCODE.md §5).
            Round r = Running();
            Assert.That(r.TryPlace(new PlaceRequest(new ConnectorRef(r.Lattice.Start, Face.North),
                TestLattice.Tee, Rotation.R0, null)).Ok, Is.True);
            Assert.That(r.TryPlace(new PlaceRequest(new ConnectorRef(new Coord(0, 1, 0), Face.East),
                TestLattice.Straight, Rotation.R90, null)).Ok, Is.True);
            Assert.That(r.TryExplore(0, new Coord(0, 1, 0)), Is.EqualTo(ExploreError.None));

            // Without this the test passes on an empty log against an untouched
            // lattice, which is exactly the case it exists to rule out.
            Assert.That(r.Log.Events.Count, Is.EqualTo(3));

            (Lattice _, Derived replayed) = EventLog.Replay(
                r.Log, TestLattice.Registry(), TestLattice.Start, Rotation.R0, r.Settings);

            Assert.That(replayed.Hash, Is.EqualTo(r.Derived.Hash));
        }

        [Test]
        public void TelemetryFromOffTheLatticeIsDropped()
        {
            // Regression for #38: an off-lattice coord made the leak rule
            // report that Sleeper as trapped, blocking every placement for the
            // rest of the round.
            Round r = Running();
            Coord before = r.Sleepers[0].Cube;

            r.UpdateSleeperCube(0, new Coord(99, 99, 0));
            Assert.That(r.Sleepers[0].Cube, Is.EqualTo(before), "a coord with no cube is not stored");

            PlaceVerdict v = r.TryPlace(new PlaceRequest(
                new ConnectorRef(r.Lattice.Start, Face.North), TestLattice.Straight, Rotation.R0, null));
            Assert.That(v.Ok, Is.True, $"the Nightmare can still build, got {v}");
        }

        [Test]
        public void TelemetryTracksASleeperWhoIsActuallyThere()
        {
            Round r = Running();
            r.TryPlace(new PlaceRequest(new ConnectorRef(r.Lattice.Start, Face.North),
                TestLattice.Straight, Rotation.R0, null));

            r.UpdateSleeperCube(0, new Coord(0, 1, 0));
            Assert.That(r.Sleepers[0].Cube, Is.EqualTo(new Coord(0, 1, 0)));
        }

        [Test]
        public void DisconnectAndReturnRestoreTheSleeper()
        {
            Round r = Running(sleepers: 2);

            r.ReportDisconnect(0, reconnected: false);
            Assert.That(r.Sleepers[0].Status, Is.EqualTo(SleeperStatus.Disconnected));
            Assert.That(r.IsOver, Is.False, "the other Sleeper is still running");

            r.ReportDisconnect(0, reconnected: true);
            Assert.That(r.Sleepers[0].Status, Is.EqualTo(SleeperStatus.InDream));
        }

        [Test]
        public void ADisconnectedSleeperKeepsTheRoundOpenUntilDawn()
        {
            // docs/CORE-API.md §8: over when every Sleeper is Awake or Consumed.
            // Disconnected is neither — the player is inside their reconnect
            // grace (docs/NETCODE.md §10), so ending here would end the round on
            // a network blip and then un-end it on their return.
            Round r = Running();
            r.ReportDisconnect(0, reconnected: false);

            Assert.That(r.IsOver, Is.False, "they may still come back");

            r.ReportDisconnect(0, reconnected: true);
            Assert.That(r.IsOver, Is.False);
            Assert.That(r.Sleepers[0].Status, Is.EqualTo(SleeperStatus.InDream));
        }

        [Test]
        public void DawnTakesASleeperWhoNeverCameBack()
        {
            // Otherwise pulling the cable is strictly better than being eaten:
            // the Sleeper survives dawn and the Nightmare is denied the points.
            Round r = Running();
            r.ReportDisconnect(0, reconnected: false);
            r.Advance(r.Settings.RoundLengthMs);

            Assert.That(r.Sleepers[0].Status, Is.EqualTo(SleeperStatus.Consumed));
            Assert.That(Scoring.Compute(r, new PlayerId(0))[new PlayerId(0)],
                Is.EqualTo(Scoring.PerConsumedSleeper), "the Nightmare is still fed");
        }

        [Test]
        public void NobodyWakesAfterDawn()
        {
            Round r = Running();
            r.Advance(r.Settings.RoundLengthMs);

            // What actually closes waking is dawn's consume, not the phase
            // check in TryWake: once consumed, the status check refuses first.
            // Assert the mechanism, so this does not read as coverage of a
            // guard it cannot reach.
            Assert.That(r.Sleepers[0].Status, Is.EqualTo(SleeperStatus.Consumed));
            Assert.That(r.TryWake(0, new ConnectorRef(r.Lattice.Start, Face.North)),
                Is.EqualTo(WakeVerdict.NotInDream), "the round is over");
        }

        [Test]
        public void TheMazeStopsGrowingAtDawn()
        {
            // Every client re-derives from the broadcast log, so a placement
            // accepted now would grow the maze on the results screen.
            Round r = Running();
            r.Advance(r.Settings.RoundLengthMs);
            int events = r.Log.Events.Count;

            PlaceVerdict v = r.TryPlace(new PlaceRequest(
                new ConnectorRef(r.Lattice.Start, Face.North), TestLattice.Straight, Rotation.R0, null));

            Assert.That(v.Ok, Is.False);
            Assert.That(r.Log.Events.Count, Is.EqualTo(events));
            Assert.That(r.TryExplore(0, new Coord(0, 1, 0)), Is.Not.EqualTo(ExploreError.None));
        }

        [Test]
        public void TheClockAndBudgetStopExactlyAtDawn()
        {
            // A single huge step and a million small ones must reach the same
            // total, or the Nightmare's economy depends on the host's tick size.
            var settings = new RoundSettings();

            Round coarse = Running(settings);
            coarse.Advance(1_000_000);

            Round fine = Running(settings);
            while (!fine.IsOver) fine.Advance(100);

            Assert.That(coarse.ClockMs, Is.EqualTo(settings.RoundLengthMs), "the clock stops at dawn");
            Assert.That(fine.ClockMs, Is.EqualTo(settings.RoundLengthMs));
            Assert.That(coarse.Budget.Points, Is.EqualTo(fine.Budget.Points),
                "and so does the trickle");
        }

        [Test]
        public void AZeroLengthHeadStartIsAlreadyRunning()
        {
            var r = new Round(new RoundSettings(HeadStartMs: 0), TestLattice.Registry(),
                TestLattice.Start, Rotation.R0, new[] { new PlayerId(1) });

            Assert.That(r.Phase, Is.EqualTo(Phase.Running), "phase follows the clock");
            Assert.That(r.TryWake(0, new ConnectorRef(r.Lattice.Start, Face.North)),
                Is.EqualTo(WakeVerdict.Woke));
        }

        [Test]
        public void OnlyALiveSleeperCanReportExploration()
        {
            // The reporter's id goes into the log and out to every client, and a
            // late packet from someone already awake would still seal doors.
            Round r = Running(sleepers: 2);

            // Wake Sleeper 1 while the start door is still an exit; building on
            // it first would turn it Attached and the wake would not take.
            Assert.That(r.TryWake(1, new ConnectorRef(r.Lattice.Start, Face.North)),
                Is.EqualTo(WakeVerdict.Woke), "fixture precondition");

            r.TryPlace(new PlaceRequest(new ConnectorRef(r.Lattice.Start, Face.North),
                TestLattice.Straight, Rotation.R0, null));

            Assert.That(r.TryExplore(-5, new Coord(0, 1, 0)), Is.Not.EqualTo(ExploreError.None));
            Assert.That(r.TryExplore(9, new Coord(0, 1, 0)), Is.Not.EqualTo(ExploreError.None));
            Assert.That(r.TryExplore(1, new Coord(0, 1, 0)), Is.Not.EqualTo(ExploreError.None),
                "an awake Sleeper's late packet must not solidify doors");

            Assert.That(r.Log.Events.Count, Is.EqualTo(1), "only the placement was logged");
            Assert.That(r.TryExplore(0, new Coord(0, 1, 0)), Is.EqualTo(ExploreError.None));
        }
    }
}
