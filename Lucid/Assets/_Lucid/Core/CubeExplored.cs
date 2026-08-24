namespace Lucid.Core
{
    /// <summary>
    /// A Sleeper set foot in a cube for the first time. Global: the host
    /// applies it once, whichever dream reported it (docs/SPEC.md §5).
    /// </summary>
    public sealed record CubeExplored(long Seq, Coord Cube, int SleeperId) : LatticeEvent(Seq);
}
