namespace Lucid.Core
{
    /// <summary>
    /// The four states of a connector (docs/SPEC.md §7). Fog and Exit are the
    /// only ones the Nightmare may build on; Solid is permanent.
    /// </summary>
    public enum ConnectorState : byte
    {
        Attached = 0,
        Fog = 1,
        Exit = 2,
        Solid = 3,
    }
}
