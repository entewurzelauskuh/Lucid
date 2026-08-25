using System.IO;
using System.Linq;
using Lucid.Editor.Cubes;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Lucid.Tests.EditMode.Cubes
{
    /// <summary>
    /// CLAUDE.md rule 5, as the validator enforces it. The pre-commit hook
    /// enforces the same rule, so these also pin that the two agree — a cube
    /// author should not be able to build clean and then fail at commit.
    /// </summary>
    /// <remarks>
    /// The rule had no test at all until the review of #48, and no committed
    /// cube has an assets/ folder, so it had never run on real input in either
    /// direction. #31 shipped a bypass inside the asset gate itself, which is
    /// why this one is worth pinning.
    /// </remarks>
    public sealed class LicenceRuleTests
    {
        const string Folder = "Assets/_Lucid/Packs/licencetest/Cubes/probe";
        GameObject _cube;

        [SetUp]
        public void SetUp() => Directory.CreateDirectory($"{Folder}/assets");

        [TearDown]
        public void TearDown()
        {
            if (_cube != null) Object.DestroyImmediate(_cube);
            AssetDatabase.DeleteAsset("Assets/_Lucid/Packs/licencetest");
            if (Directory.Exists("Assets/_Lucid/Packs/licencetest"))
                Directory.Delete("Assets/_Lucid/Packs/licencetest", true);
            AssetDatabase.Refresh();
        }

        void Ledger(string body) => File.WriteAllText($"{Folder}/assets/LICENSES.md", body);
        void Asset(string relative) => File.WriteAllText($"{Folder}/assets/{relative}", "x");

        ValidationReport Validate()
        {
            CubeSpec spec = CubeSpecReader.Read(SpecFixtures.Straight).Spec;

            // The rule reads the folder, not the prefab, so a bare object is
            // enough to reach it.
            _cube = new GameObject("probe");
            var shell = new GameObject("Shell");
            shell.transform.SetParent(_cube.transform, false);

            return CubeValidator.Validate(_cube, spec, Folder);
        }

        static void Blames(ValidationReport report, string fragment)
        {
            Assert.That(report.Problems.Where(p => p.Rule == "licences").Select(p => p.Message),
                Has.Some.Contains(fragment), report.Describe());
        }

        [Test]
        public void ACommittedAssetNeedsALedgerLine()
        {
            Asset("wall.png");
            Blames(Validate(), "no assets/LICENSES.md");
        }

        [Test]
        public void ACC0AssetWithALedgerLinePasses()
        {
            Asset("wall.png");
            Ledger("| wall.png | https://ambientcg.com/a/Wall | CC0 |");

            Assert.That(Validate().Problems.Select(p => p.Rule), Has.None.EqualTo("licences"));
        }

        [Test]
        public void ANonRedistributableLicenceIsRejected()
        {
            Asset("hero.fbx");
            Ledger("| hero.fbx | https://assetstore.unity.com/x | Unity Asset Store Standard EULA |");

            Blames(Validate(), "not CC0 or CC-BY");
        }

        [Test]
        public void ASubstringOfAListedNameDoesNotCount()
        {
            // The same bypass #50 fixed in tools/check-licenses.py: an unlisted
            // wall.png must not pass on a line for stonewall.png.
            Asset("wall.png");
            Ledger("| stonewall.png | https://ambientcg.com/a/Stone | CC0 |");

            Blames(Validate(), "no line in assets/LICENSES.md");
        }

        [Test]
        public void AnAssetInASubfolderIsStillChecked()
        {
            // The hook walks assets/ recursively; the validator used to look
            // only at the top level, so the two disagreed.
            Directory.CreateDirectory($"{Folder}/assets/textures");
            Asset("textures/wall.png");
            Ledger("| nothing.png | x | CC0 |");

            Blames(Validate(), "no line in assets/LICENSES.md");
        }

        [Test]
        public void AManifestAssetMayNotBeCommitted()
        {
            // Anything the manifest lists is fetched at build time. A ledger
            // line does not make it committable.
            Asset("statue.fbx");
            Ledger("| statue.fbx | https://example.com/s | CC0 |");
            File.WriteAllText($"{Folder}/assets.manifest.json",
                "{ \"assets\": [ { \"file\": \"statue.fbx\", \"url\": \"https://e.com/s\" } ] }");

            Blames(Validate(), "must not be committed");
        }

        [Test]
        public void TheLedgerItselfNeedsNoLedgerLine()
        {
            Ledger("| nothing | x | CC0 |");
            Assert.That(Validate().Problems.Select(p => p.Rule), Has.None.EqualTo("licences"));
        }
    }
}
