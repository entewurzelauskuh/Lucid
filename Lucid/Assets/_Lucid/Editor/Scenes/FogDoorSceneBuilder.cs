using System;
using Lucid.Core;
using Lucid.Runtime;
using UnityEditor;
using UnityEngine;

namespace Lucid.Editor.Scenes
{
    /// <summary>
    /// A room of fog doors, one per state, for looking at the door language
    /// (docs/UI.md §1) with your own eyes.
    /// </summary>
    /// <remarks>
    /// The M0.5 acceptance asks for the transitions to be "verified in a test
    /// scene and by a PlayMode test". The PlayMode test settles what a door
    /// *does*; only a person can settle whether Fog reads as dark and matte
    /// and Exit as bright and radiant, and whether the two are still telling
    /// apart for someone who cannot separate the hues.
    /// </remarks>
    public static class FogDoorSceneBuilder
    {
        public const string ScenePath = "Assets/_Lucid/Scenes/FogDoors.unity";

        /// <summary>Metres between neighbouring doors.</summary>
        const float Spacing = 5f;

        /// <summary>The four states, in the order a door lives through them.</summary>
        static readonly ConnectorState[] States =
        {
            ConnectorState.Fog,
            ConnectorState.Exit,
            ConnectorState.Solid,
            ConnectorState.Attached,
        };

        [MenuItem("Lucid/Build Fog Door Scene")]
        public static void Build()
        {
            bool wrote = GeneratedScene.Write(ScenePath, Populate);
            Debug.Log(wrote
                ? $"fogdoors: wrote {ScenePath} with {States.Length} doors"
                : $"fogdoors: unchanged {ScenePath}");
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
                Debug.LogError($"fogdoors: {e}");
                code = 1;
            }

            if (Environment.CommandLine.Contains("-batchmode")) EditorApplication.Exit(code);
        }

        static void Populate()
        {
            Floor();
            Sun();

            for (int i = 0; i < States.Length; i++)
            {
                float x = (i - (States.Length - 1) / 2f) * Spacing;
                Door(States[i], x);
                Wall(x);
            }

            Cycler();

            var runner = SleeperRig.Create(
                new Vector3(0f, 0.1f, -6f), Vector3.forward, "Sleeper");

            var actions = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(
                GauntletSceneBuilder.InputActionsPath);
            if (actions != null)
                runner.gameObject.AddComponent<SleeperInputSource>().Bind(actions);
        }

        /// <summary>
        /// One door that will not hold still, set apart from the row so the
        /// four frozen ones stay comparable beside it.
        /// </summary>
        static void Cycler()
        {
            const float z = 8f;
            var go = new GameObject("FogDoor-Cycling");
            go.transform.position = new Vector3(0f, 0f, z);
            go.AddComponent<FogDoor>();
            go.AddComponent<FogDoorVisual>();
            go.AddComponent<Lucid.Runtime.Dev.FogDoorCycle>().Configure(
                1.6f,
                ConnectorState.Fog, ConnectorState.Exit,
                ConnectorState.Fog, ConnectorState.Solid);

            Label(go.transform, "cycling", new Vector3(0f, CubeMetrics.DoorHeight + 0.9f, 0f));

            float pier = (Spacing - CubeMetrics.DoorWidth) / 2f;
            float offset = (CubeMetrics.DoorWidth + pier) / 2f;
            Box("CyclePierL", new Vector3(-offset, CubeMetrics.DoorHeight / 2f, z),
                new Vector3(pier, CubeMetrics.DoorHeight, 0.4f));
            Box("CyclePierR", new Vector3(offset, CubeMetrics.DoorHeight / 2f, z),
                new Vector3(pier, CubeMetrics.DoorHeight, 0.4f));
        }

        /// <summary>
        /// A wall around each doorway, because mist in mid-air is not the
        /// thing being judged — a door is read against the frame it fills.
        /// </summary>
        static void Wall(float x)
        {
            float pier = (Spacing - CubeMetrics.DoorWidth) / 2f;
            float offset = (CubeMetrics.DoorWidth + pier) / 2f;

            Box($"Pier{x:0}L", new Vector3(x - offset, CubeMetrics.DoorHeight / 2f, 0f),
                new Vector3(pier, CubeMetrics.DoorHeight, 0.4f));
            Box($"Pier{x:0}R", new Vector3(x + offset, CubeMetrics.DoorHeight / 2f, 0f),
                new Vector3(pier, CubeMetrics.DoorHeight, 0.4f));
            Box($"Lintel{x:0}", new Vector3(x, CubeMetrics.DoorHeight + 0.25f, 0f),
                new Vector3(Spacing, 0.5f, 0.4f));
        }

        static void Door(ConnectorState state, float x)
        {
            var go = new GameObject($"FogDoor-{state}");
            go.transform.position = new Vector3(x, 0f, 0f);
            go.AddComponent<FogDoor>().Initialise(state);
            go.AddComponent<FogDoorVisual>();

            Label(go.transform, state.ToString(),
                new Vector3(0f, CubeMetrics.DoorHeight + 0.9f, 0f));
        }

        /// <summary>
        /// A name a person can read from where they are standing. The first
        /// version of this made an empty GameObject that rendered nothing, so
        /// the four doors were indistinguishable to the eye — which is exactly
        /// the comparison the scene exists for.
        /// </summary>
        static void Label(Transform parent, string text, Vector3 localPosition)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                Debug.LogWarning("no built-in font; fog door labels will be missing.");
                return;
            }

            var go = new GameObject($"Label-{text}");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.font = font;
            mesh.fontSize = 64;
            mesh.characterSize = 0.06f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            go.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }

        static void Floor() =>
            Box("Floor", new Vector3(0f, -0.5f, 0f), new Vector3(Spacing * 6f, 1f, 24f));

        static void Sun()
        {
            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        static void Box(string name, Vector3 centre, Vector3 size)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.position = centre;
            box.transform.localScale = size;
        }
    }
}
