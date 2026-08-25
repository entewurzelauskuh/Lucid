using System.IO;
using System.Linq;
using Lucid.Core;
using Lucid.Editor.Cubes;
using Lucid.Runtime;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Lucid.Tests.EditMode.Cubes
{
    /// <summary>
    /// The builder end to end, against a scratch pack that is deleted
    /// afterwards. Idempotence is the interesting one: Unity mints fresh
    /// fileIDs on every save and orders the YAML by them, so writing
    /// unconditionally rewrites an unchanged cube completely.
    /// </summary>
    public sealed class CubeBuilderTests
    {
        const string Pack = "buildertest";
        static string PackRoot => $"{CubeBuilder.PacksRoot}/{Pack}";
        static string CubeFolder => $"{PackRoot}/Cubes/straight";
        static string SpecPath => $"{CubeFolder}/cube.spec.json";
        static string PrefabPath => $"{CubeFolder}/straight.prefab";

        static string Spec(float width = 4f) => SpecFixtures.Straight
            .Replace(@"""id"": ""core.straight""", $@"""id"": ""{Pack}.straight""")
            .Replace(@"""pack"": ""core""", $@"""pack"": ""{Pack}""")
            .Replace(@"""width"": 4", $@"""width"": {width}");

        [SetUp]
        public void SetUp()
        {
            Directory.CreateDirectory(CubeFolder);
            File.WriteAllText(SpecPath, Spec());
            AssetDatabase.Refresh();

            if (AssetDatabase.LoadAssetAtPath<GameObject>(CubeTemplateBuilder.TemplatePath) == null)
                CubeTemplateBuilder.Build();
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(PackRoot);
            AssetDatabase.Refresh();
        }

        static CubeBuildResult Build() => CubeBuilder.BuildFromSpec(SpecPath);

        [Test]
        public void ACubeIsBuiltFromItsSpecAlone()
        {
            CubeBuildResult r = Build();
            Assert.That(r.Ok, Is.True, r.Describe());

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null, "a prefab was written");

            var definition = AssetDatabase.LoadAssetAtPath<CubeDefinition>(
                $"{CubeFolder}/straight.asset");
            Assert.That(definition, Is.Not.Null, "and a CubeDefinition beside it");
            Assert.That(definition.Id, Is.EqualTo($"{Pack}.straight"));
            Assert.That(definition.Connectors, Is.EqualTo(FaceMask.North | FaceMask.South));
            Assert.That(definition.Cost, Is.EqualTo(1));
            Assert.That(definition.Prefab, Is.EqualTo(prefab));
        }

        [Test]
        public void EverySocketIsAtItsStandardPositionAndCarriesADoor()
        {
            Build();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Transform sockets = prefab.transform.Find("Sockets");

            Assert.That(sockets, Is.Not.Null);
            foreach (Face face in Faces.All)
            {
                Transform socket = sockets.Find(face.ToString());
                Assert.That(socket, Is.Not.Null, $"no socket for {face}");
                Assert.That(socket.localPosition, Is.EqualTo(CubeGeometry.Centre(face)),
                    $"{face} socket is off its standard position");

                var connector = socket.GetComponent<Connector>();
                Assert.That(connector, Is.Not.Null);
                Assert.That(connector.Door, Is.Not.Null, "a walled face still carries its FogDoor");
            }
        }

        [Test]
        public void OnlyTheSpecsFacesAreDoorways()
        {
            Build();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Transform sockets = prefab.transform.Find("Sockets");

            foreach (Face face in Faces.All)
            {
                bool expected = face == Face.North || face == Face.South;
                Assert.That(sockets.Find(face.ToString()).GetComponent<Connector>().IsDoorway,
                    Is.EqualTo(expected), $"{face}");
            }
        }

        [Test]
        public void TheCubeIsRegisteredInItsPack()
        {
            Build();
            var pack = AssetDatabase.LoadAssetAtPath<DreamPack>($"{PackRoot}/{Pack}.asset");

            Assert.That(pack, Is.Not.Null, "the pack is created on first use");
            Assert.That(pack.Cubes.Count, Is.EqualTo(1));
            Assert.That(pack.Cubes[0].Id, Is.EqualTo($"{Pack}.straight"));

            // And the rules engine accepts what came out.
            var registry = new CubeRegistry();
            Assert.That(() => pack.RegisterAll(registry), Throws.Nothing);
            Assert.That(registry.Contains($"{Pack}.straight"), Is.True);
        }

        [Test]
        public void RebuildingChangesNothingOnDisk()
        {
            Build();

            // #47 says "rebuilding changes nothing on disk", which is all three
            // generated files, not only the hard one.
            string definitionPath = $"{CubeFolder}/straight.asset";
            string packPath = $"{PackRoot}/{Pack}.asset";

            byte[] prefab = File.ReadAllBytes(PrefabPath);
            byte[] definition = File.ReadAllBytes(definitionPath);
            byte[] pack = File.ReadAllBytes(packPath);

            for (int i = 0; i < 3; i++)
            {
                CubeBuildResult again = Build();
                Assert.That(again.Ok, Is.True, again.Describe());
                Assert.That(again.PrefabChanged, Is.False, $"rebuild {i + 1} rewrote the prefab");
                Assert.That(again.PackChanged, Is.False, $"rebuild {i + 1} rewrote the pack");
                Assert.That(again.DefinitionChanged, Is.False,
                    $"rebuild {i + 1} rewrote the CubeDefinition");
            }

            Assert.That(File.ReadAllBytes(PrefabPath), Is.EqualTo(prefab), "prefab");
            Assert.That(File.ReadAllBytes(definitionPath), Is.EqualTo(definition), "CubeDefinition");
            Assert.That(File.ReadAllBytes(packPath), Is.EqualTo(pack), "DreamPack");
        }

        [Test]
        public void ButARealChangeIsStillWritten()
        {
            // The risk in writing only on change is a builder that goes blind.
            Build();
            byte[] before = File.ReadAllBytes(PrefabPath);

            File.WriteAllText(SpecPath, Spec(width: 6f));
            AssetDatabase.Refresh();

            CubeBuildResult r = Build();
            Assert.That(r.PrefabChanged, Is.True, "a wider corridor is a different cube");
            Assert.That(File.ReadAllBytes(PrefabPath), Is.Not.EqualTo(before));

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab.transform.Find("Shell/wall_east").localScale.x,
                Is.EqualTo(1f).Within(1e-3f), "walls moved out to suit the 6 m interior");
        }

        [Test]
        public void ABadSpecIsReportedAndBuildsNothing()
        {
            File.WriteAllText(SpecPath, Spec().Replace(@"""cost"": 1", @"""cost"": 99"));
            AssetDatabase.Refresh();

            CubeBuildResult r = Build();

            Assert.That(r.Ok, Is.False);
            Assert.That(r.Problems.Select(p => p.Field), Has.Some.Contains("cost"));
            Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), Is.Null,
                "nothing reached the asset database");
        }
    }
}
