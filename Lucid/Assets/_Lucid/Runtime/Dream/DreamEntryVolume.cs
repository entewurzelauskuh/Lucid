using System.Collections.Generic;
using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// The volume that notices a Sleeper, on a child of its own rather than on
    /// the cube root beside the shell's colliders.
    /// </summary>
    /// <remarks>
    /// Only a Sleeper counts as having explored a cube. Mobs walk through
    /// rooms, thrown props land in them and the projectiles of M0.7 onwards
    /// cross them, and none of that is a footprint — the same rule
    /// <see cref="FogDoor"/> applies to waking. And the explored rule is
    /// global across every dream (docs/SPEC.md §5), so a stray collider here
    /// would harden doors in everyone's maze.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class DreamEntryVolume : MonoBehaviour
    {
        [SerializeField] DreamCube _cube;

        readonly HashSet<Collider> _inside = new HashSet<Collider>();

        /// <summary>Whether a Sleeper is standing in here now.</summary>
        public bool IsOccupied
        {
            get
            {
                // A body destroyed or disabled mid-round never sends its exit,
                // so a stale entry would keep the room occupied for ever.
                _inside.RemoveWhere(c => c == null || !c.enabled);
                return _inside.Count > 0;
            }
        }

        internal void Bind(DreamCube cube) => _cube = cube;

        void OnTriggerEnter(Collider other)
        {
            if (_cube == null || other == null) return;
            if (other.GetComponentInParent<SleeperMotor>() == null) return;

            _inside.Add(other);
            _cube.OnSleeperInside();
        }

        void OnTriggerExit(Collider other)
        {
            if (other != null) _inside.Remove(other);
        }
    }
}
