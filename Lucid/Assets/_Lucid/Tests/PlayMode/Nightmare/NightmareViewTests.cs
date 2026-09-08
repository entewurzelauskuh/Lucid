using System.Collections;
using System.Linq;
using Lucid.Core;
using Lucid.Runtime;
using Lucid.Runtime.UI;
using Lucid.Tests.PlayMode.Flow;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Lucid.Tests.PlayMode.Nightmare
{
    /// <summary>
    /// M0.7's acceptance (docs/WORKPLAN.md §4): every connector type can be
    /// placed on any fog door; every rejection reason renders; placements go
    /// through Validate. Driven through the controller's own methods, which is
    /// what the input layer calls — a raycast needs a screen, and the runner
    /// has none.
    /// </summary>
    public sealed class NightmareViewTests
    {
        [UnityTearDown]
        public IEnumerator UnloadEverything()
        {
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
            yield return null;   // the HUD's Bind runs in the controller's Start
        }

        static SandboxScene Sandbox() => Object.FindFirstObjectByType<SandboxScene>();
        static NightmareController Controller() => Sandbox().Nightmare;
        static NightmareHud Hud() => Object.FindFirstObjectByType<NightmareHud>();
        static readonly ConnectorRef BedroomDoor = new ConnectorRef(new Coord(0, 0, 0), Face.North);

        [UnityTest]
        public IEnumerator TheSandboxOpensInTheGodView()
        {
            yield return OpenTheSandbox();

            Assert.That(Sandbox().Mode, Is.EqualTo(SandboxMode.Nightmare));
            Assert.That(Object.FindObjectsByType<SleeperMotor>(FindObjectsSortMode.None), Is.Empty,
                "a Sleeper is standing in a dream nobody has dropped into");
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            Assert.That(cameras, Has.Length.EqualTo(1));
            Assert.That(cameras[0].GetComponent<GodViewCamera>(), Is.Not.Null, "the one camera is not the god view");
            Assert.That(cameras[0].transform.position.y, Is.GreaterThan(CubeMetrics.Size), "the god view is not above the lattice");
        }

        [UnityTest]
        public IEnumerator ThePaletteIsTheConnectorsAndNothingElse()
        {
            yield return OpenTheSandbox();
            NightmareController c = Controller();

            // docs/WORKPLAN.md §4: "palette (connectors only)". The core pack's
            // four, in id order, at cost 1 (docs/SPEC.md §8).
            Assert.That(c.Palette.Select(d => d.Id),
                Is.EqualTo(new[] { "core.corner", "core.cross", "core.straight", "core.tee" }));
            Assert.That(c.Palette.All(d => d.Category == CubeCategory.Connector), Is.True);

            VisualElement cards = Hud().Root.Q("cube-cards");
            Assert.That(cards.Query(className: "palette-tile").ToList(), Has.Count.EqualTo(4));
            Assert.That(cards.Query<Label>(name: "cost").ToList().Select(l => l.text), Is.All.EqualTo("1"));
        }

        [UnityTest]
        public IEnumerator EveryConnectorCanBePlacedOnAFogDoor()
        {
            // The acceptance's first clause, as a chain: each type goes on the
            // deepest fog door the last one left, rotated until Core says yes.
            yield return OpenTheSandbox();
            NightmareController c = Controller();
            Round round = c.Round.Round;
            int budgetBefore = round.Budget.Points;

            foreach (CubeDefinition type in c.Palette.ToArray())
            {
                ConnectorRef door = round.Derived.Exits[0];
                c.Select(type);
                c.Hover(door);
                for (int turn = 0; turn < 4 && !c.LastVerdict.Ok; turn++) c.RotateGhost();
                Assert.That(c.LastVerdict.Ok, Is.True, $"{type.Id} fits no rotation on {door}: {c.LastVerdict}");
                Assert.That(c.Ghost.IsShown && c.Ghost.Ok, Is.True, "the ghost is not green for a legal placement");

                PlaceVerdict placed = c.Place();
                yield return null;

                Assert.That(placed.Ok, Is.True, $"{type.Id}: {placed}");
                Coord where = door.Cube.Offset(door.Face);
                Assert.That(round.Lattice.Has(where), Is.True, "Core did not take the cube");
                Assert.That(c.Round.Dream.Cubes.ContainsKey(where), Is.True, "the dream did not stand the cube up");
            }

            Assert.That(round.Budget.Points, Is.EqualTo(budgetBefore - c.Palette.Sum(d => d.Cost)),
                "the budget did not pay for what was built");
        }

        [UnityTest]
        public IEnumerator ARejectedPlacementShowsItsReasonAndPlacesNothing()
        {
            // Every connector in the core pack has a south door at R0, so all
            // four fit the bedroom's north door as they come out of the
            // palette. A corner is south and east; turned twice it is north
            // and west, and has nothing facing the door it is placed on.
            yield return OpenTheSandbox();
            NightmareController c = Controller();
            CubeDefinition corner = c.Palette.First(d => d.Id == "core.corner");
            int cubesBefore = c.Round.Round.Lattice.Cubes.Count;
            int budgetBefore = c.Round.Round.Budget.Points;

            c.Select(corner);
            c.Hover(BedroomDoor);
            Assert.That(c.LastVerdict.Ok, Is.True, "a corner at R0 has a south door and fits");
            c.RotateGhost();
            c.RotateGhost();
            Assert.That(c.Rotation, Is.EqualTo(Rotation.R180));
            Assert.That(c.LastVerdict.Error, Is.EqualTo(PlaceError.DoesNotFit));
            Assert.That(c.Ghost.IsShown && !c.Ghost.Ok, Is.True, "the ghost is not red");
            Assert.That(Hud().RejectionText, Is.EqualTo("Doesn't fit here"), "the reason is not on screen, verbatim");

            // The acceptance's third clause: confirm anyway, and Core refuses.
            PlaceVerdict placed = c.Place();
            yield return null;
            Assert.That(placed.Error, Is.EqualTo(PlaceError.DoesNotFit));
            Assert.That(c.Round.Round.Lattice.Cubes.Count, Is.EqualTo(cubesBefore), "a refused placement grew the lattice");
            Assert.That(c.Round.Round.Budget.Points, Is.EqualTo(budgetBefore), "a refused placement was paid for");

            // Turn it until it fits, and the label goes in the same frame.
            for (int turn = 0; turn < 4 && !c.LastVerdict.Ok; turn++) c.RotateGhost();
            Assert.That(c.LastVerdict.Ok, Is.True);
            Assert.That(Hud().RejectionText, Is.Null, "the reason outlived the red ghost");
        }

        [UnityTest]
        public IEnumerator TheBudgetAndTheTimerAreCores()
        {
            yield return OpenTheSandbox();
            NightmareController c = Controller();
            Round round = c.Round.Round;
            NightmareHud hud = Hud();

            Assert.That(hud.Root.Q<Label>("budget-value").text, Is.EqualTo(round.Budget.Points.ToString()));
            Assert.That(hud.Root.Q<Label>("budget-rate").text, Is.EqualTo("1 per 4 s"));
            Assert.That(hud.Root.Q<Label>("dawn-value").text, Does.Match(@"^\d+:\d\d$"));
            Assert.That(hud.Root.Q<Label>("phase").text, Does.StartWith("The Sleepers stir in "),
                "the head start is not on the banner");

            c.Select(c.Palette.First(d => d.Id == "core.straight"));
            c.Hover(BedroomDoor);
            c.Place();
            yield return null;

            Assert.That(hud.Root.Q<Label>("budget-value").text, Is.EqualTo((round.Budget.Points).ToString()));
            Assert.That(round.Budget.Points, Is.EqualTo(11), "a straight costs one");
        }

        [UnityTest]
        public IEnumerator TheLayerSliderHidesRenderersAndNothingElse()
        {
            yield return OpenTheSandbox();
            NightmareController c = Controller();
            DreamCube bedroom = c.Round.Dream.Cubes[new Coord(0, 0, 0)];

            c.View.LayerDown();   // from the top of the dream, down one
            while (c.View.Cutaway.Layer >= 0) c.View.LayerDown();

            Assert.That(bedroom.IsCutAway, Is.True);
            Assert.That(bedroom.GetComponentsInChildren<Renderer>(true).All(r => !r.enabled), Is.True, "a renderer is still on");
            Assert.That(bedroom.GetComponentsInChildren<Collider>(true).Any(col => col.enabled), Is.True,
                "a collider went with the renderers: the Sleeper would fall through a hidden room");

            // The picker will not build on what the slider hid: aimed at the
            // bedroom's one door, it finds it with the room showing and not
            // with the room cut away.
            FogDoor door = bedroom.Doors[Face.North];
            Vector2 atDoor = c.View.Camera.WorldToScreenPoint(door.transform.position);
            Assert.That(DoorPicker.Pick(c.View.Camera, atDoor, out _, out _), Is.False, "picked a door on a cut-away cube");

            c.View.SetCutaway(LayerCutaway.ShowAll(Limits.Default));
            Assert.That(bedroom.IsCutAway, Is.False);
            Assert.That(bedroom.GetComponentsInChildren<Renderer>(true).All(r => r.enabled), Is.True, "a renderer stayed off");
            Assert.That(DoorPicker.Pick(c.View.Camera, atDoor, out ConnectorRef picked, out _), Is.True,
                "the bedroom's door is not under the cursor that points at it");
            Assert.That(picked, Is.EqualTo(BedroomDoor));
        }

        [UnityTest]
        public IEnumerator EscapeDropsTheSelectionBeforeItLeaves()
        {
            yield return OpenTheSandbox();
            SandboxScene sandbox = Sandbox();
            NightmareController c = Controller();

            c.Select(c.Palette[0]);
            sandbox.Back();
            yield return null;
            Assert.That(c.Selected, Is.Null, "Esc did not drop the selection");
            // Both halves: the state is still Sandbox, and no leave is under way
            // either — a Go(Title) shows first as IsTransitioning, not as State.
            Assert.That(Services.Current.Flow.State, Is.EqualTo(FlowState.Sandbox), "Esc left with a cube in hand");
            Assert.That(Services.Current.Flow.IsTransitioning, Is.False, "Esc started leaving with a cube in hand");

            sandbox.Back();
            yield return SceneFlowTests.Settled(FlowState.Title);
        }

        [UnityTest]
        public IEnumerator AtDawnNothingCanBePlaced()
        {
            // docs/SPEC.md §5: the round ends at dawn. The local round is
            // Core's, so its clock can be run out directly; the ghost, the
            // label and the banner all follow in the next frame.
            yield return OpenTheSandbox();
            NightmareController c = Controller();
            Round round = c.Round.Round;
            int cubesBefore = round.Lattice.Cubes.Count;

            round.Advance(round.Settings.RoundLengthMs);
            yield return null;
            Assert.That(round.Phase, Is.EqualTo(Phase.Dawn));
            Assert.That(Hud().Root.Q<Label>("phase").text, Is.EqualTo("Dawn."));
            Assert.That(Hud().Root.Q<Label>("dawn-value").text, Is.EqualTo("0:00"));

            c.Select(c.Palette.First(d => d.Id == "core.straight"));
            c.Hover(BedroomDoor);
            Assert.That(c.LastVerdict.Ok, Is.False, "the ghost stands green after dawn");
            Assert.That(Hud().RejectionText, Is.EqualTo("Not a door"));
            Assert.That(c.Place().Ok, Is.False, "a cube was placed after dawn");
            yield return null;
            Assert.That(round.Lattice.Cubes.Count, Is.EqualTo(cubesBefore));
        }

        [UnityTest]
        public IEnumerator ASleeperWalkingInHardensTheDoorsOnTheGodView()
        {
            // The host loop's one line the Sandbox plays for itself: the
            // Sleeper's report goes through Core and comes back to the dream.
            yield return OpenTheSandbox();
            NightmareController c = Controller();
            Round round = c.Round.Round;

            // A T on the bedroom door has two open doors, both at depth one
            // and so both exits; a straight beyond one of them puts the
            // deepest door at depth two, and the T's other door is now fog.
            // Exploring the T hardens that one and leaves the exit alone.
            c.Select(c.Palette.First(d => d.Id == "core.tee"));
            c.Hover(BedroomDoor);
            for (int turn = 0; turn < 4 && !c.LastVerdict.Ok; turn++) c.RotateGhost();
            Assert.That(c.Place().Ok, Is.True);
            yield return null;
            Coord tee = BedroomDoor.Cube.Offset(BedroomDoor.Face);

            ConnectorRef beyond = round.Derived.Exits.First(e => e.Cube == tee);
            c.Select(c.Palette.First(d => d.Id == "core.straight"));
            c.Hover(beyond);
            for (int turn = 0; turn < 4 && !c.LastVerdict.Ok; turn++) c.RotateGhost();
            Assert.That(c.Place().Ok, Is.True, "no straight fits beyond the T");
            yield return null;
            int fogBefore = round.Derived.Connectors.Count(k => k.Key.Cube == tee && k.Value == ConnectorState.Fog);
            Assert.That(fogBefore, Is.GreaterThan(0), "the T has no fog door to harden");

            // The Sleeper arrives by report rather than by feet: the cube's
            // entry is what the host loop consumes, and the feet are M0.6's.
            Sandbox().EnterSleeper();
            yield return null;
            Assert.That(Sandbox().Sleeper, Is.Not.Null);
            Assert.That(c.gameObject.activeInHierarchy, Is.False, "the god view is still up beside the Sleeper");
            Assert.That(Object.FindFirstObjectByType<NightmareHud>(FindObjectsInactive.Include).gameObject.activeInHierarchy,
                Is.False, "the Nightmare's HUD is still up on the Sleeper's screen");
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            Assert.That(cameras, Has.Length.EqualTo(1), "two cameras are rendering");
            Assert.That(cameras[0].GetComponent<GodViewCamera>(), Is.Null, "the Sleeper is looking through the god view");

            c.Round.Dream.Cubes[tee].OnSleeperInside();
            yield return null;

            Assert.That(round.Lattice.IsExplored(tee), Is.True, "Core never heard the Sleeper");
            // Where they stand is the trap rule's input (docs/CORE-API.md §10),
            // so the report has to move Core's Sleeper as well as explore.
            Assert.That(round.Sleepers[LocalRound.LocalSleeper].Cube, Is.EqualTo(tee), "Core does not know where the Sleeper is");
            int fogAfter = round.Derived.Connectors.Count(k => k.Key.Cube == tee && k.Value == ConnectorState.Fog);
            Assert.That(fogAfter, Is.EqualTo(0), "the fog doors did not harden");
            Assert.That(c.Round.Dream.Cubes[tee].Doors.Values.Count(d => d.State == ConnectorState.Solid), Is.EqualTo(fogBefore),
                "the god view's doors do not show what Core derived");
        }
    }
}
