using System;
using Lucid.Core;
using Lucid.Runtime;
using UnityEngine;

namespace Lucid.Netcode
{
    /// <summary>
    /// A Sleeper's machine (docs/SPEC.md §14, docs/NETCODE.md §6): the dream
    /// stands up from the mirror, the body walks it, and the two things the
    /// rules care about go up the wire — the first entry into a cube and a
    /// white door touched — with a 10 Hz telemetry packet under them. What
    /// comes back down is applied to the dream and to the body: an event to
    /// the cubes, a wake verdict to the Sleeper.
    /// </summary>
    public sealed class DreamClient : IDisposable
    {
        public const float TelemetryHz = 10f;

        readonly RoundSync _sync;
        readonly DreamInstance _dream;
        readonly Func<Vector3, Vector3, SleeperMotor> _spawn;
        float _telemetryTimer;
        Coord _cube;
        bool _hasCube;
        ConnectorRef? _lastTouched;

        public DreamClient(RoundSync sync, DreamInstance dream, Func<Vector3, Vector3, SleeperMotor> spawnSleeper)
        {
            _sync = sync ?? throw new ArgumentNullException(nameof(sync));
            _dream = dream ?? throw new ArgumentNullException(nameof(dream));
            _spawn = spawnSleeper ?? throw new ArgumentNullException(nameof(spawnSleeper));

            _sync.OnRoundStarted += OnRoundStarted;
            _sync.OnLatticeApplied += OnLatticeApplied;
            _sync.OnWakeVerdict += HandleWakeVerdict;
            _dream.Explored += OnExplored;
            _dream.TouchedExit += OnTouchedExit;
            _dream.SleeperArrived += OnArrived;

            // RoundStart may already have landed before this end was listening.
            if (_sync.RoundStarted && _sync.Mirror != null) OnRoundStarted(_sync.Start);
        }

        public SleeperMotor Sleeper { get; private set; }
        public bool Woke { get; private set; }
        /// <summary>Which dream this machine is, from RoundStart: the index of its own client id among the Sleepers.</summary>
        public int DreamId { get; private set; } = -1;
        public int Explorations { get; private set; }
        public int ExitTouches { get; private set; }
        public int TelemetrySent { get; private set; }
        public TelemetryMsg LastTelemetry { get; private set; }

        /// <summary>For a test: the door a touch report went out for, which a refused wake rolls back into.</summary>
        internal void NoteTouched(ConnectorRef door) => _lastTouched = door;
        /// <summary>Where the body is, by the dream's own volumes; the start cube until it has stepped anywhere.</summary>
        public Coord Cube => _hasCube ? _cube : _dream.Start;

        void OnRoundStarted(RoundStartMsg start)
        {
            // A new round: a new body, and the last one's waking is over.
            Woke = false;
            _hasCube = false;
            _lastTouched = null;
            DreamId = -1;
            ulong me = _sync.NetworkManager.LocalClientId;
            for (int i = 0; i < start.SleeperCount; i++) if (start.SleeperClient(i) == me) { DreamId = i; break; }

            _dream.Apply(_sync.Mirror.Lattice, _sync.Mirror.Derived);
            if (Sleeper == null) Sleeper = _spawn(_dream.SpawnPoint, _dream.SpawnFacing);
            _sync.SendDreamReady(DreamId);
        }

        void OnLatticeApplied(LatticeEventMsg _, MirrorResult r)
        {
            if (r.Applied) _dream.Apply(_sync.Mirror.Lattice, _sync.Mirror.Derived);
        }

        void OnExplored(Coord cube)
        {
            Explorations++;
            _sync.SendExplored(cube);
        }

        void OnTouchedExit(ConnectorRef door)
        {
            if (Woke) return;
            ExitTouches++;
            _lastTouched = door;
            _sync.SendTouchedExit(door);
        }

        void OnArrived(Coord cube)
        {
            _cube = cube;
            _hasCube = true;
        }

        /// <summary>
        /// 311. Accepted: the Sleeper is awake and the body is done — M0.9's
        /// results and spectator view take it from here. Refused because a
        /// placement beat the report: "the client rolls the Sleeper back into
        /// the doorway" (docs/NETCODE.md §6) — a metre inside the room, on the
        /// door they touched, from where the new cube is the way on.
        /// </summary>
        internal void HandleWakeVerdict(WakeVerdictMsg verdict)
        {
            if (verdict.Accepted)
            {
                Woke = true;
                if (Sleeper != null) UnityEngine.Object.Destroy(Sleeper.gameObject);
                Sleeper = null;
                return;
            }

            if ((WakeVerdict)verdict.Reason == WakeVerdict.NotAnExit && Sleeper != null && _lastTouched != null)
                Sleeper.Warp(Doorway(_lastTouched.Value));
        }

        /// <summary>A metre inside the room, in front of the door, in the dream's frame.</summary>
        public Vector3 Doorway(ConnectorRef door) =>
            _dream.transform.TransformPoint(DreamSpace.Origin(door.Cube) + DreamSpace.Direction(door.Face) * (CubeMetrics.Half - 1f));

        /// <summary>302 at 10 Hz, from the body's pose in the cube it is in.</summary>
        public void Tick(float dt)
        {
            if (Sleeper == null || !_sync.RoundStarted) return;
            _telemetryTimer += dt;
            if (_telemetryTimer < 1f / TelemetryHz) return;
            _telemetryTimer = 0f;

            // Origin is the cube's floor centre; the wire's local position is
            // from its south-west floor corner (docs/NETCODE.md §11), so x and
            // z are shifted by half a cube and y is height above the floor.
            Vector3 local = Sleeper.Feet - _dream.transform.TransformPoint(DreamSpace.Origin(Cube));
            var t = new TelemetryMsg
            {
                Cube = WireCoord.From(Cube),
                LocalX = Quantise(local.x + CubeMetrics.Half), LocalY = Quantise(local.y), LocalZ = Quantise(local.z + CubeMetrics.Half),
                Yaw = (byte)Mathf.RoundToInt(Mathf.Repeat(Sleeper.transform.eulerAngles.y, 360f) / 360f * 255f),
                Health = 100, Lives = 1, Status = (byte)SleeperStatus.InDream, Flags = 0,
            };
            _sync.SendTelemetry(t);
            LastTelemetry = t;
            TelemetrySent++;
        }

        /// <summary>Metres to 1/256 m, clamped to the sixteen bits.</summary>
        static ushort Quantise(float metres) =>
            (ushort)Mathf.Clamp(Mathf.RoundToInt(metres * 256f), 0, ushort.MaxValue);

        public void Dispose()
        {
            _sync.OnRoundStarted -= OnRoundStarted;
            _sync.OnLatticeApplied -= OnLatticeApplied;
            _sync.OnWakeVerdict -= HandleWakeVerdict;
            _dream.Explored -= OnExplored;
            _dream.TouchedExit -= OnTouchedExit;
            _dream.SleeperArrived -= OnArrived;
        }
    }
}
