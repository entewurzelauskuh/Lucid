using Lucid.Core;
using Lucid.Runtime;
using Lucid.Tests.EditMode.Core;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Nightmare
{
    /// <summary>
    /// docs/UI.md §12's "unlimited budget and no timer" in Core's own terms:
    /// the settings are ordinary, and the rules run as they always do.
    /// </summary>
    public sealed class SandboxSettingsTests
    {
        static Round NewRound() => new Round(LocalRound.SandboxSettings, TestLattice.Registry(),
            TestLattice.Start, Rotation.R0, new[] { new PlayerId(0) });

        [Test]
        public void ThereIsNoHeadStartAndNoDawnWorthWaitingFor()
        {
            Round r = NewRound();
            Assert.That(r.Phase, Is.EqualTo(Phase.Running), "a Sandbox with a head start");

            // A day, in one step and in many; neither reaches dawn, and the
            // clock and the budget agree with themselves either way.
            r.Advance(86_400_000);
            Assert.That(r.Phase, Is.EqualTo(Phase.Running));
            Round many = NewRound();
            for (int i = 0; i < 24; i++) many.Advance(3_600_000);
            Assert.That(many.ClockMs, Is.EqualTo(r.ClockMs));
            Assert.That(many.Budget.Points, Is.EqualTo(r.Budget.Points));
        }

        [Test]
        public void TheBudgetCannotBeSpentDownAndDoesNotTrickle()
        {
            Round r = NewRound();
            int before = r.Budget.Points;
            r.Advance(3_600_000);
            Assert.That(r.Budget.Points, Is.EqualTo(before), "the Sandbox's budget trickled");
            Assert.That(r.Budget.MsUntilNextPoint, Is.EqualTo(0));

            // A thousand cubes at the dearest cost in the spec's kit is nothing to it.
            Assert.That(r.Budget.CanAfford(1000 * 4), Is.True);
            Assert.That(before - 1000 * 4, Is.GreaterThan(1_000_000), "the budget is merely large, not unlimited");
        }

        [Test]
        public void TheRulesStillRun()
        {
            // Unlimited money buys no exemption from fit: the test corner is
            // North-East, so at R0 nothing faces the bedroom's door.
            Round r = NewRound();
            PlaceVerdict verdict = r.TryPlace(new PlaceRequest(
                new ConnectorRef(new Coord(0, 0, 0), Face.North), TestLattice.Corner, Rotation.R0, "*"));
            Assert.That(verdict.Error, Is.EqualTo(PlaceError.DoesNotFit));
            Assert.That(r.Lattice.Cubes.Count, Is.EqualTo(1));
        }
    }
}
