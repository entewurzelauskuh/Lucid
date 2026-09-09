using System.Collections;
using System.Linq;
using Lucid.Core;
using Lucid.Netcode;
using Lucid.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Lucid.Tests.PlayMode.Netcode
{
    /// <summary>
    /// The acceptance with a body in it: placement, exploration, the exit
    /// moving and waking, across the wire, with the Sleeper on the client
    /// walking a dream the host never stands up.
    /// </summary>
    public sealed class DreamClientTests : NetHarness
    {
        DreamInstance _dream;
        DreamClient _dreamClient;
        SimulationMode _physicsWas;

        protected override IEnumerator OnSetup()
        {
            _physicsWas = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;
            yield return base.OnSetup();
        }

        protected override void OnClientSyncReady(RoundSync client)
        {
            var go = new GameObject("ClientDream");
            _spawned.Add(go);
            _dream = go.AddComponent<DreamInstance>();
            _dream.Bind(_pack, Start, Rotation.R0);
            _dreamClient = new DreamClient(client, _dream, (feet, facing) =>
            {
                SleeperMotor motor = SleeperRig.Create(feet, facing);
                _spawned.Add(motor.gameObject);
                return motor;
            });
        }

        protected override void OnTearDownClient()
        {
            _dreamClient?.Dispose();
            _dreamClient = null;
            Physics.simulationMode = _physicsWas;
        }

        /// <summary>Holds forward, one physics step and one frame at a time, until the condition or the budget is spent.</summary>
        IEnumerator WalkForward(SleeperMotor motor, float seconds, System.Func<bool> until)
        {
            const float dt = 1f / 60f;
            for (int i = 0; i < Mathf.RoundToInt(seconds / dt); i++)
            {
                if (until()) yield break;
                motor.Tick(SleeperInput.Forward, dt);
                Physics.Simulate(dt);
                _dreamClient.Tick(dt);
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator ASleeperOnTheClientExploresAndWakesAcrossTheWire()
        {
            yield return Begin();

            Assert.That(_dream.Cubes.Count, Is.EqualTo(1), "the client's dream did not stand up from RoundStart");
            Assert.That(_dreamClient.Sleeper, Is.Not.Null, "nobody in the bedroom");
            Assert.That(_dreamClient.DreamId, Is.EqualTo(0), "the client does not know which dream it is");
            yield return WaitForConditionOrTimeOut(() => _host.DreamsReady.Contains(ClientId));
            AssertOnTimeout("DreamReady never arrived");

            // The host builds two rooms north; the client's dream grows without any hand of its own.
            yield return Place(On(new Coord(0, 0, 0), Face.North, Straight));
            yield return Place(On(new Coord(0, 1, 0), Face.North, Straight));
            yield return WaitForConditionOrTimeOut(() => _dream.Cubes.Count == 3);
            AssertOnTimeout("the client's dream did not grow");
            Coord corridor = new Coord(0, 1, 0);
            Coord far = new Coord(0, 2, 0);
            Assert.That(_dream.Cubes[far].Doors[Face.North].State, Is.EqualTo(ConnectorState.Exit), "the exit did not move north on the client");

            // The body walks in: the entry volume reports, the host adjudicates,
            // the event comes back and the hashes still agree.
            SleeperMotor sleeper = _dreamClient.Sleeper;
            yield return WalkForward(sleeper, 4f, () => _round.Lattice.IsExplored(corridor));
            Assert.That(_round.Lattice.IsExplored(corridor), Is.True, $"the host never explored the corridor; feet at {sleeper.Feet}");
            Assert.That(_dreamClient.Explorations, Is.EqualTo(1));
            yield return WaitForConditionOrTimeOut(() => _client.AppliedEvents == 3 && _host.HashMatches == 3);
            AssertOnTimeout("the exploration did not come back to the client");
            AssertInSync("after the Sleeper explored");
            Assert.That(_round.Sleepers[0].Cube, Is.EqualTo(corridor), "telemetry or the report did not move Core's Sleeper");
            Assert.That(_dreamClient.TelemetrySent, Is.GreaterThan(0), "no telemetry went up");
            Assert.That(_dreamClient.LastTelemetry.LocalY, Is.LessThan(256), "the body's height reads as if from a cube below the floor");
            Assert.That(_dreamClient.LastTelemetry.LocalX, Is.EqualTo(CubeMetrics.Half * 256).Within(128), "x is not from the cube's west edge");

            // Into the far room and back into the corridor: the corridor is
            // explored already, so no report goes up, and only telemetry can
            // tell Core the body is there again.
            yield return WalkForward(sleeper, 4f, () => _round.Lattice.IsExplored(far));
            Assert.That(_round.Lattice.IsExplored(far), Is.True, $"never reached the far room; feet at {sleeper.Feet}");
            yield return WaitForConditionOrTimeOut(() => _round.Sleepers[0].Cube == far);
            AssertOnTimeout("Core's Sleeper did not reach the far room");
            sleeper.transform.rotation = Quaternion.LookRotation(DreamSpace.Direction(Face.South), Vector3.up);
            yield return WalkForward(sleeper, 4f, () => _round.Sleepers[0].Cube == corridor);
            Assert.That(_round.Sleepers[0].Cube, Is.EqualTo(corridor), $"telemetry did not bring Core's Sleeper back; feet at {sleeper.Feet}");
            Assert.That(_dreamClient.Explorations, Is.EqualTo(2), "walking back into an explored room reported again");
            sleeper.transform.rotation = Quaternion.LookRotation(DreamSpace.Direction(Face.North), Vector3.up);

            // On through the far room into the white door: the host says woke, the body is done.
            yield return WalkForward(sleeper, 6f, () => _dreamClient.Woke);
            Assert.That(_dreamClient.ExitTouches, Is.EqualTo(1), "the exit was not touched once");
            Assert.That(_dreamClient.Woke, Is.True, $"no wake; feet at {(sleeper != null ? sleeper.Feet.ToString() : "gone")}");
            Assert.That(_round.Sleepers[0].Status, Is.EqualTo(SleeperStatus.Awake));
            Assert.That(_dreamClient.Sleeper == null, Is.True, "the body outlived the waking");
            Assert.That(_host.Desyncs, Is.Empty);

            // A next round on the same wire (M0.9's loop, M1.4's resume): a new
            // body in the bedroom, and the last round's waking forgotten.
            var next = new Round(new RoundSettings(HeadStartMs: 0), _host.Registry, Start, Rotation.R0, new[] { new PlayerId((int)ClientId) });
            _host.BeginRound(next, m_ServerNetworkManager.LocalClientId, new[] { ClientId });
            yield return WaitForConditionOrTimeOut(() => _dreamClient.Sleeper != null && !_dreamClient.Woke);
            AssertOnTimeout("the next round did not put a body back, or left it awake");
            Assert.That(_dream.Cubes.Count, Is.EqualTo(1), "the last round's cubes survived into the next");
            Assert.That(_dreamClient.DreamId, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator ARefusedWakeRollsTheBodyBackIntoItsRoom()
        {
            // docs/NETCODE.md §14: a placement beat the report, so the door the
            // Sleeper touched is a doorway now; the client rolls them back into
            // the room they stood in rather than leave them in the new cube.
            yield return Begin();
            SleeperMotor sleeper = _dreamClient.Sleeper;
            var door = new ConnectorRef(new Coord(0, 0, 0), Face.North);
            Vector3 centre = _dream.transform.TransformPoint(DreamSpace.Origin(door.Cube));
            sleeper.Warp(centre + new Vector3(0f, 0.1f, CubeMetrics.Half + 0.5f));   // past the door, in the cube that just appeared
            _dreamClient.NoteTouched(door);

            _dreamClient.HandleWakeVerdict(new WakeVerdictMsg { Accepted = false, Reason = (byte)WakeVerdict.NotAnExit });
            Assert.That(_dreamClient.Woke, Is.False);
            Assert.That(_dreamClient.Sleeper, Is.Not.Null);
            Vector3 doorway = _dreamClient.Doorway(door);
            Assert.That((sleeper.Feet - doorway).magnitude, Is.LessThan(0.5f), $"not rolled back into the doorway {doorway}: {sleeper.Feet}");
            Assert.That(sleeper.Feet.z, Is.EqualTo(CubeMetrics.Half - 1f).Within(0.5f), "not a metre inside the room");
            Assert.That((sleeper.Feet - centre).magnitude, Is.GreaterThan(1f), "rolled back to the room's centre instead of its doorway");
        }
    }
}
