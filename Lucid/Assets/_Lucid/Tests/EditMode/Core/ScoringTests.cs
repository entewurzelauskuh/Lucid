using System.Collections.Generic;
using Lucid.Core;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Core
{
    /// <summary>docs/SPEC.md §12, via docs/CORE-API.md §8.</summary>
    public sealed class ScoringTests
    {
        static readonly PlayerId Nightmare = new PlayerId(0);

        static Round Running(RoundSettings s, params int[] sleeperIds)
        {
            var players = new List<PlayerId>();
            foreach (int id in sleeperIds) players.Add(new PlayerId(id));
            var r = new Round(s, TestLattice.Registry(), TestLattice.Start, Rotation.R0, players);
            r.Advance(s.HeadStartMs);
            return r;
        }

        [Test]
        public void WakingEarlyBeatsWakingLate()
        {
            var settings = new RoundSettings(HeadStartMs: 0, RoundLengthMs: 300_000);

            Round early = Running(settings, 1);
            early.Advance(60_000);
            early.TryWake(0, new ConnectorRef(early.Lattice.Start, Face.North));

            Round late = Running(settings, 1);
            late.Advance(240_000);
            late.TryWake(0, new ConnectorRef(late.Lattice.Start, Face.North));

            int earlyScore = Scoring.Compute(early, Nightmare)[new PlayerId(1)];
            int lateScore = Scoring.Compute(late, Nightmare)[new PlayerId(1)];

            Assert.That(earlyScore, Is.EqualTo(100 + 240), "100 plus the 240 s left on the clock");
            Assert.That(lateScore, Is.EqualTo(100 + 60));
            Assert.That(earlyScore, Is.GreaterThan(lateScore), "running is rewarded over waiting");
        }

        [Test]
        public void AConsumedSleeperScoresNothingAndFeedsTheNightmare()
        {
            var settings = new RoundSettings(HeadStartMs: 0, RoundLengthMs: 300_000);
            Round r = Running(settings, 1, 2);

            r.Advance(300_000);   // dawn takes both

            IReadOnlyDictionary<PlayerId, int> scores = Scoring.Compute(r, Nightmare);
            Assert.That(scores[new PlayerId(1)], Is.Zero);
            Assert.That(scores[new PlayerId(2)], Is.Zero);
            Assert.That(scores[Nightmare], Is.EqualTo(200), "100 per consumed Sleeper");
        }

        [Test]
        public void AMixedRoundSplitsThePoints()
        {
            var settings = new RoundSettings(HeadStartMs: 0, RoundLengthMs: 300_000);
            Round r = Running(settings, 1, 2);

            r.Advance(100_000);
            r.TryWake(0, new ConnectorRef(r.Lattice.Start, Face.North));
            r.Advance(200_000);   // dawn takes the other

            IReadOnlyDictionary<PlayerId, int> scores = Scoring.Compute(r, Nightmare);
            Assert.That(scores[new PlayerId(1)], Is.EqualTo(100 + 200));
            Assert.That(scores[new PlayerId(2)], Is.Zero);
            Assert.That(scores[Nightmare], Is.EqualTo(100));
        }

        [Test]
        public void TheNightmareAppearsEvenWithNothingToEat()
        {
            var settings = new RoundSettings(HeadStartMs: 0, RoundLengthMs: 300_000);
            Round r = Running(settings, 1);
            r.TryWake(0, new ConnectorRef(r.Lattice.Start, Face.North));

            IReadOnlyDictionary<PlayerId, int> scores = Scoring.Compute(r, Nightmare);
            Assert.That(scores.ContainsKey(Nightmare), Is.True);
            Assert.That(scores[Nightmare], Is.Zero);
        }
    }
}
