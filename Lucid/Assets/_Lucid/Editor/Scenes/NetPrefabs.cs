using System;
using Lucid.Netcode;
using Lucid.Runtime;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Lucid.Editor.Scenes
{
    /// <summary>
    /// The one network prefab (docs/NETCODE.md §13): a NetworkObject carrying
    /// RoundSync and the core pack it hashes over. Written by script like the
    /// PanelSettings, and only when a field differs, so verify-generated.sh
    /// sees drift in what this compares and a clean tree stays clean.
    /// </summary>
    public static class NetPrefabs
    {
        public const string RoundSyncPath = "Assets/_Lucid/Netcode/RoundSync.prefab";

        [MenuItem("Lucid/Build Net Prefabs")]
        public static void Build()
        {
            var pack = AssetDatabase.LoadAssetAtPath<DreamPack>(FlowSceneBuilders.CorePackPath);
            if (pack == null) throw new Exception($"netprefabs: nothing at {FlowSceneBuilders.CorePackPath}");

            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(RoundSyncPath);
            if (existing != null && Matches(existing, pack))
            {
                Debug.Log($"netprefabs: unchanged {RoundSyncPath}");
                return;
            }

            var go = new GameObject("RoundSync");
            try
            {
                go.AddComponent<NetworkObject>();
                go.AddComponent<RoundSync>().Configure(pack);
                PrefabUtility.SaveAsPrefabAsset(go, RoundSyncPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
            Debug.Log($"netprefabs: wrote {RoundSyncPath}");
        }

        static bool Matches(GameObject prefab, DreamPack pack)
        {
            if (prefab.GetComponent<NetworkObject>() == null) return false;
            var sync = prefab.GetComponent<RoundSync>();
            if (sync == null) return false;
            var so = new SerializedObject(sync);
            return so.FindProperty("_pack").objectReferenceValue == pack;
        }
    }
}
