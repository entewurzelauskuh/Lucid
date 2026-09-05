using System;
using System.Collections;
using System.Collections.Generic;
using Lucid.Core;
using Lucid.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Lucid.Tests.PlayMode.Dream
{
    /// <summary>
    /// M0.6's acceptance (docs/WORKPLAN.md §4): replaying a log builds the
    /// expected geometry; entering a cube raises Explored exactly once; walking
    /// into a white door raises TouchedExit.
    /// </summary>
    /// <remarks>
    /// The cubes here are built in code rather than loaded from the Core pack.
    /// What <see cref="DreamInstance"/> owns is where a cube stands, which way
    /// it faces and which door shows which state; what is inside the shell is
    /// <c>CubeBuilder</c>'s, and EditMode already holds it to the spec. Loading
    /// the real assets would test the pack as well as the runtime, and fail for
    /// two different reasons without saying which.
    /// </remarks>
    public sealed class DreamInstanceTests
    {
        const string Start = "test.start";
        const string Straight = "test.straight";
        const string Corner = "test.corner";
        const string Shaft = "test.shaft";
        const string Riser = "test.riser";

        readonly List<GameObject> _spawned = new List<GameObject>();
        readonly List<UnityEngine.Object> _assets = new List<UnityEngine.Object>();

        SimulationMode _physicsWas;

        [SetUp]
        public void DrivePhysicsByHand()
        {
            // As in FogDoorTests: the pilot ticks the motor inside one frame,
            // so the physics step that dispatches OnTriggerEnter has to be
            // stepped by hand or no trigger ever fires.
            _physicsWas = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;
        }

        [TearDown]
        public void CleanUp()
        {
            Physics.simulationMode = _physicsWas;

            foreach (GameObject go in _spawned)
                if (go != null) UnityEngine.Object.DestroyImmediate(go);
            _spawned.Clear();

            foreach (UnityEngine.Object asset in _assets)
                if (asset != null) UnityEngine.Object.DestroyImmediate(asset);
            _assets.Clear();
        }

        // ---- the world under test -------------------------------------------------

        /// <summary>
        /// A cube shaped like the ones <c>CubeTemplateBuilder</c> makes: a
        /// socket on every face carrying a FogDoor, walled faces included, and
        /// a floor to stand on.
        /// </summary>
        GameObject CubePrefab(string id, FaceMask doorways)
        {
            var body = new GameObject($"prefab {id}");

            // Parked far below, so the source object's own colliders never take
            // part in a test. Instantiate is given an explicit pose, so where
            // the source sits does not reach the clone.
            body.transform.position = new Vector3(0f, -10000f, 0f);

            // The template's own nodes (docs/SPEC.md §17). Interior is here so
            // that the tests take the same path a real cube does — the entry
            // volume must not adopt it.
            foreach (string node in new[] { "Shell", "Interior", "Logic" })
            {
                var child = new GameObject(node);
                child.transform.SetParent(body.transform, false);
            }

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "floor";
            floor.transform.SetParent(body.transform, false);
            floor.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(CubeMetrics.Size, 1f, CubeMetrics.Size);

            foreach (Face face in Faces.All)
            {
                var socket = new GameObject(face.ToString());
                socket.transform.SetParent(body.transform, false);
                socket.transform.localPosition = SocketCentre(face);
                socket.transform.localRotation =
                    Quaternion.LookRotation(DreamSpace.Direction(face), Vector3.up);

                var doorObject = new GameObject("FogDoor");
                doorObject.transform.SetParent(socket.transform, false);
                var door = doorObject.AddComponent<FogDoor>();
                door.Configure(face);

                socket.AddComponent<Connector>()
                    .Configure(face, Faces.Has(doorways, face), door);
            }

            _spawned.Add(body);
            return body;
        }

        /// <summary>Where a socket sits, mirroring <c>CubeGeometry.Centre</c>.</summary>
        static Vector3 SocketCentre(Face face)
        {
            switch (face)
            {
                case Face.North: return new Vector3(0f, 0f, CubeMetrics.Half);
                case Face.East: return new Vector3(CubeMetrics.Half, 0f, 0f);
                case Face.South: return new Vector3(0f, 0f, -CubeMetrics.Half);
                case Face.West: return new Vector3(-CubeMetrics.Half, 0f, 0f);
                case Face.Up: return new Vector3(0f, CubeMetrics.Size, 0f);
                default: return Vector3.zero;
            }
        }

        CubeDefinition Definition(
            string id, FaceMask doorways, CubeCategory category, bool climbable = false)
        {
            var d = ScriptableObject.CreateInstance<CubeDefinition>();
            d.Configure(id, "test", $"display name of {id}", category, doorways,
                climbable, 1, CubePrefab(id, doorways), new[] { "*" });
            _assets.Add(d);
            return d;
        }

        /// <summary>Two cube types: a start with one door north, and a corridor.</summary>
        DreamPack Pack()
        {
            var pack = ScriptableObject.CreateInstance<DreamPack>();
            pack.Configure("test");
            pack.AddOrReplace(Definition(Start, FaceMask.North, CubeCategory.Start));
            pack.AddOrReplace(Definition(Straight, FaceMask.North | FaceMask.South, CubeCategory.Connector));
            pack.AddOrReplace(Definition(Corner, FaceMask.North | FaceMask.East, CubeCategory.Connector));
            pack.AddOrReplace(Definition(Shaft, FaceMask.Down | FaceMask.North, CubeCategory.Vertical));
            pack.AddOrReplace(Definition(
                Riser, FaceMask.South | FaceMask.Up, CubeCategory.Vertical, climbable: true));
            _assets.Add(pack);
            return pack;
        }

        DreamInstance Dream(Rotation startRotation = Rotation.R0)
        {
            var go = new GameObject("Dream");
            _spawned.Add(go);
            var dream = go.AddComponent<DreamInstance>();
            dream.Bind(Pack(), Start, startRotation);
            return dream;
        }

        /// <summary>A log that runs a corridor north out of the bedroom.</summary>
        static EventLog Corridor(int cubes)
        {
            var log = new EventLog();
            for (int i = 1; i <= cubes; i++)
            {
                log.Append(new CubePlaced(
                    log.NextSeq, new Coord(0, i, 0), Straight, Rotation.R0, "*"));
            }
            return log;
        }

        SleeperMotor Runner(Vector3 feet)
        {
            var motor = SleeperRig.Create(feet, Vector3.forward);
            _spawned.Add(motor.gameObject);
            return motor;
        }

        /// <summary>Holds forward, stepping physics so triggers fire.</summary>
        static void WalkForward(SleeperMotor motor, float seconds)
        {
            const float dt = 1f / 60f;
            for (int i = 0; i < Mathf.RoundToInt(seconds / dt); i++)
            {
                motor.Tick(SleeperInput.Forward, dt);
                Physics.Simulate(dt);
            }
        }

        /// <summary>Turns on the spot. Forward is relative to where they look.</summary>
        static void Turn(SleeperMotor motor, float degrees) =>
            motor.transform.rotation *= Quaternion.Euler(0f, degrees, 0f);

        static void TurnAround(SleeperMotor motor) => Turn(motor, 180f);

        // ---- geometry -------------------------------------------------------------

        [Test]
        public void ReplayingALogBuildsTheExpectedGeometry()
        {
            DreamInstance dream = Dream();

            dream.Build(Corridor(2));

            Assert.That(dream.Cubes.Count, Is.EqualTo(3), "start plus two placements");
            Assert.That(dream.Cubes[new Coord(0, 0, 0)].transform.position,
                Is.EqualTo(Vector3.zero));
            Assert.That(dream.Cubes[new Coord(0, 1, 0)].transform.position,
                Is.EqualTo(new Vector3(0f, 0f, 8f)));
            Assert.That(dream.Cubes[new Coord(0, 2, 0)].transform.position,
                Is.EqualTo(new Vector3(0f, 0f, 16f)));
        }

        [Test]
        public void ACubeIsTurnedTheWayTheLogSaysAndItsDoorsTurnWithIt()
        {
            // A corner with doors North and East, laid a quarter turn
            // clockwise: North looks East and East looks South, so it is the
            // socket built as East that closes the gap back to the bedroom.
            var log = new EventLog();
            log.Append(new CubePlaced(0, new Coord(0, 1, 0), Corner, Rotation.R90, "*"));

            DreamInstance dream = Dream();
            dream.Build(log);

            DreamCube cube = dream.Cubes[new Coord(0, 1, 0)];
            Assert.That(cube.transform.rotation.eulerAngles.y, Is.EqualTo(90f).Within(1e-3f));

            // The sharp one: the door keyed under world East is the socket the
            // builder made as North. Wire the lookup without the rotation and
            // this pairs every door with the state of the wall opposite.
            Assert.That(cube.Doors[Face.East].Face, Is.EqualTo(Face.North));
            Assert.That(cube.Doors[Face.South].Face, Is.EqualTo(Face.East));

            Assert.That(cube.Doors[Face.South].State, Is.EqualTo(ConnectorState.Attached),
                "the door facing the bedroom is the one the fit rule matched");
            Assert.That(cube.Doors[Face.East].State, Is.EqualTo(ConnectorState.Exit),
                "the only fog door left in the dream is the deepest, so it is the way out");
            Assert.That(cube.Doors.Keys, Is.EquivalentTo(new[] { Face.East, Face.South }),
                "world North and the rest are wall on this cube");
        }

        [Test]
        public void AWalledFaceHasNoDoorAtAll()
        {
            DreamInstance dream = Dream();
            dream.Build(new EventLog());

            DreamCube start = dream.Cubes[new Coord(0, 0, 0)];

            // The start cube has one doorway, north. The template carries a
            // FogDoor on all six sockets, but ShellBuilder has already walled
            // the other five — a door there would add a second barrier inside
            // the wall, and the one on Down stands 0.125 m above a floor whose
            // step offset is 0.1, so every room would have a lip in it.
            Assert.That(start.Doors.Keys, Is.EquivalentTo(new[] { Face.North }));
            Assert.That(start.Doors[Face.North].State, Is.EqualTo(ConnectorState.Exit));

            foreach (Connector socket in start.GetComponentsInChildren<Connector>(true))
            {
                if (socket.IsDoorway) continue;
                Assert.That(socket.Door.gameObject.activeSelf, Is.False, socket.Face.ToString());
            }
        }

        [Test]
        public void TheEntryVolumeDoesNotAdoptTheTemplatesInteriorNode()
        {
            // "Interior" is the template's own (docs/SPEC.md §17: collision
            // geometry and chicane logic). Squatting on it meant the first cube
            // to put a collider there had its floor turned into a 7 m trigger.
            DreamInstance dream = Dream();
            dream.Build(new EventLog());

            Transform interior = dream.Cubes[new Coord(0, 0, 0)].transform.Find("Interior");
            Assert.That(interior, Is.Not.Null, "the fixture no longer mirrors the template");
            Assert.That(interior.GetComponent<Collider>(), Is.Null);
            Assert.That(interior.GetComponent<DreamEntryVolume>(), Is.Null);
        }

        [Test]
        public void TheDeepestFogDoorIsTheOneShowingWhite()
        {
            DreamInstance dream = Dream();
            dream.Build(Corridor(2));

            // Start north and the first corridor's doors are spent or interior;
            // the far end of the corridor is the way out (docs/SPEC.md §7).
            Assert.That(dream.Cubes[new Coord(0, 0, 0)].Doors[Face.North].State,
                Is.EqualTo(ConnectorState.Attached));
            Assert.That(dream.Cubes[new Coord(0, 1, 0)].Doors[Face.North].State,
                Is.EqualTo(ConnectorState.Attached));
            Assert.That(dream.Cubes[new Coord(0, 2, 0)].Doors[Face.North].State,
                Is.EqualTo(ConnectorState.Exit));
        }

        [Test]
        public void BuildingOnAnExitMovesTheLightWithoutRebuildingTheRoom()
        {
            DreamInstance dream = Dream();
            dream.Build(Corridor(1));

            DreamCube wasTheEnd = dream.Cubes[new Coord(0, 1, 0)];
            Assert.That(wasTheEnd.Doors[Face.North].State, Is.EqualTo(ConnectorState.Exit));

            dream.Build(Corridor(2));

            // The same object, not a fresh one: rebuilding a standing cube
            // would restart its doors and re-arm its entry volume, so a live
            // round would forget where the Sleeper had been on every placement.
            Assert.That(dream.Cubes[new Coord(0, 1, 0)], Is.SameAs(wasTheEnd));
            Assert.That(wasTheEnd.Doors[Face.North].State, Is.EqualTo(ConnectorState.Attached));

            // And it animated. The converse of the fresh-cube rule: a door that
            // changes during a round plays §7's transition, so leaving `_fresh`
            // set for ever would snap every change in the dream.
            Assert.That(wasTheEnd.Doors[Face.North].Playing,
                Is.EqualTo(FogDoorTransition.Dissolve));
            Assert.That(dream.Cubes[new Coord(0, 2, 0)].Doors[Face.North].State,
                Is.EqualTo(ConnectorState.Exit));
        }

        [Test]
        public void ACubeArrivesInItsStateRatherThanAnimatingIntoIt()
        {
            var log = Corridor(1);
            log.Append(new CubeExplored(log.NextSeq, new Coord(0, 1, 0), 1));

            DreamInstance dream = Dream();
            dream.Build(log);

            // This cube was explored before this runtime existed, so its doors
            // are wall from the first frame — they did not harden while anyone
            // was looking, and playing the condense would be a lie about what
            // happened. Every door on a freshly built cube is the same case.
            foreach (KeyValuePair<Face, FogDoor> door in dream.Cubes[new Coord(0, 1, 0)].Doors)
            {
                Assert.That(door.Value.Playing, Is.EqualTo(FogDoorTransition.None), door.Key.ToString());
                Assert.That(door.Value.Progress, Is.EqualTo(1f), door.Key.ToString());
            }
        }

        [Test]
        public void TheEntryVolumeFillsTheRoomAndNothingElse()
        {
            DreamInstance dream = Dream();
            dream.Build(Corridor(1));

            Transform volume = dream.Cubes[new Coord(0, 1, 0)].transform.Find("EntryVolume");
            Assert.That(volume, Is.Not.Null);
            Bounds b = volume.GetComponent<BoxCollider>().bounds;

            // Inset half a metre from all six faces of the cube at (0,1,0),
            // which stands on y 0 and spans z 4..12. Asserted as a box rather
            // than by whether a Sleeper standing on the floor happens to be in
            // it: a volume centred on the origin would still catch that one,
            // and would also reach 3.5 m down into the layer below.
            Assert.That(b.min, Is.EqualTo(new Vector3(-3.5f, 0.5f, 4.5f)));
            Assert.That(b.max, Is.EqualTo(new Vector3(3.5f, 7.5f, 11.5f)));
        }

        [Test]
        public void ACubeInTheLayerAboveStandsOnTheOneBelow()
        {
            // Every other test lays cubes on one floor, so the vertical arm of
            // the mapping is never exercised through a real cube: placing the
            // whole dream at y 0 would pass all of them.
            var log = new EventLog();
            log.Append(new CubePlaced(0, new Coord(0, 1, 0), Riser, Rotation.R0, "*"));
            log.Append(new CubePlaced(1, new Coord(0, 1, 1), Shaft, Rotation.R0, "*"));

            DreamInstance dream = Dream();
            dream.Build(log);

            DreamCube above = dream.Cubes[new Coord(0, 1, 1)];
            Assert.That(above.transform.position, Is.EqualTo(new Vector3(0f, 8f, 8f)));

            Bounds b = above.transform.Find("EntryVolume").GetComponent<BoxCollider>().bounds;
            Assert.That(b.min.y, Is.EqualTo(8.5f).Within(1e-4f));
            Assert.That(b.max.y, Is.EqualTo(15.5f).Within(1e-4f));
        }

        [Test]
        public void APackThatCannotBuildWhatTheLatticeHoldsSaysSo()
        {
            // The two an author hits: a log naming a cube the pack does not
            // ship, and a CubeDefinition whose prefab never got built. Both
            // would otherwise be a hole in the maze, which is the kind of thing
            // that gets debugged for an hour.
            var go = new GameObject("Dream");
            _spawned.Add(go);
            var dream = go.AddComponent<DreamInstance>();

            var pack = ScriptableObject.CreateInstance<DreamPack>();
            pack.Configure("test");
            _assets.Add(pack);
            dream.Bind(pack, Start, Rotation.R0);

            Assert.That(() => dream.Build(new EventLog()),
                Throws.TypeOf<KeyNotFoundException>().Or.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void ACubeWithNoPrefabIsNamedRatherThanLeftAsAHole()
        {
            var pack = ScriptableObject.CreateInstance<DreamPack>();
            pack.Configure("test");
            var d = ScriptableObject.CreateInstance<CubeDefinition>();
            d.Configure(Start, "test", "Bedroom", CubeCategory.Start, FaceMask.North,
                false, 1, null, new[] { "*" });
            pack.AddOrReplace(d);
            _assets.Add(d);
            _assets.Add(pack);

            var go = new GameObject("Dream");
            _spawned.Add(go);
            var dream = go.AddComponent<DreamInstance>();
            dream.Bind(pack, Start, Rotation.R0);

            Assert.That(() => dream.Build(new EventLog()),
                Throws.InvalidOperationException.With.Message.Contains("no prefab"));
        }

        [Test]
        public void RespawningIntoADreamThatWasNeverBuiltSaysSo()
        {
            var go = new GameObject("Dream");
            _spawned.Add(go);
            var dream = go.AddComponent<DreamInstance>();

            SleeperMotor motor = Runner(new Vector3(0f, 0.1f, 0f));
            Assert.That(() => dream.Respawn(motor), Throws.InvalidOperationException);
        }

        [Test]
        public void EveryCubeHangsUnderTheDream()
        {
            // docs/NETCODE.md §8 has every subscriber rebuilding the dream from
            // its own lattice, so the cubes have to be somewhere that can be
            // moved as one — and unparented they would also outlive the dream.
            DreamInstance dream = Dream();
            dream.Build(Corridor(2));

            foreach (KeyValuePair<Coord, DreamCube> pair in dream.Cubes)
                Assert.That(pair.Value.transform.parent, Is.SameAs(dream.transform), pair.Key.ToString());
        }

        // ---- exploration ----------------------------------------------------------

        [UnityTest]
        public IEnumerator EnteringACubeRaisesExploredExactlyOnce()
        {
            DreamInstance dream = Dream();
            dream.Build(Corridor(2));

            var reported = new List<Coord>();
            dream.Explored += reported.Add;

            SleeperMotor motor = Runner(dream.SpawnPoint + new Vector3(0f, 0.1f, 0f));
            yield return null;

            // Into the first corridor and no further: its volume starts at
            // z 4.5 and the next one at 12.5, so 9 m lands well inside.
            WalkForward(motor, 1.5f);

            Assert.That(reported, Is.EqualTo(new[] { new Coord(0, 1, 0) }),
                $"stopped at z {motor.Feet.z:0.00}");

            // Out the way they came and back in again. The explored rule fires
            // on first entry, not on every crossing, so the second visit to a
            // room a Sleeper already stood in reports nothing.
            TurnAround(motor);
            WalkForward(motor, 1.5f);
            Assert.That(motor.Feet.z, Is.LessThan(3.5f), "never got clear of the corridor's volume");
            TurnAround(motor);
            WalkForward(motor, 1.5f);

            Assert.That(reported.Count, Is.EqualTo(1),
                $"reported {reported.Count} times, ending at z {motor.Feet.z:0.00}");
        }

        [UnityTest]
        public IEnumerator TheStartCubeIsNeverReportedExplored()
        {
            // docs/SPEC.md §7: "the start cube is exempt". A Sleeper stands in
            // it from the first frame, so without the exemption every round
            // would open by solidifying the bedroom.
            DreamInstance dream = Dream();
            dream.Build(Corridor(1));

            var reported = new List<Coord>();
            dream.Explored += reported.Add;

            Runner(dream.SpawnPoint + new Vector3(0f, 0.1f, 0f));
            yield return null;
            Physics.Simulate(1f / 60f);

            Assert.That(reported, Is.Empty);
        }

        [UnityTest]
        public IEnumerator ACubeTheLogAlreadyExploredIsNotReportedAgain()
        {
            var log = Corridor(1);
            log.Append(new CubeExplored(log.NextSeq, new Coord(0, 1, 0), 1));

            DreamInstance dream = Dream();
            dream.Build(log);

            var reported = new List<Coord>();
            dream.Explored += reported.Add;

            SleeperMotor motor = Runner(dream.SpawnPoint + new Vector3(0f, 0.1f, 0f));
            yield return null;
            WalkForward(motor, 2.5f);

            // It was explored by whoever walked there, not by this runtime
            // coming up and finding a Sleeper standing in the room.
            Assert.That(reported, Is.Empty, $"stopped at z {motor.Feet.z:0.00}");
        }

        [UnityTest]
        public IEnumerator OnlyASleeperExploresACube()
        {
            // Mobs walk through rooms, thrown props land in them and M0.7's
            // projectiles cross them. None of that is a footprint, and the
            // explored rule is global across every dream (docs/SPEC.md §5) —
            // so a stray collider would harden doors in everyone's maze.
            DreamInstance dream = Dream();
            dream.Build(Corridor(1));

            var reported = new List<Coord>();
            dream.Explored += reported.Add;

            var prop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            prop.name = "Prop";
            prop.transform.position = DreamSpace.Centre(new Coord(0, 1, 0));
            prop.AddComponent<Rigidbody>().useGravity = false;
            _spawned.Add(prop);
            yield return null;

            Physics.Simulate(1f / 60f);
            Physics.Simulate(1f / 60f);

            Assert.That(reported, Is.Empty);

            // The positive control. Without it "nothing was reported" could
            // just as well mean "a body that starts inside a trigger never
            // fires one", and the test would pass with the Sleeper check gone.
            SleeperMotor motor = Runner(DreamSpace.Origin(new Coord(0, 1, 0)) + new Vector3(0f, 0.1f, 0f));
            yield return null;
            Physics.Simulate(1f / 60f);
            Physics.Simulate(1f / 60f);

            Assert.That(reported, Is.EqualTo(new[] { new Coord(0, 1, 0) }),
                $"a Sleeper standing at {motor.Feet} was not noticed either");
        }

        [UnityTest]
        public IEnumerator StandingInADoorwayIsNotBeingInEitherRoom()
        {
            // The entry volume is inset from every face so that a Sleeper on
            // the threshold belongs to one cube, not two. Without the inset a
            // step into a doorway hardens the doors of the room beyond it,
            // before the Sleeper has committed to going in.
            DreamInstance dream = Dream();
            dream.Build(Corridor(2));

            var reported = new List<Coord>();
            dream.Explored += reported.Add;

            SleeperMotor motor = Runner(new Vector3(0f, 0.1f, 12f));
            yield return null;
            Physics.Simulate(1f / 60f);

            Assert.That(reported, Is.Empty, "reported while still in the doorway");

            WalkForward(motor, 0.5f);

            Assert.That(reported, Is.EqualTo(new[] { new Coord(0, 2, 0) }),
                $"stopped at z {motor.Feet.z:0.00}");
        }

        [UnityTest]
        public IEnumerator ASecondApplyDoesNotSubscribeToTheSameCubeTwice()
        {
            // The host pushes every new lattice back through Apply, so a
            // subscription made outside the spawn branch would be added once
            // per placement and Explored would fire once per placement so far.
            DreamInstance dream = Dream();
            dream.Build(Corridor(1));
            dream.Build(Corridor(2));
            dream.Build(Corridor(2));

            var reported = new List<Coord>();
            dream.Explored += reported.Add;

            SleeperMotor motor = Runner(dream.SpawnPoint + new Vector3(0f, 0.1f, 0f));
            yield return null;
            WalkForward(motor, 1.5f);

            Assert.That(reported, Is.EqualTo(new[] { new Coord(0, 1, 0) }),
                $"stopped at z {motor.Feet.z:0.00}");
        }

        [UnityTest]
        public IEnumerator ASecondRoundDoesNotLeaveTheLastOnesRoomsStanding()
        {
            DreamInstance dream = Dream();
            dream.Build(Corridor(2));
            GameObject orphan = dream.Cubes[new Coord(0, 2, 0)].gameObject;
            GameObject bedroom = dream.Cubes[new Coord(0, 0, 0)].gameObject;

            // A lattice never loses a cube inside a round, so this only happens
            // when the same instance is handed the next round's log — which is
            // exactly when a leftover room is worst: doors frozen at last
            // round's states, and its events still wired to this instance.
            dream.Build(new EventLog());
            yield return null;   // Destroy lands at the end of the frame

            Assert.That(dream.Cubes.Keys, Is.EquivalentTo(new[] { new Coord(0, 0, 0) }));
            Assert.That(orphan == null, Is.True, "the room from the last round is still standing");

            // The bedroom goes too, and comes back new. Kept, it would still be
            // holding last round's Attached on the door the corridor was built
            // on, and be asked to walk it back to Exit — which §7's table has
            // no transition for.
            Assert.That(bedroom == null, Is.True, "the bedroom survived into the next round");
            Assert.That(dream.Cubes[new Coord(0, 0, 0)].Doors[Face.North].State,
                Is.EqualTo(ConnectorState.Exit));
            Assert.That(dream.Cubes[new Coord(0, 0, 0)].Doors[Face.North].Playing,
                Is.EqualTo(FogDoorTransition.None), "arrived in its state rather than animating");
        }

        // ---- waking ---------------------------------------------------------------

        [UnityTest]
        public IEnumerator WalkingIntoAWhiteDoorRaisesTouchedExit()
        {
            DreamInstance dream = Dream();
            dream.Build(Corridor(1));

            var touched = new List<ConnectorRef>();
            dream.TouchedExit += touched.Add;

            SleeperMotor motor = Runner(dream.SpawnPoint + new Vector3(0f, 0.1f, 0f));
            yield return null;

            // Bedroom to the far end of the corridor: 12 m at 6 m/s.
            WalkForward(motor, 3.5f);

            Assert.That(touched, Is.EqualTo(new[] { new ConnectorRef(new Coord(0, 1, 0), Face.North) }),
                $"stopped at z {motor.Feet.z:0.00}");
        }

        [UnityTest]
        public IEnumerator AnAttachedDoorwayRaisesNothing()
        {
            // The interesting negative: Attached is passable exactly like Exit,
            // so a report driven by "did they pass through a door?" would fire
            // on every ordinary doorway in the dream.
            DreamInstance dream = Dream();
            dream.Build(Corridor(2));

            var touched = new List<ConnectorRef>();
            dream.TouchedExit += touched.Add;

            SleeperMotor motor = Runner(dream.SpawnPoint + new Vector3(0f, 0.1f, 0f));
            yield return null;

            // Through the bedroom's attached door and no further.
            WalkForward(motor, 2f);

            Assert.That(touched, Is.Empty, $"stopped at z {motor.Feet.z:0.00}");
        }

        [UnityTest]
        public IEnumerator ARotatedCubeReportsTheWorldFaceItWokeThemThrough()
        {
            // The rotation bug one level up. Every other walk is into a cube at
            // R0, where the local and world faces are the same string, so a
            // report carrying the socket's own face passes them all — and hands
            // the host a ConnectorRef that is not a connector in the lattice,
            // which Round.TryWake would refuse.
            var log = new EventLog();
            log.Append(new CubePlaced(0, new Coord(0, 1, 0), Corner, Rotation.R90, "*"));

            DreamInstance dream = Dream();
            dream.Build(log);

            var touched = new List<ConnectorRef>();
            dream.TouchedExit += touched.Add;

            SleeperMotor motor = Runner(dream.SpawnPoint + new Vector3(0f, 0.1f, 0f));
            yield return null;

            WalkForward(motor, 1.5f);          // north, into the corner at (0,1,0)
            Turn(motor, 90f);                  // now looking east
            WalkForward(motor, 0.65f);         // into its one fog door, at x 4

            Assert.That(touched, Is.EqualTo(new[] { new ConnectorRef(new Coord(0, 1, 0), Face.East) }),
                $"stopped at {motor.Feet}");
        }

        // ---- respawn --------------------------------------------------------------

        [UnityTest]
        public IEnumerator RespawnPutsASleeperBackInTheStartCubeFacingTheWayOut()
        {
            DreamInstance dream = Dream();
            dream.Build(Corridor(2));

            SleeperMotor motor = Runner(dream.SpawnPoint + new Vector3(0f, 0.1f, 0f));
            yield return null;
            WalkForward(motor, 2.5f);
            Assert.That(motor.Feet.z, Is.GreaterThan(4f), "never left the bedroom");

            TurnAround(motor);
            dream.Respawn(motor);

            // The literal, not `dream.SpawnPoint` — that is the expression
            // under test, and comparing it with itself passes for any value.
            Assert.That(motor.Feet, Is.EqualTo(Vector3.zero));

            // Facing the bedroom's own door, so the way out is in front of them
            // rather than behind. Turned around first, because a fresh rig
            // already looks +z and this would hold whether Respawn set the
            // rotation or not.
            Assert.That(motor.transform.forward.z, Is.EqualTo(1f).Within(1e-3f));
        }

        [UnityTest]
        public IEnumerator ASleeperFacesTheDoorWhicheverWayTheBedroomIsLaid()
        {
            // The one test where the answer is not +z, which is what a fresh
            // SleeperRig looks along and what R0 happens to produce.
            DreamInstance dream = Dream(Rotation.R90);
            dream.Build(new EventLog());
            yield return null;

            Assert.That(dream.SpawnFacing.x, Is.EqualTo(1f).Within(1e-3f),
                "the bedroom is turned a quarter clockwise, so its door looks east");

            SleeperMotor motor = Runner(new Vector3(0f, 0.1f, 0f));
            dream.Respawn(motor);

            Assert.That(motor.transform.forward.x, Is.EqualTo(1f).Within(1e-3f));
        }
    }
}
