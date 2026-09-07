using System;
using System.Linq;
using Lucid.Core;
using Lucid.Runtime;
using Lucid.Runtime.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Lucid.Editor.Scenes
{
    /// <summary>
    /// The three scenes the flow moves between (docs/WORKPLAN.md §4, M0.6b),
    /// and the build-settings list that lets them be loaded by name.
    /// </summary>
    /// <remarks>
    /// One file for the three because they are one design: Boot is always
    /// there and holds the flow; Title and Dream are loaded beside it and never
    /// load each other. Each is built by <see cref="GeneratedScene.Write"/>
    /// like the gauntlet, so a rebuild on an unchanged tree changes nothing.
    /// </remarks>
    public static class FlowSceneBuilders
    {
        public const string BootPath = "Assets/_Lucid/Scenes/Boot.unity";
        public const string TitlePath = "Assets/_Lucid/Scenes/Title.unity";
        public const string DreamPath = "Assets/_Lucid/Scenes/Dream.unity";

        public const string CorePackPath = "Assets/_Lucid/Packs/core/core.asset";
        public const string TitleUxmlPath = "Assets/_Lucid/Runtime/UI/Screens/Title.uxml";
        public const string StartTypeId = "core.start";

        /// <summary>
        /// Boot first — a player build starts on scene 0, and without Boot there
        /// are no Services — then the flow's scenes, then the two dev scenes,
        /// so that they can be loaded by name as well as opened by hand.
        /// </summary>
        public static readonly string[] BuildList =
        {
            BootPath, TitlePath, DreamPath,
            GauntletSceneBuilder.ScenePath, FogDoorSceneBuilder.ScenePath,
        };

        [MenuItem("Lucid/Build Flow Scenes")]
        public static void BuildAll()
        {
            Report("boot", BootPath, GeneratedScene.Write(BootPath, PopulateBoot));
            Report("title", TitlePath, GeneratedScene.Write(TitlePath, PopulateTitle));
            Report("dream", DreamPath, GeneratedScene.Write(DreamPath, PopulateDream));
            WriteBuildSettings();
        }

        static void Report(string tag, string path, bool wrote) =>
            Debug.Log(wrote ? $"{tag}: wrote {path}" : $"{tag}: unchanged {path}");

        // ---- Boot ---------------------------------------------------------------

        static void PopulateBoot()
        {
            var go = new GameObject("Bootstrap");
            go.AddComponent<GameFlow>();
            go.AddComponent<Bootstrap>();
        }

        // ---- Title --------------------------------------------------------------

        static void PopulateTitle()
        {
            // The backdrop is the real bedroom (docs/UI.md §3), not an image.
            DreamInstance dream = Bedroom();

            // Standing where a Sleeper would, looking at the door. The start
            // cube's one connector is north, four metres from the origin.
            var camera = new GameObject("Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0.6f, 1.5f, -2.2f);
            camera.transform.LookAt(new Vector3(0f, 1.4f, CubeMetrics.Half));
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.03f, 0.04f, 0.07f);
            camera.transform.SetParent(dream.transform, true);

            AddNightLight(dream.transform);

            var ui = new GameObject("Title");
            var document = ui.AddComponent<UIDocument>();
            document.panelSettings = Load<PanelSettings>(UiAssets.PanelSettingsPath);
            document.visualTreeAsset = Load<VisualTreeAsset>(TitleUxmlPath);
            ui.AddComponent<TitleController>();
        }

        // ---- Dream --------------------------------------------------------------

        static void PopulateDream()
        {
            DreamInstance dream = Bedroom();
            AddNightLight(dream.transform);

            var sandbox = dream.gameObject.AddComponent<SandboxScene>();
            sandbox.Configure(Load<InputActionAsset>(GauntletSceneBuilder.InputActionsPath));
        }

        // ---- shared -------------------------------------------------------------

        static DreamInstance Bedroom()
        {
            var go = new GameObject("Dream");
            var dream = go.AddComponent<DreamInstance>();
            dream.Bind(Load<DreamPack>(CorePackPath), StartTypeId, Rotation.R0);
            go.AddComponent<EmptyDream>();
            return dream;
        }

        /// <summary>A bedroom at night: one dim, cool light from above.</summary>
        static void AddNightLight(Transform parent)
        {
            var light = new GameObject("Night").AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.72f, 0.80f, 1f);
            light.intensity = 1.2f;
            light.range = 12f;
            light.transform.SetParent(parent, false);
            light.transform.localPosition = new Vector3(0f, CubeMetrics.Size - 1f, 0f);
        }

        static T Load<T>(string path) where T : UnityEngine.Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) throw new Exception($"scenes: nothing at {path}");
            return asset;
        }

        /// <summary>
        /// Boot first, then the scenes the flow loads by name. Written only when
        /// the list differs, for the same reason the scenes are.
        /// </summary>
        static void WriteBuildSettings()
        {
            string[] wanted = BuildList;
            string[] have = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

            if (have.SequenceEqual(wanted))
            {
                Debug.Log("buildsettings: unchanged");
                return;
            }

            EditorBuildSettings.scenes = wanted.Select(p => new EditorBuildSettingsScene(p, true)).ToArray();
            Debug.Log($"buildsettings: wrote {wanted.Length} scenes");
        }
    }
}
