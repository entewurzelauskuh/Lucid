using System.Collections.Generic;
using Lucid.Editor.Scenes;
using Lucid.Runtime;
using Lucid.Runtime.Dev;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        public void AValueWithNoChildrenOfItsOwnIsStillCompared()
        {
            // A mask has no child properties to descend into, so it is only
            // seen if its own arm reads it. The first draft returned the name
            // of the property's type for anything it did not recognise, which
            // made this — and every AnimationCurve, Hash128 and ExposedReference
            // — invisible: the builder would have stopped rewriting a scene it
            // really had changed, which is the one failure that must not be
            // silent.
            var go = new GameObject("Sun");
            _spawned.Add(go);
            var light = go.AddComponent<Light>();
            light.cullingMask = ~0;
            string before = Of(go);

            light.cullingMask = 1;

            Assert.That(Of(go), Is.Not.EqualTo(before));
        }

        [Test]
        public void WhetherAComponentIsSwitchedOnIsCompared()
        {
            // m_Enabled is serialized, but Unity draws it in the component
            // header rather than as a field row, so the property walk never
            // reaches it. Measured: disabling this collider left the signature
            // identical until it was read explicitly.
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _spawned.Add(go);
            string before = Of(go);

            go.GetComponent<BoxCollider>().enabled = false;

            Assert.That(Of(go), Is.Not.EqualTo(before));
        }

        [Test]
        public void TheGameObjectsOwnFieldsAreCompared()
        {
            // A GameObject is not a Component, so walking the components alone
            // never saw its tag, its static flags or its icon.
            var go = new GameObject("Thing");
            _spawned.Add(go);
            string before = Of(go);

            go.tag = "Respawn";

            Assert.That(Of(go), Is.Not.EqualTo(before));
        }

        [Test]
        public void TwoAssetsInOneFileAreToldApart()
        {
            // Every built-in mesh lives in one file and shares its GUID, so a
            // GUID alone made Cube and Sphere the same asset.
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _spawned.Add(cube);
            _spawned.Add(sphere);
            sphere.name = cube.name;
            sphere.GetComponent<MeshFilter>().sharedMesh =
                Resources.GetBuiltinResource<Mesh>("Sphere.fbx");

            Assert.That(Of(sphere), Is.Not.EqualTo(Of(cube)));
        }

        [Test]
        public void ANestedSerializedClassIsCompared()
        {
            // SleeperMotor keeps the whole movement kit in a nested
            // [Serializable] class. If the walk did not descend into it, the
            // committed scene could keep one set of numbers while the code
            // said another.
            var go = new GameObject("Sleeper");
            _spawned.Add(go);
            var motor = go.AddComponent<SleeperMotor>();
            string before = Of(go);

            var serialized = new SerializedObject(motor);
            serialized.FindProperty("_settings").FindPropertyRelative("_runSpeed").floatValue = 99f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Assert.That(Of(go), Is.Not.EqualTo(before));
        }

        [Test]
        public void ASignatureSurvivesBeingSavedAndReloaded()
        {
            // The comparison the builder actually performs is between a
            // hierarchy in memory and one read back out of YAML. Every other
            // test here compares two in-memory hierarchies, which would not
            // notice a value that fails to survive the round trip — and that
            // failure rewrites the scene on every single build.
            const string temp = "Assets/_Lucid/Tests/SceneSignatureRoundTrip.unity";

            // Unity refuses to open a scene beside an untitled one, and the
            // test runner's scene is untitled in batch mode. Replacing it is
            // safe when it is empty and never safe when it is not, which is
            // the same judgement GauntletSceneBuilder has to make.
            var active = SceneManager.GetActiveScene();
            bool untitled = string.IsNullOrEmpty(active.path);
            if (untitled && active.isDirty)
                Assert.Ignore("an unsaved untitled scene is open; running this would discard it");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,
                untitled ? NewSceneMode.Single : NewSceneMode.Additive);
            try
            {
                var previous = SceneManager.GetActiveScene();
                SceneManager.SetActiveScene(scene);
                GauntletBuilder.Build(GauntletLayout.Standard);
                var sun = new GameObject("Sun").AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                if (!untitled) SceneManager.SetActiveScene(previous);

                string built = SceneSignature.Of(scene.GetRootGameObjects());
                Assert.That(EditorSceneManager.SaveScene(scene, temp), Is.True, "could not save");

                var reloaded = EditorSceneManager.OpenScene(temp, OpenSceneMode.Single);
                Assert.That(SceneSignature.Of(reloaded.GetRootGameObjects()), Is.EqualTo(built),
                    "the signature changed by being written to YAML and read back");
            }
            finally
            {
                AssetDatabase.DeleteAsset(temp);
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
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
