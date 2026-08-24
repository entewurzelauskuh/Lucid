namespace Lucid.Core
{
    /// <summary>A cube placed in the lattice.</summary>
    public sealed record CubeInstance(
        string TypeId,
        Rotation Rotation,
        string SkinId,
        long PlacedSeq);
}
