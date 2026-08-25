using Lucid.Core;
using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// The mist in a doorway. Its state is derived by <c>Lucid.Core</c> and
    /// pushed in; the door never decides anything for itself.
    /// </summary>
    /// <remarks>
    /// M0.3 needs the component to exist and to carry state so the builder can
    /// place it and the validator can find it. The shader, the animated
    /// transitions and the wake trigger are M0.5 (#5); until then
    /// <see cref="SetState"/> only records what it was told.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class FogDoor : MonoBehaviour
    {
        [SerializeField] Face _face;
        [SerializeField] ConnectorState _state = ConnectorState.Fog;

        public Face Face => _face;
        public ConnectorState State => _state;

        /// <summary>Whether a Sleeper can walk through (docs/SPEC.md §7).</summary>
        public bool IsPassable => _state == ConnectorState.Attached || _state == ConnectorState.Exit;

        /// <summary>Whether touching this door wakes a Sleeper.</summary>
        public bool IsExit => _state == ConnectorState.Exit;

        public void SetState(ConnectorState state) => _state = state;

        internal void Configure(Face face) => _face = face;
    }
}
