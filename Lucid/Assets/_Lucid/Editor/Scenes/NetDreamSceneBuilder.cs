using Lucid.Netcode;
using Lucid.Runtime;
using Lucid.Runtime.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Lucid.Editor.Scenes
{
    /// <summary>
    /// The netcode dev scene (docs/WORKPLAN.md §4, M0.8): the Dream scene's
    /// bedroom, round and god view, a NetworkManager on Unity Transport, and
    /// the Host / Connect panel. Opened by hand in two editors, or by
    /// Multiplayer Play Mode's virtual player, or started with <c>--host</c>
    /// and <c>--connect</c>.
    /// </summary>
    public static class NetDreamSceneBuilder
    {
        public const string ScenePath = "Assets/_Lucid/Scenes/NetDream.unity";
        public const string PanelUxmlPath = "Assets/_Lucid/Runtime/UI/Screens/NetDev.uxml";

        [MenuItem("Lucid/Build Net Dream Scene")]
        public static void Build()
        {
            bool wrote = GeneratedScene.Write(ScenePath, Populate);
            Debug.Log(wrote ? $"netdream: wrote {ScenePath}" : $"netdream: unchanged {ScenePath}");
        }

        static void Populate()
        {
            DreamInstance dream = FlowSceneBuilders.Bedroom();
            FlowSceneBuilders.AddNightLight(dream.transform);
            var round = dream.gameObject.AddComponent<LocalRound>();   // bounded: the round has a timer here
            (GameObject rig, GameObject ui) = FlowSceneBuilders.AddNightmare(dream, round);

            var net = new GameObject("NetworkManager");
            var manager = net.AddComponent<NetworkManager>();
            manager.NetworkConfig.NetworkTransport = net.AddComponent<UnityTransport>();
            manager.NetworkConfig.EnableSceneManagement = false;
            manager.NetworkConfig.ConnectionApproval = true;

            var panelGo = new GameObject("NetDev");
            var panel = panelGo.AddComponent<UIDocument>();
            panel.panelSettings = FlowSceneBuilders.Load<PanelSettings>(UiAssets.PanelSettingsPath);
            panel.visualTreeAsset = FlowSceneBuilders.Load<VisualTreeAsset>(PanelUxmlPath);

            var scene = dream.gameObject.AddComponent<NetDreamScene>();
            scene.Configure(manager, FlowSceneBuilders.Load<GameObject>(NetPrefabs.RoundSyncPath), dream, round, rig, ui, panel,
                FlowSceneBuilders.Load<InputActionAsset>(GauntletSceneBuilder.InputActionsPath));
        }
    }
}
