using System.Collections.Generic;
using Lucid.Core;
using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// Every cube a pack ships. Registration order is the wire's type index
    /// order, so it is kept stable and sorted by id rather than by whatever
    /// order the builder happened to run in (docs/NETCODE.md §4, §5).
    /// </summary>
    [CreateAssetMenu(menuName = "Lucid/Dream Pack", fileName = "DreamPack")]
    public sealed class DreamPack : ScriptableObject
    {
        [SerializeField] string _packId;
        [SerializeField] List<CubeDefinition> _cubes = new List<CubeDefinition>();

        public string PackId => _packId;
        public IReadOnlyList<CubeDefinition> Cubes => _cubes;

        /// <summary>Registers every cube in this pack with the rules engine.</summary>
        public void RegisterAll(CubeRegistry registry)
        {
            foreach (CubeDefinition cube in _cubes)
            {
                if (cube != null) registry.Register(cube.ToCubeType());
            }
        }

        /// <summary>
        /// Adds or replaces a cube, keeping the list sorted by id. Replacing
        /// rather than appending is what makes rebuilding a pack idempotent.
        /// </summary>
        internal bool AddOrReplace(CubeDefinition cube)
        {
            if (cube == null) return false;

            var before = new List<CubeDefinition>(_cubes);

            int existing = _cubes.FindIndex(c => c != null && c.Id == cube.Id);
            if (existing >= 0) _cubes[existing] = cube;
            else _cubes.Add(cube);

            _cubes.RemoveAll(c => c == null);
            _cubes.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));

            // Compare the whole list, not just the slot: dropping a null or
            // restoring the sort order changes the asset too, and a caller that
            // skips saving on false would lose it. The order is the wire's type
            // index order, so losing it matters.
            if (before.Count != _cubes.Count) return true;
            for (int i = 0; i < _cubes.Count; i++)
            {
                if (before[i] != _cubes[i]) return true;
            }
            return false;
        }

        internal void Configure(string packId) => _packId = packId;
    }
}
