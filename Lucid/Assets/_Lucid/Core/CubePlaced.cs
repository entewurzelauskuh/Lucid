namespace Lucid.Core
{
    /// <summary>
    /// A cube appeared at <paramref name="Cube"/>. The coord rather than the
    /// door it was built on, so replay never has to re-derive the target.
    /// </summary>
    public sealed record CubePlaced(
        long Seq, Coord Cube, string TypeId, Rotation Rotation, string SkinId) : LatticeEvent(Seq);
}
