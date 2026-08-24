namespace Lucid.Core
{
    /// <summary>
    /// Clockwise rotation seen from above: North becomes East, East becomes
    /// South, and so on. Up and Down are unaffected.
    /// </summary>
    public enum Rotation : byte
    {
        R0 = 0,
        R90 = 1,
        R180 = 2,
        R270 = 3,
    }
}
