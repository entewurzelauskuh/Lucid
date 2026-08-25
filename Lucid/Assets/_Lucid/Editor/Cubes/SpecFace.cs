namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// The schema's `face` enum. Lower case on the wire; mapped to
    /// <see cref="Lucid.Core.Face"/> by <see cref="CubeSpecMapping"/>.
    /// </summary>
    public enum SpecFace
    {
        North,
        East,
        South,
        West,
        Up,
        Down,
    }
}
