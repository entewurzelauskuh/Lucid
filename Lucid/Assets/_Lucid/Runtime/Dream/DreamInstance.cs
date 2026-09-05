using System;
using System.Collections.Generic;
using Lucid.Core;
using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// One Sleeper's dream, standing in the scene: the lattice built out of
    /// cubes, its doors showing what Core derived, and the two things a
    /// Sleeper does that the rules care about.
    /// </summary>
    /// <remarks>
    /// It reports and does not decide. Entering a cube raises
    /// <see cref="Explored"/> and walking into an exit raises
    /// <see cref="TouchedExit"/>; neither is applied to the lattice here,
    /// because exploration is global across every dream and waking ends a
    /// Sleeper's round — both are the host's to adjudicate (docs/SPEC.md §5,
    /// §14). The host applies the event and the new lattice comes back through
    /// <see cref="Apply"/>, which is also how a replay and a live round end up
    /// on exactly the same path.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class DreamInstance : MonoBehaviour
    {
        [SerializeField] DreamPack _pack;
        [SerializeField] string _startTypeId = "core.start";
        [SerializeField] Rotation _startRotation = Rotation.R0;

        readonly Dictionary<Coord, DreamCube> _cubes = new Dictionary<Coord, DreamCube>();
        readonly Dictionary<string, CubeDefinition> _definitions =
            new Dictionary<string, CubeDefinition>(StringComparer.Ordinal);

        /// <summary>A Sleeper set foot in this cube for the first time.</summary>
        public event Action<Coord> Explored;

        /// <summary>A Sleeper walked into this exit door.</summary>
        public event Action<ConnectorRef> TouchedExit;

        public Lattice Lattice { get; private set; }
        public Derived Derived { get; private set; }
        public CubeRegistry Registry { get; private set; }

        /// <summary>The cubes standing in the scene, by lattice coord.</summary>
        public IReadOnlyDictionary<Coord, DreamCube> Cubes => _cubes;

        /// <summary>Where the start cube is; also where a Sleeper comes back to.</summary>
        public Coord Start => Lattice != null ? Lattice.Start : new Coord(0, 0, 0);

        public void Bind(DreamPack pack, string startTypeId, Rotation startRotation)
        {
            _pack = pack;
            _startTypeId = startTypeId;
            _startRotation = startRotation;
        }

        /// <summary>
        /// Replays <paramref name="log"/> and builds what it describes.
        /// </summary>
        public (Lattice lattice, Derived derived) Build(
            EventLog log, RoundSettings settings = null)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));

            Registry = BuildRegistry();
            (Lattice lattice, Derived derived) =
                EventLog.Replay(log, Registry, _startTypeId, _startRotation, settings);

            Apply(lattice, derived);
            return (lattice, derived);
        }

        /// <summary>
        /// Shows a lattice and its derived state. Cubes that are already
        /// standing stay standing — rebuilding them would restart every door's
        /// transition and re-arm every entry volume, so a live round would
        /// forget where the Sleeper had been every time the Nightmare built.
        /// </summary>
        public void Apply(Lattice lattice, Derived derived)
        {
            if (lattice == null) throw new ArgumentNullException(nameof(lattice));
            if (derived == null) throw new ArgumentNullException(nameof(derived));

            Lattice = lattice;
            Derived = derived;
            if (Registry == null) Registry = BuildRegistry();

            foreach (Coord coord in lattice.CoordsInOrder())
            {
                if (!_cubes.TryGetValue(coord, out DreamCube cube))
                {
                    cube = Spawn(coord, lattice.At(coord));
                    _cubes.Add(coord, cube);

                    // The start cube is exempt from the explored rule, and a
                    // cube the log already explored was explored by whoever
                    // walked there, not by this runtime coming up.
                    if (coord == lattice.Start || lattice.IsExplored(coord)) cube.MarkEntered();

                    cube.Entered += OnCubeEntered;
                    cube.DoorTouched += OnDoorTouched;
                }

                cube.Apply(derived);
            }
        }

        /// <summary>Where a Sleeper stands at the start of a round, and again after a fall.</summary>
        public Vector3 SpawnPoint => DreamSpace.Origin(Start);

        /// <summary>
        /// Which way they face: towards the start cube's own door, so the way
        /// out is in front of them rather than behind.
        /// </summary>
        public Vector3 SpawnFacing
        {
            get
            {
                if (Lattice == null || Registry == null) return Vector3.forward;

                FaceMask doors = Lattice.ConnectorsAt(Start, Registry);
                foreach (Face f in Faces.Of(doors))
                {
                    if (!Faces.IsVertical(f)) return DreamSpace.Direction(f);
                }

                return Vector3.forward;
            }
        }

        /// <summary>Puts a Sleeper back in the start cube (docs/SPEC.md §7).</summary>
        public void Respawn(SleeperMotor sleeper)
        {
            if (sleeper == null) throw new ArgumentNullException(nameof(sleeper));

            sleeper.Warp(SpawnPoint);

            Vector3 flat = new Vector3(SpawnFacing.x, 0f, SpawnFacing.z);
            if (flat.sqrMagnitude > 0f)
                sleeper.transform.rotation = Quaternion.LookRotation(flat.normalized, Vector3.up);
        }

        CubeRegistry BuildRegistry()
        {
            if (_pack == null)
                throw new InvalidOperationException(
                    $"{name}: no DreamPack bound, so there is nothing to build cubes from");

            var registry = new CubeRegistry();
            _pack.RegisterAll(registry);

            _definitions.Clear();
            foreach (CubeDefinition d in _pack.Cubes)
            {
                if (d != null) _definitions[d.Id] = d;
            }

            return registry;
        }

        DreamCube Spawn(Coord coord, CubeInstance instance)
        {
            if (instance == null)
                throw new InvalidOperationException($"{name}: nothing to build at {coord}");

            if (!_definitions.TryGetValue(instance.TypeId, out CubeDefinition definition))
                throw new InvalidOperationException(
                    $"{name}: the lattice has '{instance.TypeId}' at {coord}, and the pack " +
                    $"'{(_pack != null ? _pack.PackId : "?")}' has no cube by that id");

            if (definition.Prefab == null)
                throw new InvalidOperationException(
                    $"{name}: cube '{instance.TypeId}' has no prefab, so {coord} would be a hole");

            return DreamCube.Create(definition.Prefab, coord, instance.Rotation, transform);
        }

        void OnCubeEntered(DreamCube cube) => Explored?.Invoke(cube.Coord);

        void OnDoorTouched(DreamCube cube, Face face) =>
            TouchedExit?.Invoke(new ConnectorRef(cube.Coord, face));
    }
}
