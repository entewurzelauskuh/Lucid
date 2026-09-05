using System;
using System.Collections.Generic;
using Lucid.Core;
using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// One cube standing in the dream: where it is, which way it faces, and
    /// the doors it shows the Sleeper.
    /// </summary>
    /// <remarks>
    /// The prefab's <see cref="Connector"/>s carry faces in the cube's own
    /// frame; the lattice names them in the world's. A cube turned a quarter
    /// turn has its local North looking East, so every lookup here rotates the
    /// local face first. Getting that backwards would wire a door to the state
    /// of the connector on the opposite side of the room.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class DreamCube : MonoBehaviour
    {
        /// <summary>
        /// How far inside the cube the entry volume starts, in metres.
        /// </summary>
        /// <remarks>
        /// Wider than a Sleeper's 0.4 m radius, so that standing in a doorway
        /// is not being in both cubes at once: with this inset a Sleeper's
        /// capsule reaches the volume only once its centre is past the face.
        /// </remarks>
        public const float EntryInset = 0.5f;

        /// <summary>
        /// The entry volume's node. Deliberately not "Interior": that name is
        /// the template's own (docs/SPEC.md §17), and adopting whatever
        /// collider a cube put there would turn a floor or a wall into a
        /// trigger the moment any cube used it for what it is for.
        /// </summary>
        const string EntryVolumeName = "EntryVolume";

        [SerializeField] Coord _coord;
        [SerializeField] Rotation _rotation;
        [SerializeField] bool _exempt;
        [SerializeField] bool _reported;

        DreamEntryVolume _volume;

        readonly Dictionary<Face, FogDoor> _doors = new Dictionary<Face, FogDoor>();

        bool _fresh = true;

        /// <summary>
        /// A Sleeper is in here and the lattice does not know it yet. Raised on
        /// first entry, and again on any lattice change that still does not
        /// show this cube explored — the report is the host's to act on, and a
        /// dropped one has no other way back.
        /// </summary>
        public event Action<DreamCube> Entered;

        /// <summary>A Sleeper walked into one of this cube's exit doors.</summary>
        public event Action<DreamCube, Face> DoorTouched;

        public Coord Coord => _coord;
        public Rotation Rotation => _rotation;

        /// <summary>Whether an entry here is already on its way to the host.</summary>
        public bool HasBeenReported => _reported;

        /// <summary>Whether a Sleeper is standing in here now.</summary>
        public bool IsOccupied => _volume != null && _volume.IsOccupied;

        /// <summary>The doors, by the world face each one faces.</summary>
        public IReadOnlyDictionary<Face, FogDoor> Doors => _doors;

        /// <summary>
        /// Plants <paramref name="prefab"/> at <paramref name="coord"/> and
        /// wires it up. The cube is not yet showing any state; call
        /// <see cref="Apply"/>.
        /// </summary>
        public static DreamCube Create(
            GameObject prefab, Coord coord, Rotation rotation, FaceMask expected, Transform parent)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));

            GameObject body = Instantiate(prefab, parent);
            body.transform.localPosition = DreamSpace.Origin(coord);
            body.transform.localRotation = DreamSpace.Orientation(rotation);
            body.name = $"Cube {coord}";

            var cube = body.GetComponent<DreamCube>();
            if (cube == null) cube = body.AddComponent<DreamCube>();
            cube.Configure(coord, rotation, expected);
            return cube;
        }

        void Configure(Coord coord, Rotation rotation, FaceMask expected)
        {
            _coord = coord;
            _rotation = rotation;
            _doors.Clear();

            foreach (Connector socket in GetComponentsInChildren<Connector>(true))
            {
                if (socket.Door == null) continue;

                Face world = Faces.Rotate(socket.Face, rotation);
                if (!socket.IsDoorway)
                {
                    // Not a door: ShellBuilder already put a wall here, and a
                    // FogDoor driven to Solid would add a second barrier inside
                    // it — the one on Down standing 0.125 m above a floor whose
                    // step offset is 0.1.
                    socket.Door.gameObject.SetActive(false);
                    continue;
                }

                socket.Door.gameObject.SetActive(true);
                _doors[world] = socket.Door;
                socket.Door.Touched += OnDoorTouched;
            }

            // The prefab's sockets and the registry's mask are both generated
            // from the same cube.spec.json, so a disagreement means one of them
            // is stale. Rendering it rather than saying so would put an
            // invisible barrier across a real doorway, or a live wake trigger
            // inside a wall.
            FaceMask built = FaceMask.None;
            foreach (Face f in _doors.Keys) built |= Faces.ToMask(f);
            if (expected != built)
            {
                Debug.LogError(
                    $"{name}: the prefab has doorways {built} and the cube type says " +
                    $"{expected}. One of them is stale; rebuild the cube.", this);
            }

            BuildEntryVolume();
        }

        /// <summary>
        /// Shows what Core derived. The first call after
        /// <see cref="Create"/> puts every door straight into its state: a
        /// cube replayed out of an event log may arrive already explored, and
        /// its doors did not harden, they were always wall.
        /// </summary>
        public void Apply(Derived derived, bool explored)
        {
            if (derived == null) throw new ArgumentNullException(nameof(derived));

            foreach (KeyValuePair<Face, FogDoor> pair in _doors)
            {
                ConnectorState state = derived.StateOf(new ConnectorRef(_coord, pair.Key));
                if (_fresh) pair.Value.Initialise(state);
                else pair.Value.SetState(state);
            }

            _fresh = false;

            // The lattice is the truth about what has been explored, and this
            // is where the local flag is reconciled with it rather than left to
            // shadow it. A report the host took is settled. One it dropped —
            // Dawn, or a Sleeper who blinked out and back — is made again while
            // the Sleeper is still standing here, because Unity will not
            // re-fire OnTriggerEnter for a body that never left, and without
            // this the cube's fog doors would never solidify for anyone.
            if (explored) _reported = true;
            else if (!_exempt && IsOccupied) Report();
            else _reported = false;
        }

        /// <summary>
        /// Never report an entry here. For the start cube, which
        /// docs/SPEC.md §7 exempts from the explored rule outright.
        /// </summary>
        public void Exempt() => _exempt = true;

        /// <summary>
        /// The volume that notices a Sleeper. Inset from every face so a
        /// Sleeper standing in a doorway belongs to one cube, not two.
        /// </summary>
        void BuildEntryVolume()
        {
            Transform existing = transform.Find(EntryVolumeName);
            GameObject volume;
            if (existing != null)
            {
                volume = existing.gameObject;
            }
            else
            {
                volume = new GameObject(EntryVolumeName);
                volume.transform.SetParent(transform, false);
            }

            var box = volume.GetComponent<BoxCollider>();
            if (box == null) box = volume.AddComponent<BoxCollider>();

            box.isTrigger = true;
            float side = CubeMetrics.Size - 2f * EntryInset;
            box.size = new Vector3(side, side, side);
            box.center = new Vector3(0f, CubeMetrics.Half, 0f);

            _volume = volume.GetComponent<DreamEntryVolume>();
            if (_volume == null) _volume = volume.AddComponent<DreamEntryVolume>();
            _volume.Bind(this);
        }

        internal void OnSleeperInside()
        {
            if (_reported || _exempt) return;
            Report();
        }

        void Report()
        {
            _reported = true;
            Entered?.Invoke(this);
        }

        void OnDoorTouched(FogDoor door)
        {
            foreach (KeyValuePair<Face, FogDoor> pair in _doors)
            {
                if (pair.Value != door) continue;
                DoorTouched?.Invoke(this, pair.Key);
                return;
            }
        }

        void OnDestroy()
        {
            foreach (KeyValuePair<Face, FogDoor> pair in _doors)
            {
                if (pair.Value != null) pair.Value.Touched -= OnDoorTouched;
            }
        }
    }
}
