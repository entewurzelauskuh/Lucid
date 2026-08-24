namespace Lucid.Core
{
    /// <summary>
    /// A kind of cube. Immutable content, registered once per round from the
    /// active Dream Packs.
    /// </summary>
    public sealed record CubeType(
        string Id,
        string Pack,
        CubeCategory Category,
        FaceMask Connectors,
        bool Climbable,
        int Cost);
}
