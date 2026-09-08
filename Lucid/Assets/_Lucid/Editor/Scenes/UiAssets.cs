using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Lucid.Editor.Scenes
{
    /// <summary>
    /// The one <see cref="PanelSettings"/> every screen renders through,
    /// written by script so it is reproducible and <c>verify-generated.sh</c>
    /// can see it drift (CLAUDE.md rule 4, in spirit: an asset nobody edits by
    /// hand).
    /// </summary>
    /// <remarks>
    /// The theme is attached here, once, rather than per UXML — the guide's §1:
    /// a restyle is one file. Scale-with-screen-size at 1920×1080 with match
    /// 0.5, because the mockups are authored at that size and the Results
    /// screen is designed to survive 1280×720 at 0.667× (docs/UI.md §15), which
    /// is exactly what this scale mode does to it.
    /// </remarks>
    public static class UiAssets
    {
        public const string PanelSettingsPath = "Assets/_Lucid/Runtime/UI/LucidPanel.asset";
        public const string ThemePath = "Assets/_Lucid/Runtime/UI/Styles/Lucid.tss";

        static readonly Vector2Int k_Reference = new Vector2Int(1920, 1080);
        const float k_Match = 0.5f;

        [MenuItem("Lucid/Build UI Assets")]
        public static void Build()
        {
            var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
            if (theme == null)
                throw new Exception($"ui: no theme at {ThemePath}; is Lucid.tss imported?");

            var settings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            bool fresh = settings == null;
            if (fresh)
            {
                settings = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(settings, PanelSettingsPath);
            }

            // Only touched when a value differs: SaveAssets on a clean asset
            // still rewrites it, and a rewrite is a diff verify-generated.sh
            // would report as drift.
            bool changed = fresh;
            if (settings.themeStyleSheet != theme) { settings.themeStyleSheet = theme; changed = true; }
            if (settings.scaleMode != PanelScaleMode.ScaleWithScreenSize)
            {
                settings.scaleMode = PanelScaleMode.ScaleWithScreenSize; changed = true;
            }
            if (settings.referenceResolution != k_Reference) { settings.referenceResolution = k_Reference; changed = true; }
            if (!Mathf.Approximately(settings.match, k_Match)) { settings.match = k_Match; changed = true; }

            if (changed)
            {
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }

            Debug.Log(changed ? $"ui: wrote {PanelSettingsPath}" : $"ui: unchanged {PanelSettingsPath}");
        }
    }
}
