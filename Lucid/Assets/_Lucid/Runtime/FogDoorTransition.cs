namespace Lucid.Runtime
{
    /// <summary>
    /// What a door does when its connector state changes (docs/SPEC.md §7).
    /// </summary>
    public enum FogDoorTransition
    {
        /// <summary>The state did not change.</summary>
        None = 0,

        /// <summary>Fog → Exit. The light found this door; the exit moved here.</summary>
        Kindle = 1,

        /// <summary>Exit → Fog. The exit moved somewhere deeper.</summary>
        Dim = 2,

        /// <summary>Fog or Exit → Attached. The Nightmare built here; the mist clears.</summary>
        Dissolve = 3,

        /// <summary>Fog → Solid. A Sleeper explored the cube; the mist hardens into wall.</summary>
        Condense = 4,

        /// <summary>
        /// A change docs/SPEC.md §7 does not allow. Never played; it means the
        /// state pushed in did not come from a lawful derivation.
        /// </summary>
        Forbidden = 5,
    }
}
