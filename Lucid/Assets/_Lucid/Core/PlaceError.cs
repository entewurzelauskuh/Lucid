namespace Lucid.Core
{
    /// <summary>
    /// Why a placement was refused. The Nightmare sees these as the reason on
    /// a red ghost (docs/UI.md §8), so each one has to name something the
    /// player can act on.
    /// </summary>
    public enum PlaceError : byte
    {
        None = 0,
        UnknownType = 1,
        NotADoor = 2,
        DoorIsSolid = 3,
        DoorOccupied = 4,
        OutOfBounds = 5,
        DoesNotFit = 6,
        NotEnoughBudget = 7,
        WouldTrap = 8,
        StartProtected = 9,
    }
}
