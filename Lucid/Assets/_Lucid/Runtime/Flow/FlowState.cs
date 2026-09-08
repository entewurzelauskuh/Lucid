namespace Lucid.Runtime
{
    /// <summary>
    /// Where the game is, at the level of screens (docs/UI.md §2). M0 has the
    /// offline subset; M0.9 adds the lobby, round and results as more states.
    /// </summary>
    public enum FlowState
    {
        /// <summary>The persistent scene is up and nothing else is.</summary>
        Boot = 0,
        Title = 1,
        Sandbox = 2,
    }
}
