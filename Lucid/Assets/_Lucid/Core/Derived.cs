using System.Collections.Generic;

namespace Lucid.Core
{
    /// <summary>
    /// Everything the rules and the renderer need that is a pure function of
    /// the lattice. Recomputed on every change; a few hundred cubes cost well
    /// under a millisecond (docs/CORE-API.md §3).
    /// </summary>
    public sealed record Derived(
        IReadOnlyDictionary<Coord, int> Depth,
        IReadOnlyDictionary<ConnectorRef, ConnectorState> Connectors,
        IReadOnlyList<ConnectorRef> Exits,
        int ExitDepth,
        ulong Hash)
    {
        public ConnectorState StateOf(ConnectorRef door) =>
            Connectors.TryGetValue(door, out ConnectorState s) ? s : ConnectorState.Solid;

        public int DepthOf(Coord c) => Depth.TryGetValue(c, out int d) ? d : -1;

        public bool IsExit(ConnectorRef door) => StateOf(door) == ConnectorState.Exit;
    }
}
