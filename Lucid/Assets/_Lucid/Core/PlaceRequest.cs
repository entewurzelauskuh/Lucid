namespace Lucid.Core
{
    /// <summary>The Nightmare asking to build on a door.</summary>
    public sealed record PlaceRequest(
        ConnectorRef Target,
        string TypeId,
        Rotation Rotation,
        string SkinId);
}
