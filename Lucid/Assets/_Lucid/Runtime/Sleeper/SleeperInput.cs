using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// One tick of a Sleeper's intent, already decoded from whatever device
    /// produced it. The motor consumes this and nothing else, so a test can
    /// script a run without the Input System in the loop
    /// (docs/WORKPLAN.md §4, M0.4).
    /// </summary>
    public struct SleeperInput
    {
        /// <summary>Strafe in x, forward in y, each clamped to [-1, 1].</summary>
        public Vector2 Move;

        /// <summary>Look delta for this tick, in device units.</summary>
        public Vector2 Look;

        /// <summary>True only on the tick the jump key went down.</summary>
        public bool JumpPressed;

        /// <summary>True while crouch is held (SPEC §9 has no toggle yet).</summary>
        public bool Crouch;

        public static SleeperInput Forward =>
            new SleeperInput { Move = new Vector2(0f, 1f) };
    }
}
