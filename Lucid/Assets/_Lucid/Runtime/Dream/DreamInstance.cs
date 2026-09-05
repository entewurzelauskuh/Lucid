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

            Retire(lattice);

            foreach (Coord coord in lattice.CoordsInOrder())
            {
                if (_cubes.TryGetValue(coord, out DreamCube standing) && standing == null)
                {
                    // Destroyed from under us. Better to build it again than to
                    // throw on the next field access.
                    _cubes.Remove(coord);
                    standing = null;
                }

                if (standing == null)
                {
                    standing = Spawn(coord, lattice.At(coord));
                    _cubes[coord] = standing;

                    // docs/SPEC.md §7 exempts the start cube from the explored
                    // rule outright; every other cube reconciles with the
                    // lattice on each Apply.
                    if (coord == lattice.Start) standing.Exempt();

                    standing.Entered += OnCubeEntered;
                    standing.DoorTouched += OnDoorTouched;
                }

                standing.Apply(derived, lattice.IsExplored(coord));
            }
        }

        /// <summary>Where a Sleeper stands at the start of a round, and again after a fall.</summary>
        /// <remarks>
        /// Through this instance's transform, because the cubes are laid out in
        /// its frame: docs/NETCODE.md §8 has every subscriber rebuilding the
        /// dream from the lattice it already holds, and two of them at the
        /// world origin would interpenetrate.
        /// </remarks>
        public Vector3 SpawnPoint => transform.TransformPoint(DreamSpace.Origin(Start));

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
                    if (!Faces.IsVertical(f))
                        return transform.TransformDirection(DreamSpace.Direction(f));
                }

                return transform.forward;
            }
        }

        /// <summary>Puts a Sleeper back in the start cube (docs/SPEC.md §7).</summary>
        public void Respawn(SleeperMotor sleeper)
        {
            if (sleeper == null) throw new ArgumentNullException(nameof(sleeper));
            if (Lattice == null)
                throw new InvalidOperationException(
                    $"{name}: nothing has been built, so there is no start cube to return to");

            sleeper.Warp(SpawnPoint);

            Vector3 flat = new Vector3(SpawnFacing.x, 0f, SpawnFacing.z);
            if (flat.sqrMagnitude > 0f)
                sleeper.transform.rotation = Quaternion.LookRotation(flat.normalized, Vector3.up);
        }

        /// <summary>
        /// Takes the dream down when it is handed a different one.
        /// </summary>
        /// <remarks>
        /// A lattice never loses a cube within a round — <c>Lattice.WithCube</c>
        /// is add-only — so a cube that is standing here and missing there
        /// means a different lattice, which is to say the next round. Then
        /// everything goes, not just the cubes that are gone: the survivors
        /// would otherwise keep last round's door states and be asked to walk
        /// back to them, and the start cube's own door going Attached → Exit is
        /// a transition docs/SPEC.md §7 does not have. A new round is a new
        /// dream, and its doors arrive in their states rather than animating
        /// back into them.
        /// </remarks>
        void Retire(Lattice lattice)
        {
            bool different = false;
            foreach (KeyValuePair<Coord, DreamCube> pair in _cubes)
            {
                if (lattice.Has(pair.Key)) continue;
                different = true;
                break;
            }

            if (!different) return;

            foreach (KeyValuePair<Coord, DreamCube> pair in _cubes)
            {
                DreamCube cube = pair.Value;
                if (cube == null) continue;

                cube.Entered -= OnCubeEntered;
                cube.DoorTouched -= OnDoorTouched;
                Destroy(cube.gameObject);
            }

            _cubes.Clear();
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
            if (!_definitions.TryGetValue(instance.TypeId, out CubeDefinition definition))
                throw new InvalidOperationException(
                    $"{name}: the lattice has '{instance.TypeId}' at {coord}, and the pack " +
                    $"'{(_pack != null ? _pack.PackId : "?")}' has no cube by that id");

            if (definition.Prefab == null)
                throw new InvalidOperationException(
                    $"{name}: cube '{instance.TypeId}' has no prefab, so {coord} would be a hole");

            FaceMask expected = Faces.Rotate(definition.Connectors, instance.Rotation);
            return DreamCube.Create(
                definition.Prefab, coord, instance.Rotation, expected, transform);
        }

        void OnCubeEntered(DreamCube cube) => Explored?.Invoke(cube.Coord);

        void OnDoorTouched(DreamCube cube, Face face) =>
            TouchedExit?.Invoke(new ConnectorRef(cube.Coord, face));
    }
}
