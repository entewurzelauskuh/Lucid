using System;

namespace Lucid.Core
{
    /// <summary>Which faces of a cube type are connectors, unrotated.</summary>
    [Flags]
    public enum FaceMask : byte
    {
        None = 0,
        North = 1,
        East = 2,
        South = 4,
        West = 8,
        Up = 16,
        Down = 32,
    }
}
