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

            Blames(Validate(), "not CC0 or a bare CC-BY");
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

            Blames(Validate(), "not CC0 or a bare CC-BY");
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

        /// <summary>
        /// Every licence string both gates have to agree about.
        /// </summary>
        static readonly (string Licence, bool Allowed)[] Licences =
        {
            ("CC0 1.0", true), ("CC0", true), ("CC0 1.0 Universal", true),
            ("CC-BY 4.0", true), ("CC BY 4.0", true), ("CC-BY-4.0", true), ("CC BY 3.0", true),

            ("CC-BY-NC 4.0", false), ("CC-BY-ND 4.0", false), ("CC-BY-SA 4.0", false),
            ("CC-BY-NC4.0", false), ("CC-BY-NCSA", false), ("CC-BY-NC_4.0", false),
            ("CC-BY - NC 4.0", false), ("CC-BY-NonCommercial 4.0", false),
            ("CC-BY-ShareAlike", false), ("CC-BY-SA4.0", false), ("CC BY NC 4.0", false),
            ("cc-by-nc", false),
            ("Unity Asset Store Standard EULA", false), ("All rights reserved", false),
        };

        [Test]
        public void TheHookAndTheValidatorReachTheSameVerdict()
        {
            // Comparing the two patterns as strings proved nothing: the script
            // compiles ALLOWED from ALLOWED_PATTERN, so rewriting the compile
            // line regressed the gate while the literal — and the test reading
            // it — stayed correct. Only running the hook shows they agree.
            string script = Application.dataPath + "/../../tools/check-licenses.py";
            Assert.That(File.Exists(script), Is.True, $"no hook at {script}");

            var disagreements = new System.Collections.Generic.List<string>();
            foreach ((string licence, bool _) in Licences)
            {
                string row = $"| hero.fbx | https://example.invalid/x | {licence} |";
                bool validator = CubeValidator.IsRedistributable(row);

                Asset("hero.fbx");
                Ledger(row);
                // Absolute: Folder is Unity-relative ("Assets/…"), and the hook
                // runs from the repository root where the same file is under
                // "Lucid/Assets/…". Handing it the Unity-relative path made it
                // find nothing and reject everything.
                bool? hook = HookAccepts(script,
                    Application.dataPath + "/_Lucid/Packs/licencetest/Cubes/probe/assets/hero.fbx");
                if (hook == null) Assert.Ignore("python3 is not on PATH; the hook cannot be run");

                if (validator != hook)
                    disagreements.Add($"{licence}: validator={validator}, hook={hook}");
            }

            Assert.That(disagreements, Is.Empty,
                "the pre-commit hook and the validator disagree:\n  " +
                string.Join("\n  ", disagreements));
        }

        /// <summary>Runs the hook on one path. Null when python3 is unavailable.</summary>
        static bool? HookAccepts(string script, string assetPath)
        {
            try
            {
                using var process = new System.Diagnostics.Process();
                process.StartInfo = new System.Diagnostics.ProcessStartInfo("python3")
                {
                    Arguments = $"\"{script}\" \"{assetPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Application.dataPath + "/../..",
                };
                process.Start();
                process.StandardOutput.ReadToEnd();
                process.StandardError.ReadToEnd();
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return null;
            }
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
