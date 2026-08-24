namespace Lucid.Core
{
    /// <summary>
    /// An event that changes the lattice. Only <see cref="CubePlaced"/> and
    /// <see cref="CubeExplored"/> feed <see cref="Deriver"/>; round events such
    /// as SleeperWoke are the runtime's business (docs/CORE-API.md §7).
    /// </summary>
    public abstract record LatticeEvent(long Seq);
}
