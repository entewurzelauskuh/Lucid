using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// The 8 m a cube owns in the lattice, carried on the template root so the
    /// validator and anyone opening the prefab can see it (docs/SPEC.md §17).
    /// </summary>
    /// <remarks>
    /// Not centred on the origin: the floor slab hangs below y = 0 so that the
    /// walkable surface is the origin plane exactly as docs/CUBE-SPEC.md §1
    /// states, and the ceiling stops the same distance short of the top so the
    /// cube stacked above has somewhere to put its own floor. The span is still
    /// exactly 8 m. See docs/DECISIONS.md.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class CubeBounds : MonoBehaviour
    {
        [SerializeField] float _size = 8f;
        [SerializeField] float _floorDrop = 0.3f;

        public float Size => _size;

        /// <summary>How far the floor slab reaches below the origin plane.</summary>
        public float FloorDrop => _floorDrop;

        /// <summary>The box everything the builder emits must stay inside.</summary>
        public Bounds Local => new Bounds(
            new Vector3(0f, _size / 2f - _floorDrop, 0f),
            new Vector3(_size, _size, _size));

        public void SetExtent(float size, float floorDrop)
        {
            _size = size;
            _floorDrop = floorDrop;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.5f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Local.center, Local.size);
        }
    }
}
