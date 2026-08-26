using System.Collections;
using System.Collections.Generic;
using Lucid.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Lucid.Tests.PlayMode.Sleeper
{
    /// <summary>
    /// The movement kit of SPEC §9 measured on the real controller: run 6 m/s,
    /// a jump that rises 1.2 m, crouch 1 m tall at 2.5 m/s, and nothing else —
    /// no sprint, no double jump, no mantle.
    /// </summary>
    public sealed class SleeperMotorTests
    {
        const float Dt = SleeperPilot.Dt;

        readonly List<GameObject> _spawned = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _spawned)
                if (go != null) Object.DestroyImmediate(go);
            _spawned.Clear();
        }

        GameObject Box(string name, Vector3 centre, Vector3 size)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.position = centre;
            box.transform.localScale = size;
            _spawned.Add(box);
            Physics.SyncTransforms();
            return box;
        }

        GameObject Floor() =>
            Box("Floor", new Vector3(0f, -0.5f, 0f), new Vector3(60f, 1f, 60f));

        SleeperMotor Runner(Vector3 feet)
        {
            var motor = SleeperRig.Create(feet, Vector3.forward);
            _spawned.Add(motor.gameObject);
            return motor;
        }

        /// <summary>Ticks forward, returning how high the feet ever got.</summary>
        static float Apex(SleeperMotor motor, float seconds, int jumpOnTick = -1)
        {
            float apex = motor.Feet.y;
            int ticks = Mathf.RoundToInt(seconds / Dt);
            for (int i = 0; i < ticks; i++)
            {
                var input = new SleeperInput { JumpPressed = i == jumpOnTick };
                motor.Tick(input, Dt);
                apex = Mathf.Max(apex, motor.Feet.y);
            }
            return apex;
        }

        [UnityTest]
        public IEnumerator RunsAtTheSpecSpeed()
        {
            Floor();
            var motor = Runner(new Vector3(0f, 0.1f, 0f));
            yield return null;
            SleeperPilot.Settle(motor);

            float start = motor.Feet.z;
            SleeperPilot.RunForward(motor, 1f);

            Assert.That(motor.Feet.z - start, Is.EqualTo(motor.Settings.RunSpeed).Within(0.05f));
        }

        [UnityTest]
        public IEnumerator JumpRisesTheSpecHeight()
        {
            Floor();
            var motor = Runner(new Vector3(0f, 0.1f, 0f));
            yield return null;
            SleeperPilot.Settle(motor);

            float ground = motor.Feet.y;
            float apex = Apex(motor, 1f, jumpOnTick: 0);

            Assert.That(apex - ground, Is.EqualTo(motor.Settings.JumpRise).Within(0.03f));
        }

        [UnityTest]
        public IEnumerator AFlatJumpCarriesTheSettingsTravel()
        {
            Floor();
            var motor = Runner(new Vector3(0f, 0.1f, 0f));
            yield return null;
            SleeperPilot.Settle(motor);

            float takeoff = motor.Feet.z;
            bool left = false;
            float landed = takeoff;

            for (int i = 0; i < Mathf.RoundToInt(2f / Dt); i++)
            {
                var input = SleeperInput.Forward;
                input.JumpPressed = i == 0;
                motor.Tick(input, Dt);

                if (!motor.IsGrounded) left = true;
                else if (left) { landed = motor.Feet.z; break; }
            }

            Assert.That(left, Is.True, "never left the ground");
            Assert.That(landed - takeoff,
                Is.EqualTo(motor.Settings.JumpTravel).Within(0.2f));
        }

        [UnityTest]
        public IEnumerator ThereIsNoDoubleJump()
        {
            Floor();
            var motor = Runner(new Vector3(0f, 0.1f, 0f));
            yield return null;
            SleeperPilot.Settle(motor);

            float ground = motor.Feet.y;
            float apex = ground;
            // Press jump on every single tick. A body that can only jump from
            // the floor rises exactly as far as one that jumps once.
            for (int i = 0; i < Mathf.RoundToInt(1f / Dt); i++)
            {
                motor.Tick(new SleeperInput { JumpPressed = true }, Dt);
                apex = Mathf.Max(apex, motor.Feet.y);
            }

            Assert.That(apex - ground, Is.EqualTo(motor.Settings.JumpRise).Within(0.03f));
        }

        [UnityTest]
        public IEnumerator StandingDoesNotBankFallingSpeed()
        {
            // The ground stick pushes the body down every tick it is grounded.
            // Left to accumulate the way a real fall does, it reaches tens of
            // metres per second while the body is simply standing there, and
            // the first step off a ledge drops like a stone instead of arcing.
            //
            // An earlier version of this test jumped after the long stand and
            // measured the apex — which can never fail, because a jump assigns
            // the vertical speed rather than adding to it. Mutation testing
            // caught it: deleting the reset left that version green.
            Floor();
            var motor = Runner(new Vector3(0f, 0.1f, 0f));
            yield return null;
            SleeperPilot.Settle(motor, 2f);

            var kit = motor.Settings;
            float held = -(kit.GroundStick + kit.Gravity * Dt);

            Assert.That(motor.VerticalSpeed, Is.GreaterThanOrEqualTo(held - 0.01f),
                $"two seconds of standing banked {motor.VerticalSpeed:0.0} m/s of fall");
        }

        [UnityTest]
        public IEnumerator CrouchingLowersTheBodyAndSlowsTheRun()
        {
            Floor();
            var motor = Runner(new Vector3(0f, 0.1f, 0f));
            yield return null;
            SleeperPilot.Settle(motor);

            Assert.That(motor.Height, Is.EqualTo(motor.Settings.StandHeight).Within(1e-4f));

            float start = motor.Feet.z;
            var crouchForward = SleeperInput.Forward;
            crouchForward.Crouch = true;
            for (int i = 0; i < Mathf.RoundToInt(1f / Dt); i++) motor.Tick(crouchForward, Dt);

            Assert.That(motor.IsCrouched, Is.True);
            Assert.That(motor.Height, Is.EqualTo(motor.Settings.CrouchHeight).Within(1e-4f));
            Assert.That(motor.Feet.z - start,
                Is.EqualTo(motor.Settings.CrouchSpeed).Within(0.05f));
        }

        [UnityTest]
        public IEnumerator ReleasingCrouchInTheOpenStandsBackUp()
        {
            // The control for the test below: releasing crouch normally works,
            // so a body that stays down under a ceiling is being stopped by the
            // ceiling and not by a stance that never releases.
            Floor();
            var motor = Runner(new Vector3(0f, 0.1f, 0f));
            yield return null;
            SleeperPilot.Settle(motor);

            motor.Tick(new SleeperInput { Crouch = true }, Dt);
            Assert.That(motor.IsCrouched, Is.True);

            motor.Tick(default, Dt);

            Assert.That(motor.IsCrouched, Is.False);
            Assert.That(motor.Height, Is.EqualTo(motor.Settings.StandHeight).Within(1e-4f));
        }

        [UnityTest]
        public IEnumerator CannotStandUpInsideACrawlSpace()
        {
            // SPEC §9 guarantees crawl spaces of 1.1 m are passable; the body
            // that fits under one must not pop up through it on release.
            Floor();
            var motor = Runner(new Vector3(0f, 0.1f, 0f));
            yield return null;
            SleeperPilot.Settle(motor);

            motor.Tick(new SleeperInput { Crouch = true }, Dt);
            Assert.That(motor.IsCrouched, Is.True, "did not crouch");

            // Built after the body is already down: spawning a standing capsule
            // inside a ceiling would test the controller's depenetration rather
            // than the stance rule.
            Box("Ceiling", new Vector3(0f, 1.1f + 0.5f, 0f), new Vector3(8f, 1f, 8f));

            for (int i = 0; i < Mathf.RoundToInt(0.5f / Dt); i++) motor.Tick(default, Dt);

            Assert.That(motor.IsCrouched, Is.True, "stood up through a 1.1 m ceiling");
            Assert.That(motor.Height, Is.EqualTo(motor.Settings.CrouchHeight).Within(1e-4f));
        }
    }
}
