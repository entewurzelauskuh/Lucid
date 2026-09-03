using Lucid.Core;
using Lucid.Runtime;
using NUnit.Framework;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace Lucid.Tests.EditMode.Doors
{
    /// <summary>
    /// That a transition actually plays, and that the mist follows it. The
    /// M0.5 acceptance asks for "the right transition for every state change",
    /// and which transition is chosen is only half of that — the other half is
    /// whether anything happens.
    /// </summary>
    public sealed class FogDoorPlaybackTests
    {
        GameObject _go;
        FogDoor _door;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("door");
            _door = _go.AddComponent<FogDoor>();
            _door.Initialise(ConnectorState.Fog);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_go);

        [Test]
        public void AStateChangeStartsItsTransitionAtTheBeginning()
        {
            _door.SetState(ConnectorState.Exit);

            Assert.That(_door.Playing, Is.EqualTo(FogDoorTransition.Kindle));
            Assert.That(_door.Progress, Is.EqualTo(0f).Within(1e-4f));
        }

        [Test]
        public void TickingCarriesTheTransitionThroughToTheEnd()
        {
            _door.SetState(ConnectorState.Exit);

            _door.Tick(0.05f);
            float part = _door.Progress;
            Assert.That(part, Is.GreaterThan(0f), "the transition did not advance");
            Assert.That(part, Is.LessThan(1f), "the transition finished in one tick");

            for (int i = 0; i < 100; i++) _door.Tick(0.05f);

            Assert.That(_door.Progress, Is.EqualTo(1f).Within(1e-4f));
            Assert.That(_door.Playing, Is.EqualTo(FogDoorTransition.None),
                "the door is still playing something after the transition ended");
        }

        [Test]
        public void ProgressNeverRunsPastTheEnd()
        {
            _door.SetState(ConnectorState.Exit);
            _door.Tick(1000f);

            Assert.That(_door.Progress, Is.EqualTo(1f).Within(1e-4f));
        }

        [Test]
        public void SettingTheSameStateAgainPlaysNothing()
        {
            _door.SetState(ConnectorState.Fog);

            Assert.That(_door.Playing, Is.EqualTo(FogDoorTransition.None));
            Assert.That(_door.Progress, Is.EqualTo(1f).Within(1e-4f));
        }

        [Test]
        public void TheMistArrivesAtTheLookOfTheStateItEndedIn()
        {
            var visual = _go.AddComponent<FogDoorVisual>();
            _door.SetState(ConnectorState.Exit);
            _door.Tick(1000f);
            visual.Refresh();

            FogDoorLook exit = FogDoorLook.Exit;
            Assert.That(visual.Shown.Brightness, Is.EqualTo(exit.Brightness).Within(1e-3f));
            Assert.That(visual.Shown.Density, Is.EqualTo(exit.Density).Within(1e-3f));
        }

        [Test]
        public void TheMistIsPartWayThroughWhileTheTransitionIs()
        {
            var visual = _go.AddComponent<FogDoorVisual>();
            visual.Refresh();
            float before = visual.Shown.Brightness;

            _door.SetState(ConnectorState.Exit);
            _door.Tick(0.1f);
            visual.Refresh();

            float half = visual.Shown.Brightness;
            Assert.That(half, Is.GreaterThan(before), "the mist did not brighten at all");
            Assert.That(half, Is.LessThan(FogDoorLook.Exit.Brightness),
                "the mist jumped straight to the end instead of playing");
        }

        [Test]
        public void ADoorInterruptedCarriesOnFromWhereItLooks()
        {
            // Fog → Exit, caught halfway, then the exit moves away again. The
            // mist must dim from the half-bright it is showing, not snap back
            // to full fog and start over.
            var visual = _go.AddComponent<FogDoorVisual>();
            visual.Refresh();

            _door.SetState(ConnectorState.Exit);
            _door.Tick(0.1f);
            visual.Refresh();
            float interrupted = visual.Shown.Brightness;

            _door.SetState(ConnectorState.Fog);
            visual.Refresh();

            Assert.That(visual.Shown.Brightness, Is.EqualTo(interrupted).Within(1e-3f),
                "the mist jumped when the transition was interrupted");

            // And it has to finish somewhere: blending from the right place is
            // half the property, arriving at the right place is the other half.
            _door.Tick(1000f);
            visual.Refresh();
            Assert.That(visual.Shown.Brightness, Is.EqualTo(FogDoorLook.Fog.Brightness).Within(1e-3f),
                "the interrupted door never arrived back at fog");
        }

        [Test]
        public void DissolvePlaysThroughToAnOpening()
        {
            // The transition with its own shader path: an opening clears in
            // wisps rather than fading evenly, so the end state has to be
            // reached through the blend and not asserted as a constant.
            var visual = _go.AddComponent<FogDoorVisual>();
            visual.Refresh();

            _door.SetState(ConnectorState.Attached);
            Assert.That(_door.Playing, Is.EqualTo(FogDoorTransition.Dissolve));

            _door.Tick(0.1f);
            visual.Refresh();
            Assert.That(visual.Shown.Dissolve, Is.GreaterThan(0f).And.LessThan(1f),
                "the door was not part-way through dissolving");

            _door.Tick(1000f);
            visual.Refresh();
            Assert.That(visual.Shown.Dissolve, Is.EqualTo(1f).Within(1e-3f));
            Assert.That(visual.Shown.Density, Is.EqualTo(0f).Within(1e-3f));
        }

        [Test]
        public void CondensePlaysThroughToSomethingThatHasStoppedMoving()
        {
            var visual = _go.AddComponent<FogDoorVisual>();
            visual.Refresh();
            float drifting = visual.Shown.Drift;
            Assert.That(drifting, Is.GreaterThan(0f), "fog was not drifting to begin with");

            _door.SetState(ConnectorState.Solid);
            Assert.That(_door.Playing, Is.EqualTo(FogDoorTransition.Condense));

            _door.Tick(0.1f);
            visual.Refresh();
            Assert.That(visual.Shown.Drift, Is.LessThan(drifting).And.GreaterThan(0f),
                "the mist did not slow gradually");

            _door.Tick(1000f);
            visual.Refresh();
            Assert.That(visual.Shown.Drift, Is.EqualTo(0f).Within(1e-4f),
                "a hardened door is still breathing");
        }

        [Test]
        public void AChangeSpecForbidsIsReportedAndStillApplied()
        {
            // The guard the whole table exists to power, and it had no test.
            // Applied anyway on purpose: the door's job is to show what Core
            // derived, and a door left showing the wrong state would hide the
            // bug rather than surface it.
            _door.Initialise(ConnectorState.Solid);

            LogAssert.Expect(LogType.Error, new Regex("Solid → Attached is not a transition"));
            _door.SetState(ConnectorState.Attached);

            Assert.That(_door.State, Is.EqualTo(ConnectorState.Attached));

            // Applied, but not animated. Playing the change would present an
            // unlawful derivation as though it were an ordinary one.
            Assert.That(_door.Playing, Is.EqualTo(FogDoorTransition.None));
            Assert.That(_door.Progress, Is.EqualTo(1f).Within(1e-4f));
        }

        [Test]
        public void BeingToldTheSameStateAgainDoesNotCancelWhatIsPlaying()
        {
            // The shape a per-frame refresh from M0.6's lattice mirror has. As
            // a reset it would snap every transition to its end on the frame
            // after it started.
            _door.SetState(ConnectorState.Exit);
            _door.Tick(0.1f);
            float midway = _door.Progress;

            _door.SetState(ConnectorState.Exit);

            Assert.That(_door.Progress, Is.EqualTo(midway).Within(1e-4f));
            Assert.That(_door.Playing, Is.EqualTo(FogDoorTransition.Kindle));
        }

        [Test]
        public void ADoorAdoptsTheCollidersItAlreadyHasInsteadOfAddingMore()
        {
            // A door saved into a scene comes back with its colliders but not
            // with any "already built" flag, so building again added a second
            // barrier and — on an exit — a second live wake trigger, and one
            // touch was reported twice. The committed FogDoors.unity had eight
            // of them baked in. The same thing happens on a domain reload in
            // play mode, where Awake does not run again.
            var go = new GameObject("saved door");
            try
            {
                var barrier = go.AddComponent<BoxCollider>();
                var wake = go.AddComponent<BoxCollider>();
                wake.isTrigger = true;

                var door = go.AddComponent<FogDoor>();
                door.Initialise(ConnectorState.Exit);

                var colliders = go.GetComponents<BoxCollider>();
                Assert.That(colliders, Has.Length.EqualTo(2),
                    "the door added colliders beside the ones it already had");
                Assert.That(colliders, Has.Member(barrier).And.Member(wake));

                // And the adopted pair is the pair the state drives.
                Assert.That(wake.enabled, Is.True, "the adopted trigger is not the live one");
                Assert.That(barrier.enabled, Is.False, "an exit is still blocking");
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void AVerticalConnectorIsASquareHoleNotADoorway()
        {
            // docs/CUBE-SPEC.md §1: a vertical connector is a 2.5 m square in
            // the floor or ceiling. Using the doorway's 2.5 × 3 covered half of
            // it and buried 1.75 m of collider in the slab, so a Sleeper would
            // have walked through the uncovered half of a Fog floor that §7
            // calls solid to the touch.
            Vector3 flat = FogDoor.OpeningSize(Face.Up);
            Assert.That(flat.x, Is.EqualTo(CubeMetrics.VerticalHole).Within(1e-4f));
            Assert.That(flat.y, Is.EqualTo(CubeMetrics.VerticalHole).Within(1e-4f));
            Assert.That(FogDoor.OpeningCentre(Face.Up), Is.EqualTo(Vector3.zero));

            Vector3 upright = FogDoor.OpeningSize(Face.North);
            Assert.That(upright.x, Is.EqualTo(CubeMetrics.DoorWidth).Within(1e-4f));
            Assert.That(upright.y, Is.EqualTo(CubeMetrics.DoorHeight).Within(1e-4f));
            Assert.That(FogDoor.OpeningCentre(Face.North).y,
                Is.EqualTo(CubeMetrics.DoorHeight / 2f).Within(1e-4f));
        }

        [Test]
        public void AHardenedDoorIsNotSeeThrough()
        {
            // Drifting mist is only as opaque as the noise happens to be, so a
            // Solid door composited to about half transparent — you could see
            // the room through a wall. docs/DECISIONS.md says opaque, and this
            // is what makes that true.
            Assert.That(FogDoorLook.Solid.Opacity, Is.EqualTo(1f).Within(1e-4f));
            Assert.That(FogDoorLook.Fog.Opacity, Is.LessThan(1f),
                "fog is meant to be mist, not a wall");
        }

        [Test]
        public void TheNumbersReachTheShader()
        {
            // Everything else here asserts FogDoorLook.Shown, which is computed
            // before a single property is set. A misspelt property name, or a
            // missing SetPropertyBlock, would leave every door rendering at the
            // material defaults with all of these tests still green.
            var visual = _go.AddComponent<FogDoorVisual>();
            _door.SetState(ConnectorState.Exit);
            _door.Tick(1000f);
            visual.Refresh();

            var renderers = _go.GetComponentsInChildren<Renderer>();
            Assert.That(renderers, Is.Not.Empty, "the quad stack was never built");

            var block = new MaterialPropertyBlock();
            renderers[0].GetPropertyBlock(block);

            Assert.That(block.GetFloat("_Brightness"),
                Is.EqualTo(FogDoorLook.Exit.Brightness).Within(1e-3f),
                "the brightness never reached the renderer");
            Assert.That(block.GetFloat("_Drift"), Is.GreaterThan(0f),
                "the drift never reached the renderer");
        }

        [Test]
        public void FogAndExitDifferByMoreThanTheirColour()
        {
            // docs/UI.md §1: "Door states never depend on hue alone." A viewer
            // who cannot separate the two hues must still see a dense dark
            // sheet against a thin radiant one.
            FogDoorLook fog = FogDoorLook.Fog, exit = FogDoorLook.Exit;

            Assert.That(exit.Brightness, Is.GreaterThan(fog.Brightness * 2f),
                "exit is not conspicuously brighter than fog");
            Assert.That(exit.Density, Is.LessThan(fog.Density * 0.75f),
                "exit is not conspicuously thinner than fog");
        }

        [Test]
        public void SolidStopsMovingAndAttachedDisappears()
        {
            // The two tells that do not depend on colour at all: a wall does
            // not breathe, and an opening has nothing in it.
            Assert.That(FogDoorLook.Solid.Drift, Is.EqualTo(0f).Within(1e-4f));
            Assert.That(FogDoorLook.Attached.Density, Is.EqualTo(0f).Within(1e-4f));
            Assert.That(FogDoorLook.Attached.Dissolve, Is.EqualTo(1f).Within(1e-4f));
        }
    }
}
