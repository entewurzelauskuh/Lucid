using System;
using System.Collections.Generic;

namespace Lucid.Core
{
    /// <summary>
    /// Every cube type available this round. The registry is ordered, so its
    /// hash and the wire's type indices agree across machines
    /// (docs/NETCODE.md §5).
    /// </summary>
    public sealed class CubeRegistry
    {
        readonly Dictionary<string, CubeType> _byId = new Dictionary<string, CubeType>(StringComparer.Ordinal);
        readonly List<CubeType> _inOrder = new List<CubeType>();

        /// <summary>
        /// Registers a type. Every cube needs at least two connectors so that
        /// a placement can never seal the dream (docs/SPEC.md §7); the start
        /// cube is the sole exception and has exactly one.
        /// </summary>
        public void Register(CubeType t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (_byId.ContainsKey(t.Id))
                throw new ArgumentException($"cube type '{t.Id}' is already registered", nameof(t));

            int connectors = Faces.Count(t.Connectors);
            if (t.Category == CubeCategory.Start)
            {
                if (connectors != 1)
                    throw new ArgumentException(
                        $"start cube '{t.Id}' must have exactly one connector, has {connectors}", nameof(t));
            }
            else if (connectors < 2)
            {
                throw new ArgumentException(
                    $"cube type '{t.Id}' must have at least two connectors, has {connectors}", nameof(t));
            }

            _byId.Add(t.Id, t);
            _inOrder.Add(t);
        }

        public CubeType Get(string id)
        {
            if (!_byId.TryGetValue(id, out CubeType t))
                throw new KeyNotFoundException($"no cube type '{id}' is registered");
            return t;
        }

        public bool TryGet(string id, out CubeType t) => _byId.TryGetValue(id, out t);

        public bool Contains(string id) => _byId.ContainsKey(id);

        /// <summary>Registration order, which is what type indices on the wire mean.</summary>
        public IReadOnlyList<CubeType> All => _inOrder;

        /// <summary>
        /// Ordered hash of every registered id, compared at RoundStart so a
        /// content mismatch is caught before the first placement
        /// (docs/NETCODE.md §4).
        /// </summary>
        public ulong ContentHash()
        {
            ulong h = Fnv.Basis;
            foreach (CubeType t in _inOrder) h = Fnv.String(h, t.Id);
            return h;
        }
    }
}
