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
            if (untitled && previous.isDirty)
                throw new Exception(
                    "save the open scene first, or close it: Unity cannot add a scene " +
                    "beside an unsaved untitled one, and replacing it would lose your work.");

            // Beside a saved scene, additively, and put back afterwards: no
            // reason to close what the person at the keyboard is working on.
            bool additive = !untitled;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,
                additive ? NewSceneMode.Additive : NewSceneMode.Single);

            try
            {
                SceneManager.SetActiveScene(scene);

                var gauntlet = GauntletBuilder.Build();
                AddSun();
                AddRunner(gauntlet);

                Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
                if (!EditorSceneManager.SaveScene(scene, ScenePath))
                    throw new Exception($"could not save {ScenePath}");

                Debug.Log($"gauntlet: wrote {ScenePath} with {gauntlet.Lanes.Count} lanes");
            }
            finally
            {
                if (additive)
                {
                    if (previous.IsValid()) SceneManager.SetActiveScene(previous);
                    EditorSceneManager.CloseScene(scene, removeScene: true);
                }
                AssetDatabase.Refresh();
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
