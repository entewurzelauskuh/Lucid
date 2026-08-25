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
    /// The five cubes M0 ships, as committed. These check the artefacts in the
    /// repository rather than rebuilding them, so a cube whose committed prefab
    /// drifted from its spec fails here.
    /// </summary>
    public sealed class CorePackTests
    {
        const string Pack = "Assets/_Lucid/Packs/core";
        static readonly string[] Cubes = { "straight", "corner", "tee", "cross", "start" };

        static string Folder(string cube) => $"{Pack}/Cubes/{cube}";

        [Test]
        public void EveryCubeHasASpecAPrefabAndADefinition()
        {
            foreach (string cube in Cubes)
            {
                Assert.That(File.Exists($"{Folder(cube)}/cube.spec.json"), Is.True, $"{cube} spec");
                Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>($"{Folder(cube)}/{cube}.prefab"),
                    Is.Not.Null, $"{cube} prefab");
                Assert.That(AssetDatabase.LoadAssetAtPath<CubeDefinition>($"{Folder(cube)}/{cube}.asset"),
                    Is.Not.Null, $"{cube} definition");
            }
        }

        [Test]
        public void EveryCommittedCubeValidates()
        {
            foreach (string cube in Cubes)
            {
                CubeSpecResult read = CubeSpecReader.ReadFile($"{Folder(cube)}/cube.spec.json");
                Assert.That(read.Ok, Is.True, $"{cube}: {read.Describe()}");

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{Folder(cube)}/{cube}.prefab");
                ValidationReport report = CubeValidator.Validate(prefab, read.Spec, Folder(cube));

                Assert.That(report.Ok, Is.True, report.Describe());
            }
        }

        [Test]
        public void EveryCubeHasItsThreePreviewsAndAReport()
        {
            foreach (string cube in Cubes)
            {
                foreach (string angle in new[] { "iso", "entrance", "top" })
                {
                    string path = $"{Folder(cube)}/Previews/{angle}.png";
                    Assert.That(File.Exists(path), Is.True, path);

                    // A render that failed silently writes a tiny or empty file.
                    Assert.That(new FileInfo(path).Length, Is.GreaterThan(2000), path);
                }

                Assert.That(File.Exists($"{Folder(cube)}/Previews/report.json"), Is.True, cube);
            }
        }

        [Test]
        public void ThePreviewsShowTheInsideOfTheCube()
        {
            // The iso and top views cut away, or they are a picture of a closed
            // box and tell a reviewer nothing. Checked by pixel, because that is
            // the failure this went through: the cut-away silently did nothing
            // while another copy of the cube stood in the same place.
            foreach (string cube in Cubes)
            {
                var top = new Texture2D(2, 2);
                try
                {
                    top.LoadImage(File.ReadAllBytes($"{Folder(cube)}/Previews/top.png"));
                    Color centre = top.GetPixel(top.width / 2, top.height / 2);
                    Assert.That(centre.r, Is.LessThan(0.3f),
                        $"{cube}: the middle of the plan is solid, so the ceiling was not cut away");
                }
                finally
                {
                    Object.DestroyImmediate(top);
                }
            }
        }

        [Test]
        public void EveryCubeIsRegisteredInThePack()
        {
            var pack = AssetDatabase.LoadAssetAtPath<DreamPack>($"{Pack}/core.asset");
            Assert.That(pack, Is.Not.Null);

            Assert.That(pack.Cubes.Select(c => c.Id), Is.EquivalentTo(
                new[] { "core.straight", "core.corner", "core.tee", "core.cross", "core.start" }));

            // Registration order is the wire's type index order
            // (docs/NETCODE.md §4, §5), so it is sorted rather than incidental.
            string[] ids = pack.Cubes.Select(c => c.Id).ToArray();
            string[] sorted = ids.OrderBy(id => id, System.StringComparer.Ordinal).ToArray();
            Assert.That(ids, Is.EqualTo(sorted), "the pack is not in id order");
        }

        [Test]
        public void ThePackIsAcceptedByTheRulesEngine()
        {
            // The strongest end-to-end check: content built from specs is
            // something Lucid.Core will actually run a round with.
            var pack = AssetDatabase.LoadAssetAtPath<DreamPack>($"{Pack}/core.asset");
            var registry = new CubeRegistry();

            Assert.That(() => pack.RegisterAll(registry), Throws.Nothing);
            Assert.That(registry.All.Count, Is.EqualTo(5));

            // And a round can be started on the start cube.
            Assert.That(() => Lattice.New(registry, "core.start", Rotation.R0), Throws.Nothing);
        }

        [Test]
        public void TheCubeSetCoversTheShapesM0Needs()
        {
            var reg = new CubeRegistry();
            AssetDatabase.LoadAssetAtPath<DreamPack>($"{Pack}/core.asset").RegisterAll(reg);

            Assert.That(reg.Get("core.straight").Connectors,
                Is.EqualTo(FaceMask.North | FaceMask.South), "a run that does not turn");
            Assert.That(reg.Get("core.corner").Connectors,
                Is.EqualTo(FaceMask.South | FaceMask.East), "a turn");
            Assert.That(Faces.Count(reg.Get("core.tee").Connectors), Is.EqualTo(3), "a choice");
            Assert.That(Faces.Count(reg.Get("core.cross").Connectors), Is.EqualTo(4), "a crossroads");

            CubeType start = reg.Get("core.start");
            Assert.That(start.Category, Is.EqualTo(CubeCategory.Start));
            Assert.That(Faces.Count(start.Connectors), Is.EqualTo(1), "one way out of the bedroom");
            Assert.That(start.Cost, Is.Zero);
        }
    }
}
