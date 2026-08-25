using Lucid.Core;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Core
{
    /// <summary>docs/CORE-API.md §12, group "Powers".</summary>
    public sealed class PowersTests
    {
        static Powers NewPowers(int dreams = 4) =>
            new Powers(EffectSpec.Defaults, triggerCooldownMs: 6_000, dreamCount: dreams);

        [Test]
        public void AnEffectIsOnCooldownInTheDreamItLandedIn()
        {
            Powers p = NewPowers();
            var budget = new Budget(20, 0);

            Assert.That(p.ValidateEffect(EffectKind.Dark, new PowerTarget(1), budget, 0),
                Is.EqualTo(PowerError.None));
            p.ApplyEffect(EffectKind.Dark, new PowerTarget(1), budget, 0);

            Assert.That(p.ValidateEffect(EffectKind.Dark, new PowerTarget(1), budget, 0),
                Is.EqualTo(PowerError.OnCooldown));
            Assert.That(p.ValidateEffect(EffectKind.Dark, new PowerTarget(2), budget, 0),
                Is.EqualTo(PowerError.None), "cooldowns are per dream");
        }

        [Test]
        public void CooldownExpiresOnTheClock()
        {
            Powers p = NewPowers();
            var budget = new Budget(20, 0);
            p.ApplyEffect(EffectKind.Dark, new PowerTarget(0), budget, 1_000);

            int cooldown = 30_000;
            Assert.That(p.CooldownRemainingMs(EffectKind.Dark, 0, 1_000), Is.EqualTo(cooldown));
            Assert.That(p.ValidateEffect(EffectKind.Dark, new PowerTarget(0), budget, 1_000 + cooldown - 1),
                Is.EqualTo(PowerError.OnCooldown));
            Assert.That(p.ValidateEffect(EffectKind.Dark, new PowerTarget(0), budget, 1_000 + cooldown),
                Is.EqualTo(PowerError.None));
            Assert.That(p.CooldownRemainingMs(EffectKind.Dark, 0, 1_000 + cooldown), Is.Zero);
        }

        [Test]
        public void AllConsumesTheCooldownEverywhere()
        {
            // All is the efficient choice and targeting is the precise one
            // (docs/SPEC.md §10).
            Powers p = NewPowers();
            var budget = new Budget(20, 0);

            p.ApplyEffect(EffectKind.Fog, PowerTarget.All, budget, 0);

            for (int dream = 0; dream < 4; dream++)
            {
                Assert.That(p.ValidateEffect(EffectKind.Fog, new PowerTarget(dream), budget, 0),
                    Is.EqualTo(PowerError.OnCooldown), $"dream {dream} should be on cooldown");
            }
            Assert.That(p.ValidateEffect(EffectKind.Fog, PowerTarget.All, budget, 0),
                Is.EqualTo(PowerError.OnCooldown));
        }

        [Test]
        public void AllIsRefusedIfAnySingleDreamIsStillOnCooldown()
        {
            Powers p = NewPowers();
            var budget = new Budget(20, 0);
            p.ApplyEffect(EffectKind.Dark, new PowerTarget(2), budget, 0);

            Assert.That(p.ValidateEffect(EffectKind.Dark, PowerTarget.All, budget, 0),
                Is.EqualTo(PowerError.OnCooldown));
        }

        [Test]
        public void AnEffectCostsTheSameWhateverItTargets()
        {
            // Flat cost per use: All is cheaper per dream, which is the point.
            Powers p = NewPowers(dreams: 4);
            EffectSpec dark = EffectSpec.Defaults[0];

            // Enough for four separate casts, so spending per dream would leave
            // a different number behind than spending once.
            var budget = new Budget(4 * dark.Cost, 0);

            Assert.That(p.ApplyEffect(EffectKind.Dark, PowerTarget.All, budget, 0), Is.True);
            Assert.That(budget.Points, Is.EqualTo(3 * dark.Cost),
                "one flat cost for All, not one per dream");
        }

        [Test]
        public void AnEffectYouCannotAffordIsRefused()
        {
            Powers p = NewPowers();
            var broke = new Budget(0, 0);
            Assert.That(p.ValidateEffect(EffectKind.Molasses, PowerTarget.All, broke, 0),
                Is.EqualTo(PowerError.NotEnoughBudget));
        }

        [Test]
        public void ThereIsNoSuchDream()
        {
            Powers p = NewPowers(dreams: 2);
            var budget = new Budget(20, 0);

            Assert.That(p.ValidateEffect(EffectKind.Dark, new PowerTarget(5), budget, 0),
                Is.EqualTo(PowerError.NoSuchDream));
            Assert.That(p.ValidateTrigger(new Coord(0, 1, 0), new PowerTarget(5), 0),
                Is.EqualTo(PowerError.NoSuchDream));
        }

        [Test]
        public void PossessionDisablesEverythingElse()
        {
            // While possessed the maze stops growing for everyone, which is
            // what makes it a gamble (docs/SPEC.md §10).
            Powers p = NewPowers();
            var budget = new Budget(20, 0);
            p.PossessionActive = true;

            Assert.That(p.ValidateEffect(EffectKind.Dark, PowerTarget.All, budget, 0),
                Is.EqualTo(PowerError.Possessed));
            Assert.That(p.ValidateTrigger(new Coord(0, 1, 0), PowerTarget.All, 0),
                Is.EqualTo(PowerError.Possessed));

            p.PossessionActive = false;
            Assert.That(p.ValidateEffect(EffectKind.Dark, PowerTarget.All, budget, 0),
                Is.EqualTo(PowerError.None));
        }

        [Test]
        public void TriggersAreFreeButHaveTheirOwnCooldown()
        {
            Powers p = NewPowers();
            var cube = new Coord(0, 1, 0);

            // Triggers take no Budget parameter at all, which is the structural
            // form of "free" (docs/SPEC.md §10). Asserting on a budget here
            // would assert on an object nothing under test can reach.
            Assert.That(p.ValidateTrigger(cube, new PowerTarget(0), 0), Is.EqualTo(PowerError.None));
            Assert.That(p.ApplyTrigger(cube, new PowerTarget(0), 0), Is.True);

            Assert.That(p.ValidateTrigger(cube, new PowerTarget(0), 0), Is.EqualTo(PowerError.OnCooldown));
            Assert.That(p.TriggerCooldownRemainingMs(cube, 0, 0), Is.EqualTo(6_000));
            Assert.That(p.ValidateTrigger(cube, new PowerTarget(0), 6_000), Is.EqualTo(PowerError.None));
        }

        [Test]
        public void TriggerCooldownsArePerTrapAndPerDream()
        {
            Powers p = NewPowers();
            var trap = new Coord(0, 1, 0);
            var otherTrap = new Coord(0, 2, 0);

            p.ApplyTrigger(trap, new PowerTarget(0), 0);

            Assert.That(p.ValidateTrigger(trap, new PowerTarget(1), 0), Is.EqualTo(PowerError.None),
                "the same trap in another dream is untouched");
            Assert.That(p.ValidateTrigger(otherTrap, new PowerTarget(0), 0), Is.EqualTo(PowerError.None),
                "another trap in the same dream is untouched");
        }

        [Test]
        public void AnUnaffordableEffectDoesNotFireOrBurnItsCooldown()
        {
            // The host validates, then may spend budget on a placement, then
            // applies (docs/CORE-API.md §10). Apply has to re-check, or an
            // effect the Nightmare can no longer afford fires for free and
            // starts a 30 s cooldown anyway.
            Powers p = NewPowers();
            var budget = new Budget(EffectSpec.Defaults[0].Cost, 0);

            Assert.That(p.ValidateEffect(EffectKind.Dark, PowerTarget.All, budget, 0),
                Is.EqualTo(PowerError.None));

            budget.TrySpend(budget.Points);   // a placement got there first

            Assert.That(p.ApplyEffect(EffectKind.Dark, PowerTarget.All, budget, 0), Is.False);
            Assert.That(p.CooldownRemainingMs(EffectKind.Dark, 0, 0), Is.Zero,
                "nothing happened, so nothing is on cooldown");
        }

        [Test]
        public void ApplyingARefusedTriggerChangesNothing()
        {
            Powers p = NewPowers();
            var cube = new Coord(0, 1, 0);
            p.ApplyTrigger(cube, new PowerTarget(0), 0);

            Assert.That(p.ApplyTrigger(cube, new PowerTarget(0), 0), Is.False, "still on cooldown");
            Assert.That(p.TriggerCooldownRemainingMs(cube, 0, 0), Is.EqualTo(6_000),
                "the cooldown was not extended by the refused attempt");
        }

        [Test]
        public void PossessionAndAnUnknownEffectAreDifferentAnswers()
        {
            // The HUD has to tell "you are possessing" from "that effect is not
            // in this round's list".
            var p = new Powers(new[] { EffectSpec.Defaults[0] }, 6_000, 2);
            var budget = new Budget(20, 0);

            Assert.That(p.ValidateEffect(EffectKind.Fog, PowerTarget.All, budget, 0),
                Is.EqualTo(PowerError.Disabled), "Fog was not configured this round");

            p.PossessionActive = true;
            Assert.That(p.ValidateEffect(EffectKind.Dark, PowerTarget.All, budget, 0),
                Is.EqualTo(PowerError.Possessed));
        }

        [Test]
        public void AnUntriggeredTrapHasNoCooldown()
        {
            Powers p = NewPowers();
            Assert.That(p.TriggerCooldownRemainingMs(new Coord(3, 3, 0), 0, 0), Is.Zero);
        }
    }
}
