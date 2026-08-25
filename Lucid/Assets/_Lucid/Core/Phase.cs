namespace Lucid.Core
{
    /// <summary>
    /// Where the round is (docs/SPEC.md §11). The head start is the Sleepers'
    /// grace: the start door is misted and nobody can wake yet.
    /// </summary>
    public enum Phase : byte
    {
        HeadStart = 0,
        Running = 1,
        Dawn = 2,
    }
}
