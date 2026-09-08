using System.Collections;
using Lucid.Runtime;
using Lucid.Runtime.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Lucid.Tests.PlayMode.UI
{
    /// <summary>
    /// Phase 1 of the design system's guide: the Title renders with the real
    /// tokens and the real fonts, and its two M0 entries do what docs/UI.md §3
    /// and §16 say.
    /// </summary>
    public sealed class TitleScreenTests
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

        static IEnumerator OpenTheTitle()
        {
            yield return SceneManager.LoadSceneAsync("Boot", LoadSceneMode.Additive);
            float deadline = Time.realtimeSinceStartup + 10f;
            while (Services.Current == null || Services.Current.Flow.State != FlowState.Title
                   || Services.Current.Flow.IsTransitioning)
            {
                if (Time.realtimeSinceStartup > deadline) Assert.Fail("the Title never opened");
                yield return null;
            }
            // Two frames: one for the UIDocument to build its tree, one for
            // layout and style resolution, which is what the font test reads.
            yield return null;
            yield return null;
        }

        static TitleController Title() =>
            Object.FindFirstObjectByType<TitleController>();

        [UnityTest]
        public IEnumerator TheTitleShowsTheTwoEntriesThatWorkOffline()
        {
            yield return OpenTheTitle();
            VisualElement root = Title().Root;

            // docs/UI.md §16: Sandbox and Quit, and nothing that needs Steam.
            Assert.That(root.Q<Button>(TitleController.SandboxButton)?.text, Is.EqualTo("Sandbox"));
            Assert.That(root.Q<Button>(TitleController.QuitButton)?.text, Is.EqualTo("Quit"));
            Assert.That(root.Q("host"), Is.Null, "Host a dream is M1");
            Assert.That(root.Q("join"), Is.Null, "Join is M1");
            Assert.That(root.Q("options"), Is.Null, "Options is M1");

            Assert.That(root.Q<Label>("wordmark")?.text, Is.EqualTo("LUCID"));
            Assert.That(root.Q<Label>(TitleController.BuildLabel)?.text, Is.Not.Empty, "no build string");
        }

        [UnityTest]
        public IEnumerator TheWordmarkIsSetInTheDisplayFace()
        {
            // The whole point of Phase 1: real fonts, not the fallback. A
            // missing TTF or a wrong url() in the tokens leaves the label in
            // Unity's default face and every other assertion still green.
            yield return OpenTheTitle();
            var wordmark = Title().Root.Q<Label>("wordmark");

            FontDefinition face = wordmark.resolvedStyle.unityFontDefinition;
            string name = face.fontAsset != null ? face.fontAsset.name : face.font != null ? face.font.name : "";
            Assert.That(name, Does.Contain("Cormorant"), $"the wordmark resolved to '{name}'");
        }

        [UnityTest]
        public IEnumerator TheBuildLabelIsSetInTabularFigures()
        {
            yield return OpenTheTitle();
            var build = Title().Root.Q<Label>(TitleController.BuildLabel);

            FontDefinition face = build.resolvedStyle.unityFontDefinition;
            string name = face.fontAsset != null ? face.fontAsset.name : face.font != null ? face.font.name : "";
            Assert.That(name, Does.Contain("Tabular"), $"the build label resolved to '{name}'");
        }

        [UnityTest]
        public IEnumerator QuitAsksToEndTheSession()
        {
            // The real handler ends play mode, which no test survives, so the
            // hook is replaced and the wiring is what is asserted.
            yield return OpenTheTitle();
            TitleController title = Title();
            bool asked = false;
            title.QuitHandler = () => asked = true;

            using (var submit = NavigationSubmitEvent.GetPooled())
            {
                submit.target = title.Root.Q<Button>(TitleController.QuitButton);
                title.Root.Q<Button>(TitleController.QuitButton).SendEvent(submit);
            }
            yield return null;

            Assert.That(asked, Is.True, "Quit is not wired");
        }

        [UnityTest]
        public IEnumerator SandboxTakesThePlayerIntoTheDream()
        {
            yield return OpenTheTitle();
            var sandbox = Title().Root.Q<Button>(TitleController.SandboxButton);

            // Pressed the way a keyboard or gamepad presses it: Button turns a
            // navigation submit into a click. A pointer click needs a pointer,
            // and a synthesized ClickEvent alone does not reach Clickable.
            using (var submit = NavigationSubmitEvent.GetPooled())
            {
                submit.target = sandbox;
                sandbox.SendEvent(submit);
            }

            float deadline = Time.realtimeSinceStartup + 10f;
            while (Services.Current.Flow.State != FlowState.Sandbox || Services.Current.Flow.IsTransitioning)
            {
                if (Time.realtimeSinceStartup > deadline)
                    Assert.Fail($"clicking Sandbox left the flow on {Services.Current.Flow.State}");
                yield return null;
            }

            Scene dream = SceneManager.GetSceneByName("Dream");
            Assert.That(dream.isLoaded, Is.True);
            SandboxScene scene = Object.FindFirstObjectByType<SandboxScene>();
            Assert.That(scene.Sleeper, Is.Not.Null, "no Sleeper standing in the bedroom");
            Assert.That(scene.GetComponent<DreamInstance>().Cubes.Count, Is.EqualTo(1), "no bedroom to stand in");
        }
    }
}
