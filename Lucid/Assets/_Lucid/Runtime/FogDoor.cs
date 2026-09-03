using System;
using Lucid.Core;
using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// The mist in a doorway. Its state is derived by <c>Lucid.Core</c> and
    /// pushed in; the door never decides anything for itself.
    /// </summary>
    /// <remarks>
    /// What the door does own is the consequences of the state it was told:
    /// whether a Sleeper is stopped, whether touching it means anything, and
    /// which transition plays (docs/SPEC.md §7).
    ///
    /// It does not own <b>waking</b>. Walking into the light raises
    /// <see cref="Touched"/> and nothing more. The host adjudicates a wake
    /// through <c>Round.TryWake</c>, and SPEC §14 requires the client to be
    /// able to walk the Sleeper back into the doorway when a placement beat
    /// them to it — a door that woke someone locally would have performed an
    /// act with no undo. M0.6 turns this event into <c>TouchedExit</c>.
    ///
    /// Its colliders are built here rather than carried in the prefab. They
    /// are fully determined by <see cref="CubeMetrics"/> and by the state, so
    /// serialising them would only create something able to disagree with the
    /// doorway it fills — the same reasoning as <see cref="SleeperRig"/>.
    /// </remarks>
    [DisallowMultipleComponent]
    public sealed class FogDoor : MonoBehaviour
    {
        /// <summary>How thick the mist is, in metres.</summary>
        public const float Depth = 0.25f;

        [SerializeField] Face _face;
        [SerializeField] ConnectorState _state = ConnectorState.Fog;
        [SerializeField] float _transitionSeconds = 0.35f;

        BoxCollider _barrier;
        BoxCollider _wake;
        bool _built;

        /// <summary>A Sleeper walked into this door while it was an exit.</summary>
        public event Action<FogDoor> Touched;

        public Face Face => _face;
        public ConnectorState State => _state;

        /// <summary>Whether a Sleeper can walk through (docs/SPEC.md §7).</summary>
        public bool IsPassable => FogDoorTransitions.IsPassable(_state);

        /// <summary>Whether touching this door wakes a Sleeper.</summary>
        public bool IsExit => FogDoorTransitions.Wakes(_state);

        /// <summary>The transition currently playing, if any.</summary>
        public FogDoorTransition Playing { get; private set; }

        /// <summary>How far through that transition, 0 to 1.</summary>
        public float Progress { get; private set; } = 1f;

        void Awake() => Build();

        void Update() => Tick(Time.deltaTime);

        /// <summary>
        /// Put the door straight into a state, with no transition and no
        /// complaint about how it got there.
        /// </summary>
        /// <remarks>
        /// For a door that has just been instantiated. A cube built from an
        /// event log may arrive already explored, so its doors are Solid from
        /// the first frame — they did not harden, they were always that way,
        /// and playing the condense would be a lie about what happened.
        /// <see cref="SetState"/> is for changes during a round.
        /// </remarks>
        public void Initialise(ConnectorState state)
        {
            Build();
            _state = state;
            Playing = FogDoorTransition.None;
            Progress = 1f;
            ApplyState();
        }

        /// <summary>
        /// Tell the door what Core derived. A change plays its transition; the
        /// collision and the trigger follow immediately, because a door that
        /// looked open while still blocking would be worse than an abrupt cut.
        /// </summary>
        public void SetState(ConnectorState state)
        {
            Build();

            FogDoorTransition transition = FogDoorTransitions.For(_state, state);
            if (transition == FogDoorTransition.Forbidden)
            {
                Debug.LogError(
                    $"{name}: {_state} → {state} is not a transition docs/SPEC.md §7 allows. " +
                    "Applying it anyway; the derivation that produced it is the bug.", this);
            }

            _state = state;
            Playing = transition;
            Progress = transition == FogDoorTransition.None ? 1f : 0f;
            ApplyState();
        }

        /// <summary>
        /// Advance the transition. Takes its own dt so a test can play one out
        /// in a single frame rather than waiting for it.
        /// </summary>
        public void Tick(float dt)
        {
            if (dt <= 0f || Progress >= 1f) return;

            Progress = _transitionSeconds <= 0f
                ? 1f
                : Mathf.Min(1f, Progress + dt / _transitionSeconds);

            if (Progress >= 1f) Playing = FogDoorTransition.None;
        }

        internal void Configure(Face face) => _face = face;

        void Build()
        {
            if (_built) return;
            _built = true;

            // Sized to the opening it fills, and thin: the mist is a wall in
            // the doorway, not a volume the doorway contains.
            var size = new Vector3(CubeMetrics.DoorWidth, CubeMetrics.DoorHeight, Depth);
            var centre = new Vector3(0f, CubeMetrics.DoorHeight / 2f, 0f);

            _barrier = gameObject.AddComponent<BoxCollider>();
            _barrier.size = size;
            _barrier.center = centre;

            _wake = gameObject.AddComponent<BoxCollider>();
            _wake.size = size;
            _wake.center = centre;
            _wake.isTrigger = true;

            ApplyState();
        }

        void ApplyState()
        {
            if (_barrier == null || _wake == null) return;

            // Fog and Solid stop a Sleeper; Attached and Exit do not.
            _barrier.enabled = !IsPassable;

            // Only the light wakes. Attached is passable too, so a trigger left
            // live there would wake anyone walking between ordinary rooms.
            _wake.enabled = IsExit;
        }

        void OnTriggerEnter(Collider other)
        {
            if (IsExit) Touched?.Invoke(this);
        }
    }
}
