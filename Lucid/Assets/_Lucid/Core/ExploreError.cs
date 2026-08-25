namespace Lucid.Core
{
    /// <summary>
    /// Why an exploration report was ignored. All three are reported so the
    /// host can drop the event quietly rather than log a fault
    /// (docs/CORE-API.md §6).
    /// </summary>
    public enum ExploreError : byte
    {
        None = 0,
        NoCube = 1,
        AlreadyExplored = 2,
        StartCube = 3,
    }
}
