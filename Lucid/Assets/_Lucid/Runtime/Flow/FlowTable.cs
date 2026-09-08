using System.Collections.Generic;

namespace Lucid.Runtime
{
    /// <summary>
    /// Which screen may follow which (docs/UI.md §2), and which scene each one
    /// lives in. Pure, so an EditMode test can hold the whole table.
    /// </summary>
    /// <remarks>
    /// Scenes never load each other; this table is the only place a transition
    /// is written down, and <see cref="GameFlow"/> refuses anything not in it.
    /// Adding a state is a row here and a scene builder, nothing more.
    /// </remarks>
    public static class FlowTable
    {
        static readonly HashSet<(FlowState, FlowState)> s_Allowed = new HashSet<(FlowState, FlowState)>
        {
            (FlowState.Boot, FlowState.Title),
            (FlowState.Title, FlowState.Sandbox),
            (FlowState.Sandbox, FlowState.Title),
        };

        public static bool Allows(FlowState from, FlowState to) => s_Allowed.Contains((from, to));

        /// <summary>
        /// The scene a state lives in, loaded additively beside Boot. Null for
        /// Boot itself, which is the scene that is always there.
        /// </summary>
        public static string SceneOf(FlowState state)
        {
            switch (state)
            {
                case FlowState.Title: return "Title";
                case FlowState.Sandbox: return "Dream";
                default: return null;
            }
        }
    }
}
