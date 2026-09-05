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

        readonly List<GameObject> _spawned = new List<GameObject>();
        readonly List<Object> _assets = new List<Object>();

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
                if (go != null) Object.DestroyImmediate(go);
            _spawned.Clear();

            foreach (Object asset in _assets)
                if (asset != null) Object.DestroyImmediate(asset);
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

        CubeDefinition Definition(string id, FaceMask doorways, CubeCategory category)
        {
            var d = ScriptableObject.CreateInstance<CubeDefinition>();
            d.Configure(id, "test", id, category, doorways,
                false, 1, CubePrefab(id, doorways), new[] { "*" });
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
            _assets.Add(pack);
            return pack;
        }

        DreamInstance Dream()
        {
            var go = new GameObject("Dream");
            _spawned.Add(go);
            var dream = go.AddComponent<DreamInstance>();
            dream.Bind(Pack(), Start, Rotation.R0);
            return dream;
        }

        /// <summary>A log that runs a corridor north out of the bedroom.</summary>
        static EventLog Corridor(int cubes, Rotation rotation = Rotation.R0)
        {
            var log = new EventLog();
            for (int i = 1; i <= cubes; i++)
            {
                log.Append(new CubePlaced(
                    log.NextSeq, new Coord(0, i, 0), Straight, rotation, "*"));
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
        static void TurnAround(SleeperMotor motor) =>
            motor.transform.rotation *= Quaternion.Euler(0f, 180f, 0f);

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
            Assert.That(cube.Doors[Face.North].State, Is.EqualTo(ConnectorState.Solid),
                "world North is a wall on this cube");
        }

        [Test]
        public void AWalledFaceShowsWall()
        {
            DreamInstance dream = Dream();
            dream.Build(new EventLog());

            DreamCube start = dream.Cubes[new Coord(0, 0, 0)];

            // The start cube has one doorway, north. The other five sockets
            // carry a FogDoor too — Derived says nothing about them, and the
            // fallback has to be wall rather than mist a Sleeper walks through.
            Assert.That(start.Doors[Face.North].State, Is.EqualTo(ConnectorState.Exit));
            foreach (Face f in new[] { Face.East, Face.South, Face.West, Face.Up, Face.Down })
                Assert.That(start.Doors[f].State, Is.EqualTo(ConnectorState.Solid), f.ToString());
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
            Assert.That(motor.Feet.z, Is.LessThan(4f), "never made it back to the bedroom");
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

            dream.Respawn(motor);

            Assert.That(motor.Feet, Is.EqualTo(dream.SpawnPoint));

            // Facing the bedroom's own door, so the way out is in front of them
            // rather than behind: the start cube's one connector is north.
            Assert.That(motor.transform.forward.z, Is.EqualTo(1f).Within(1e-3f));
        }
    }
}
