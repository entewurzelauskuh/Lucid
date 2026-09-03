using Lucid.Core;

namespace Lucid.Runtime
{
    /// <summary>
    /// The transition table of docs/SPEC.md §7, as a pure function of the two
    /// states.
    /// </summary>
    /// <remarks>
    /// Written as a table rather than inferred from the states because §7's
    /// list is not symmetric and the asymmetries are load-bearing:
    ///
    /// - <b>Fog ↔ Exit</b> runs both ways, as the depth ranking changes.
    /// - <b>Fog → Solid</b> runs one way only. Solid is permanent: "the
    ///   Nightmare has lost those connectors for good".
    /// - <b>Exit never becomes Solid.</b> This is the one that matters. A
    ///   Sleeper's own exploration solidifies the fog doors of the cube they
    ///   enter, and without this rule a Sleeper could seal the way out by
    ///   walking into the cube that holds it.
    /// - <b>Attached</b> is terminal too. A cube occupies the doorway; there is
    ///   no mist left to change.
    /// </remarks>
    public static class FogDoorTransitions
    {
        public static FogDoorTransition For(ConnectorState from, ConnectorState to)
        {
            if (from == to) return FogDoorTransition.None;

            switch (from)
            {
                case ConnectorState.Fog:
                    switch (to)
                    {
                        case ConnectorState.Exit: return FogDoorTransition.Kindle;
                        case ConnectorState.Attached: return FogDoorTransition.Dissolve;
                        case ConnectorState.Solid: return FogDoorTransition.Condense;
                    }
                    break;

                case ConnectorState.Exit:
                    switch (to)
                    {
                        case ConnectorState.Fog: return FogDoorTransition.Dim;
                        case ConnectorState.Attached: return FogDoorTransition.Dissolve;
                        // Exit → Solid is refused, not merely unlisted.
                    }
                    break;

                // Attached and Solid are terminal.
            }

            return FogDoorTransition.Forbidden;
        }

        /// <summary>Whether a Sleeper can walk through a door in this state.</summary>
        public static bool IsPassable(ConnectorState state) =>
            state == ConnectorState.Attached || state == ConnectorState.Exit;

        /// <summary>Whether walking through this door wakes a Sleeper.</summary>
        public static bool Wakes(ConnectorState state) => state == ConnectorState.Exit;
    }
}
