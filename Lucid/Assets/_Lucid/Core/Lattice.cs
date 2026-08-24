using System;
using System.Collections.Generic;

namespace Lucid.Core
{
    /// <summary>
    /// The dream's cubes and the two facts exploration leaves behind. Read-only
    /// to callers: changes go through <see cref="Rules"/>, which returns new
    /// state, so validation can never mutate anything by accident
    /// (docs/CORE-API.md §2).
    /// </summary>
    public sealed class Lattice
    {
        readonly Dictionary<Coord, CubeInstance> _cubes;
        readonly HashSet<ConnectorRef> _solidified;
        readonly HashSet<Coord> _explored;

        Lattice(Coord start, string startTypeId,
                Dictionary<Coord, CubeInstance> cubes,
                HashSet<ConnectorRef> solidified,
                HashSet<Coord> explored)
        {
            Start = start;
            StartTypeId = startTypeId;
            _cubes = cubes;
            _solidified = solidified;
            _explored = explored;
        }

        public Coord Start { get; }
        public string StartTypeId { get; }

        public IReadOnlyDictionary<Coord, CubeInstance> Cubes => _cubes;
        public IReadOnlyCollection<ConnectorRef> Solidified => _solidified;
        public IReadOnlyCollection<Coord> Explored => _explored;

        public static Lattice New(CubeRegistry reg, string startTypeId, Rotation startRotation)
        {
            if (reg == null) throw new ArgumentNullException(nameof(reg));

            CubeType type = reg.Get(startTypeId);
            if (type.Category != CubeCategory.Start)
                throw new ArgumentException($"'{startTypeId}' is not a start cube", nameof(startTypeId));

            var start = new Coord(0, 0, 0);
            var cubes = new Dictionary<Coord, CubeInstance>
            {
                [start] = new CubeInstance(startTypeId, startRotation, null, 0),
            };
            return new Lattice(start, startTypeId, cubes,
                new HashSet<ConnectorRef>(), new HashSet<Coord>());
        }

        public bool Has(Coord c) => _cubes.ContainsKey(c);

        public CubeInstance At(Coord c) => _cubes.TryGetValue(c, out CubeInstance i) ? i : null;

        /// <summary>The cube's connector mask, rotated into world orientation.</summary>
        public FaceMask ConnectorsAt(Coord c, CubeRegistry reg)
        {
            if (!_cubes.TryGetValue(c, out CubeInstance instance)) return FaceMask.None;
            CubeType type = reg.Get(instance.TypeId);
            return Faces.Rotate(type.Connectors, instance.Rotation);
        }

        public bool HasConnector(Coord c, Face f, CubeRegistry reg) =>
            Faces.Has(ConnectorsAt(c, reg), f);

        public bool IsSolidified(ConnectorRef door) => _solidified.Contains(door);

        public bool IsExplored(Coord c) => _explored.Contains(c);

        /// <summary>Cubes in (Z, Y, X) order, so every walk that feeds a hash matches.</summary>
        public IEnumerable<Coord> CoordsInOrder()
        {
            var ordered = new List<Coord>(_cubes.Keys);
            ordered.Sort(Coord.Ordering);
            return ordered;
        }

        public Lattice Clone() =>
            new Lattice(Start, StartTypeId,
                new Dictionary<Coord, CubeInstance>(_cubes),
                new HashSet<ConnectorRef>(_solidified),
                new HashSet<Coord>(_explored));

        /// <summary>
        /// A copy with one more cube. Internal because adding a cube without
        /// the placement rules would break the invariants in §11; the rules
        /// are the only legitimate caller.
        /// </summary>
        internal Lattice WithCube(Coord c, CubeInstance instance)
        {
            Lattice next = Clone();
            next._cubes[c] = instance;
            return next;
        }

        /// <summary>A copy with a cube marked explored and its fog doors solidified.</summary>
        internal Lattice WithExplored(Coord c, IEnumerable<ConnectorRef> solidify)
        {
            Lattice next = Clone();
            next._explored.Add(c);
            foreach (ConnectorRef door in solidify) next._solidified.Add(door);
            return next;
        }
    }
}
