using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lucid.Runtime.UI
{
    /// <summary>
    /// The Title screen (docs/UI.md §3): wires its two M0 entries to the flow.
    /// </summary>
    /// <remarks>
    /// Every visible string is in the UXML or §14; this file composes only the
    /// build string, which is data rather than copy — §3 asks for "build
    /// version and branch", and the branch is not known at run time, so it is
    /// the version and the engine.
    /// </remarks>
    [RequireComponent(typeof(UIDocument))]
    public sealed class TitleController : MonoBehaviour
    {
        public const string SandboxButton = "sandbox";
        public const string QuitButton = "quit";
        public const string BuildLabel = "build";

        UIDocument _document;
        Button _sandbox;
        Button _quit;

        /// <summary>The document's root, for tests that read the screen.</summary>
        public VisualElement Root => _document != null ? _document.rootVisualElement : null;

        // The UIDocument builds its tree in its own OnEnable, and Unity runs
        // OnEnable in component order, so the root is there by the time this
        // runs on the same object. A null root is therefore a broken scene —
        // no PanelSettings, no source asset — and is said so, not skipped: the
        // first draft returned silently on null, and a Title whose buttons did
        // nothing would have passed every test that merely read the tree.
        void OnEnable()
        {
            _document = GetComponent<UIDocument>();
            VisualElement root = _document.rootVisualElement;
            if (root == null)
                throw new InvalidOperationException(
                    $"{name}: the UIDocument has no root; is a PanelSettings and a source asset assigned?");
            Bind(root);
        }

        void Bind(VisualElement root)
        {
            _sandbox = root.Q<Button>(SandboxButton);
            _quit = root.Q<Button>(QuitButton);
            if (_sandbox == null || _quit == null)
                throw new InvalidOperationException(
                    $"{name}: Title.uxml has no '{SandboxButton}' or '{QuitButton}' button");

            _sandbox.clicked += OnSandbox;
            _quit.clicked += OnQuit;

            var build = root.Q<Label>(BuildLabel);
            if (build != null) build.text = $"{Application.version} · Unity {Application.unityVersion}";
        }

        void OnDisable()
        {
            if (_sandbox != null) _sandbox.clicked -= OnSandbox;
            if (_quit != null) _quit.clicked -= OnQuit;
        }

        void OnSandbox()
        {
            Services services = Services.Current;
            if (services == null)
                throw new InvalidOperationException(
                    $"{name}: no Services — the Title was loaded without the Boot scene");
            services.Flow.Go(FlowState.Sandbox);
        }

        static void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
