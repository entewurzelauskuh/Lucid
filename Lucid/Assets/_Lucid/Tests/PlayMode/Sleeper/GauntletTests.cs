using System.Collections;
using System.Collections.Generic;
using Lucid.Runtime;
using Lucid.Runtime.Dev;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Lucid.Tests.PlayMode.Sleeper
{
    /// <summary>
    /// The M0.4 acceptance (docs/WORKPLAN.md §4): a scripted run clears a
    /// 3.5 m gap and a 1.1 m ledge and fails a 4.5 m gap and a 1.4 m one.
    /// Those four are the chicane guideline of SPEC §9 made executable — every
    /// cube built from here on promises to be crossable with this kit.
    /// </summary>
    public sealed class GauntletTests
    {
        const float RunSeconds = 4f;

        readonly List<GameObject> _spawned = new List<GameObject>();
        Gauntlet _gauntlet;

        [SetUp]
        public void SetUp()
        {
            _gauntlet = GauntletBuilder.Build();
            _spawned.Add(_gauntlet.Root);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _spawned)
                if (go != null) Object.DestroyImmediate(go);
            _spawned.Clear();
        }

        SleeperMotor Runner(int lane)
        {
            var motor = SleeperRig.Create(_gauntlet.SpawnFor(lane), Vector3.forward);
            _spawned.Add(motor.gameObject);
            return motor;
        }

        IEnumerator Attempt(GauntletObstacle obstacle, float size, bool expectCleared)
        {
            int lane = _gauntlet.IndexOf(obstacle, size);
            var motor = Runner(lane);

            // One frame so the colliders built this test are live for the
            // controller's sweeps.
            yield return null;

            SleeperPilot.Settle(motor);
            SleeperPilot.RunForward(motor, RunSeconds, SleeperPilot.PolicyFor(_gauntlet, lane));

            float landing = _gauntlet.LandingHeightFor(lane);
            Vector3 feet = motor.Feet;
            string where = $"{obstacle} of {size} m ended at z {feet.z:0.00}, y {feet.y:0.00}";

            if (expectCleared)
            {
                Assert.That(feet.z, Is.GreaterThan(Gauntlet.ObstacleZ + 0.2f),
                    $"never got past the obstacle: {where}");
                Assert.That(feet.y, Is.EqualTo(landing).Within(0.1f),
                    $"did not finish standing on the far side: {where}");
                Assert.That(motor.IsGrounded, Is.True, $"still in the air: {where}");
            }
            else if (obstacle == GauntletObstacle.Gap)
            {
                Assert.That(feet.y, Is.LessThan(-1f), $"should have fallen in: {where}");
            }
            else
            {
                Assert.That(feet.y, Is.LessThan(size - 0.2f), $"should not have got up: {where}");
                Assert.That(feet.z, Is.LessThan(Gauntlet.ObstacleZ),
                    $"should still be at the foot of the ledge: {where}");
            }
        }

        // The two the guideline promises are beatable.
        [UnityTest]
        public IEnumerator ClearsTheWidestAllowedGap() =>
            Attempt(GauntletObstacle.Gap, 3.5f, expectCleared: true);

        [UnityTest]
        public IEnumerator ClearsTheTallestAllowedLedge() =>
            Attempt(GauntletObstacle.Ledge, 1.1f, expectCleared: true);

        // The two that must stay out of reach, or the kit is not a constraint.
        [UnityTest]
        public IEnumerator FallsIntoAGapWiderThanTheKit() =>
            Attempt(GauntletObstacle.Gap, 4.5f, expectCleared: false);

        [UnityTest]
        public IEnumerator CannotReachALedgeTallerThanTheKit() =>
            Attempt(GauntletObstacle.Ledge, 1.4f, expectCleared: false);

        // The comfortable ones, so a regression that breaks jumping outright
        // shows up as four failures rather than two.
        [UnityTest]
        public IEnumerator ClearsAComfortableGap() =>
            Attempt(GauntletObstacle.Gap, 3.0f, expectCleared: true);

        [UnityTest]
        public IEnumerator ClearsAComfortableLedge() =>
            Attempt(GauntletObstacle.Ledge, 1.0f, expectCleared: true);

        [UnityTest]
        public IEnumerator WalkingOffTheEdgeWithoutJumpingFalls()
        {
            // Guards the tests above: if the runner could cross a gap by
            // walking, "cleared" would prove nothing about the jump.
            int lane = _gauntlet.IndexOf(GauntletObstacle.Gap, 3.0f);
            var motor = Runner(lane);
            yield return null;

            SleeperPilot.Settle(motor);
            SleeperPilot.RunForward(motor, RunSeconds);

            Assert.That(motor.Feet.y, Is.LessThan(-1f),
                $"walked across a {3.0f} m gap, ending at y {motor.Feet.y:0.00}");

            // And it falls as a body that was running does. A fall entered with
            // speed already banked from standing drops nearly straight down,
            // which is both wrong and a way for the runs above to succeed for
            // the wrong reason.
            float dropped = -motor.Feet.y;
            float expected = motor.Settings.RunSpeed
                * Mathf.Sqrt(2f * dropped / motor.Settings.Gravity);
            Assert.That(motor.Feet.z, Is.EqualTo(expected).Within(0.75f),
                $"fell {dropped:0.0} m but only carried {motor.Feet.z:0.00} m forward");
        }
    }
}
