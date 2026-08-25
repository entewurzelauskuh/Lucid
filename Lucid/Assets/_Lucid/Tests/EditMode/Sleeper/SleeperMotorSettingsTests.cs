using Lucid.Runtime;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Sleeper
{
    /// <summary>
    /// The kit of SPEC §9 and the arithmetic that turns it into a gravity.
    /// These are pure numbers, so they are checked here rather than in
    /// PlayMode: if they are wrong, no amount of controller tuning saves the
    /// chicane guideline.
    /// </summary>
    public sealed class SleeperMotorSettingsTests
    {
        static SleeperMotorSettings Kit => new SleeperMotorSettings();

        [Test]
        public void TheDefaultsAreTheSpecDefaults()
        {
            var kit = Kit;
            Assert.That(kit.RunSpeed, Is.EqualTo(6f));
            Assert.That(kit.CrouchSpeed, Is.EqualTo(2.5f));
            Assert.That(kit.JumpRise, Is.EqualTo(1.2f));
            Assert.That(kit.CrouchHeight, Is.EqualTo(1f));
        }

        [Test]
        public void GravityIsWhatMakesTheRiseAndTheReachBothCorrect()
        {
            var kit = Kit;

            // Fly the parabola the derived numbers describe and check it lands
            // where the kit says it should.
            float apex = kit.JumpSpeed * kit.JumpSpeed / (2f * kit.Gravity);
            float airtime = 2f * kit.JumpSpeed / kit.Gravity;

            Assert.That(apex, Is.EqualTo(kit.JumpRise).Within(1e-4f));
            Assert.That(airtime * kit.RunSpeed, Is.EqualTo(kit.JumpTravel).Within(1e-4f));
        }

        [Test]
        public void TheDreamIsHeavierThanEarth()
        {
            // Not a preference: a 1.2 m rise under 9.81 m/s² hangs for most of
            // a second and carries nearly 6 m, half again the reach SPEC §9
            // allows. The two spec numbers only coexist at about three g, and
            // that is a thing the M0 play-test should feel for
            // (docs/DECISIONS.md).
            Assert.That(Kit.Gravity, Is.GreaterThan(2f * 9.81f));
        }

        [Test]
        public void TheCentreAloneCrossesTheWidestAllowedGap()
        {
            var kit = Kit;

            // The rounded foot wins a little purchase at each lip, so the gap
            // actually crossed is longer than the centre's flight — but only a
            // little, and how much is a fact about PhysX rather than about the
            // kit. So the arithmetic is held to the version that owes the
            // capsule nothing: the centre must clear 3.5 m on its own, and even
            // half a metre of slop must not reach 4.5 m. Where the boundary
            // really falls is measured, not derived: see GauntletTests.
            Assert.That(kit.JumpTravel, Is.GreaterThan(3.5f),
                "a 3.5 m gap must be crossable without help from the capsule (SPEC §9)");
            Assert.That(kit.JumpTravel + 0.5f, Is.LessThan(4.5f),
                "no amount of capsule slop may reach the gap the acceptance fails");
        }

        [Test]
        public void TheJumpStraddlesTheLedgeLimits()
        {
            var kit = Kit;
            Assert.That(kit.JumpRise, Is.GreaterThan(1.1f + kit.SkinWidth),
                "ledges up to 1.1 m are promised clearable (SPEC §9)");
            Assert.That(kit.JumpRise, Is.LessThan(1.4f),
                "a 1.4 m ledge must stay out of reach (M0.4 acceptance)");
        }

        [Test]
        public void TheCrouchedBodyFitsTheSmallestCrawlSpace()
        {
            var kit = Kit;
            Assert.That(kit.CrouchHeight + kit.SkinWidth, Is.LessThanOrEqualTo(1.1f),
                "crawl spaces are guaranteed passable from 1.1 m up (SPEC §9)");
        }

        [Test]
        public void TheStepOffsetDoesNotExtendTheJump()
        {
            var kit = Kit;

            // SleeperMotor's no-mantle rule already stops a step from being
            // taken in mid-air. This is the second line: even if that rule were
            // lost, a jump plus a step must not reach the 1.4 m ledge the
            // acceptance requires to fail. The usual 0.3 m does reach it.
            Assert.That(kit.JumpRise + kit.StepOffset, Is.LessThan(1.4f),
                "a 1.4 m ledge must stay out of reach of a jump plus a step");

            // And it must not turn the shortest ledge into a walk.
            Assert.That(kit.StepOffset, Is.LessThan(1f));
        }
    }
}
