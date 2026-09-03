using System.Collections;
using System.Collections.Generic;
using Lucid.Core;
using Lucid.Runtime;
using Lucid.Tests.PlayMode.Sleeper;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Lucid.Tests.PlayMode.Doors
{
    /// <summary>
    /// The M0.5 acceptance (docs/WORKPLAN.md §4): collision and the wake
    /// trigger driven by state. Driven with a real Sleeper rather than by
    /// reading flags, because the thing being tested is whether a body is
    /// stopped, not whether a boolean says it should be.
    /// </summary>
    public sealed class FogDoorTests
    {
        const float DoorZ = 5f;

        readonly List<GameObject> _spawned = new List<GameObject>();

        [TearDown]
        public void DestroySpawned()
        {
            foreach (var go in _spawned)
                if (go != null) Object.DestroyImmediate(go);
            _spawned.Clear();
        }

        void Floor()
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = "Floor";
            box.transform.position = new Vector3(0f, -0.5f, DoorZ);
            box.transform.localScale = new Vector3(20f, 1f, 40f);
            _spawned.Add(box);
        }

        FogDoor Door(ConnectorState state)
        {
            var go = new GameObject($"FogDoor-{state}");
            go.transform.position = new Vector3(0f, 0f, DoorZ);
            var door = go.AddComponent<FogDoor>();
            door.Initialise(state);
            _spawned.Add(go);
            Physics.SyncTransforms();
            return door;
        }

        SleeperMotor Runner()
        {
            var motor = SleeperRig.Create(new Vector3(0f, 0.1f, 0f), Vector3.forward);
            _spawned.Add(motor.gameObject);
            return motor;
        }

        SimulationMode _physicsWas;

        [SetUp]
        public void DrivePhysicsByHand()
        {
            _physicsWas = Physics.simulationMode;
            // SleeperPilot ticks the motor many times inside one frame, which
            // is what makes the movement tests fast and frame-rate independent.
            // Collision survives that — CharacterController.Move sweeps
            // synchronously — but OnTriggerEnter is dispatched by the physics
            // step, which never runs. Stepping the simulation by hand keeps
            // both properties: real trigger callbacks, still no waiting.
            Physics.simulationMode = SimulationMode.Script;
        }

        // Restores what was there rather than assuming FixedUpdate: this is
        // the only fixture in the tree that touches a global, and guessing its
        // previous value would quietly rewrite the project's setting.
        [TearDown]
        public void RestorePhysics() => Physics.simulationMode = _physicsWas;

        /// <summary>
        /// Stopped *by the door*, rather than merely not past it.
        /// </summary>
        /// <remarks>
        /// "z &lt; the door" is also true of a Sleeper that fell through the
        /// floor, never spawned its controller or received no input. Requiring
        /// it to have arrived at the door as well pins the barrier's front face
        /// and makes the assertion about the mist.
        /// </remarks>
        static void AssertStoppedAtTheDoor(float reached, string what)
        {
            Assert.That(reached, Is.LessThan(DoorZ),
                $"walked through {what} to z {reached:0.00}");
            Assert.That(reached, Is.GreaterThan(DoorZ - 1f),
                $"never reached {what} at all — stopped at z {reached:0.00}");
        }

        /// <summary>Walks a Sleeper at the door and reports how far it got.</summary>
        static float WalkInto(SleeperMotor motor)
        {
            const float dt = SleeperPilot.Dt;
            for (int i = 0; i < Mathf.RoundToInt(2.5f / dt); i++)
            {
                motor.Tick(SleeperInput.Forward, dt);
                Physics.Simulate(dt);
            }
            return motor.Feet.z;
        }

        [UnityTest]
        public IEnumerator FogStopsASleeper()
        {
            Floor();
            var door = Door(ConnectorState.Fog);
            var motor = Runner();
            yield return null;

            float reached = WalkInto(motor);

            Assert.That(door.IsPassable, Is.False);
            AssertStoppedAtTheDoor(reached, "grey mist");
        }

        [UnityTest]
        public IEnumerator SolidStopsASleeper()
        {
            Floor();
            Door(ConnectorState.Solid);
            var motor = Runner();
            yield return null;

            float reached = WalkInto(motor);

            AssertStoppedAtTheDoor(reached, "hardened wall");
        }

        [UnityTest]
        public IEnumerator AnAttachedDoorwayLetsASleeperThroughWithoutWaking()
        {
            // The interesting negative. Attached is passable exactly like Exit,
            // so a wake driven by "did they pass through?" would fire on every
            // ordinary doorway in the dream.
            Floor();
            var door = Door(ConnectorState.Attached);
            int touched = 0;
            door.Touched += _ => touched++;
            var motor = Runner();
            yield return null;

            float reached = WalkInto(motor);

            Assert.That(reached, Is.GreaterThan(DoorZ),
                $"an open passage stopped a Sleeper at z {reached:0.00}");
            Assert.That(touched, Is.Zero, "walking between two rooms woke the Sleeper");
        }

        [UnityTest]
        public IEnumerator TheExitLetsASleeperThroughAndReportsIt()
        {
            Floor();
            var door = Door(ConnectorState.Exit);
            var seen = new List<FogDoor>();
            door.Touched += seen.Add;
            var motor = Runner();
            yield return null;

            float reached = WalkInto(motor);

            Assert.That(reached, Is.GreaterThan(DoorZ),
                $"the light stopped a Sleeper at z {reached:0.00}");
            Assert.That(seen, Has.Count.EqualTo(1), "the exit did not report exactly one touch");
            Assert.That(seen[0], Is.SameAs(door));
        }

        [UnityTest]
        public IEnumerator ADoorThatHardensStopsASleeperItWasLettingThrough()
        {
            // Fog → Solid is what a Sleeper's own exploration does to the cube
            // they enter, and it has to take effect the moment it is told.
            Floor();
            var door = Door(ConnectorState.Exit);
            var motor = Runner();
            yield return null;

            door.SetState(ConnectorState.Fog);
            door.SetState(ConnectorState.Solid);
            Physics.SyncTransforms();

            float reached = WalkInto(motor);

            AssertStoppedAtTheDoor(reached, "a door that hardened");
        }

        [UnityTest]
        public IEnumerator ADoorThatDissolvesLetsThroughASleeperItWasStopping()
        {
            Floor();
            var door = Door(ConnectorState.Fog);
            var motor = Runner();
            yield return null;

            door.SetState(ConnectorState.Attached);
            Physics.SyncTransforms();

            float reached = WalkInto(motor);

            Assert.That(reached, Is.GreaterThan(DoorZ),
                $"a dissolved door still blocked a Sleeper at z {reached:0.00}");
        }
    }
}
