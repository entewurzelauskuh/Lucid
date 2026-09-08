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

        DreamInstance _dream;
        float _carryMs;

        public Round Round { get; private set; }
        public DreamInstance Dream => _dream;

        /// <summary>
        /// The dream was handed a new lattice or derived state — a placement
        /// or an exploration — so the god view re-applies what it lays over
        /// the cubes: the cut-away, the buildable highlights, the ghost.
        /// </summary>
        public event Action Applied;

        internal void Configure(int headStartMs, int roundLengthMs, int startingBudget, int trickleIntervalMs)
        {
            _headStartMs = headStartMs;
            _roundLengthMs = roundLengthMs;
            _startingBudget = startingBudget;
            _trickleIntervalMs = trickleIntervalMs;
        }

        void Awake()
        {
            _dream = GetComponent<EmptyDream>().Ensure();
            Round = new Round(
                new RoundSettings(_headStartMs, _roundLengthMs, StartingBudget: _startingBudget,
                    TrickleIntervalMs: _trickleIntervalMs),
                _dream.Registry, _dream.StartTypeId, _dream.StartRotation,
                new[] { new PlayerId(LocalSleeper) });
            _dream.Explored += OnExplored;
        }

        void OnDestroy()
        {
            if (_dream != null) _dream.Explored -= OnExplored;
        }

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
            Applied?.Invoke();
            return verdict;
        }

        void OnExplored(Coord cube)
        {
            Round.UpdateSleeperCube(LocalSleeper, cube);
            if (Round.TryExplore(LocalSleeper, cube) == ExploreError.None)
            {
                _dream.Apply(Round.Lattice, Round.Derived);
                Applied?.Invoke();
            }
        }
    }
}
