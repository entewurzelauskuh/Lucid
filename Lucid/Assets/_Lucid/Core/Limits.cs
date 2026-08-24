namespace Lucid.Core
{
    /// <summary>How far the dream may spread. Defaults from docs/CORE-API.md §2.</summary>
    public sealed record Limits(int FootprintHalf = 12, int LayerMin = -3, int LayerMax = 3)
    {
        public static readonly Limits Default = new Limits();

        public bool Contains(Coord c) =>
            c.X >= -FootprintHalf && c.X <= FootprintHalf &&
            c.Y >= -FootprintHalf && c.Y <= FootprintHalf &&
            c.Z >= LayerMin && c.Z <= LayerMax;
    }
}
