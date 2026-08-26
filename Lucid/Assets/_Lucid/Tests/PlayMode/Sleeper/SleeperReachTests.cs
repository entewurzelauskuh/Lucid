using System.Collections;
using Lucid.Runtime;
using Lucid.Runtime.Dev;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Lucid.Tests.PlayMode.Sleeper
{
    /// <summary>
    /// Where the kit's reach actually falls, found by bisection rather than
    /// asserted. <see cref="GauntletTests"/> samples the four points the M0.4
    /// acceptance names; those samples would still pass if the jump barely
    /// scraped 3.5 m, or if ledges failed at 1.4 m because they failed at
    /// everything. These two pin the boundary itself.
    /// </summary>
    /// <remarks>
    /// Each attempt builds its obstacle alone in an empty scene. An earlier
    /// draft ran them alongside the standard gauntlet and put every lane at
    /// x = 0, so a runner that missed a 4.5 m gap landed on the neighbouring
    /// lane's platform and was recorded as clearing it.
    /// </remarks>
    public sealed class SleeperReachTests
    {
        const float RunSeconds = 4f;

        static bool Cleared(GauntletObstacle obstacle, float size)
        {
            var gauntlet = GauntletBuilder.Build(new[] { new GauntletLane(obstacle, size) });
            var motor = SleeperRig.Create(gauntlet.SpawnFor(0), Vector3.forward);

            SleeperPilot.Settle(motor);
            SleeperPilot.RunForward(motor, RunSeconds, SleeperPilot.PolicyFor(gauntlet, 0));

            bool cleared = motor.Feet.z > Gauntlet.ObstacleZ + 0.05f
                && Mathf.Abs(motor.Feet.y - gauntlet.LandingHeightFor(0)) < 0.15f;

            Object.DestroyImmediate(motor.gameObject);
            Object.DestroyImmediate(gauntlet.Root);
            return cleared;
        }

        static float Boundary(GauntletObstacle obstacle, float clears, float fails, float resolution)
        {
            Assert.That(Cleared(obstacle, clears), Is.True,
                $"the search needs a {obstacle} of {clears} m to be beatable");
            Assert.That(Cleared(obstacle, fails), Is.False,
                $"the search needs a {obstacle} of {fails} m to be unbeatable");

            while (fails - clears > resolution)
            {
                float middle = 0.5f * (clears + fails);
                if (Cleared(obstacle, middle)) clears = middle;
                else fails = middle;
            }
            return 0.5f * (clears + fails);
        }

        [UnityTest]
        public IEnumerator TheWidestCrossableGapIsAboutFourMetres()
        {
            yield return null;
            float widest = Boundary(GauntletObstacle.Gap, 3.0f, 5.0f, 0.05f);

            Assert.That(widest, Is.EqualTo(4f).Within(0.4f),
                $"SPEC §9 promises a jump of about 4 m; this one reaches {widest:0.00} m");
        }

        [UnityTest]
        public IEnumerator TheTallestClimbableLedgeIsTheJumpItself()
        {
            yield return null;
            var kit = new SleeperMotorSettings();
            float tallest = Boundary(GauntletObstacle.Ledge, 0.9f, 1.5f, 0.025f);

            // With no mantle to help, the ledge a Sleeper can take is the
            // height they jump, less the little the freeze costs: horizontal
            // motion is held until the feet clear the lip, so the last few
            // centimetres of apex are too brief to cross the capsule's own
            // radius. That lands near 1.19 m. Before the rule existed the
            // capsule's rounded foot climbed to 1.4 m off a 1.2 m apex.
            //
            // The search starts below 1.1 m and ends above 1.4 m on purpose:
            // bounded at the acceptance figures, the lower half of this
            // assertion would be satisfied by the search range rather than by
            // the controller.
            Assert.That(tallest, Is.EqualTo(kit.JumpRise).Within(0.1f),
                $"a {kit.JumpRise:0.00} m jump reached a {tallest:0.00} m ledge");
        }
    }
}
