using System.Collections.Generic;
using Lucid.Editor.Scenes;
using Lucid.Runtime.Dev;
using NUnit.Framework;
using UnityEngine;

namespace Lucid.Tests.EditMode.Scenes
{
    /// <summary>
    /// The signature that decides whether a generated scene is rewritten
    /// (#59). It has to see through everything Unity varies between saves and
    /// none of what a generator decides.
    /// </summary>
    public sealed class SceneSignatureTests
    {
        readonly List<GameObject> _spawned = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _spawned)
                if (go != null) Object.DestroyImmediate(go);
            _spawned.Clear();
        }

        GameObject Build(IReadOnlyList<GauntletLane> lanes)
        {
            GameObject root = GauntletBuilder.Build(lanes).Root;
            _spawned.Add(root);
            return root;
        }

        static string Of(params GameObject[] roots) => SceneSignature.Of(roots);

        [Test]
        public void TheSameCourseBuiltTwiceSignsTheSame()
        {
            // The whole point: a rebuild that changed nothing must not write.
            string first = Of(Build(GauntletLayout.Standard));
            string second = Of(Build(GauntletLayout.Standard));

            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public void AWiderGapSignsDifferently()
        {
            string narrow = Of(Build(new[] { new GauntletLane(GauntletObstacle.Gap, 3f) }));
            string wide = Of(Build(new[] { new GauntletLane(GauntletObstacle.Gap, 3.5f) }));

            Assert.That(wide, Is.Not.EqualTo(narrow));
        }

        [Test]
        public void ATallerLedgeSignsDifferently()
        {
            string low = Of(Build(new[] { new GauntletLane(GauntletObstacle.Ledge, 1f) }));
            string high = Of(Build(new[] { new GauntletLane(GauntletObstacle.Ledge, 1.1f) }));

            Assert.That(high, Is.Not.EqualTo(low));
        }

        [Test]
        public void AnExtraLaneSignsDifferently()
        {
            string one = Of(Build(new[] { new GauntletLane(GauntletObstacle.Gap, 3f) }));
            string two = Of(Build(new[]
            {
                new GauntletLane(GauntletObstacle.Gap, 3f),
                new GauntletLane(GauntletObstacle.Ledge, 1f),
            }));

            Assert.That(two, Is.Not.EqualTo(one));
        }

        [Test]
        public void RootOrderIsNotPartOfTheSignature()
        {
            // Unity does not promise to hand the roots back in creation order,
            // and a generator that emits the same objects in another order has
            // generated the same scene.
            //
            // The two roots must differ, or this cannot fail: identical roots
            // render identical blocks, and concatenating them is the same
            // either way whether or not anything is sorted. The first draft of
            // this test built the same lane twice and was therefore vacuous.
            GameObject a = Build(new[] { new GauntletLane(GauntletObstacle.Gap, 3f) });
            GameObject b = Build(new[] { new GauntletLane(GauntletObstacle.Ledge, 1.4f) });

            Assert.That(Of(a, b), Is.EqualTo(Of(b, a)));
        }

        [Test]
        public void ChildOrderIsNotPartOfTheSignature()
        {
            var parent = new GameObject("Parent");
            _spawned.Add(parent);
            var first = new GameObject("A");
            var second = new GameObject("B");
            first.transform.SetParent(parent.transform, false);
            second.transform.SetParent(parent.transform, false);
            string before = Of(parent);

            second.transform.SetSiblingIndex(0);

            Assert.That(Of(parent), Is.EqualTo(before));
        }

        [Test]
        public void TwoNodesAlikeInNameAndPlaceStillSignStably()
        {
            // Sorting the objects by name and position left a tie here, and
            // List.Sort is unstable, so the signature could differ between
            // runs over the same hierarchy. Sorting the rendered text cannot
            // tie: identical text is identical either way round.
            var parent = new GameObject("Parent");
            _spawned.Add(parent);
            foreach (string child in new[] { "Same", "Same" })
            {
                var go = new GameObject(child);
                go.transform.SetParent(parent.transform, false);
            }
            parent.transform.GetChild(1).gameObject.AddComponent<BoxCollider>();

            string first = Of(parent);
            parent.transform.GetChild(1).SetSiblingIndex(0);

            Assert.That(Of(parent), Is.EqualTo(first));
        }

        [Test]
        public void MovingAnObjectSignsDifferently()
        {
            GameObject root = Build(new[] { new GauntletLane(GauntletObstacle.Gap, 3f) });
            string before = Of(root);

            root.transform.GetChild(0).localPosition += new Vector3(0f, 1f, 0f);

            Assert.That(Of(root), Is.Not.EqualTo(before));
        }

        [Test]
        public void ChangingAComponentValueSignsDifferently()
        {
            // The reason this walks serialized properties rather than a list of
            // fields someone has to remember to extend: a generator can emit
            // anything and the signature still sees it.
            var go = new GameObject("Sun");
            _spawned.Add(go);
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            string before = Of(go);

            light.intensity += 0.5f;

            Assert.That(Of(go), Is.Not.EqualTo(before));
        }

        [Test]
        public void AddingAComponentSignsDifferently()
        {
            var go = new GameObject("Thing");
            _spawned.Add(go);
            string before = Of(go);

            go.AddComponent<BoxCollider>();

            Assert.That(Of(go), Is.Not.EqualTo(before));
        }

        [Test]
        public void RenamingSignsDifferently()
        {
            var go = new GameObject("Before");
            _spawned.Add(go);
            string before = Of(go);

            go.name = "After";

            Assert.That(Of(go), Is.Not.EqualTo(before));
        }
    }
}
