using System;
using System.Collections.Generic;
using System.IO;
using Lucid.Core;
using Lucid.Runtime;
using Unity.Netcode;
using UnityEngine;

namespace Lucid.Netcode
{
    /// <summary>
    /// docs/NETCODE.md §13: the RPCs of §4–§7 for M0.8 (101–103, 201–203,
    /// 301–304, 311, 401, 402, 408). On the host every handler is a thin call
    /// into the one <see cref="Round"/>; on a client the events land in a
    /// <see cref="LatticeMirror"/> and the rest are raised for the scene.
    /// </summary>
    /// <remarks>
    /// The host does not tick the round: whoever owns the <see cref="Round"/>
    /// advances it (the Sandbox's <see cref="LocalRound"/>, a test), and this
    /// watches for the phase to change and the budget to move. The host also
    /// receives its own broadcasts — NGO invokes ClientsAndHost locally — and
    /// ignores them, since it has the round itself rather than a mirror.
    /// </remarks>
    public sealed class RoundSync : NetworkBehaviour
    {
        const float BudgetHeartbeatSeconds = 1f;

        [SerializeField] DreamPack _pack;

        CubeRegistry _registry;
        Codec _codec;

        // ---- host ----
        Round _round;
        HashLedger _ledger;
        readonly Dictionary<ulong, int> _sleeperOf = new Dictionary<ulong, int>();
        readonly Dictionary<ulong, ushort> _lastTelemetry = new Dictionary<ulong, ushort>();
        readonly List<DesyncNoticeMsg> _desyncs = new List<DesyncNoticeMsg>();
        readonly HashSet<ulong> _desynced = new HashSet<ulong>();
        readonly HashSet<ulong> _dreamsReady = new HashSet<ulong>();
        ulong _nightmare;
        Phase _lastPhase;
        int _lastBudgetPoints = -1;
        float _budgetTimer;
        int _hashMatches;

        // ---- client ----
        ushort _nextReqId;
        ushort _nextTelemetrySeq;

        /// <summary>Set before the prefab is spawned; the registry both ends build from it is what the hashes are over.</summary>
        public void Configure(DreamPack pack) => _pack = pack;

        public CubeRegistry Registry => _registry;
        public Codec Codec => _codec;

        /// <summary>The host's round, once <see cref="BeginRound"/> has run; null on a client.</summary>
        public Round HostRound => _round;
        public LatticeMirror Mirror { get; private set; }
        public bool RoundStarted { get; private set; }
        public RoundStartMsg Start { get; private set; }
        /// <summary>On a client, the phase the host last announced; on the host, the round's.</summary>
        public Phase Phase => _round != null ? _round.Phase : _clientPhase;
        Phase _clientPhase;
        public IReadOnlyList<DesyncNoticeMsg> Desyncs => _desyncs;
        public int HashMatches => _hashMatches;
        public string LastSavedLog { get; private set; }
        public PlaceReplyMsg LastPlaceReply { get; private set; }
        public WakeVerdictMsg LastWakeVerdict { get; private set; }
        public BudgetStateMsg LastBudget { get; private set; }
        public IReadOnlyCollection<ulong> DreamsReady => _dreamsReady;

        // Counts, so a caller can wait for "one more arrived" rather than on a value a default already has.
        public int PlaceReplies { get; private set; }
        public int WakeVerdicts { get; private set; }
        public int AppliedEvents { get; private set; }
        public int BudgetStates { get; private set; }
        public int SleeperStatuses { get; private set; }
        public int DesyncNotices { get; private set; }
        /// <summary>Mismatches after a client's first: hashes are cumulative, so one divergence is every later report too. Counted, not announced.</summary>
        public int SuppressedDesyncs { get; private set; }
        public int Faults { get; private set; }
        public SleeperStatusMsg LastSleeperStatus { get; private set; }

        /// <summary>Where a desync's .lucidlog goes; a test points it at a temp folder.</summary>
        public string LogFolder { get; set; }

        public event Action<RoundStartMsg> OnRoundStarted;
        public event Action<LatticeEventMsg, MirrorResult> OnLatticeApplied;
        public event Action<PhaseChangedMsg> OnPhaseChanged;
        public event Action<SleeperStatusMsg> OnSleeperStatus;
        public event Action<DesyncNoticeMsg> OnDesync;
        public event Action<WakeVerdictMsg> OnWakeVerdict;
        public event Action<PlaceReplyMsg> OnPlaceReply;
        public event Action<BudgetStateMsg> OnBudgetState;
        public event Action<ulong, sbyte> OnDreamReady;

        public override void OnNetworkSpawn()
        {
            if (_pack == null)
            {
                Debug.LogError($"{name}: no DreamPack; the registry the hashes are over cannot be built", this);
                return;
            }
            _registry = new CubeRegistry();
            _pack.RegisterAll(_registry);
            _codec = new Codec(_registry);
        }

        // ---- host: the round begins -----------------------------------------------------

        /// <summary>
        /// 101. The host's round and who is in it: the Nightmare's client and
        /// the Sleepers' by dream id. Every client builds its mirror from this.
        /// </summary>
        public void BeginRound(Round round, ulong nightmareClientId, IReadOnlyList<ulong> sleeperClientIds)
        {
            if (!IsServer) throw new InvalidOperationException("only the host begins a round");
            if (round == null) throw new ArgumentNullException(nameof(round));
            if (sleeperClientIds == null) throw new ArgumentNullException(nameof(sleeperClientIds));
            if (sleeperClientIds.Count > 4) throw new ArgumentOutOfRangeException(nameof(sleeperClientIds), "four dreams at most");

            _round = round;
            _ledger = new HashLedger();
            _nightmare = nightmareClientId;
            _sleeperOf.Clear();
            _lastTelemetry.Clear();
            _desyncs.Clear();
            _desynced.Clear();
            _dreamsReady.Clear();
            _hashMatches = 0;
            SuppressedDesyncs = 0;
            _lastPhase = round.Phase;
            _lastBudgetPoints = -1;

            var msg = new RoundStartMsg
            {
                HeadStartMs = round.Settings.HeadStartMs, RoundLengthMs = round.Settings.RoundLengthMs, Lives = round.Settings.Lives,
                StartingBudget = round.Settings.StartingBudget, TrickleIntervalMs = round.Settings.TrickleIntervalMs,
                ExitHysteresis = round.Settings.ExitHysteresis,
                Seed = 0, NightmareClientId = nightmareClientId, SleeperCount = (byte)sleeperClientIds.Count,
                StartTypeIndex = _codec.TypeIndex(round.Lattice.At(round.Lattice.Start).TypeId),
                StartRotation = (byte)round.Lattice.At(round.Lattice.Start).Rotation,
                RegistryHash = _registry.ContentHash(),
                RoundStartServerTime = NetworkManager.ServerTime.Time,
            };
            for (int i = 0; i < sleeperClientIds.Count; i++)
            {
                msg.SetSleeper(i, sleeperClientIds[i]);
                _sleeperOf[sleeperClientIds[i]] = i;
            }
            Start = msg;
            RoundStarted = true;
            RoundStartRpc(msg);
            // A round that has already left its head start — a Sleeper who
            // connected late in the dev scene, M1.4's resume — is told where
            // it stands; RoundStart alone would leave the client at HeadStart.
            if (round.Phase != Phase.HeadStart)
                PhaseChangedRpc(new PhaseChangedMsg { Phase = (byte)round.Phase, AtServerTime = NetworkManager.ServerTime.Time });
            SendBudget();
        }

        /// <summary>A test's hand on the wire: an event the host's log never held.</summary>
        internal void BroadcastRaw(LatticeEventMsg m)
        {
            _ledger.Broadcast(m.Seq, m.PostHash);
            LatticeEventRpc(m);
        }

        /// <summary>Broadcast handlers run on every client; only the host may have sent them (NGO proxies a client's ClientsAndHost through the server).</summary>
        static bool FromHost(RpcParams rpc) => rpc.Receive.SenderClientId == NetworkManager.ServerClientId;

        /// <summary>201 for an event Core appended on this machine (the host's own Nightmare, the host's own dream).</summary>
        public void Broadcast(LatticeEvent e, ulong postHash)
        {
            if (!IsServer || _round == null) return;
            LatticeEventMsg m = _codec.Encode(e, postHash);
            _ledger.Broadcast(m.Seq, m.PostHash);
            LatticeEventRpc(m);
        }

        void BroadcastLast()
        {
            IReadOnlyList<LatticeEvent> events = _round.Log.Events;
            Broadcast(events[events.Count - 1], _round.Derived.Hash);
        }

        void Update()
        {
            if (!IsServer || _round == null) return;

            if (_round.Phase != _lastPhase)
            {
                _lastPhase = _round.Phase;
                PhaseChangedRpc(new PhaseChangedMsg { Phase = (byte)_round.Phase, AtServerTime = NetworkManager.ServerTime.Time });
            }

            // 408: after every change and at 1 Hz.
            _budgetTimer += Time.deltaTime;
            if (_round.Budget.Points != _lastBudgetPoints || _budgetTimer >= BudgetHeartbeatSeconds) SendBudget();
        }

        void SendBudget()
        {
            _budgetTimer = 0f;
            _lastBudgetPoints = _round.Budget.Points;
            var msg = new BudgetStateMsg { Points = _round.Budget.Points, MsUntilNextPoint = _round.Budget.MsUntilNextPoint, CooldownCount = 0, PossessionActive = false };
            BudgetStateRpc(msg, RpcTarget.Single(_nightmare, RpcTargetUse.Temp));
        }

        bool TrySleeper(ulong clientId, out int sleeperId) => _sleeperOf.TryGetValue(clientId, out sleeperId);

        // ---- client: what the scene sends ------------------------------------------------

        /// <summary>401. Returns the request id the reply will carry.</summary>
        public ushort SendPlaceRequest(PlaceRequest request)
        {
            ushort id = _nextReqId++;
            PlaceRequestRpc(_codec.Encode(request, id));
            return id;
        }

        public void SendExplored(Coord cube) => ExploredRpc(new ExploredMsg { Cube = WireCoord.From(cube) });

        public void SendTouchedExit(ConnectorRef door) =>
            TouchedExitRpc(new TouchedExitMsg { Cube = WireCoord.From(door.Cube), Face = (byte)door.Face });

        public void SendDreamReady(int dreamId) => DreamReadyRpc(new DreamReadyMsg { DreamId = (sbyte)dreamId });

        /// <summary>302. The sequence number is this end's; the host drops anything older than the last it saw.</summary>
        public void SendTelemetry(TelemetryMsg t)
        {
            t.Seq = _nextTelemetrySeq++;
            SendTelemetryRaw(t);
        }

        internal void SendTelemetryRaw(TelemetryMsg t) => TelemetryRpc(t);

        /// <summary>401 by hand, for a test that sends what the codec would refuse to build.</summary>
        internal void SendPlaceRequestRaw(PlaceRequestMsg m) => PlaceRequestRpc(m);

        /// <summary>202, by hand: a test corrupts a hash to prove the host notices.</summary>
        internal void ReportHash(uint seq, ulong hash) => HashReportRpc(new HashReportMsg { Seq = seq, Hash = hash });

        // ---- 1xx: round lifecycle, host → all -------------------------------------------

        [Rpc(SendTo.ClientsAndHost)]
        void RoundStartRpc(RoundStartMsg msg, RpcParams rpc = default)
        {
            if (IsServer || !FromHost(rpc)) return;
            if (_codec == null) return;

            if (msg.RegistryHash != _registry.ContentHash())
            {
                // Cannot differ when contentHash matched at approval (§4); a guard.
                Debug.LogError($"{name}: registry hash {msg.RegistryHash:x16} != ours {_registry.ContentHash():x16}; leaving", this);
                NetworkManager.Shutdown();
                return;
            }
            if (!_codec.TryTypeId(msg.StartTypeIndex, out string startType))
            {
                Debug.LogError($"{name}: start type index {msg.StartTypeIndex} is not in the registry; leaving", this);
                NetworkManager.Shutdown();
                return;
            }

            Start = msg;
            Mirror = new LatticeMirror(_codec, startType, (Rotation)msg.StartRotation, msg.Settings);
            _clientPhase = msg.HeadStartMs <= 0 ? Phase.Running : Phase.HeadStart;
            RoundStarted = true;
            OnRoundStarted?.Invoke(msg);
        }

        [Rpc(SendTo.ClientsAndHost)]
        void PhaseChangedRpc(PhaseChangedMsg msg, RpcParams rpc = default)
        {
            if (!FromHost(rpc)) return;
            if (!IsServer) _clientPhase = (Phase)msg.Phase;
            OnPhaseChanged?.Invoke(msg);
        }

        [Rpc(SendTo.ClientsAndHost)]
        void SleeperStatusRpc(SleeperStatusMsg msg, RpcParams rpc = default)
        {
            if (!FromHost(rpc)) return;
            SleeperStatuses++;
            LastSleeperStatus = msg;
            OnSleeperStatus?.Invoke(msg);
        }

        // ---- 2xx: the lattice --------------------------------------------------------------

        [Rpc(SendTo.ClientsAndHost)]
        void LatticeEventRpc(LatticeEventMsg msg, RpcParams rpc = default)
        {
            if (IsServer || !FromHost(rpc)) return;
            if (Mirror == null)
            {
                Debug.LogError($"{name}: a lattice event before RoundStart (seq {msg.Seq})", this);
                return;
            }

            MirrorResult r = Mirror.Apply(msg);
            if (r.Gap)
            {
                // A lost reliable message cannot happen; ResumeRequest is M1.4's recovery (§10).
                Debug.LogError($"{name}: seq gap — got {msg.Seq}, expected {Mirror.NextSeq}", this);
                return;
            }
            if (r.Unknown || r.Faulted)
            {
                // Not applied, and the host must hear so rather than wait: a
                // hash that cannot be the host's is the report (§5).
                Faults++;
                Debug.LogError($"{name}: seq {msg.Seq} {(r.Unknown ? "names a type or kind this registry has not got" : "cannot be applied to this lattice")}; reporting the fault", this);
                HashReportRpc(new HashReportMsg { Seq = msg.Seq, Hash = ~msg.PostHash });
                return;
            }

            HashReportRpc(new HashReportMsg { Seq = msg.Seq, Hash = r.Hash });
            if (!r.InSync) Debug.LogWarning($"{name}: seq {msg.Seq}: {r}", this);
            AppliedEvents++;
            OnLatticeApplied?.Invoke(msg, r);
        }

        [Rpc(SendTo.Server)]
        void HashReportRpc(HashReportMsg msg, RpcParams rpc = default)
        {
            if (_ledger == null) return;
            ulong client = rpc.Receive.SenderClientId;
            switch (_ledger.Report(msg.Seq, msg.Hash))
            {
                case HashLedger.Verdict.Match:
                    _hashMatches++;
                    break;
                case HashLedger.Verdict.UnknownSeq:
                    Debug.LogWarning($"{name}: client {client} reported seq {msg.Seq}, which was never broadcast", this);
                    break;
                case HashLedger.Verdict.Mismatch:
                    if (!_desynced.Add(client))
                    {
                        // Every report after the first from a diverged client
                        // mismatches too; one notice and one log per client.
                        SuppressedDesyncs++;
                        break;
                    }
                    var notice = new DesyncNoticeMsg { Seq = msg.Seq, ClientId = client };
                    _desyncs.Add(notice);
                    LastSavedLog = SaveLog(client, $"desync at seq {msg.Seq} from client {client}: reported {msg.Hash:x16}");
                    Debug.LogError($"{name}: DESYNC at seq {msg.Seq} from client {client}; log saved to {LastSavedLog}", this);
                    DesyncNoticeRpc(notice);
                    break;
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        void DesyncNoticeRpc(DesyncNoticeMsg msg, RpcParams rpc = default)
        {
            if (!FromHost(rpc)) return;
            DesyncNotices++;
            OnDesync?.Invoke(msg);
        }

        /// <summary>§5, §14: the automatic .lucidlog. Never throws into an RPC handler; a failed save is logged and the notice still goes out.</summary>
        string SaveLog(ulong client, string note)
        {
            try
            {
                string folder = LogFolder ?? Path.Combine(Application.persistentDataPath, "lucidlogs");
                Directory.CreateDirectory(folder);
                string path = Path.Combine(folder, $"{DateTime.UtcNow:yyyyMMdd-HHmmss}-client{client}-seq{_round.Log.NextSeq}.lucidlog");
                var players = new List<LucidLog.Player>();
                foreach (SleeperState s in _round.Sleepers) players.Add(new LucidLog.Player(s.Id, s.Player.ToString()));
                using (FileStream f = File.Create(path))
                    LucidLog.Write(f, _round.Settings, players, _registry, _round.Log, note);
                return path;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                Debug.LogError($"{name}: could not save the .lucidlog: {e.Message}", this);
                return null;
            }
        }

        // ---- 3xx: Sleeper ↔ host ----------------------------------------------------------

        [Rpc(SendTo.Server)]
        void DreamReadyRpc(DreamReadyMsg msg, RpcParams rpc = default)
        {
            _dreamsReady.Add(rpc.Receive.SenderClientId);
            OnDreamReady?.Invoke(rpc.Receive.SenderClientId, msg.DreamId);
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
        void TelemetryRpc(TelemetryMsg msg, RpcParams rpc = default)
        {
            if (_round == null) return;
            ulong client = rpc.Receive.SenderClientId;
            if (!TrySleeper(client, out int sleeper)) return;

            // Sequenced: a packet older than the last one seen is dropped, with
            // the wrap the sixteen bits allow.
            if (_lastTelemetry.TryGetValue(client, out ushort last) && (short)(msg.Seq - last) <= 0) return;
            _lastTelemetry[client] = msg.Seq;

            _round.UpdateSleeperCube(sleeper, msg.Cube.ToCoord());
        }

        [Rpc(SendTo.Server)]
        void ExploredRpc(ExploredMsg msg, RpcParams rpc = default)
        {
            if (_round == null) return;
            if (!TrySleeper(rpc.Receive.SenderClientId, out int sleeper)) return;

            Coord cube = msg.Cube.ToCoord();
            _round.UpdateSleeperCube(sleeper, cube);
            if (_round.TryExplore(sleeper, cube) == ExploreError.None) BroadcastLast();
        }

        [Rpc(SendTo.Server)]
        void TouchedExitRpc(TouchedExitMsg msg, RpcParams rpc = default)
        {
            ulong client = rpc.Receive.SenderClientId;
            var target = RpcTarget.Single(client, RpcTargetUse.Temp);
            if (_round == null || !TrySleeper(client, out int sleeper))
            {
                WakeVerdictRpc(new WakeVerdictMsg { Accepted = false, Reason = (byte)WakeVerdict.NotInDream }, target);
                return;
            }

            WakeVerdict w;
            try
            {
                w = _round.TryWake(sleeper, new ConnectorRef(msg.Cube.ToCoord(), (Face)msg.Face));
            }
            catch (Exception e)
            {
                // §14: answered with an error rather than dropped, and logged with the message id.
                Debug.LogError($"{name}: {Message.TouchedExit} from client {client} threw: {e}", this);
                WakeVerdictRpc(new WakeVerdictMsg { Accepted = false, Reason = (byte)WakeVerdict.NotInDream }, target);
                return;
            }
            WakeVerdictRpc(new WakeVerdictMsg { Accepted = w == WakeVerdict.Woke, Reason = (byte)w }, target);
            if (w == WakeVerdict.Woke)
            {
                SleeperState s = _round.Sleepers[sleeper];
                SleeperStatusRpc(new SleeperStatusMsg { DreamId = (sbyte)sleeper, Status = (byte)s.Status, LivesLeft = (byte)s.Lives, AtClockMs = (uint)_round.ClockMs, Cause = 0 });
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void WakeVerdictRpc(WakeVerdictMsg msg, RpcParams rpc)
        {
            WakeVerdicts++;
            LastWakeVerdict = msg;
            OnWakeVerdict?.Invoke(msg);
        }

        // ---- 4xx: Nightmare ↔ host --------------------------------------------------------

        [Rpc(SendTo.Server)]
        void PlaceRequestRpc(PlaceRequestMsg msg, RpcParams rpc = default)
        {
            ulong client = rpc.Receive.SenderClientId;
            var target = RpcTarget.Single(client, RpcTargetUse.Temp);
            if (_round == null || client != _nightmare)
            {
                PlaceReplyRpc(new PlaceReplyMsg { ReqId = msg.ReqId, Verdict = (byte)PlaceError.NotADoor, TrappedDreamId = -1 }, target);
                return;
            }

            PlaceRequest request = _codec.Decode(msg);
            if (request == null)
            {
                PlaceReplyRpc(new PlaceReplyMsg { ReqId = msg.ReqId, Verdict = (byte)PlaceError.UnknownType, TrappedDreamId = -1 }, target);
                return;
            }

            PlaceVerdict v;
            try
            {
                v = _round.TryPlace(request);
            }
            catch (Exception e)
            {
                // §14: the request is answered with an error rather than
                // dropped, so the Nightmare's ghost never waits for ever.
                Debug.LogError($"{name}: {Message.PlaceRequest} {msg.ReqId} from client {client} threw: {e}", this);
                PlaceReplyRpc(new PlaceReplyMsg { ReqId = msg.ReqId, Verdict = (byte)PlaceError.NotADoor, TrappedDreamId = -1 }, target);
                return;
            }
            PlaceReplyRpc(new PlaceReplyMsg { ReqId = msg.ReqId, Verdict = (byte)v.Error, TrappedDreamId = (sbyte)v.TrappedSleeper }, target);
            if (v.Ok)
            {
                BroadcastLast();
                SendBudget();
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void PlaceReplyRpc(PlaceReplyMsg msg, RpcParams rpc)
        {
            PlaceReplies++;
            LastPlaceReply = msg;
            OnPlaceReply?.Invoke(msg);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void BudgetStateRpc(BudgetStateMsg msg, RpcParams rpc)
        {
            BudgetStates++;
            LastBudget = msg;
            OnBudgetState?.Invoke(msg);
        }
    }
}
