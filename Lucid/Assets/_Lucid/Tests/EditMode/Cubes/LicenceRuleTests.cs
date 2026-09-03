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

        [TestCase("CC-BY-NC 4.0", TestName = "NonCommercial")]
        [TestCase("CC-BY-ND 4.0", TestName = "NoDerivatives")]
        [TestCase("CC-BY-SA 4.0", TestName = "ShareAlike")]
        [TestCase("CC BY-SA 4.0", TestName = "ShareAlike spaced")]
        [TestCase("CC-BY-NC-SA 4.0", TestName = "NonCommercial ShareAlike")]
        public void ACreativeCommonsLicenceWithExtraClausesIsRejected(string licence)
        {
            // These are the licences rule 5 and docs/SPEC.md §18 exist to keep
            // out of a public MIT repository, and the gate waved every one of
            // them through: a word boundary sits between the Y and the hyphen,
            // so the old pattern matched "CC-BY-NC" happily. The only rejection
            // test was the Asset Store EULA, which it did catch, so nothing
            // noticed.
            Asset("hero.fbx");
            Ledger($"| hero.fbx | https://example.invalid/x | {licence} |");

            Blames(Validate(), "not CC0 or CC-BY");
        }

        [TestCase("CC0 1.0", TestName = "CC0")]
        [TestCase("CC-BY 4.0", TestName = "CC-BY hyphenated")]
        [TestCase("CC BY 4.0", TestName = "CC BY spaced, the canonical spelling")]
        public void ARedistributableLicenceIsAccepted(string licence)
        {
            // "CC BY 4.0" is how Creative Commons writes it, and the old pattern
            // refused it — an author copying the name from the source was told
            // their correct entry was wrong.
            Asset("hero.fbx");
            Ledger($"| hero.fbx | https://example.invalid/x | {licence} |");

            ValidationReport report = Validate();
            Assert.That(report.Problems.Where(p => p.Rule == "licences").Select(p => p.Message),
                Is.Empty, $"'{licence}' should be allowed — {report.Describe()}");
        }

        [Test]
        public void TheHookAndTheValidatorUseTheSamePattern()
        {
            // Both gates carry the pattern so a cube cannot build clean here and
            // then fail at commit. They were kept in step by hand, which meant
            // they agreed on the same bug for as long as it existed.
            string script = File.ReadAllText(
                Application.dataPath + "/../../tools/check-licenses.py");

            var match = System.Text.RegularExpressions.Regex.Match(
                script, "ALLOWED_PATTERN = r\"(?<pattern>.*)\"");

            Assert.That(match.Success, Is.True,
                "could not find ALLOWED_PATTERN in tools/check-licenses.py");
            Assert.That(match.Groups["pattern"].Value, Is.EqualTo(CubeValidator.AllowedLicence),
                "the pre-commit hook and the validator have drifted apart");
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
