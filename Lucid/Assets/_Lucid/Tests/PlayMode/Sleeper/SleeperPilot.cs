using System;
using Lucid.Runtime;
using Lucid.Runtime.Dev;
using UnityEngine;

namespace Lucid.Tests.PlayMode.Sleeper
{
    /// <summary>
    /// Scripted input for a Sleeper (docs/WORKPLAN.md §4 asks the acceptance
    /// test to drive one). It steps <see cref="SleeperMotor.Tick"/> directly at
    /// a fixed dt rather than waiting on frames: a run takes microseconds, and
    /// the result does not depend on how busy the machine was.
    /// </summary>
    public static class SleeperPilot
    {
        public const float Dt = 1f / 60f;

        /// <summary>Decides, before a tick, whether this is the tick to jump.</summary>
        /// <param name="motor">The body, as it stands now.</param>
        /// <param name="advanced">How far it moved down the lane last tick.</param>
        public delegate bool JumpPolicy(SleeperMotor motor, float advanced);

        static int Ticks(float seconds) => Mathf.RoundToInt(seconds / Dt);

        /// <summary>Let the body come to rest on whatever is under it.</summary>
        public static void Settle(SleeperMotor motor, float seconds = 0.5f)
        {
            for (int i = 0; i < Ticks(seconds); i++) motor.Tick(default, Dt);
        }

        /// <summary>How many ticks off the ground count as a flight, not a bump.</summary>
        const int AirborneTicks = 3;

        /// <summary>
        /// Hold forward for at most <paramref name="seconds"/>, jumping once,
        /// when <paramref name="policy"/> says so. One jump only: a runner that
        /// could press again mid-air would be testing a double jump SPEC §9
        /// does not grant.
        ///
        /// The run ends at the first landing after a flight. What an obstacle
        /// did to the runner is settled the moment it touches down again, and a
        /// runner that kept holding forward would simply sprint off the far end
        /// of the landing platform and fall — which is how the first draft of
        /// these tests reported four false failures.
        /// </summary>
        /// <returns>Whether the jump was ever taken.</returns>
        public static bool RunForward(
            SleeperMotor motor, float seconds,
            JumpPolicy policy = null, bool stopOnLanding = true)
        {
            float previousZ = motor.Feet.z;
            bool jumped = false;
            int airborne = 0;

            for (int i = 0; i < Ticks(seconds); i++)
            {
                float advanced = motor.Feet.z - previousZ;
                previousZ = motor.Feet.z;

                var input = SleeperInput.Forward;
                if (!jumped && policy != null && policy(motor, advanced))
                {
                    input.JumpPressed = true;
                    jumped = true;
                }

                motor.Tick(input, Dt);

                if (!motor.IsGrounded) airborne++;
                else if (stopOnLanding && airborne >= AirborneTicks) return jumped;
                else airborne = 0;
            }

            return jumped;
        }

        /// <summary>Jump on the last tick the near lip is still underfoot.</summary>
        /// <remarks>
        /// The threshold sits half a step short of the lip, not on it. The
        /// standard run-up is 8 m and a tick carries 0.1 m, so a runner arrives
        /// exactly on z = 0 — and a comparison against 0 is then settled by
        /// whether float accumulation lands a millionth above or below it. That
        /// moved take-off by a whole tick, which is 0.1 m of the measured reach:
        /// twice the resolution the bisection in SleeperReachTests claims.
        /// Half-way between two reachable positions, nothing can round across.
        /// </remarks>
        public static JumpPolicy AtTheEdge(float edgeZ) =>
            (motor, _) =>
                motor.IsGrounded
                && motor.Feet.z >= edgeZ - motor.Settings.RunSpeed * Dt * 0.5f;

        /// <summary>
        /// Jump when the run stops making progress, which is what running into
        /// the face of a ledge feels like. Ignores the standing start, where
        /// nothing has moved yet either.
        /// </summary>
        public static JumpPolicy WhenBlocked(float startZ, float minimumRun = 1f)
        {
            return (motor, advanced) =>
                motor.IsGrounded
                && motor.Feet.z - startZ > minimumRun
                && advanced < motor.Settings.RunSpeed * Dt * 0.25f;
        }

        /// <summary>The policy that suits <paramref name="lane"/>.</summary>
        public static JumpPolicy PolicyFor(Gauntlet gauntlet, int lane)
        {
            var obstacle = gauntlet.Lanes[lane].Obstacle;
            switch (obstacle)
            {
                case GauntletObstacle.Gap: return AtTheEdge(Gauntlet.ObstacleZ);
                case GauntletObstacle.Ledge: return WhenBlocked(gauntlet.SpawnFor(lane).z);
                default: throw new NotSupportedException(obstacle.ToString());
            }
        }
    }
}
