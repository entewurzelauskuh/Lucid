using Lucid.Core;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Core
{
    /// <summary>docs/CORE-API.md §12, group "Budget".</summary>
    public sealed class BudgetTests
    {
        [Test]
        public void TrickleKeepsItsRemainder()
        {
            // The heart of it: a round advanced in many small steps must grant
            // exactly as many points as one advanced in a single large step, or
            // the Nightmare's economy depends on the host's frame rate.
            var coarse = new Budget(0, 4_000);
            coarse.Advance(10_000);

            var fine = new Budget(0, 4_000);
            for (int i = 0; i < 1_000; i++) fine.Advance(10);   // the same 10 s

            Assert.That(coarse.Points, Is.EqualTo(2));
            Assert.That(fine.Points, Is.EqualTo(coarse.Points), "remainders must not be lost");
            Assert.That(fine.MsUntilNextPoint, Is.EqualTo(coarse.MsUntilNextPoint));
        }

        [Test]
        public void AwkwardStepSizesStillAccumulateExactly()
        {
            var b = new Budget(0, 4_000);
            for (int i = 0; i < 3_000; i++) b.Advance(7);   // 21 s in steps that never align

            Assert.That(b.Points, Is.EqualTo(5), "21000 / 4000 = 5 whole points");
            Assert.That(b.MsUntilNextPoint, Is.EqualTo(4_000 - (21_000 - 20_000)));
        }

        [Test]
        public void ManyPointsCanArriveInOneStep()
        {
            var b = new Budget(0, 1_000);
            b.Advance(5_500);
            Assert.That(b.Points, Is.EqualTo(5));
            Assert.That(b.MsUntilNextPoint, Is.EqualTo(500));
        }

        [Test]
        public void ZeroIntervalMeansNoTrickle()
        {
            var b = new Budget(7, 0);
            b.Advance(1_000_000);
            Assert.That(b.Points, Is.EqualTo(7));
            Assert.That(b.MsUntilNextPoint, Is.EqualTo(0));
        }

        [Test]
        public void YouCannotOverspend()
        {
            var b = new Budget(3, 0);

            Assert.That(b.CanAfford(4), Is.False);
            Assert.That(b.TrySpend(4), Is.False);
            Assert.That(b.Points, Is.EqualTo(3), "a refused spend takes nothing");

            Assert.That(b.TrySpend(3), Is.True);
            Assert.That(b.Points, Is.EqualTo(0));
            Assert.That(b.TrySpend(1), Is.False);
        }

        [Test]
        public void SpendingIsExactAtTheBoundary()
        {
            var b = new Budget(5, 0);
            Assert.That(b.CanAfford(5), Is.True, "affording means cost <= points");
            Assert.That(b.CanAfford(0), Is.True);
            Assert.That(b.TrySpend(0), Is.True);
            Assert.That(b.Points, Is.EqualTo(5));
        }

        [Test]
        public void NegativeInputsAreRefused()
        {
            Assert.That(() => new Budget(-1, 0), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => new Budget(0, -1), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => new Budget(1, 0).Advance(-1), Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(() => new Budget(1, 0).TrySpend(-1), Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }
    }
}
