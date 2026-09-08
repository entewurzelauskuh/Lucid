using System.Collections;
using Lucid.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lucid.Tests.PlayMode.Flow
{
    /// <summary>
    /// Esc, pressed on a keyboard. <c>Leave()</c> being callable proves nothing
    /// about the map, the binding or the subscription; a key goes through all
    /// three.
    /// </summary>
    /// <remarks>
    /// On the Input System's own fixture, and not by choice: in play mode with
    /// no game focus — which batch mode never has — the Input System disables
    /// a newly added device unless its settings say to ignore focus, so a
    /// hand-rolled virtual keyboard had its events dropped on arrival
    /// (<c>InputManager.cs</c>, "running in the background"). The fixture
    /// swaps in a test runtime that has focus.
    /// </remarks>
    public sealed class SandboxInputTests : InputTestFixture
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

        [UnityTest]
        public IEnumerator EscapeLeavesTheSandboxAndDoesNothingOnTheTitle()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();

            yield return SceneFlowTests.BootToTitle();
            Services.Current.Flow.Go(FlowState.Sandbox);
            yield return SceneFlowTests.Settled(FlowState.Sandbox);

            // Each link, named, so a failure says which one broke.
            SandboxScene sandbox = Object.FindFirstObjectByType<SandboxScene>();
            Assert.That(sandbox.BackInput, Is.Not.Null, "no Back action: the Flow map or its action is missing from the asset");
            Assert.That(sandbox.BackInput.enabled, Is.True, "Back is not enabled");
            Assert.That(sandbox.BackInput.controls.Count, Is.GreaterThan(0),
                $"Back resolved to no control; bindings: {string.Join(", ", sandbox.BackInput.bindings)}; devices: {string.Join(", ", InputSystem.devices)}");

            Press(keyboard.escapeKey);
            yield return null;
            Release(keyboard.escapeKey);
            yield return SceneFlowTests.Settled(FlowState.Title);
            Assert.That(SceneFlowTests.Loaded("Dream"), Is.False);

            // On the Title the action must be disabled and unsubscribed. A stale
            // handler on the shared asset would call Leave on a destroyed
            // SandboxScene, whose Go(Title) from Title throws — which the test
            // framework reports as an unexpected error.
            Press(keyboard.escapeKey);
            yield return null;
            Release(keyboard.escapeKey);
            yield return null;

            Assert.That(Services.Current.Flow.State, Is.EqualTo(FlowState.Title));
            Assert.That(Services.Current.Flow.IsTransitioning, Is.False);
        }

        [UnityTest]
        public IEnumerator F5SwitchesBetweenTheGodViewAndTheSleeper()
        {
            // docs/UI.md §12: F5 drops in, F5 again comes back. Through the
            // key, so the map, the binding and the subscription are all tested.
            var keyboard = InputSystem.AddDevice<Keyboard>();

            yield return SceneFlowTests.BootToTitle();
            Services.Current.Flow.Go(FlowState.Sandbox);
            yield return SceneFlowTests.Settled(FlowState.Sandbox);
            yield return null;

            SandboxScene sandbox = Object.FindFirstObjectByType<SandboxScene>();
            Assert.That(sandbox.SwitchInput, Is.Not.Null, "no Switch action: the Flow map or its action is missing from the asset");
            Assert.That(sandbox.SwitchInput.enabled, Is.True, "Switch is not enabled");
            Assert.That(sandbox.SwitchInput.controls.Count, Is.GreaterThan(0), "Switch resolved to no control");
            Assert.That(sandbox.Mode, Is.EqualTo(SandboxMode.Nightmare));

            Press(keyboard.f5Key);
            yield return null;
            Release(keyboard.f5Key);
            yield return null;
            Assert.That(sandbox.Mode, Is.EqualTo(SandboxMode.Sleeper), "F5 did not drop the player in");
            Assert.That(sandbox.Sleeper, Is.Not.Null);
            Assert.That(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None), Has.Length.EqualTo(1));

            Press(keyboard.f5Key);
            yield return null;
            Release(keyboard.f5Key);
            yield return null;
            Assert.That(sandbox.Mode, Is.EqualTo(SandboxMode.Nightmare), "F5 did not bring the god view back");
            Assert.That(sandbox.Sleeper == null, Is.True, "the Sleeper outlived the switch");
            Assert.That(Services.Current.Flow.State, Is.EqualTo(FlowState.Sandbox), "F5 left the Sandbox");
        }
    }
}
