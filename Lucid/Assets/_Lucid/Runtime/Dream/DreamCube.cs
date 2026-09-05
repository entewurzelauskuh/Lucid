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

        const string InteriorName = "Interior";

        [SerializeField] Coord _coord;
        [SerializeField] Rotation _rotation;
        [SerializeField] bool _entered;

        readonly Dictionary<Face, FogDoor> _doors = new Dictionary<Face, FogDoor>();

        bool _fresh = true;

        /// <summary>A Sleeper set foot in here for the first time.</summary>
        public event Action<DreamCube> Entered;

        /// <summary>A Sleeper walked into one of this cube's exit doors.</summary>
        public event Action<DreamCube, Face> DoorTouched;

        public Coord Coord => _coord;
        public Rotation Rotation => _rotation;

        /// <summary>Whether a Sleeper has already been counted in here.</summary>
        public bool HasBeenEntered => _entered;

        /// <summary>The doors, by the world face each one faces.</summary>
        public IReadOnlyDictionary<Face, FogDoor> Doors => _doors;

        /// <summary>
        /// Plants <paramref name="prefab"/> at <paramref name="coord"/> and
        /// wires it up. The cube is not yet showing any state; call
        /// <see cref="Apply"/>.
        /// </summary>
        public static DreamCube Create(
            GameObject prefab, Coord coord, Rotation rotation, Transform parent)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));

            GameObject body = Instantiate(
                prefab, DreamSpace.Origin(coord), DreamSpace.Orientation(rotation), parent);
            body.name = $"Cube {coord}";

            var cube = body.GetComponent<DreamCube>();
            if (cube == null) cube = body.AddComponent<DreamCube>();
            cube.Configure(coord, rotation);
            return cube;
        }

        void Configure(Coord coord, Rotation rotation)
        {
            _coord = coord;
            _rotation = rotation;
            _doors.Clear();

            foreach (Connector socket in GetComponentsInChildren<Connector>(true))
            {
                if (socket.Door == null) continue;

                Face world = Faces.Rotate(socket.Face, rotation);
                _doors[world] = socket.Door;
                socket.Door.Touched += OnDoorTouched;
            }

            BuildInterior();
        }

        /// <summary>
        /// Shows what Core derived. The first call after
        /// <see cref="Create"/> puts every door straight into its state: a
        /// cube replayed out of an event log may arrive already explored, and
        /// its doors did not harden, they were always wall.
        /// </summary>
        public void Apply(Derived derived)
        {
            if (derived == null) throw new ArgumentNullException(nameof(derived));

            foreach (KeyValuePair<Face, FogDoor> pair in _doors)
            {
                ConnectorState state = derived.StateOf(new ConnectorRef(_coord, pair.Key));
                if (_fresh) pair.Value.Initialise(state);
                else pair.Value.SetState(state);
            }

            _fresh = false;
        }

        /// <summary>
        /// Counts this cube as already entered, without reporting it. For the
        /// start cube, which docs/SPEC.md §7 exempts, and for cubes a replayed
        /// log says were explored before this runtime existed.
        /// </summary>
        public void MarkEntered() => _entered = true;

        /// <summary>
        /// The volume that notices a Sleeper. Inset from every face so a
        /// Sleeper standing in a doorway belongs to one cube, not two.
        /// </summary>
        void BuildInterior()
        {
            Transform existing = transform.Find(InteriorName);
            GameObject interior;
            if (existing != null)
            {
                interior = existing.gameObject;
            }
            else
            {
                interior = new GameObject(InteriorName);
                interior.transform.SetParent(transform, false);
            }

            var box = interior.GetComponent<BoxCollider>();
            if (box == null) box = interior.AddComponent<BoxCollider>();

            box.isTrigger = true;
            float side = CubeMetrics.Size - 2f * EntryInset;
            box.size = new Vector3(side, side, side);
            box.center = new Vector3(0f, CubeMetrics.Half, 0f);

            var relay = interior.GetComponent<DreamCubeInterior>();
            if (relay == null) relay = interior.AddComponent<DreamCubeInterior>();
            relay.Bind(this);
        }

        internal void OnSleeperInside()
        {
            if (_entered) return;
            _entered = true;
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
