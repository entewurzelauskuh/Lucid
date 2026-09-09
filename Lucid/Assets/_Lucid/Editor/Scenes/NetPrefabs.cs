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
            if (existing != null && MatchesButForHash(existing, pack))
            {
                // The prefab is right and only its hash never reached the
                // disk: validate and save, rather than write a new asset with a
                // new file id — which would be a new hash for every build.
                Validate(existing.GetComponent<NetworkObject>());
                if (HashOf(existing) == 0) throw new Exception($"netprefabs: {RoundSyncPath} would not take its GlobalObjectIdHash");
                Debug.Log($"netprefabs: wrote {RoundSyncPath} (hash {HashOf(existing)})");
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

            // NGO computes GlobalObjectIdHash in NetworkObject.OnValidate, from
            // the asset's GUID and file id — stable across machines once
            // written, and zero until the asset has been loaded in an editor
            // and saved. A prefab committed at zero would be hashed on the
            // next editor that opened it and refuse a build that still held
            // zero (ForceSamePrefabs). So: import, load, save.
            AssetDatabase.ImportAsset(RoundSyncPath, ImportAssetOptions.ForceUpdate);
            var written = AssetDatabase.LoadAssetAtPath<GameObject>(RoundSyncPath);
            Validate(written.GetComponent<NetworkObject>());
            if (HashOf(written) == 0)
                throw new Exception($"netprefabs: {RoundSyncPath} has no GlobalObjectIdHash after saving");
            Debug.Log($"netprefabs: wrote {RoundSyncPath} (hash {HashOf(written)})");
        }

        /// <summary>
        /// Runs NGO's own OnValidate — internal, and the one place the hash
        /// is computed — then saves, so the bytes on disk carry it.
        /// </summary>
        static void Validate(NetworkObject networkObject)
        {
            var validate = typeof(NetworkObject).GetMethod("OnValidate",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (validate == null) throw new Exception("netprefabs: NetworkObject.OnValidate is gone; the hash cannot be computed");
            validate.Invoke(networkObject, null);
            EditorUtility.SetDirty(networkObject);
            AssetDatabase.SaveAssets();
        }

        static bool MatchesButForHash(GameObject prefab, DreamPack pack)
        {
            if (prefab.GetComponent<NetworkObject>() == null) return false;
            var sync = prefab.GetComponent<RoundSync>();
            if (sync == null) return false;
            var so = new SerializedObject(sync);
            return so.FindProperty("_pack").objectReferenceValue == pack;
        }

        /// <summary>
        /// The hash as the file holds it. The loaded asset is no witness:
        /// NGO's OnValidate runs on load and puts a hash in memory that the
        /// bytes on disk — what a clone and a build carry — may not have.
        /// </summary>
        static uint HashOf(GameObject _)
        {
            string yaml = System.IO.File.ReadAllText(RoundSyncPath);
            var m = System.Text.RegularExpressions.Regex.Match(yaml, @"\n  GlobalObjectIdHash: (\d+)\n");
            return m.Success ? uint.Parse(m.Groups[1].Value) : 0u;
        }

        static bool Matches(GameObject prefab, DreamPack pack) => MatchesButForHash(prefab, pack) && HashOf(prefab) != 0;
    }
}
