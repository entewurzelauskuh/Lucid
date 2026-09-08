using System.Collections;
using System.Linq;
using Lucid.Core;
using Lucid.Runtime;
using Lucid.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Lucid.Tests.PlayMode.Flow
{
    /// <summary>
    /// M0.9b's acceptance (docs/WORKPLAN.md §4): build ten cubes, drop in,
    /// walk to an exit, return to the god view, all without a network
    /// session — and docs/UI.md §12's unlimited budget and no timer.
    /// </summary>
    public sealed class SandboxTests
    {
        SimulationMode _physicsWas;

        [SetUp]
        public void StepPhysicsByHand()
        {
            // Trigger volumes fire from physics steps; the walk below takes
            // them rather than waiting a frame per step.
            _physicsWas = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;
        }

        [UnityTearDown]
        public IEnumerator UnloadEverything()
        {
            Physics.simulationMode = _physicsWas;
            foreach (string name in new[] { "Dream", "Title", "Boot" })
            {
                Scene s = SceneManager.GetSceneByName(name);
                if (s.IsValid() && s.isLoaded) yield return SceneManager.UnloadSceneAsync(s);
            }
        }

        static IEnumerator OpenTheSandbox()
        {
            yield return SceneFlowTests.BootToTitle();
            Services.Current.Flow.Go(FlowState.Sandbox);
            yield return SceneFlowTests.Settled(FlowState.Sandbox);
            yield return null;
            yield return null;
            Controller().DetachInput();
        }

        static SandboxScene Sandbox() => Object.FindFirstObjectByType<SandboxScene>();
        static NightmareController Controller() => Sandbox().Nightmare;
        static NightmareHud Hud() => Object.FindFirstObjectByType<NightmareHud>(FindObjectsInactive.Include);

        /// <summary>A corridor of straights north out of the bedroom, one on each new exit.</summary>
        static void BuildCorridor(NightmareController c, int cubes)
        {
            CubeDefinition straight = c.Palette.First(d => d.Id == "core.straight");
            for (int i = 0; i < cubes; i++)
            {
                c.Select(straight);
                c.Hover(c.Round.Round.Derived.Exits[0]);
                for (int turn = 0; turn < 4 && !c.LastVerdict.Ok; turn++) c.RotateGhost();
                PlaceVerdict placed = c.Place();
                Assert.That(placed.Ok, Is.True, $"cube {i + 1}: {placed}");
            }
        }

        static void Step(int steps = 3)
        {
            for (int i = 0; i < steps; i++) Physics.Simulate(1f / 60f);
        }

        /// <summary>Holds forward for at most <paramref name="seconds"/>, stopping when <paramref name="until"/> says so.</summary>
        static void WalkForward(SleeperMotor motor, float seconds, System.Func<bool> until = null)
        {
            const float dt = 1f / 60f;
            for (int i = 0; i < Mathf.RoundToInt(seconds / dt); i++)
            {
                if (until != null && until()) return;
                motor.Tick(SleeperInput.Forward, dt);
                Physics.Simulate(dt);
            }
        }

        [UnityTest]
        public IEnumerator TheSandboxHasNothingToSpendOrCountDown()
        {
            yield return OpenTheSandbox();
            NightmareController c = Controller();
            Round round = c.Round.Round;
            NightmareHud hud = Hud();

            Assert.That(c.Round.Unbounded, Is.True);
            Assert.That(round.Phase, Is.EqualTo(Phase.Running), "the Sandbox has a head start");
            Assert.That(hud.ShowsRoundReadouts, Is.False);
            Assert.That(hud.Root.Q("budget").resolvedStyle.display, Is.EqualTo(DisplayStyle.None), "the budget is on screen");
            Assert.That(hud.Root.Q("dawn").resolvedStyle.display, Is.EqualTo(DisplayStyle.None), "the timer is on screen");

            // An hour of building, ten cubes: no dawn, nothing refused for
            // money, nothing greyed out.
            round.Advance(3_600_000);
            BuildCorridor(c, 10);
            yield return null;
            Assert.That(round.Phase, Is.EqualTo(Phase.Running), "dawn came");
            Assert.That(round.Lattice.Cubes.Count, Is.EqualTo(11));
            Assert.That(round.Budget.CanAfford(c.Palette.Max(d => d.Cost)), Is.True, "the Sandbox ran out of budget");
            Assert.That(hud.Root.Q("cube-cards").Query(className: "palette-tile--unaffordable").ToList(), Is.Empty);
        }

        [UnityTest]
        public IEnumerator TenCubesDropInWalkToAnExitAndBack()
        {
            // The acceptance, as written.
            yield return OpenTheSandbox();
            SandboxScene sandbox = Sandbox();
            NightmareController c = Controller();
            DreamInstance dream = c.Round.Dream;
            NightmareHud hud = Hud();

            BuildCorridor(c, 10);
            yield return null;
            Coord far = new Coord(0, 10, 0);
            Assert.That(dream.Cubes.ContainsKey(far), Is.True, "the corridor does not reach ten");
            Assert.That(c.Round.Round.Derived.Exits, Is.EqualTo(new[] { new ConnectorRef(far, Face.North) }));

            sandbox.EnterSleeper();
            yield return null;
            SleeperMotor sleeper = sandbox.Sleeper;
            Assert.That(sleeper, Is.Not.Null, "nobody dropped in");
            Assert.That((sleeper.Feet - dream.SpawnPoint).magnitude, Is.LessThan(0.5f), "the Sleeper did not start in the bedroom");
            Assert.That(hud.gameObject.activeInHierarchy, Is.False, "the god view's HUD is on the Sleeper's screen");

            // Ten rooms is eighty metres; the walk that matters is the last one.
            sleeper.Warp(DreamSpace.Origin(far) + new Vector3(0f, 0.1f, 0f));
            sleeper.transform.rotation = Quaternion.LookRotation(DreamSpace.Direction(Face.North), Vector3.up);
            Step();
            Assert.That(c.Round.Round.Sleepers[0].Cube, Is.EqualTo(far), "Core does not know the Sleeper is in the last room");
            // Stop walking the moment the exit answers: the respawn puts the
            // Sleeper in the bedroom, and holding forward from there would
            // march them back up the corridor.
            WalkForward(sleeper, 2f, () => sandbox.ExitsReached > 0);

            Assert.That(sandbox.ExitsReached, Is.EqualTo(1), $"no exit reached; feet at {sleeper.Feet}");
            Assert.That((sleeper.Feet - dream.SpawnPoint).magnitude, Is.LessThan(0.5f), "reaching the exit did not put the Sleeper back in the bedroom");
            Assert.That(sandbox.Mode, Is.EqualTo(SandboxMode.Sleeper), "reaching the exit threw the Sleeper out");
            Step();
            Assert.That(c.Round.Round.Sleepers[0].Cube, Is.EqualTo(dream.Start), "Core still thinks the Sleeper is at the far end");
            Assert.That(c.Round.Round.Sleepers[0].Status, Is.EqualTo(SleeperStatus.InDream), "the Sandbox woke the Sleeper");

            sandbox.EnterNightmare();
            yield return null;
            Assert.That(sandbox.Mode, Is.EqualTo(SandboxMode.Nightmare));
            Assert.That(Object.FindObjectsByType<SleeperMotor>(FindObjectsSortMode.None), Is.Empty);
            Assert.That(hud.gameObject.activeInHierarchy, Is.True, "the HUD did not come back with the god view");
            Assert.That(c.Round.Round.Lattice.Cubes.Count, Is.EqualTo(11), "the lattice did not survive the round trip");
        }

        [UnityTest]
        public IEnumerator WalkingBackIntoAnExploredRoomMovesCoresSleeper()
        {
            // docs/CORE-API.md §10 feeds UpdateSleeperCube from telemetry; the
            // Sandbox feeds it from the entry volumes, on every arrival. The
            // first draft fed it from first entry only, and a Sleeper walking
            // back was, to Core, still in the room they had last explored.
            yield return OpenTheSandbox();
            SandboxScene sandbox = Sandbox();
            NightmareController c = Controller();
            Round round = c.Round.Round;
            BuildCorridor(c, 2);
            yield return null;

            sandbox.EnterSleeper();
            yield return null;
            SleeperMotor sleeper = sandbox.Sleeper;
            Coord one = new Coord(0, 1, 0), two = new Coord(0, 2, 0);

            // On foot, not by warp: a warp toggles the controller, and the
            // trigger events that makes are not the ones a walk makes.
            sleeper.Warp(DreamSpace.Origin(one) + new Vector3(0f, 0.1f, 0f));
            sleeper.transform.rotation = Quaternion.LookRotation(DreamSpace.Direction(Face.North), Vector3.up);
            Step();
            Assert.That(round.Sleepers[0].Cube, Is.EqualTo(one));
            Assert.That(round.Lattice.IsExplored(one), Is.True);

            WalkForward(sleeper, 3f, () => round.Sleepers[0].Cube == two);
            Assert.That(round.Sleepers[0].Cube, Is.EqualTo(two), $"never reached the second room; feet at {sleeper.Feet}");
            Assert.That(round.Lattice.IsExplored(two), Is.True);

            sleeper.transform.rotation = Quaternion.LookRotation(DreamSpace.Direction(Face.South), Vector3.up);
            WalkForward(sleeper, 3f, () => round.Sleepers[0].Cube == one);
            Assert.That(round.Sleepers[0].Cube, Is.EqualTo(one), $"walking back into an explored room did not move Core's Sleeper; feet at {sleeper.Feet}");

            // F5 back to the god view: no body, so no phantom in room one —
            // Core's Sleeper stands where the next drop-in puts them.
            sandbox.EnterNightmare();
            yield return null;
            Assert.That(round.Sleepers[0].Cube, Is.EqualTo(round.Lattice.Start), "Core keeps a phantom Sleeper while the Nightmare builds");
            Assert.That(round.Sleepers[0].Status, Is.EqualTo(SleeperStatus.InDream));
        }
    }
}
