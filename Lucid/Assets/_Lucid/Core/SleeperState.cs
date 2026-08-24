namespace Lucid.Core
{
    /// <summary>One Sleeper as the rules see them. Id is the lobby index 0-3.</summary>
    public sealed record SleeperState(
        int Id,
        PlayerId Player,
        SleeperStatus Status,
        Coord Cube,
        int Lives);
}
