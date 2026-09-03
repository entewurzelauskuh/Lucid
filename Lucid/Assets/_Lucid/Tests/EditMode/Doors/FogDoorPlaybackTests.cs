using Lucid.Core;
using Lucid.Runtime;
using NUnit.Framework;
using UnityEngine;

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
            _door.Tick(_door.Progress >= 1f ? 0f : 0.1f);
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
        }

        [Test]
        public void FogAndExitDifferByMoreThanTheirColour()
        {
            // docs/UI.md §5: "Door states never depend on hue alone." A viewer
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
