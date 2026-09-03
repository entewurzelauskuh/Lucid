using System;
using System.IO;
using Lucid.Runtime;
using Lucid.Runtime.Dev;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Lucid.Editor.Scenes
{
    /// <summary>
    /// Writes the movement gauntlet scene (docs/WORKPLAN.md §4, M0.4). The
    /// geometry comes from <see cref="GauntletBuilder"/>, the same code the
    /// PlayMode tests run against, so walking the scene and reading the test
    /// results are two views of one course.
    /// </summary>
    public static class GauntletSceneBuilder
    {
        public const string ScenePath = "Assets/_Lucid/Scenes/Gauntlet.unity";
        public const string InputActionsPath =
            "Assets/_Lucid/Runtime/Input/Sleeper.inputactions";

        [MenuItem("Lucid/Build Gauntlet Scene")]
        public static void Build()
        {
            var previous = SceneManager.GetActiveScene();
            bool untitled = string.IsNullOrEmpty(previous.path);

            // Unity refuses to add a scene beside an untitled one, dirty or
            // not, so an untitled scene has to be replaced rather than joined.
            // Replacing an empty one costs nothing — batch mode always starts
            // that way — but replacing one with unsaved work in it would throw
            // that work away, and rebuilding a dev scene is no reason to.
            // OpenScene on a scene that is already open hands back that same
            // scene, and the signature read would then close it — taking away
            // what the developer was looking at, unsaved edits and all.
            if (previous.path == ScenePath)
                throw new Exception(
                    $"close {ScenePath} first: the builder cannot rewrite the scene you " +
                    "have open. tools/build-gauntlet.sh has no such trouble.");

            if (untitled && previous.isDirty)
                throw new Exception(
                    "save the open scene first, or close it: Unity cannot add a scene " +
                    "beside an unsaved untitled one, and replacing it would lose your work.");

            // Beside a saved scene, additively, and put back afterwards: no
            // reason to close what the person at the keyboard is working on.
            bool additive = !untitled;

            string committed = CommittedSignature(additive);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,
                additive ? NewSceneMode.Additive : NewSceneMode.Single);

            bool saved = false;
            try
            {
                SceneManager.SetActiveScene(scene);

                var gauntlet = GauntletBuilder.Build();
                AddSun();
                AddRunner(gauntlet);

                string built = SceneSignature.Of(scene.GetRootGameObjects());

                // Saving mints fresh fileIDs and reorders the file, so writing
                // an unchanged scene rewrites most of it (#59). Nothing but a
                // real difference is allowed to touch the disk.
                if (built == committed)
                {
                    Debug.Log($"gauntlet: unchanged {ScenePath}");
                    return;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
                if (!EditorSceneManager.SaveScene(scene, ScenePath))
                    throw new Exception($"could not save {ScenePath}");
                saved = true;

                Debug.Log($"gauntlet: wrote {ScenePath} with {gauntlet.Lanes.Count} lanes");
            }
            finally
            {
                if (additive)
                {
                    if (previous.IsValid()) SceneManager.SetActiveScene(previous);
                    EditorSceneManager.CloseScene(scene, removeScene: true);
                }
                else if (!saved && File.Exists(ScenePath))
                {
                    // Nothing was written, so the scene built here is untitled
                    // and dirty. Leaving it open means the next build refuses
                    // (the untitled-and-dirty guard above) and the developer is
                    // holding unsaved work they never asked for.
                    EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                }
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// The signature of the scene already on disk, or null when there is
        /// none. Opened and closed again here rather than held open, so the
        /// build never has two scenes loaded at once.
        /// </summary>
        static string CommittedSignature(bool additive)
        {
            if (!File.Exists(ScenePath)) return null;

            // Untitled means the empty scene batch mode starts with, which the
            // caller has already established is not worth keeping — and Unity
            // refuses to open anything additively beside it.
            var mode = additive ? OpenSceneMode.Additive : OpenSceneMode.Single;

            Scene existing;
            try
            {
                existing = EditorSceneManager.OpenScene(ScenePath, mode);
            }
            catch (Exception e)
            {
                // The question this answers is "may I skip the write?", and the
                // answer for a scene that cannot be read is no. Overwriting a
                // corrupt file is the recovery; failing the build would leave
                // the developer deleting it by hand.
                Debug.LogWarning($"gauntlet: could not read {ScenePath} ({e.Message}); rewriting it");
                return null;
            }

            try
            {
                return SceneSignature.Of(existing.GetRootGameObjects());
            }
            finally
            {
                if (additive) EditorSceneManager.CloseScene(existing, removeScene: true);
            }
        }

        /// <summary>Entry point for <c>-executeMethod</c>.</summary>
        public static void BuildFromCommandLine()
        {
            int code = 0;
            try
            {
                Build();
            }
            catch (Exception e)
            {
                Debug.LogError($"gauntlet: {e}");
                code = 1;
            }

            if (Environment.CommandLine.Contains("-batchmode")) EditorApplication.Exit(code);
        }

        static void AddSun()
        {
            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        static void AddRunner(Gauntlet gauntlet)
        {
            var motor = SleeperRig.Create(gauntlet.SpawnFor(0), Vector3.forward);

            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (actions == null)
            {
                Debug.LogWarning(
                    $"gauntlet: no action asset at {InputActionsPath}; the runner will not move.");
                return;
            }

            motor.gameObject.AddComponent<SleeperInputSource>().Bind(actions);
        }
    }
}
