namespace Lucid.Core
{
    /// <summary>
    /// The six faces of a cube. North is +Y, East +X, South -Y, West -X,
    /// Up +Z, Down -Z (docs/CORE-API.md §1). Order matters: <see cref="Faces.Of"/>
    /// enumerates in this order, and the derived-state hash depends on it.
    /// </summary>
    public enum Face : byte
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3,
        Up = 4,
        Down = 5,
    }
}
