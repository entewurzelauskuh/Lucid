using Lucid.Runtime.UI;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Nightmare
{
    /// <summary>The two §14 readouts the god view's HUD builds from a number.</summary>
    public sealed class LucidStringsTests
    {
        [Test]
        public void TheTrickleReadsWholeSecondsOrOneDecimal()
        {
            Assert.That(LucidStrings.Trickle(4000), Is.EqualTo("1 per 4 s"));
            // A knob at 2.5 s must not read "1 per 2 s": the readout is a rule
            // (docs/UI.md §1.6), and a truncated one is wrong by a quarter.
            Assert.That(LucidStrings.Trickle(2500), Is.EqualTo("1 per 2.5 s"));
            Assert.That(LucidStrings.Trickle(1250), Is.EqualTo("1 per 1.3 s"));
        }

        [Test]
        public void ASleeperWithNoNameIsTheirSeat()
        {
            Assert.That(LucidStrings.SleeperSeat(1), Is.EqualTo("Sleeper 1"));
            Assert.That(LucidStrings.WouldTrap(LucidStrings.SleeperSeat(3)), Is.EqualTo("Would trap Sleeper 3"));
        }
    }
}
