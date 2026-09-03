using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lucid.Editor.Scenes
{
    /// <summary>
    /// Writes a generated scene, and only when it changed.
    /// </summary>
    /// <remarks>
    /// Extracted from GauntletSceneBuilder when the fog-door scene became the
    /// second caller. It was left inline deliberately until then: a helper
    /// shaped by one caller usually fits the second one badly, and the review
    /// of #65 asked for exactly this once there was something to shape it
    /// against.
    ///
    /// Every awkward part here is Unity's, and each was a defect first:
    /// a scene cannot be opened additively beside an untitled one, so an
    /// untitled scene has to be replaced rather than joined; opening a scene
    /// that is already open returns that same scene, so reading its signature
    /// would close what the developer was looking at; and an unchanged scene
    /// that returns without saving leaves an untitled dirty one behind, which
    /// makes the next run refuse.
    /// </remarks>
    public static class GeneratedScene
    {
        /// <summary>
        /// Populates a fresh scene, then saves it to <paramref name="path"/>
        /// only if the result differs from what is already there.
        /// </summary>
        /// <returns>True when the file was written.</returns>
        public static bool Write(string path, Action populate)
        {
            if (populate == null) throw new ArgumentNullException(nameof(populate));

            var previous = SceneManager.GetActiveScene();
            bool untitled = string.IsNullOrEmpty(previous.path);

            // Looking at the scene you are rebuilding is the normal case, not a
            // mistake — and it is what you are left in after building one from
            // an untitled scene, so refusing outright made the command usable
            // exactly once. It is replaced in place instead, and refused only
            // when that would discard unsaved edits.
            // Any loaded scene, not just the active one. Looking only at the
            // active scene meant a target opened additively beside it was read
            // for its signature and then closed — with the developer's unsaved
            // edits in it, without a prompt.
            Scene target = SceneManager.GetSceneByPath(path);
            bool loaded = target.IsValid() && target.isLoaded;
            bool rebuildingOpenScene = loaded;

            if (loaded && target.isDirty)
                throw new Exception(
                    $"save or discard your changes to {path} first: rebuilding would replace it.");

            if (untitled && previous.isDirty)
                throw new Exception(
                    "save the open scene first, or close it: Unity cannot add a scene beside " +
                    "an unsaved untitled one, and replacing it would lose your work.");

            bool additive = !untitled && !rebuildingOpenScene;

            string committed = CommittedSignature(path, additive);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,
                additive ? NewSceneMode.Additive : NewSceneMode.Single);

            bool saved = false;
            try
            {
                SceneManager.SetActiveScene(scene);
                populate();

                if (SceneSignature.Of(scene.GetRootGameObjects()) == committed) return false;

                Directory.CreateDirectory(Path.GetDirectoryName(path));
                if (!EditorSceneManager.SaveScene(scene, path))
                    throw new Exception($"could not save {path}");
                saved = true;
                return true;
            }
            finally
            {
                if (additive)
                {
                    if (previous.IsValid()) SceneManager.SetActiveScene(previous);
                    EditorSceneManager.CloseScene(scene, removeScene: true);
                }
                else if (!saved && File.Exists(path))
                {
                    EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                }
                AssetDatabase.Refresh();
            }
        }

        static string CommittedSignature(string path, bool additive)
        {
            if (!File.Exists(path)) return null;

            Scene existing;
            try
            {
                existing = EditorSceneManager.OpenScene(
                    path, additive ? OpenSceneMode.Additive : OpenSceneMode.Single);
            }
            catch (Exception e)
            {
                // The question this answers is "may I skip the write?", and for
                // a scene that cannot be read the answer is no.
                Debug.LogWarning($"could not read {path} ({e.Message}); rewriting it");
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
    }
}
