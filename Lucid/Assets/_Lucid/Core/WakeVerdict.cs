namespace Lucid.Core
{
    /// <summary>
    /// The host's answer to a Sleeper reporting that they touched a white door.
    /// The reason travels back so the client can roll the Sleeper into the
    /// doorway rather than guess (docs/NETCODE.md §6).
    /// </summary>
    public enum WakeVerdict : byte
    {
        Woke = 0,
        NotAnExit = 1,
        NotInDream = 2,
        HeadStart = 3,
    }
}
