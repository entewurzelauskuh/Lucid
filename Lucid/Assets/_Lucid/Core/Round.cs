using System;
using System.Collections.Generic;

namespace Lucid.Core
{
    /// <summary>
    /// The host's single object. The netcode layer forwards requests into it
    /// and broadcasts what it appends to the log; nothing else adjudicates
    /// (docs/CORE-API.md §8, §10).
    /// </summary>
    public sealed class Round
    {
        readonly SleeperState[] _sleepers;

        public Round(
            RoundSettings settings,
            CubeRegistry reg,
            string startTypeId,
            Rotation startRotation,
            IReadOnlyList<PlayerId> sleepers)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Registry = reg ?? throw new ArgumentNullException(nameof(reg));
            if (sleepers == null) throw new ArgumentNullException(nameof(sleepers));

            Lattice = Lattice.New(reg, startTypeId, startRotation);
            Derived = Deriver.Derive(Lattice, reg, settings.ExitHysteresis);
            Budget = new Budget(settings.StartingBudget, settings.TrickleIntervalMs);
            Log = new EventLog();

            // Phase is a function of the clock everywhere else, so a zero-length
            // head start must not leave the round stuck behind the mist.
            if (settings.HeadStartMs <= 0) Phase = Phase.Running;

            _sleepers = new SleeperState[sleepers.Count];
            for (int i = 0; i < sleepers.Count; i++)
            {
                _sleepers[i] = new SleeperState(
                    i, sleepers[i], SleeperStatus.InDream, Lattice.Start, settings.Lives);
            }
        }

        public RoundSettings Settings { get; }
        public CubeRegistry Registry { get; }
        public Phase Phase { get; private set; } = Phase.HeadStart;
        public int ClockMs { get; private set; }
        public Lattice Lattice { get; private set; }
        public Derived Derived { get; private set; }
        public Budget Budget { get; }
        public EventLog Log { get; }
        public IReadOnlyList<SleeperState> Sleepers => _sleepers;

        /// <summary>
        /// True once nobody is left running, or once dawn has taken them. The
        /// timer is the Nightmare's only way to win (docs/SPEC.md §7).
        /// </summary>
        public bool IsOver
        {
            get
            {
                if (Phase == Phase.Dawn) return true;
                foreach (SleeperState s in _sleepers)
                {
                    // Disconnected is neither Awake nor Consumed: the player is
                    // inside their reconnect grace and may still be running
                    // (docs/NETCODE.md §10). Ending here would end the round on
                    // a two-second network blip, then un-end it on their return.
                    if (s.Status != SleeperStatus.Awake && s.Status != SleeperStatus.Consumed)
                        return false;
                }
                return true;
            }
        }

        public void Advance(int deltaMs)
        {
            if (deltaMs < 0) throw new ArgumentOutOfRangeException(nameof(deltaMs));
            if (Phase == Phase.Dawn) return;

            // Clamp to dawn: time after the round ends is not the round's to
            // give, and an unclamped budget would make the Nightmare's final
            // total depend on the host's tick size.
            int remaining = Settings.RoundLengthMs - ClockMs;
            int step = deltaMs < remaining ? deltaMs : remaining;
            if (step < 0) step = 0;

            ClockMs += step;
            Budget.Advance(step);

            if (Phase == Phase.HeadStart && ClockMs >= Settings.HeadStartMs) Phase = Phase.Running;

            if (ClockMs >= Settings.RoundLengthMs)
            {
                Phase = Phase.Dawn;
                // Dawn takes everyone who has not woken, including anyone still
                // inside their reconnect grace (docs/NETCODE.md §10).
                for (int i = 0; i < _sleepers.Length; i++)
                {
                    SleeperStatus status = _sleepers[i].Status;
                    if (status == SleeperStatus.InDream || status == SleeperStatus.Disconnected)
                        _sleepers[i] = _sleepers[i] with { Status = SleeperStatus.Consumed, Lives = 0 };
                }
            }
        }

        public PlaceVerdict TryPlace(PlaceRequest r)
        {
            // Dawn ends the building. Every client re-derives from the broadcast
            // log, so a placement accepted now would grow the maze on the
            // results screen.
            if (Phase == Phase.Dawn) return new PlaceVerdict(PlaceError.NotADoor);

            PlaceVerdict verdict = Rules.ValidatePlace(Context(), r);
            if (!verdict.Ok) return verdict;

            long seq = Log.NextSeq;
            (Lattice lattice, Derived derived) = Rules.ApplyPlace(Context(), r, seq);
            Lattice = lattice;
            Derived = derived;

            Budget.TrySpend(Registry.Get(r.TypeId).Cost);
            Log.Append(new CubePlaced(
                seq, r.Target.Cube.Offset(r.Target.Face), r.TypeId, r.Rotation, r.SkinId));

            return verdict;
        }

        public ExploreError TryExplore(int sleeperId, Coord cube)
        {
            if (Phase == Phase.Dawn) return ExploreError.NoCube;

            // The reporter must be a Sleeper who is actually running: their id
            // goes into the log and out to every client, and a late packet from
            // someone already awake would still seal doors for everyone.
            if (!TryIndex(sleeperId, out int reporter)) return ExploreError.NoCube;
            if (_sleepers[reporter].Status != SleeperStatus.InDream) return ExploreError.NoCube;

            ExploreError error = Rules.ValidateExplore(Context(), cube);
            if (error != ExploreError.None) return error;

            long seq = Log.NextSeq;
            (Lattice lattice, Derived derived) = Rules.ApplyExplore(Context(), cube, seq);
            Lattice = lattice;
            Derived = derived;

            Log.Append(new CubeExplored(seq, cube, sleeperId));
            return ExploreError.None;
        }

        /// <summary>
        /// From 10 Hz telemetry. A coord with no cube is dropped rather than
        /// stored: the leak rule reads this, and an off-lattice Sleeper would
        /// report as unable to reach any exit and block every placement for the
        /// rest of the round.
        /// </summary>
        public void UpdateSleeperCube(int sleeperId, Coord cube)
        {
            if (!TryIndex(sleeperId, out int i)) return;
            if (_sleepers[i].Status != SleeperStatus.InDream) return;
            if (!Lattice.Has(cube)) return;

            _sleepers[i] = _sleepers[i] with { Cube = cube };
        }

        public WakeVerdict TryWake(int sleeperId, ConnectorRef door)
        {
            if (!TryIndex(sleeperId, out int i)) return WakeVerdict.NotInDream;
            if (_sleepers[i].Status != SleeperStatus.InDream) return WakeVerdict.NotInDream;

            // The start door is misted until the head start ends (docs/SPEC.md §11).
            if (Phase == Phase.HeadStart) return WakeVerdict.HeadStart;

            // Belt and braces. Dawn consumes everyone who has not woken, so the
            // status check above already covers this; the guard states the
            // intent so a future change to dawn's sweep cannot quietly reopen
            // waking after the round is over. Unreachable by design, like
            // PlaceError.DoorOccupied.
            if (Phase == Phase.Dawn) return WakeVerdict.NotInDream;

            // Confirmed only if the door is still an exit when it arrives: a
            // placement may have beaten the report here (docs/SPEC.md §14).
            if (Derived.StateOf(door) != ConnectorState.Exit) return WakeVerdict.NotAnExit;

            _sleepers[i] = _sleepers[i] with { Status = SleeperStatus.Awake, WokeAtMs = ClockMs };
            return WakeVerdict.Woke;
        }

        public DeathVerdict ReportDeath(int sleeperId)
        {
            if (!TryIndex(sleeperId, out int i)) return DeathVerdict.Ignored;
            if (_sleepers[i].Status != SleeperStatus.InDream) return DeathVerdict.Ignored;

            int lives = _sleepers[i].Lives - 1;
            if (lives > 0)
            {
                _sleepers[i] = _sleepers[i] with { Lives = lives, Cube = Lattice.Start };
                return new DeathVerdict(DeathOutcome.LostLife, lives);
            }

            _sleepers[i] = _sleepers[i] with { Lives = 0, Status = SleeperStatus.Consumed };
            return new DeathVerdict(DeathOutcome.Consumed, 0);
        }

        public void ReportDisconnect(int sleeperId, bool reconnected)
        {
            if (!TryIndex(sleeperId, out int i)) return;

            if (reconnected)
            {
                if (_sleepers[i].Status == SleeperStatus.Disconnected)
                    _sleepers[i] = _sleepers[i] with { Status = SleeperStatus.InDream };
                return;
            }

            if (_sleepers[i].Status == SleeperStatus.InDream)
                _sleepers[i] = _sleepers[i] with { Status = SleeperStatus.Disconnected };
        }

        RuleContext Context() =>
            new RuleContext(Lattice, Derived, Registry, _sleepers, Budget, Settings);

        bool TryIndex(int sleeperId, out int index)
        {
            index = sleeperId;
            return sleeperId >= 0 && sleeperId < _sleepers.Length;
        }
    }
}
