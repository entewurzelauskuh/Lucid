using System;
using UnityEditor;
using UnityEngine;

namespace Lucid.Editor.Scenes
{
    /// <summary>
    /// Every generated scene, built in one editor launch.
    /// </summary>
    /// <remarks>
    /// One entry point rather than a script per scene: launching Unity costs
    /// far more than building a scene, and #61 wants a single command it can
    /// run to check that no generated artefact has drifted from its
    /// generator.
    /// </remarks>
    public static class GeneratedScenes
    {
        public static void BuildAll()
        {
            GauntletSceneBuilder.Build();
            FogDoorSceneBuilder.Build();
        }

        /// <summary>Entry point for <c>-executeMethod</c>.</summary>
        public static void BuildAllFromCommandLine()
        {
            int code = 0;
            try
            {
                BuildAll();
            }
            catch (Exception e)
            {
                Debug.LogError($"scenes: {e}");
                code = 1;
            }

            if (Environment.CommandLine.Contains("-batchmode")) EditorApplication.Exit(code);
        }
    }
}
