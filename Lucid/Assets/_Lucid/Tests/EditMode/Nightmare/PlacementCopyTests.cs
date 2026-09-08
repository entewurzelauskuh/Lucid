using System;
using System.Linq;
using Lucid.Core;
using Lucid.Runtime;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Nightmare
{
    /// <summary>
    /// M0.7's acceptance: every rejection reason renders. Every value of
    /// <see cref="PlaceError"/> has to come out as one of docs/UI.md §14's
    /// placement strings, verbatim.
    /// </summary>
    public sealed class PlacementCopyTests
    {
        static readonly string[] Glossary =
        {
            "Door is solid", "Doesn't fit here", "Not a door",
        };

        [Test]
        public void EveryErrorHasAString()
        {
            foreach (PlaceError error in Enum.GetValues(typeof(PlaceError)).Cast<PlaceError>())
            {
                if (error == PlaceError.None) continue;
                string copy = PlacementCopy.For(new PlaceVerdict(error, 0), "Anna", 3, 4);
                Assert.That(copy, Is.Not.Null.And.Not.Empty, error.ToString());
            }
        }

        [Test]
        public void APassingVerdictHasNothingToSay()
        {
            Assert.That(PlacementCopy.For(PlaceVerdict.Pass, "Anna", 12, 1), Is.Null);
        }

        [Test]
        public void TheStringsAreTheGlossarysOwn()
        {
            // docs/UI.md §14, Placement, verbatim — including the two that carry
            // a value. A reworded reason is a bug even when it means the same.
            Assert.That(PlacementCopy.For(new PlaceVerdict(PlaceError.DoorIsSolid), null, 0, 0), Is.EqualTo("Door is solid"));
            Assert.That(PlacementCopy.For(new PlaceVerdict(PlaceError.DoesNotFit), null, 0, 0), Is.EqualTo("Doesn't fit here"));
            Assert.That(PlacementCopy.For(new PlaceVerdict(PlaceError.NotADoor), null, 0, 0), Is.EqualTo("Not a door"));
            Assert.That(PlacementCopy.For(new PlaceVerdict(PlaceError.WouldTrap, 2), "Ben", 0, 0), Is.EqualTo("Would trap Ben"));
            Assert.That(PlacementCopy.For(new PlaceVerdict(PlaceError.NotEnoughBudget), null, 3, 4), Is.EqualTo("Not enough budget (3 / 4)"));
        }

        [Test]
        public void TheSharedStringsGoWhereThePlayerCanActOnThem()
        {
            // Out of bounds reads as "doesn't fit": not here, not this way.
            // Occupied, unknown and start-protected read as "not a door": look
            // elsewhere. Recorded in docs/DECISIONS.md.
            Assert.That(PlacementCopy.For(new PlaceVerdict(PlaceError.OutOfBounds), null, 0, 0), Is.EqualTo("Doesn't fit here"));
            foreach (PlaceError e in new[] { PlaceError.DoorOccupied, PlaceError.UnknownType, PlaceError.StartProtected })
                Assert.That(PlacementCopy.For(new PlaceVerdict(e), null, 0, 0), Is.EqualTo("Not a door"), e.ToString());
        }

        [Test]
        public void EveryStringIsOneOfTheGlossarysOrCarriesAValue()
        {
            foreach (PlaceError error in Enum.GetValues(typeof(PlaceError)).Cast<PlaceError>())
            {
                if (error == PlaceError.None) continue;
                string copy = PlacementCopy.For(new PlaceVerdict(error, 0), "Anna", 3, 4);
                bool fixedString = Glossary.Contains(copy);
                bool valued = copy == "Would trap Anna" || copy == "Not enough budget (3 / 4)";
                Assert.That(fixedString || valued, Is.True, $"{error} -> '{copy}' is not a §14 string");
            }
        }
    }
}
