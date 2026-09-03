using System;
using System.Collections.Generic;
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

        /// <summary>
        /// The opening this door fills, in the socket's own frame.
        /// </summary>
        /// <remarks>
        /// A vertical connector is not a doorway. docs/CUBE-SPEC.md §1 makes it
        /// a 2.5 m square hole in the floor or ceiling, and the socket for a
        /// vertical face is rotated so its local x/y span that hole. Using the
        /// doorway's 2.5 × 3 here covered exactly half of it and buried 1.75 m
        /// of collider in the slab — and a Sleeper would have walked through
        /// the uncovered half of a Fog floor that §7 calls solid to the touch.
        /// </remarks>
        public static Vector3 OpeningSize(Face face) => IsVertical(face)
            ? new Vector3(CubeMetrics.VerticalHole, CubeMetrics.VerticalHole, Depth)
            : new Vector3(CubeMetrics.DoorWidth, CubeMetrics.DoorHeight, Depth);

        /// <summary>Where that opening sits, in the socket's own frame.</summary>
        public static Vector3 OpeningCentre(Face face) => IsVertical(face)
            ? Vector3.zero
            : new Vector3(0f, CubeMetrics.DoorHeight / 2f, 0f);

        static bool IsVertical(Face face) => face == Face.Up || face == Face.Down;

        [SerializeField] Face _face;
        [SerializeField] ConnectorState _state = ConnectorState.Fog;
        [SerializeField] float _transitionSeconds = 0.35f;

        // Serialized, and deliberately not hidden. A door saved into a scene has
        // to come back holding the colliders it was built with, and
        // SceneSignature walks visible properties — a hidden field would be
        // invisible to it, so a scene whose doors had changed would not be
        // rewritten (#72).
        [SerializeField] BoxCollider _barrier;
        [SerializeField] BoxCollider _wake;

        readonly HashSet<Collider> _alreadyInside = new HashSet<Collider>();

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

            // Told the same state again — which is the shape a per-frame
            // refresh from M0.6's lattice mirror will have. Left as a reset it
            // would snap any transition in flight to its end every frame.
            if (transition == FogDoorTransition.None) return;

            if (transition == FogDoorTransition.Forbidden)
            {
                Debug.LogError(
                    $"{name}: {_state} → {state} is not a transition docs/SPEC.md §7 allows. " +
                    "Applying it anyway; the derivation that produced it is the bug.", this);
            }

            _state = state;

            // Applied either way — the door mirrors what Core derived, and one
            // left showing the old state would hide the bug rather than surface
            // it — but an unlawful change is not dressed up as an animation.
            bool plays = transition != FogDoorTransition.Forbidden;
            Playing = plays ? transition : FogDoorTransition.None;
            Progress = plays ? 0f : 1f;
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

        /// <summary>
        /// Makes sure the two colliders exist, exactly once.
        /// </summary>
        /// <remarks>
        /// The references are serialized, so a door saved into a scene keeps
        /// the colliders it was built with. A plain "already built" flag is not
        /// enough: the flag does not survive serialization but the colliders
        /// do, so a door written to a scene in the editor came back with two
        /// barriers and — on an exit — two live wake triggers, and one touch
        /// was reported twice. The same happens across a domain reload in play
        /// mode, where Awake does not run again.
        /// </remarks>
        void Build()
        {
            if (_barrier != null && _wake != null) return;

            Vector3 size = OpeningSize(_face);
            Vector3 centre = OpeningCentre(_face);

            // Adopt anything already here before adding: a reload can leave the
            // components without the references that named them.
            var existing = GetComponents<BoxCollider>();
            foreach (BoxCollider box in existing)
            {
                if (box.isTrigger && _wake == null) _wake = box;
                else if (!box.isTrigger && _barrier == null) _barrier = box;
            }

            if (_barrier == null) _barrier = gameObject.AddComponent<BoxCollider>();
            _barrier.size = size;
            _barrier.center = centre;
            _barrier.isTrigger = false;

            if (_wake == null) _wake = gameObject.AddComponent<BoxCollider>();
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
            bool wake = IsExit;
            if (wake && !_wake.enabled) IgnoreWhoeverIsAlreadyInside();
            _wake.enabled = wake;
            if (!wake) _alreadyInside.Clear();
        }

        /// <summary>
        /// Remembers who is standing in the doorway at the moment the light
        /// arrives.
        /// </summary>
        /// <remarks>
        /// Unity re-issues OnTriggerEnter for colliders already overlapping a
        /// trigger that is switched back on. §7 lets the exit move away and
        /// back — the yo-yo it explicitly anticipates — so a Sleeper standing
        /// still in a doorway would have been reported as having walked
        /// through it, and woken without moving.
        /// </remarks>
        void IgnoreWhoeverIsAlreadyInside()
        {
            _alreadyInside.Clear();

            Vector3 centre = transform.TransformPoint(_wake.center);
            Vector3 half = Vector3.Scale(_wake.size, transform.lossyScale) * 0.5f;
            foreach (Collider hit in Physics.OverlapBox(
                         centre, half, transform.rotation, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit != null) _alreadyInside.Add(hit);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!IsExit || other == null) return;
            if (_alreadyInside.Contains(other)) return;

            // Only a Sleeper wakes. Mobs, thrown props and the projectiles of
            // M0.7 onwards will pass through an exit too, and none of them is
            // walking out of the dream.
            if (other.GetComponentInParent<SleeperMotor>() == null) return;

            Touched?.Invoke(this);
        }

        void OnTriggerExit(Collider other)
        {
            if (other != null) _alreadyInside.Remove(other);
        }
    }
}
