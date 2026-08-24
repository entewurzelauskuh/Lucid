namespace Lucid.Core
{
    /// <summary>What a cube type is for (docs/SPEC.md §8).</summary>
    public enum CubeCategory : byte
    {
        Connector = 0,
        Vertical = 1,
        Chicane = 2,
        Mob = 3,
        Gimmick = 4,
        Start = 5,
    }
}
