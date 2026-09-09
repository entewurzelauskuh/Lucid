using System.Collections;
using Lucid.Netcode;
using Lucid.Runtime;
using Lucid.Runtime.UI;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Lucid.Tests.PlayMode.Netcode
{
    /// <summary>
    /// The dev scene as generated: hosting from its panel stands the god view
    /// up, spawns the sync, and every event the host's own Nightmare places
    /// goes out on the wire. Two of these in one process is not possible, so
    /// the client's half is DreamClientTests' on the harness.
    /// </summary>
    public sealed class NetDreamSceneTests
    {
        const string SceneName = "NetDream";

        [UnityTearDown]
        public IEnumerator UnloadEverything()
        {
            NetworkManager manager = Object.FindFirstObjectByType<NetworkManager>();
            if (manager != null && manager.IsListening) manager.Shutdown();
            yield return null;
            Scene s = SceneManager.GetSceneByName(SceneName);
            if (s.IsValid() && s.isLoaded) yield return SceneManager.UnloadSceneAsync(s);
        }

        [UnityTest]
        public IEnumerator HostingFromThePanelStandsUpTheGodViewAndTheSync()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
            yield return null;
            yield return null;

            NetDreamScene scene = Object.FindFirstObjectByType<NetDreamScene>();
            Assert.That(scene, Is.Not.Null, "the scene has no NetDreamScene");
            Assert.That(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None), Is.Empty, "a camera before a role is chosen");
            UIDocument panel = Object.FindFirstObjectByType<UIDocument>();
            Assert.That(panel.rootVisualElement.Q<Button>("host"), Is.Not.Null);
            Assert.That(panel.rootVisualElement.Q<Button>("connect"), Is.Not.Null);
            Assert.That(panel.rootVisualElement.Q<TextField>("address")?.value, Is.EqualTo("127.0.0.1"));

            Assert.That(scene.Host(7799), Is.True, "could not host");
            NetworkManager manager = Object.FindFirstObjectByType<NetworkManager>();
            float deadline = Time.realtimeSinceStartup + 10f;
            while (!(manager.IsHost && scene.Sync != null && scene.Sync.IsSpawned))
            {
                if (Time.realtimeSinceStartup > deadline) Assert.Fail("hosting never settled");
                yield return null;
            }

            Assert.That(Object.FindFirstObjectByType<GodViewCamera>(), Is.Not.Null, "no god view for the host");
            Assert.That(Object.FindFirstObjectByType<NightmareHud>(), Is.Not.Null, "no HUD for the host");
            Assert.That(scene.Sync.Registry, Is.Not.Null, "the sync has no registry: the prefab carries no pack");
            Assert.That(scene.Sync.HostRound, Is.Null, "the round begins when a Sleeper connects, not before");
            Assert.That(panel.rootVisualElement.Q("form").ClassListContains("netdev--hidden"), Is.True, "the panel is still up");
        }
    }
}
