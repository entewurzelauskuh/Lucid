using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// The entry volume's own component, so the trigger sits on a child of its
    /// own rather than on the cube root beside the shell's colliders.
    /// </summary>
    /// <remarks>
    /// Only a Sleeper counts as having explored a cube. Mobs walk through
    /// rooms, thrown props land in them and the projectiles of M0.7 onwards
    /// cross them, and none of that is a footprint — the same rule
    /// <see cref="FogDoor"/> applies to waking.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class DreamCubeInterior : MonoBehaviour
    {
        [SerializeField] DreamCube _cube;

        internal void Bind(DreamCube cube) => _cube = cube;

        void OnTriggerEnter(Collider other)
        {
            if (_cube == null || other == null) return;
            if (other.GetComponentInParent<SleeperMotor>() == null) return;

            _cube.OnSleeperInside();
        }
    }
}
