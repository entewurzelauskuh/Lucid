using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Lucid.Core;
using Lucid.Netcode;
using Lucid.Runtime;
using Lucid.Tests.PlayMode.Dream;
using NUnit.Framework;
using Unity.Netcode;
using Unity.Netcode.TestHelpers.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Lucid.Tests.PlayMode.Netcode
{
    /// <summary>
    /// A host and one client in this process over Unity Transport on loopback
    /// — the harness NGO tests itself with. The host is the Nightmare, the
    /// client a Sleeper's machine. Fixtures derive from this and add what they
    /// stand up on the client's side.
    /// </summary>
    public abstract class NetHarness : NetcodeIntegrationTest
    {
        protected const string Start = "test.start";
        protected const string Straight = "test.straight";
        protected const string Tee = "test.tee";
        protected const string Corner = "test.corner";

        protected override int NumberOfClients => 1;

        // Nothing in Lucid is a player NetworkObject (docs/SPEC.md §14): the
        // approval says so, and the harness must not expect one.
        protected override bool ShouldCheckForSpawnedPlayers() => false;

        protected readonly List<Object> _assets = new List<Object>();
        protected readonly List<GameObject> _spawned = new List<GameObject>();
        protected GameObject _prefab;
        protected DreamPack _pack;
        protected Hello _hello;
        protected RoundSync _host, _client;
        protected Round _round;
        protected string _logFolder;

        protected override void OnServerAndClientsCreated()
        {
            _pack = CodeBuiltPack.Create(_assets, _spawned,
                (Start, FaceMask.North, CubeCategory.Start, false),
                (Straight, FaceMask.North | FaceMask.South, CubeCategory.Connector, false),
                (Tee, FaceMask.North | FaceMask.East | FaceMask.South, CubeCategory.Connector, false),
                (Corner, FaceMask.North | FaceMask.East, CubeCategory.Connector, false));

            _prefab = NetcodeIntegrationTestHelpers.CreateNetworkObjectPrefab("RoundSync", m_ServerNetworkManager, m_ClientNetworkManagers);
            _prefab.AddComponent<RoundSync>().Configure(_pack);

            // 001 on the happy path: the client says who it is, the host checks.
            var registry = new CubeRegistry();
            _pack.RegisterAll(registry);
            _hello = NetSession.LocalHello(registry, "Anna");
            NetSession.ArmApproval(m_ServerNetworkManager, _hello);
            foreach (NetworkManager client in m_ClientNetworkManagers) NetSession.PrepareClient(client, _hello);
        }

        protected override IEnumerator OnTearDown()
        {
            OnTearDownClient();
            foreach (GameObject go in _spawned) if (go != null) Object.DestroyImmediate(go);
            _spawned.Clear();
            foreach (Object asset in _assets) if (asset != null) Object.DestroyImmediate(asset);
            _assets.Clear();
            if (_logFolder != null && Directory.Exists(_logFolder)) Directory.Delete(_logFolder, true);
            _logFolder = null;
            yield return base.OnTearDown();
        }

        protected virtual void OnTearDownClient() { }

        protected ulong ClientId => m_ClientNetworkManagers[0].LocalClientId;

        /// <summary>The client's side stands up here, before RoundStart lands.</summary>
        protected virtual void OnClientSyncReady(RoundSync client) { }

        protected RoundSync ClientSync()
        {
            foreach (RoundSync s in Object.FindObjectsByType<RoundSync>(FindObjectsSortMode.None))
                if (s.IsSpawned && s.NetworkManager == m_ClientNetworkManagers[0]) return s;
            return null;
        }

        /// <summary>Spawns the sync, begins a round with the client as Sleeper 0, and waits for RoundStart to land.</summary>
        protected IEnumerator Begin(RoundSettings settings = null, System.Action<Round> before = null)
        {
            GameObject go = SpawnObject(_prefab, m_ServerNetworkManager);
            _host = go.GetComponent<RoundSync>();
            yield return WaitForConditionOrTimeOut(() => ClientSync() != null);
            AssertOnTimeout("the client never spawned its RoundSync");
            _client = ClientSync();
            OnClientSyncReady(_client);

            _round = new Round(settings ?? new RoundSettings(HeadStartMs: 0), _host.Registry, Start, Rotation.R0,
                new[] { new PlayerId((int)ClientId) });
            before?.Invoke(_round);
            _host.BeginRound(_round, m_ServerNetworkManager.LocalClientId, new[] { ClientId });

            yield return WaitForConditionOrTimeOut(() => _client.RoundStarted);
            AssertOnTimeout("RoundStart never reached the client");
        }

        protected static PlaceRequest On(Coord cube, Face face, string type, Rotation rotation = Rotation.R0) =>
            new PlaceRequest(new ConnectorRef(cube, face), type, rotation, "*");

        /// <summary>The host's Nightmare places (401/402), and the event reaches the client (201/202).</summary>
        protected IEnumerator Place(PlaceRequest request, PlaceError expected = PlaceError.None)
        {
            int replies = _host.PlaceReplies, applied = _client.AppliedEvents, matches = _host.HashMatches;
            ushort id = _host.SendPlaceRequest(request);
            yield return WaitForConditionOrTimeOut(() => _host.PlaceReplies > replies);
            AssertOnTimeout("no PlaceReply");
            Assert.That(_host.LastPlaceReply.ReqId, Is.EqualTo(id));
            Assert.That((PlaceError)_host.LastPlaceReply.Verdict, Is.EqualTo(expected), $"{request.TypeId} on {request.Target}");
            if (expected != PlaceError.None) yield break;

            yield return WaitForConditionOrTimeOut(() => _client.AppliedEvents > applied && _host.HashMatches > matches);
            AssertOnTimeout("the LatticeEvent or its HashReport never arrived");
        }

        protected IEnumerator Explore(Coord cube)
        {
            int applied = _client.AppliedEvents, matches = _host.HashMatches;
            _client.SendExplored(cube);
            yield return WaitForConditionOrTimeOut(() => _client.AppliedEvents > applied && _host.HashMatches > matches);
            AssertOnTimeout("the exploration never came back as an event");
        }

        protected void AssertInSync(string when)
        {
            Assert.That(_client.Mirror.Derived.Hash, Is.EqualTo(_round.Derived.Hash), $"{when}: the client's hash is not the host's");
            Assert.That(_client.Mirror.Lattice.Cubes.Count, Is.EqualTo(_round.Lattice.Cubes.Count));
            Assert.That(_client.Mirror.Derived.Exits, Is.EqualTo(_round.Derived.Exits));
        }

    }

    /// <summary>
    /// M0.8's acceptance (docs/WORKPLAN.md §4, docs/NETCODE.md §15) on the
    /// wire alone: nothing here stands a cube up, because what is under test
    /// is the protocol and the two ends' agreement about the lattice.
    /// </summary>
    public sealed class RoundSyncTests : NetHarness
    {
        [UnityTest]
        public IEnumerator RoundStartBuildsIdenticalLattices()
        {
            yield return Begin();

            Assert.That(_client.Start.RegistryHash, Is.EqualTo(_host.Registry.ContentHash()));
            Assert.That(_client.Start.SleeperClient(0), Is.EqualTo(ClientId));
            Assert.That(_client.Start.NightmareClientId, Is.EqualTo(m_ServerNetworkManager.LocalClientId));
            Assert.That(_client.Phase, Is.EqualTo(Phase.Running), "no head start was asked for");
            AssertInSync("at RoundStart");
            Assert.That(_client.Mirror.Lattice.Cubes.Count, Is.EqualTo(1), "the bedroom, and nothing else");

            // 301, and 408's first state.
            _client.SendDreamReady(0);
            yield return WaitForConditionOrTimeOut(() => _host.DreamsReady.Contains(ClientId) && _host.BudgetStates > 0);
            AssertOnTimeout("DreamReady or BudgetState never arrived");
            Assert.That(_host.LastBudget.Points, Is.EqualTo(_round.Budget.Points));
        }

        [UnityTest]
        public IEnumerator APlacementRoundTripsAndTheClientsHashMatches()
        {
            yield return Begin();
            int budgets = _host.BudgetStates;

            yield return Place(On(new Coord(0, 0, 0), Face.North, Straight));

            AssertInSync("after one placement");
            Assert.That(_client.Mirror.Lattice.Has(new Coord(0, 1, 0)), Is.True);
            Assert.That(_host.HashMatches, Is.EqualTo(1));
            Assert.That(_host.Desyncs, Is.Empty);
            yield return WaitForConditionOrTimeOut(() => _host.BudgetStates > budgets);
            AssertOnTimeout("no BudgetState after the placement");
            Assert.That(_host.LastBudget.Points, Is.EqualTo(_round.Budget.Points), "the Nightmare's budget readout is not Core's");

            // A refusal is a reply and no event: the door just built on is
            // Attached, which Core calls "not a door" (DoorOccupied is
            // unreachable by the rules' own order, docs/CORE-API.md §5).
            int applied = _client.AppliedEvents;
            yield return Place(On(new Coord(0, 0, 0), Face.North, Straight), PlaceError.NotADoor);
            yield return null; yield return null;
            Assert.That(_client.AppliedEvents, Is.EqualTo(applied), "a refused placement was broadcast");
        }

        [UnityTest]
        public IEnumerator ExploreBeforePlaceSolidifiesAndTheLateBuildIsRefused()
        {
            // docs/SPEC.md §14: "an exploration that arrives before a placement
            // on the same door solidifies it and the placement is rejected".
            yield return Begin();
            Coord tee = new Coord(0, 1, 0);
            yield return Place(On(new Coord(0, 0, 0), Face.North, Tee));   // N, E, S: S meets the bedroom
            // A T alone has two doors at the same depth, both exits; a straight
            // beyond its north door puts the exit at depth two, and the T's
            // east door is fog — the one an exploration hardens.
            yield return Place(On(tee, Face.North, Straight));
            Assert.That(_client.Mirror.Derived.StateOf(new ConnectorRef(tee, Face.East)), Is.EqualTo(ConnectorState.Fog));

            yield return Explore(tee);
            AssertInSync("after the exploration");
            Assert.That(_client.Mirror.Derived.StateOf(new ConnectorRef(tee, Face.East)), Is.EqualTo(ConnectorState.Solid), "the T's fog door did not harden");

            int cubes = _round.Lattice.Cubes.Count;
            yield return Place(On(tee, Face.East, Straight, Rotation.R90), PlaceError.DoorIsSolid);
            Assert.That(_round.Lattice.Cubes.Count, Is.EqualTo(cubes));

            // The exit is still there for the Nightmare to build on.
            yield return Place(On(new Coord(0, 2, 0), Face.North, Straight));
            AssertInSync("after building on the exit instead");
        }

        [UnityTest]
        public IEnumerator PlaceBeforeExploreAttachesAndTheExplorationFindsNoFog()
        {
            // docs/SPEC.md §14: "a placement that arrives first attaches the
            // cube and the exploration finds no fog door left".
            yield return Begin();
            Coord tee = new Coord(0, 1, 0);
            Coord east = new Coord(1, 1, 0);
            yield return Place(On(new Coord(0, 0, 0), Face.North, Tee));
            yield return Place(On(tee, Face.East, Straight, Rotation.R90));   // E, W: W meets the T

            // The straight's west door was the fog the placement attached;
            // exploring the straight now finds its only other door the exit,
            // and nothing hardens.
            int solidBefore = _round.Derived.Connectors.Count(k => k.Value == ConnectorState.Solid);
            yield return Explore(east);
            AssertInSync("after the late exploration");
            Assert.That(_client.Mirror.Derived.StateOf(new ConnectorRef(east, Face.West)), Is.EqualTo(ConnectorState.Attached));
            Assert.That(_client.Mirror.Derived.StateOf(new ConnectorRef(east, Face.East)), Is.EqualTo(ConnectorState.Exit), "the straight's far door is the way out");
            Assert.That(_round.Derived.Connectors.Count(k => k.Value == ConnectorState.Solid), Is.EqualTo(solidBefore), "the late exploration hardened a door it had no fog for");
            Assert.That(_round.Lattice.IsExplored(east), Is.True);
            Assert.That(_client.Mirror.Log.Events.Count, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator TouchedExitWakesUnlessAPlacementBeatIt()
        {
            yield return Begin();
            ConnectorRef bedroomDoor = new ConnectorRef(new Coord(0, 0, 0), Face.North);
            yield return Place(On(bedroomDoor.Cube, bedroomDoor.Face, Straight));   // the exit moves north

            // docs/NETCODE.md §14: a placement beat the report; the client rolls the Sleeper back.
            int verdicts = _client.WakeVerdicts;
            _client.SendTouchedExit(bedroomDoor);
            yield return WaitForConditionOrTimeOut(() => _client.WakeVerdicts > verdicts);
            AssertOnTimeout("no WakeVerdict");
            Assert.That(_client.LastWakeVerdict.Accepted, Is.False);
            Assert.That((WakeVerdict)_client.LastWakeVerdict.Reason, Is.EqualTo(WakeVerdict.NotAnExit));
            Assert.That(_round.Sleepers[0].Status, Is.EqualTo(SleeperStatus.InDream));

            verdicts = _client.WakeVerdicts;
            int statuses = _client.SleeperStatuses;
            _client.SendTouchedExit(new ConnectorRef(new Coord(0, 1, 0), Face.North));
            yield return WaitForConditionOrTimeOut(() => _client.WakeVerdicts > verdicts && _client.SleeperStatuses > statuses);
            AssertOnTimeout("no WakeVerdict or SleeperStatus");
            Assert.That(_client.LastWakeVerdict.Accepted, Is.True);
            Assert.That(_round.Sleepers[0].Status, Is.EqualTo(SleeperStatus.Awake), "Core did not wake the Sleeper");
            Assert.That((SleeperStatus)_client.LastSleeperStatus.Status, Is.EqualTo(SleeperStatus.Awake));
            Assert.That(_client.LastSleeperStatus.DreamId, Is.EqualTo((sbyte)0));
            Assert.That(_round.IsOver, Is.True, "one Sleeper, awake: the round is over");
        }

        [UnityTest]
        public IEnumerator ACorruptedClientHashProducesADesyncNoticeAndALogFile()
        {
            yield return Begin();
            _logFolder = Path.Combine(Application.temporaryCachePath, "lucid-test-logs-" + Path.GetRandomFileName());
            _host.LogFolder = _logFolder;
            yield return Place(On(new Coord(0, 0, 0), Face.North, Straight));
            Assert.That(_host.Desyncs, Is.Empty, "an honest report was called a desync");

            // The one thing this test lies about: the hash the client derived.
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("DESYNC at seq 0"));
            _client.ReportHash(0, _client.Mirror.Derived.Hash ^ 1);
            yield return WaitForConditionOrTimeOut(() => _host.Desyncs.Count == 1 && _client.DesyncNotices == 1);
            AssertOnTimeout("no DesyncNotice on both ends");
            Assert.That(_host.Desyncs[0].Seq, Is.EqualTo(0u));
            Assert.That(_host.Desyncs[0].ClientId, Is.EqualTo(ClientId));

            // Hashes are cumulative: the same client's next report mismatches
            // too, and is counted rather than announced and saved again.
            string first = _host.LastSavedLog;
            _client.ReportHash(0, _client.Mirror.Derived.Hash ^ 2);
            yield return WaitForConditionOrTimeOut(() => _host.SuppressedDesyncs == 1);
            AssertOnTimeout("the second mismatch was not counted");
            Assert.That(_host.Desyncs.Count, Is.EqualTo(1));
            Assert.That(_client.DesyncNotices, Is.EqualTo(1));
            Assert.That(_host.LastSavedLog, Is.EqualTo(first), "a second log was saved for the same client");

            Assert.That(_host.LastSavedLog, Is.Not.Null.And.Not.Empty, "no .lucidlog was saved");
            Assert.That(File.Exists(_host.LastSavedLog), Is.True, _host.LastSavedLog);
            using (FileStream f = File.OpenRead(_host.LastSavedLog))
            {
                LucidLog.Contents saved = LucidLog.Read(f);
                Assert.That(saved.Events.Events, Is.EqualTo(_round.Log.Events));
                (Lattice _, Derived derived) = EventLog.Replay(saved.Events, _host.Registry, Start, Rotation.R0, saved.Settings);
                Assert.That(derived.Hash, Is.EqualTo(_round.Derived.Hash), "the saved log does not replay to the host's hash");
                Assert.That(saved.Note, Does.Contain("seq 0"));
            }
        }

        [UnityTest]
        public IEnumerator TelemetryMovesTheHostsSleeperAndAnOldPacketIsDropped()
        {
            yield return Begin();
            yield return Place(On(new Coord(0, 0, 0), Face.North, Straight));
            Coord corridor = new Coord(0, 1, 0);

            _client.SendTelemetry(new TelemetryMsg { Cube = WireCoord.From(corridor), Health = 100, Lives = 1 });
            yield return WaitForConditionOrTimeOut(() => _round.Sleepers[0].Cube == corridor);
            AssertOnTimeout("telemetry did not move Core's Sleeper");

            // Sequenced: a packet with the seq the host already saw is dropped.
            _client.SendTelemetryRaw(new TelemetryMsg { Seq = 0, Cube = WireCoord.From(new Coord(0, 0, 0)) });
            for (int i = 0; i < 5; i++) yield return null;
            Assert.That(_round.Sleepers[0].Cube, Is.EqualTo(corridor), "an old telemetry packet moved the Sleeper back");

            _client.SendTelemetry(new TelemetryMsg { Cube = WireCoord.From(new Coord(0, 0, 0)) });
            yield return WaitForConditionOrTimeOut(() => _round.Sleepers[0].Cube == new Coord(0, 0, 0));
            AssertOnTimeout("a newer packet did not move the Sleeper");

            // Off the lattice is dropped by Core, not stored (docs/CORE-API.md).
            _client.SendTelemetry(new TelemetryMsg { Cube = WireCoord.From(new Coord(5, 5, 0)) });
            for (int i = 0; i < 5; i++) yield return null;
            Assert.That(_round.Sleepers[0].Cube, Is.EqualTo(new Coord(0, 0, 0)));
        }
    }

    /// <summary>The rest of §14 and §4 on the wire: answered, never dropped; a fault reported; the phase carried.</summary>
    public sealed class RoundSyncEdgeTests : NetHarness
    {
        protected override int NumberOfClients => 1;
        protected override bool ShouldCheckForSpawnedPlayers() => false;

        [UnityTest]
        public IEnumerator AnUnknownTypeOrSkinIsAnsweredNotDropped()
        {
            // docs/NETCODE.md §14: "PlaceRequest for an unknown typeIndex →
            // PlaceReply UnknownType"; a skin M0 has not got is the same
            // answer, where the first draft threw inside the handler and the
            // Nightmare's ghost waited for ever.
            yield return Begin();
            int replies = _host.PlaceReplies;
            _host.SendPlaceRequestRaw(new PlaceRequestMsg { ReqId = 77, TargetCube = WireCoord.From(new Coord(0, 0, 0)), TargetFace = (byte)Face.North, TypeIndex = 9999 });
            yield return WaitForConditionOrTimeOut(() => _host.PlaceReplies > replies);
            AssertOnTimeout("no reply to an unknown type");
            Assert.That((PlaceError)_host.LastPlaceReply.Verdict, Is.EqualTo(PlaceError.UnknownType));
            Assert.That(_host.LastPlaceReply.ReqId, Is.EqualTo((ushort)77));

            replies = _host.PlaceReplies;
            _host.SendPlaceRequestRaw(new PlaceRequestMsg { ReqId = 78, TargetCube = WireCoord.From(new Coord(0, 0, 0)), TargetFace = (byte)Face.North, TypeIndex = _host.Codec.TypeIndex(Straight), SkinIndex = 1 });
            yield return WaitForConditionOrTimeOut(() => _host.PlaceReplies > replies);
            AssertOnTimeout("no reply to an unknown skin");
            Assert.That((PlaceError)_host.LastPlaceReply.Verdict, Is.EqualTo(PlaceError.UnknownType));
            Assert.That(_round.Lattice.Cubes.Count, Is.EqualTo(1), "an unknown skin placed a cube");
        }

        [UnityTest]
        public IEnumerator ACorruptEventFaultsTheMirrorAndTheHostHears()
        {
            // An event the host's log never held — a cube on the bedroom's own
            // coord. The mirror cannot apply it; the first draft threw inside
            // the RPC, sent no report, and the host waited on nothing.
            yield return Begin();
            _logFolder = System.IO.Path.Combine(Application.temporaryCachePath, "lucid-test-logs-" + System.IO.Path.GetRandomFileName());
            _host.LogFolder = _logFolder;

            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("cannot be applied"));
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("DESYNC at seq 0"));
            _host.BroadcastRaw(new LatticeEventMsg
            {
                Seq = 0, Kind = (byte)EventKind.Placed, Cube = WireCoord.From(new Coord(0, 0, 0)),
                TypeIndex = _host.Codec.TypeIndex(Straight), PostHash = 12345,
            });
            yield return WaitForConditionOrTimeOut(() => _client.Faults == 1 && _host.Desyncs.Count == 1);
            AssertOnTimeout("the fault never reached the host as a desync");
            Assert.That(_client.Mirror.Log.NextSeq, Is.EqualTo(0), "the corrupt event moved the mirror's log");
            Assert.That(_client.AppliedEvents, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator TheHeadStartEndsOnTheWire()
        {
            // 102: the host's phase change reaches the client; and a round that
            // has already left its head start says so at RoundStart.
            yield return Begin(new RoundSettings(HeadStartMs: 200));
            Assert.That(_client.Phase, Is.EqualTo(Phase.HeadStart));
            _round.Advance(300);
            yield return WaitForConditionOrTimeOut(() => _client.Phase == Phase.Running);
            AssertOnTimeout("the client never heard the head start end");
        }

        [UnityTest]
        public IEnumerator ARoundAlreadyRunningSaysSoAtStart()
        {
            yield return Begin(new RoundSettings(HeadStartMs: 200), before: r => r.Advance(300));
            Assert.That(_round.Phase, Is.EqualTo(Phase.Running));
            yield return WaitForConditionOrTimeOut(() => _client.Phase == Phase.Running);
            AssertOnTimeout("a client joining a running round was left at HeadStart");
        }
    }

    /// <summary>docs/NETCODE.md §2 on a real connection: a client with the wrong content is turned away with the reason it shows.</summary>
    public sealed class ApprovalOnTheWireTests : NetcodeIntegrationTest
    {
        protected override int NumberOfClients => 0;

        Hello _hello;
        NetworkManager _rejected;

        protected override void OnServerAndClientsCreated()
        {
            var registry = new CubeRegistry();
            registry.Register(new CubeType("test.start", "test", CubeCategory.Start, FaceMask.North, false, 0));
            _hello = NetSession.LocalHello(registry, "Host");
            NetSession.ArmApproval(m_ServerNetworkManager, _hello);
        }

        protected override void OnNewClientCreated(NetworkManager networkManager)
        {
            _rejected = networkManager;
            NetSession.PrepareClient(networkManager, _hello with { ContentHash = _hello.ContentHash ^ 1 });
        }

        protected override bool ShouldWaitForNewClientToConnect(NetworkManager networkManager) => false;

        [UnityTest]
        public IEnumerator AClientWithOtherContentIsTurnedAwayWithTheVersionString()
        {
            yield return CreateAndStartNewClient();
            yield return WaitForConditionOrTimeOut(() => !string.IsNullOrEmpty(_rejected.DisconnectReason) || !_rejected.IsListening);
            AssertOnTimeout("the client was neither approved nor refused");
            Assert.That(_rejected.DisconnectReason, Is.EqualTo(Lucid.Runtime.UI.LucidStrings.DifferentVersion));
            Assert.That(_rejected.IsConnectedClient, Is.False);
            Assert.That(m_ServerNetworkManager.ConnectedClientsIds.Count, Is.EqualTo(1), "the host alone");
        }
    }
}
