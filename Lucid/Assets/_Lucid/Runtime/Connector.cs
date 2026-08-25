using Lucid.Core;
using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// One of a cube's six socket positions. Present on every face whether or
    /// not that face is a doorway, so the builder and the validator can check
    /// the standard positions without inferring them from geometry
    /// (docs/CUBE-SPEC.md §1).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Connector : MonoBehaviour
    {
        [SerializeField] Face _face;
        [SerializeField] bool _isDoorway;
        [SerializeField] FogDoor _door;

        /// <summary>The face this socket sits on, in the cube's own frame.</summary>
        public Face Face => _face;

        /// <summary>Whether the cube type has a doorway here.</summary>
        public bool IsDoorway => _isDoorway;

        /// <summary>
        /// The mist filling this doorway. The template carries a FogDoor on
        /// every socket, walled faces included, so that a cube type's mask is
        /// the only thing deciding what is passable.
        /// </summary>
        public FogDoor Door => _door;

        internal void Configure(Face face, bool isDoorway, FogDoor door)
        {
            _face = face;
            _isDoorway = isDoorway;
            _door = door;
        }
    }
}
