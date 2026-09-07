using System;
using System.Collections;
using Lucid.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lucid.Tests.PlayMode.Flow
{
    /// <summary>
    /// M0.6b's acceptance (docs/WORKPLAN.md §4): from Title, Sandbox loads the
    /// Dream additively and returns to Title with nothing leaked — the same
    /// Services instance on both sides of the round trip, no Dream objects
    /// left behind; and the flow refuses a transition not in its table.
    /// </summary>
    /// <remarks>
    /// The first tests in the tree to load real scene assets rather than build
    /// their world in code: what is under test is the scenes and the list that
    /// lets them be loaded by name, so stubs would test something else.
    /// </remarks>
    public sealed class SceneFlowTests
    {
        const string Boot = "Boot";
        const string Title = "Title";
        const string Dream = "Dream";

        [UnityTearDown]
        public IEnumerator UnloadEverything()
        {
            // Boot last: its Bootstrap uninstalls Services on destroy, and the
            // screens' scripts ask for Services while they are torn down.
            foreach (string name in new[] { Dream, Title, Boot })
            {
                Scene s = SceneManager.GetSceneByName(name);
                if (s.IsValid() && s.isLoaded) yield return SceneManager.UnloadSceneAsync(s);
            }
            Assert.That(Services.Current, Is.Null, "Boot's teardown did not uninstall Services");
        }

        static IEnumerator BootToTitle()
        {
            yield return SceneManager.LoadSceneAsync(Boot, LoadSceneMode.Additive);
            yield return Settled(FlowState.Title);
        }

        /// <summary>Waits for the flow to arrive somewhere, or fails loudly rather than hanging.</summary>
        static IEnumerator Settled(FlowState state, float seconds = 10f)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (Services.Current == null
                   || Services.Current.Flow.IsTransitioning
                   || Services.Current.Flow.State != state)
            {
                if (Time.realtimeSinceStartup > deadline)
                    Assert.Fail($"the flow never settled on {state}; it is " +
                                (Services.Current == null ? "not installed" :
                                    $"{Services.Current.Flow.State}, transitioning={Services.Current.Flow.IsTransitioning}"));
                yield return null;
            }
        }

        static bool Loaded(string name)
        {
            Scene s = SceneManager.GetSceneByName(name);
            return s.IsValid() && s.isLoaded;
        }

        [UnityTest]
        public IEnumerator BootInstallsServicesAndGoesToTheTitle()
        {
            yield return BootToTitle();

            Assert.That(Services.Current, Is.Not.Null);
            Assert.That(Loaded(Title), Is.True, "the Title scene is not loaded");
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(Title), "the Title is not the active scene");
            Assert.That(Loaded(Dream), Is.False);
        }

        [UnityTest]
        public IEnumerator TitleToSandboxAndBackLeavesNothingBehind()
        {
            yield return BootToTitle();
            Services before = Services.Current;

            before.Flow.Go(FlowState.Sandbox);
            yield return Settled(FlowState.Sandbox);

            Assert.That(Loaded(Dream), Is.True, "the Dream scene did not load");
            Assert.That(Loaded(Title), Is.False, "the Title stayed loaded under the Sandbox");
            Assert.That(UnityEngine.Object.FindObjectsByType<SandboxScene>(FindObjectsSortMode.None), Has.Length.EqualTo(1));

            // Back the way a player goes: Esc, which SandboxScene turns into Leave.
            UnityEngine.Object.FindFirstObjectByType<SandboxScene>().Leave();
            yield return Settled(FlowState.Title);

            Assert.That(Loaded(Dream), Is.False, "the Dream scene is still loaded");
            Assert.That(Loaded(Title), Is.True);
            Assert.That(UnityEngine.Object.FindObjectsByType<SandboxScene>(FindObjectsSortMode.None), Is.Empty,
                "the Sandbox's objects outlived its scene");

            // The Title has its own bedroom as a backdrop, so one DreamInstance
            // is right and two is the Sandbox's leaking through.
            DreamInstance[] dreams = UnityEngine.Object.FindObjectsByType<DreamInstance>(FindObjectsSortMode.None);
            Assert.That(dreams, Has.Length.EqualTo(1));
            Assert.That(dreams[0].gameObject.scene.name, Is.EqualTo(Title));
            Assert.That(dreams[0].Cubes.Count, Is.EqualTo(1), "the Title's bedroom is not standing");

            Assert.That(ReferenceEquals(Services.Current, before), Is.True,
                "Services were replaced across the round trip");
        }

        [UnityTest]
        public IEnumerator ASecondBootIsRefused()
        {
            // Services are the one static, and "installed twice" is how a second
            // Boot scene — or a first that never tore down — would announce
            // itself. Loudly, in Awake, rather than by quietly replacing the
            // instance every screen already holds.
            yield return BootToTitle();

            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("already installed"));
            yield return SceneManager.LoadSceneAsync(Boot, LoadSceneMode.Additive);
            yield return null;

            // Two Boot scenes are loaded now, and the teardown unloads by name,
            // which finds only the first; the second goes here.
            Scene first = SceneManager.GetSceneByName(Boot);
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                Scene s = SceneManager.GetSceneAt(i);
                if (s.name == Boot && s != first) yield return SceneManager.UnloadSceneAsync(s);
            }
        }

        [UnityTest]
        public IEnumerator TheFlowRefusesATransitionNotInItsTable()
        {
            // A flow of its own, in Boot with nothing else: Boot → Sandbox is not
            // in the table, and refusing must be a throw, not a silent no-op that
            // leaves the screen showing while the player waits.
            var go = new GameObject("flow-under-test");
            var flow = go.AddComponent<GameFlow>();
            yield return null;

            Assert.That(flow.State, Is.EqualTo(FlowState.Boot));
            Assert.That(() => flow.Go(FlowState.Sandbox), Throws.InvalidOperationException);
            Assert.That(flow.IsTransitioning, Is.False, "a refused transition started anyway");

            UnityEngine.Object.Destroy(go);
        }

        [UnityTest]
        public IEnumerator ATransitionCannotStartWhileOneIsUnderway()
        {
            yield return BootToTitle();
            GameFlow flow = Services.Current.Flow;

            flow.Go(FlowState.Sandbox);
            Assert.That(flow.IsTransitioning, Is.True);
            Assert.That(() => flow.Go(FlowState.Sandbox), Throws.InvalidOperationException);

            yield return Settled(FlowState.Sandbox);
        }
    }
}
