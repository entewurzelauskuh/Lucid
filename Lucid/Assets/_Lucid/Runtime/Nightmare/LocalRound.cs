using System;
using Lucid.Core;
using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// The host's rules engine, run here, for a dream with no network:
    /// one <see cref="Round"/> that owns the clock, the budget and the log,
    /// and the one page of docs/CORE-API.md §10 that this machine plays host
    /// for. M0.8 moves the same calls behind messages and changes nothing
    /// about them.
    /// </summary>
    /// <remarks>
    /// Placements go through <see cref="Round.TryPlace"/>, which validates and
    /// applies in one step; the god view's ghost asks <see cref="Validate"/>
    /// first, which applies nothing. Exploration is the Sleeper's report,
    /// adjudicated here and pushed back to the dream, so a door the Sleeper
    /// hardens is hard on the god view in the same frame.
    /// </remarks>
    [RequireComponent(typeof(EmptyDream))]
    public sealed class LocalRound : MonoBehaviour
    {
        /// <summary>The local Sleeper's id in the round. There is one.</summary>
        public const int LocalSleeper = 0;

        [SerializeField] int _headStartMs = 30_000;
        [SerializeField] int _roundLengthMs = 300_000;
        [SerializeField] int _startingBudget = 12;
        [SerializeField] int _trickleIntervalMs = 4_000;
        [SerializeField] bool _unbounded;

        /// <summary>
        /// docs/UI.md §12's "unlimited budget and no timer", said in Core's own
        /// terms rather than as a mode Core does not have: no head start, a
        /// dawn nobody will reach, no trickle, and a budget nobody can spend.
        /// Every rule still runs — fit, frontier, trap, explored — which is
        /// the point of a sandbox for trying cubes.
        /// </summary>
        public static readonly RoundSettings SandboxSettings = new RoundSettings(
            HeadStartMs: 0,
            RoundLengthMs: int.MaxValue,
            StartingBudget: int.MaxValue / 2,
            TrickleIntervalMs: 0);

        DreamInstance _dream;
        float _carryMs;

        public Round Round { get; private set; }
        public DreamInstance Dream => _dream;

        /// <summary>Whether this round runs on <see cref="SandboxSettings"/>: nothing to count down or spend.</summary>
        public bool Unbounded => Round != null ? SandboxSettings.Equals(Round.Settings) : _unbounded;

        /// <summary>
        /// The dream was handed a new lattice or derived state — a placement
        /// or an exploration — so the god view re-applies what it lays over
        /// the cubes: the cut-away, the buildable highlights, the ghost.
        /// </summary>
        public event Action Applied;

        /// <summary>
        /// Core appended an event on this machine — a placement or an
        /// exploration this round accepted — with the derived hash after it.
        /// The host's netcode broadcasts it (docs/NETCODE.md §5); nothing
        /// else listens.
        /// </summary>
        public event Action<LatticeEvent, ulong> EventAppended;

        internal void Configure(int headStartMs, int roundLengthMs, int startingBudget, int trickleIntervalMs)
        {
            _headStartMs = headStartMs;
            _roundLengthMs = roundLengthMs;
            _startingBudget = startingBudget;
            _trickleIntervalMs = trickleIntervalMs;
            _unbounded = false;
        }

        /// <summary>The Sandbox's round. M0.9's flow will choose per round; the Dream scene is built this way until then.</summary>
        internal void ConfigureUnbounded() => _unbounded = true;

        /// <summary>
        /// A test seam: a fresh round on other settings, so the bounded HUD can
        /// be tested in the scene built for the Sandbox. Not the tuning
        /// console's — `RoundSettings` says M0.9c is the only thing that
        /// changes a round's settings, and this changes the round instead.
        /// The dream is handed the new lattice, which retires the old one (a
        /// new round is a new dream). It is not M0.9's round restart either:
        /// a standing Sleeper, the HUD's binding and the Sandbox's exit count
        /// are left to the caller, and a bedroom-only dream keeps its cube.
        /// </summary>
        internal void Restart(RoundSettings settings)
        {
            Round = new Round(settings, _dream.Registry, _dream.StartTypeId, _dream.StartRotation,
                new[] { new PlayerId(LocalSleeper) });
            _carryMs = 0f;
            _dream.Apply(Round.Lattice, Round.Derived);
            Applied?.Invoke();
        }

        void Awake()
        {
            _dream = GetComponent<EmptyDream>().Ensure();
            RoundSettings settings = _unbounded
                ? SandboxSettings
                : new RoundSettings(_headStartMs, _roundLengthMs, StartingBudget: _startingBudget,
                    TrickleIntervalMs: _trickleIntervalMs);
            Round = new Round(settings,
                _dream.Registry, _dream.StartTypeId, _dream.StartRotation,
                new[] { new PlayerId(LocalSleeper) });
            _dream.SleeperArrived += OnArrived;
            _dream.Explored += OnExplored;
        }

        void OnDestroy()
        {
            if (_dream != null)
            {
                _dream.SleeperArrived -= OnArrived;
                _dream.Explored -= OnExplored;
            }
        }

        /// <summary>
        /// docs/CORE-API.md §10's "on Telemetry: UpdateSleeperCube", fed from
        /// the dream's own volumes since there is no wire here. Every arrival,
        /// so a Sleeper walking back into an explored room is where Core
        /// thinks they are, and the trap rule is judged from the right cube.
        /// </summary>
        void OnArrived(Coord cube) => Round.UpdateSleeperCube(LocalSleeper, cube);

        /// <summary>
        /// The Sleeper's body is gone — the Sandbox went back to the god view —
        /// so Core's Sleeper stands where the next drop-in will put them, the
        /// start cube, rather than as a phantom in the last room they reached
        /// with the trap rule judged from there.
        /// </summary>
        public void SleeperLeft() => Round.UpdateSleeperCube(LocalSleeper, Round.Lattice.Start);

        void Update()
        {
            // Integer milliseconds, carried between frames, because Core keeps
            // time in whole milliseconds and a truncated remainder every frame
            // would run the clock slow.
            _carryMs += Time.deltaTime * 1000f;
            int step = (int)_carryMs;
            if (step <= 0) return;
            _carryMs -= step;
            Round.Advance(step);
        }

        public RuleContext Context() => new RuleContext(
            Round.Lattice, Round.Derived, Round.Registry, Round.Sleepers, Round.Budget, Round.Settings);

        /// <summary>What Core would say, applying nothing. For the ghost.</summary>
        public PlaceVerdict Validate(PlaceRequest request) =>
            Round.Phase == Phase.Dawn ? new PlaceVerdict(PlaceError.NotADoor) : Rules.ValidatePlace(Context(), request);

        /// <summary>The Nightmare building. Applied to the dream when Core says yes.</summary>
        public PlaceVerdict TryPlace(PlaceRequest request)
        {
            PlaceVerdict verdict = Round.TryPlace(request);
            if (!verdict.Ok) return verdict;

            _dream.Apply(Round.Lattice, Round.Derived);
            EventAppended?.Invoke(Round.Log.Events[Round.Log.Events.Count - 1], Round.Derived.Hash);
            Applied?.Invoke();
            return verdict;
        }

        void OnExplored(Coord cube)
        {
            Round.UpdateSleeperCube(LocalSleeper, cube);
            if (Round.TryExplore(LocalSleeper, cube) == ExploreError.None)
            {
                _dream.Apply(Round.Lattice, Round.Derived);
                EventAppended?.Invoke(Round.Log.Events[Round.Log.Events.Count - 1], Round.Derived.Hash);
                Applied?.Invoke();
            }
        }
    }
}
