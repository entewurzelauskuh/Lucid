using System;
using Lucid.Core;
using Lucid.Runtime.UI;

namespace Lucid.Runtime
{
    /// <summary>
    /// The verbatim reason a placement was refused (docs/UI.md §8, §14), from
    /// Core's verdict. Pure, so the mapping is one table with one test.
    /// </summary>
    /// <remarks>
    /// Core has nine ways to say no and the glossary has five strings, so some
    /// share. The shares are chosen by what the Nightmare can do about it:
    /// a door that is not a door and a door that is already built on are the
    /// same "look elsewhere"; a cube outside the dream's bounds and a cube whose
    /// doors do not meet its neighbours' are the same "not here, not this way".
    /// <c>UnknownType</c> and <c>StartProtected</c> cannot reach the palette —
    /// it lists only registered types, and the start cube's door is legal to
    /// build on — so they fall to "Not a door" rather than earn a string a
    /// player would never read (`docs/DECISIONS.md`, 2026-09-08).
    /// </remarks>
    public static class PlacementCopy
    {
        /// <param name="verdict">Core's answer.</param>
        /// <param name="sleeperName">Who would be trapped, for <see cref="PlaceError.WouldTrap"/>.</param>
        /// <param name="have">Budget in hand, for <see cref="PlaceError.NotEnoughBudget"/>.</param>
        /// <param name="cost">The cube's cost, likewise.</param>
        public static string For(PlaceVerdict verdict, string sleeperName, int have, int cost)
        {
            switch (verdict.Error)
            {
                case PlaceError.None:
                    return null;
                case PlaceError.DoorIsSolid:
                    return LucidStrings.DoorIsSolid;
                case PlaceError.DoesNotFit:
                case PlaceError.OutOfBounds:
                    return LucidStrings.DoesntFit;
                case PlaceError.NotEnoughBudget:
                    return LucidStrings.NotEnoughBudget(have, cost);
                case PlaceError.WouldTrap:
                    return LucidStrings.WouldTrap(sleeperName ?? "?");
                case PlaceError.NotADoor:
                case PlaceError.DoorOccupied:
                case PlaceError.UnknownType:
                case PlaceError.StartProtected:
                    return LucidStrings.NotADoor;
                default:
                    throw new ArgumentOutOfRangeException(nameof(verdict), verdict.Error, "no copy for this error");
            }
        }
    }
}
