using System.Collections;
using Lucid.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Lucid.Tests.PlayMode.Flow
{
    /// <summary>
    /// M0.6b's last acceptance clause: the gauntlet and fog-door dev scenes
    /// still load on their own, without Boot and without Services.
    /// </summary>
    public sealed class DevSceneTests
    {
        [UnityTearDown]
        public IEnumerator UnloadDevScenes()
        {
            foreach (string name in new[] { "Gauntlet", "FogDoors" })
            {
                Scene s = SceneManager.GetSceneByName(name);
                if (s.IsValid() && s.isLoaded) yield return SceneManager.UnloadSceneAsync(s);
            }
        }

        [UnityTest]
        public IEnumerator TheGauntletLoadsOnItsOwn()
        {
            yield return SceneManager.LoadSceneAsync("Gauntlet", LoadSceneMode.Additive);
            yield return null;

            Assert.That(Services.Current, Is.Null, "a dev scene must not need the flow");
            Assert.That(Object.FindObjectsByType<SleeperMotor>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator TheFogDoorRoomLoadsOnItsOwn()
        {
            yield return SceneManager.LoadSceneAsync("FogDoors", LoadSceneMode.Additive);
            yield return null;

            Assert.That(Services.Current, Is.Null);
            Assert.That(Object.FindObjectsByType<FogDoor>(FindObjectsSortMode.None), Has.Length.GreaterThanOrEqualTo(4));
        }
    }
}
