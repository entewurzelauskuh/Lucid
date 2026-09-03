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

        static void Blames(ValidationReport report, string fragment, string because = null)
        {
            Assert.That(report.Problems.Where(p => p.Rule == "licences").Select(p => p.Message),
                Has.Some.Contains(fragment), because == null ? report.Describe() : $"{because}: {report.Describe()}");
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

            // False rejects the clause pattern must not produce: SA and ND here
            // begin an ordinary word, not a licence clause.
            ("CC-BY 4.0 - SAmple pack", true), ("CC0 - NDA cleared", true),

            // U+001C is whitespace to Python's \s and not to .NET's, so while
            // the pattern used \s this was rejected by the hook and accepted
            // here — a cube building clean and failing at commit. Neither
            // pattern uses \s now; both read it as a separator like any other
            // non-alphanumeric, and both refuse the clause behind it.
            ("CC-BY\u001CNC 4.0", false),

            // A clause name survives being broken up, and CC 3.0 spells
            // NoDerivatives "NoDerivs" — the spelling most 3.0-era Sketchfab
            // and OpenGameArt assets still carry. Three unbroken literals let
            // every one of these through both gates.
            ("CC BY-NoDerivs 3.0", false), ("CC BY Non-Commercial 4.0", false),
            ("CC BY Share-Alike 4.0", false), ("CC BY Non Commercial 4.0", false),
            ("CC BY No Derivative Works 3.0", false),
            ("Attribution-NonCommercial-ShareAlike 4.0 International", false),

            // A clause at the very start of the cell: the separator class needs
            // a character to consume, and the padding is already trimmed off.
            ("NC CC-BY 4.0", false), ("SA CC0", false), ("ND CC BY 4.0", false),
        };

        /// <summary>
        /// Whole ledger rows, because the licences above all share one shape and
        /// the cell is found by position.
        /// </summary>
        static readonly (string Row, bool Allowed)[] Rows =
        {
            ("| a.png | url | CC0 |", true),
            ("| a.png |  | CC0 |", true),                        // blank source column
            ("| a.png | url | CC-BY-NC 4.0 | CC0 base |", false), // a 4th column must not decide
            ("| a.png | url |", false),                          // too few columns
            ("|---|---|---|", false),                            // a separator row
            ("a.png is CC0, trust me", false),                   // not a row at all

            // A CC token outside the licence cell. Reading the line instead of
            // the cell — the bug f429bab fixed and every comment here cites —
            // survived every assertion in this file until these rows existed.
            // cc0-textures.com is ambientCG, which docs/SPEC.md §17 recommends
            // by name, so this is the realistic row, not the exotic one.
            ("| cc0-wall.png | https://polyhaven.com/x | Unity Asset Store Standard EULA |", false),
            ("| wall.png | https://cc0-textures.com/a/Wall | All rights reserved |", false),
            ("| wall.png | https://cc0-textures.com/a/Wall | CC-BY-NC 4.0 |", false),
        };

        [Test]
        public void TheLicenceIsReadFromItsOwnColumn()
        {
            foreach ((string row, bool allowed) in Rows)
                Assert.That(CubeValidator.IsRedistributable(row), Is.EqualTo(allowed), row);
        }

        [Test]
        public void BothGatesReadTheSameColumnOfTheSameRow()
        {
            string script = HookPath();
            foreach ((string row, bool allowed) in Rows)
            {
                Assert.That(CubeValidator.IsRedistributable(row), Is.EqualTo(allowed),
                    $"validator: {row}");
                bool? hook = JudgeWithHook(script, row);
                if (hook == null) Assert.Ignore("python3 is not on PATH");
                Assert.That(hook, Is.EqualTo(allowed), $"hook: {row}");
            }
        }

        /// <summary>
        /// Writes <paramref name="row"/> as the whole ledger, creates whichever
        /// asset it names, and runs the hook on it.
        /// </summary>
        bool? JudgeWithHook(string script, string row)
        {
            string named = row.Contains("cc0-wall.png") ? "cc0-wall.png"
                : row.Contains("wall.png") ? "wall.png"
                : row.Contains("hero.fbx") ? "hero.fbx" : "a.png";
            Asset(named);
            Ledger(row);
            return HookAccepts(script, AssetPath(named));
        }

        /// <summary>
        /// Row shapes, and rows with extra or misplaced columns, that must not
        /// pass either gate.
        /// </summary>
        static readonly (string Row, bool Allowed)[] ShapeAttacks =
        {
            // A note column before the licence decided the verdict: the T1 hole
            // arriving from the other side.
            ("| hero.fbx | Poly Haven | CC0 pack | CC-BY-NC 4.0 |", false),
            // A pipe in the source column shifted every column right.
            ("| hero.fbx | pack a|CC0 base | CC-BY-NC 4.0 |", false),
            ("| a.png | https://x/?u=1|v=2 | CC0 |", false),
            ("| a.png | url | CC-BY-NC 4.0 | CC0 base |", false),
        };

        [Test]
        public void ARowThatIsNotThreeColumnsIsRefusedByBothGates()
        {
            // Every one of these was accepted by both gates at some point in
            // this branch's history, which is why they are pinned on both sides
            // rather than only in C#: eight Python-side mutations survived while
            // the row matrix was checked against the validator alone.
            string script = HookPath();
            foreach ((string row, bool allowed) in ShapeAttacks)
            {
                Assert.That(CubeValidator.IsRedistributable(row), Is.EqualTo(allowed),
                    $"validator: {row}");

                bool? hook = JudgeWithHook(script, row);
                if (hook == null) Assert.Ignore("python3 is not on PATH");
                Assert.That(hook, Is.EqualTo(allowed), $"hook: {row}");
            }
        }

        [Test]
        public void TheVerdictDoesNotDependOnTheEditorsCulture()
        {
            // IgnoreCase folds case with the current culture. Under tr-TR the
            // uppercase I in CC-BY-NONCOMMERCIAL does not fold to the pattern's
            // i, the clause matched nothing, and the gate opened. Python's re
            // never had this, so it was a hole and a divergence at once.
            var was = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture =
                    new System.Globalization.CultureInfo("tr-TR");
                Assert.That(CubeValidator.IsRedistributable("| a.png | url | CC-BY-NONCOMMERCIAL 4.0 |"),
                    Is.False, "tr-TR");
                Assert.That(CubeValidator.IsRedistributable("| a.png | url | CC-BY-NC 4.0 |"),
                    Is.False, "tr-TR");
                Assert.That(CubeValidator.IsRedistributable("| a.png | url | CC BY 4.0 |"),
                    Is.True, "tr-TR");
            }
            finally { System.Globalization.CultureInfo.CurrentCulture = was; }
        }

        static string HookPath() => Application.dataPath + "/../../tools/check-licenses.py";

        static string AssetPath(string name) =>
            Application.dataPath + "/_Lucid/Packs/licencetest/Cubes/probe/assets/" + name;

        [Test]
        public void TheHookAndTheValidatorReachTheSameVerdict()
        {
            // Comparing the two patterns as strings proved nothing: the script
            // compiles ALLOWED from ALLOWED_PATTERN, so rewriting the compile
            // line regressed the gate while the literal — and the test reading
            // it — stayed correct. Only running the hook shows they agree.
            string script = HookPath();
            Assert.That(File.Exists(script), Is.True, $"no hook at {script}");

            var disagreements = new System.Collections.Generic.List<string>();
            foreach ((string licence, bool allowed) in Licences)
            {
                string row = $"| hero.fbx | https://example.invalid/x | {licence} |";
                bool validator = CubeValidator.IsRedistributable(row);
                // Agreement is not correctness: both gates accepting CC-BY-NC
                // passed this test for as long as the verdict was not asserted,
                // and one entry here had already gone stale that way.
                Assert.That(validator, Is.EqualTo(allowed), $"verdict for '{licence}'");

                Asset("hero.fbx");
                Ledger(row);
                // Absolute: Folder is Unity-relative ("Assets/…"), and the hook
                // runs from the repository root where the same file is under
                // "Lucid/Assets/…". Handing it the Unity-relative path made it
                // find nothing and reject everything.
                bool? hook = HookAccepts(script, AssetPath("hero.fbx"));
                if (hook == null) Assert.Ignore("python3 is not on PATH; the hook cannot be run");

                if (validator != hook)
                    disagreements.Add($"{licence}: validator={validator}, hook={hook}");
            }

            Assert.That(disagreements, Is.Empty,
                "the pre-commit hook and the validator disagree:\n  " +
                string.Join("\n  ", disagreements));
        }

        /// <summary>
        /// Runs the hook on one path. Null when python3 is unavailable.
        /// </summary>
        /// <remarks>
        /// Throws rather than returning false when the script itself fails. An
        /// unhandled Python exception exits 1, exactly like a licence
        /// violation, so without this a broken hook was reported as
        /// disagreeing with the validator — a true failure with a false
        /// diagnosis, which is the more expensive kind.
        /// </remarks>
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
                string errors = process.StandardError.ReadToEnd();
                process.WaitForExit();

                // A crash has to be told apart from a refusal, or a broken hook
                // reads as a verdict. "Traceback" alone is not enough: a
                // SyntaxError prints no traceback and still exits 1, which this
                // test then reported as a divergence from the validator.
                // A refusal is the only exit-1 that carries the banner.
                if (process.ExitCode == 0)
                    return true;
                if (process.ExitCode == 1 && errors.Contains("Asset rule violations"))
                    return false;
                throw new AssertionException(
                    $"tools/check-licenses.py failed rather than judging {assetPath} " +
                    $"(exit {process.ExitCode}):\n{errors}");
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

        /// <summary>
        /// Manifest shapes both gates have to read the same way. The hook and
        /// the validator parse the file separately, so a shape one accepts and
        /// the other cannot is a cube that builds clean and will not commit.
        /// </summary>
        static readonly (string Json, string Why)[] Manifests =
        {
            ("{ \"assets\": [ { \"file\": \"statue.fbx\" } ] }", "the documented shape"),
            ("[ { \"file\": \"statue.fbx\" } ]", "a top-level array"),
            ("{ \"assets\": [ { \"file\": null, \"path\": \"statue.fbx\" } ] }", "a JSON null before the key that has it"),
            ("{ \"assets\": [ { \"file\": \"\", \"path\": \"statue.fbx\" } ] }", "an empty string before the key that has it"),
        };

        [Test]
        public void BothGatesReadTheSameManifestShapes()
        {
            string script = HookPath();
            foreach ((string json, string why) in Manifests)
            {
                Asset("statue.fbx");
                Ledger("| statue.fbx | https://example.com/s | CC0 |");
                File.WriteAllText($"{Folder}/assets.manifest.json", json);

                Blames(Validate(), "must not be committed", why);
                bool? hook = HookAccepts(script, AssetPath("statue.fbx"));
                if (hook == null) Assert.Ignore("python3 is not on PATH");
                Assert.That(hook, Is.False, $"hook: {why}");

                File.Delete($"{Folder}/assets.manifest.json");
            }
        }

        [Test]
        public void AMalformedManifestStopsNeitherGate()
        {
            // A manifest that is not a list of entries means nothing is known to
            // be fetched; the ledger rule still applies. It must not crash
            // either gate — every shape but the array crashed the hook on every
            // commit until now.
            string script = HookPath();
            foreach (string json in new[] { "null", "\"nothing yet\"", "42", "{ \"assets\": 5 }", "{ }" })
            {
                Asset("statue.fbx");
                Ledger("| statue.fbx | https://example.com/s | CC0 |");
                File.WriteAllText($"{Folder}/assets.manifest.json", json);

                Assert.That(Validate().Problems.Select(p => p.Rule), Has.None.EqualTo("licences"), json);
                bool? hook = HookAccepts(script, AssetPath("statue.fbx"));
                if (hook == null) Assert.Ignore("python3 is not on PATH");
                Assert.That(hook, Is.True, $"hook: {json}");

                File.Delete($"{Folder}/assets.manifest.json");
            }
        }

        [Test]
        public void AStrayMetaIsNotAnAssetToEitherGate()
        {
            // Unity writes a .meta for everything and leaves them behind when
            // the asset goes. The validator skips them; the hook used to judge
            // the asset named inside the filename, so a wall.png.meta with no
            // wall.png blocked the commit and passed the build.
            Asset("wall.png.meta");
            Ledger("| nothing | x | CC0 |");

            Assert.That(Validate().Problems.Select(p => p.Rule), Has.None.EqualTo("licences"));
            bool? hook = HookAccepts(HookPath(), AssetPath("wall.png.meta"));
            if (hook == null) Assert.Ignore("python3 is not on PATH");
            Assert.That(hook, Is.True);
        }

        [Test]
        public void TheLedgerItselfNeedsNoLedgerLine()
        {
            Ledger("| nothing | x | CC0 |");
            Assert.That(Validate().Problems.Select(p => p.Rule), Has.None.EqualTo("licences"));
        }
    }
}
