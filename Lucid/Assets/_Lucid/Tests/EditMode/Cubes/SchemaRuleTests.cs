using System.Linq;
using Lucid.Editor.Cubes;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Cubes
{
    /// <summary>
    /// Schema rules that no test reached until the independent review of #46
    /// went through docs/cube-spec.schema.json property by property. Each was
    /// silently unenforced; several would have surfaced in #47 or #48 as a
    /// cube that built wrong rather than a report line.
    /// </summary>
    public sealed class SchemaRuleTests
    {
        static void Rejects(string json, string field)
        {
            CubeSpecResult r = CubeSpecReader.Read(json);
            Assert.That(r.Ok, Is.False, "expected rejection");
            Assert.That(r.Problems.Select(p => p.Field), Has.Some.Contains(field),
                $"expected a problem naming '{field}', got:\n{r.Describe()}");
        }

        // ---- enums the schema declares, previously bare strings -------------

        [Test]
        public void EnumsAreCaseSensitive()
        {
            // Newtonsoft's own StringEnumConverter matches case-insensitively,
            // so "Connector" used to be accepted where the schema allows only
            // "connector".
            Rejects(SpecFixtures.Replacing(@"""category"": ""connector""",
                @"""category"": ""Connector"""), "category");
            Rejects(SpecFixtures.Replacing(@"[""north"", ""south""]",
                @"[""North"", ""south""]"), "connectors");
        }

        [Test]
        public void ColliderMustBeOneOfTheSchemasModes()
        {
            Rejects(SpecFixtures.With("props",
                @"[{ ""name"": ""b"", ""asset"": ""generated:box"", ""position"": [0,0,0],
                     ""collider"": ""sphere"" }]"), "collider");
        }

        [Test]
        public void KillVolumeCauseMustBeOneOfTheSchemasCauses()
        {
            Rejects(SpecFixtures.With("killVolumes",
                @"[{ ""center"": [0,0,0], ""size"": [1,1,1], ""cause"": ""banana"" }]"), "cause");
        }

        [Test]
        public void LightingPresetAndRoleAreEnums()
        {
            Rejects(SpecFixtures.With("lighting", @"{ ""preset"": ""disco"" }"), "preset");
            Rejects(SpecFixtures.With("lighting",
                @"{ ""lights"": [{ ""type"": ""point"", ""position"": [0,1,0], ""role"": ""rim"" }] }"),
                "role");
        }

        [Test]
        public void PreviewCamerasAreEnums()
        {
            Rejects(SpecFixtures.With("preview", @"{ ""cameras"": [""orbit""] }"), "cameras");
        }

        // ---- bounds and patterns --------------------------------------------

        [Test]
        public void TheNameHasAMaximumLength()
        {
            Rejects(SpecFixtures.Replacing(@"""name"": ""Minimal""",
                @"""name"": ""A cube with a name far longer than the schema permits"""), "name");
        }

        [Test]
        public void AChicaneComponentMustBePascalCase()
        {
            string spec = SpecFixtures.Minimal
                .Replace(@"""category"": ""connector""", @"""category"": ""chicane""")
                .TrimEnd().TrimEnd('}').TrimEnd() + @",
  ""chicane"": { ""component"": ""trapdoor"" },
  ""intendedPaths"": [ { ""from"": ""south"", ""to"": ""north"", ""points"": [[0,0,-4],[0,0,4]] } ]
}";
            Rejects(spec, "chicane.component");
        }

        [Test]
        public void AWeakPointNeedsAJamEffectAndSaneHp()
        {
            string with = SpecFixtures.Minimal
                .Replace(@"""category"": ""connector""", @"""category"": ""chicane""")
                .TrimEnd().TrimEnd('}').TrimEnd() + @",
  ""props"": [ { ""name"": ""latch"", ""asset"": ""generated:box"", ""position"": [0,0,0] } ],
  ""chicane"": { ""component"": ""Trapdoor"" },
  ""intendedPaths"": [ { ""from"": ""south"", ""to"": ""north"", ""points"": [[0,0,-4],[0,0,4]] } ],
  ""weakPoint"": { ""prop"": ""latch"", ""hp"": HP }
}";
            // jamEffect is required by the schema; it had no Required.Always.
            Rejects(with.Replace("HP", "60"), "jamEffect");

            string ok = with.Replace(@"""hp"": HP }", @"""hp"": HP, ""jamEffect"": ""lockShut"" }");
            Rejects(ok.Replace("HP", "5000"), "weakPoint.hp");
            Rejects(ok.Replace("HP", "1"), "weakPoint.hp");
        }

        [Test]
        public void ATriggersCooldownAndLabelAreBounded()
        {
            string with = SpecFixtures.Minimal
                .Replace(@"""category"": ""connector""", @"""category"": ""chicane""")
                .TrimEnd().TrimEnd('}').TrimEnd() + @",
  ""chicane"": { ""component"": ""Trapdoor"" },
  ""intendedPaths"": [ { ""from"": ""south"", ""to"": ""north"", ""points"": [[0,0,-4],[0,0,4]] } ],
  ""trigger"": TRIGGER
}";
            Rejects(with.Replace("TRIGGER", @"{ ""kind"": ""dropNow"", ""cooldownMs"": 1 }"),
                "trigger.cooldownMs");
            Rejects(with.Replace("TRIGGER",
                @"{ ""kind"": ""dropNow"", ""label"": ""a label considerably longer than 24"" }"),
                "trigger.label");
        }

        // ---- shapes the schema allows that used to be rejected ---------------

        [Test]
        public void APropMayCarryASkinTag()
        {
            // skinTag had no model field, and MissingMemberHandling.Error turns
            // a missing field into an outright rejection of a valid spec.
            CubeSpecResult r = CubeSpecReader.Read(SpecFixtures.With("props",
                @"[{ ""name"": ""rug"", ""asset"": ""assets/rug.fbx"", ""position"": [0,0,0],
                     ""skinTag"": ""bedroom"" }]"));
            Assert.That(r.Ok, Is.True, r.Describe());
            Assert.That(r.Spec.EffectiveProps[0].SkinTag, Is.EqualTo("bedroom"));
        }

        [Test]
        public void ScaleTakesEitherANumberOrAVector()
        {
            CubeSpecResult uniform = CubeSpecReader.Read(SpecFixtures.With("props",
                @"[{ ""name"": ""b"", ""asset"": ""generated:box"", ""position"": [0,0,0], ""scale"": 2 }]"));
            Assert.That(uniform.Ok, Is.True, uniform.Describe());
            Assert.That(uniform.Spec.EffectiveProps[0].EffectiveScale.IsUniform, Is.True);
            Assert.That(uniform.Spec.EffectiveProps[0].EffectiveScale.X, Is.EqualTo(2f));

            CubeSpecResult vector = CubeSpecReader.Read(SpecFixtures.With("props",
                @"[{ ""name"": ""b"", ""asset"": ""generated:box"", ""position"": [0,0,0], ""scale"": [1,2,3] }]"));
            Assert.That(vector.Ok, Is.True, vector.Describe());
            Assert.That(vector.Spec.EffectiveProps[0].EffectiveScale.Z, Is.EqualTo(3f));

            // exclusiveMinimum: 0.
            Rejects(SpecFixtures.With("props",
                @"[{ ""name"": ""b"", ""asset"": ""generated:box"", ""position"": [0,0,0], ""scale"": 0 }]"),
                "scale");
        }

        [Test]
        public void AbsentPropDefaultsMatchTheSchema()
        {
            CubeSpecResult r = CubeSpecReader.Read(SpecFixtures.With("props",
                @"[{ ""name"": ""b"", ""asset"": ""generated:box"", ""position"": [0,0,0] }]"));
            PropSpec prop = r.Spec.EffectiveProps[0];

            Assert.That(prop.EffectiveScale.X, Is.EqualTo(1f), "scale defaults to 1");
            Assert.That(prop.EffectiveRotation.Y, Is.EqualTo(0f), "rotation defaults to [0,0,0]");
            Assert.That(prop.Collider, Is.EqualTo(ColliderMode.Auto));
            Assert.That(r.Spec.EffectiveKillVolumes, Is.Empty, "killVolumes defaults to []");
        }

        // ---- nothing escapes as an exception ---------------------------------

        [Test]
        public void HostileInputIsReportedNeverThrown()
        {
            // #46's acceptance: a malformed spec produces a message naming the
            // field, not an exception. Each of these used to escape the reader:
            // FormatException, ArgumentException, or NullReferenceException.
            string[] hostile =
            {
                SpecFixtures.With("props",
                    @"[{ ""name"": ""b"", ""asset"": ""g"", ""position"": [""x"", 0, 0] }]"),
                SpecFixtures.With("props",
                    @"[{ ""name"": ""b"", ""asset"": ""g"", ""position"": [null, 0, 0] }]"),
                SpecFixtures.With("props", "[null]"),
                SpecFixtures.With("intendedPaths", "[null]"),
                SpecFixtures.With("lighting", @"{ ""lights"": [null] }"),
                SpecFixtures.With("killVolumes", "[null]"),
                SpecFixtures.With("intendedPaths",
                    @"[{ ""from"": ""south"", ""to"": ""north"", ""points"": [[""a"",""b"",""c""],[0,0,4]] }]"),
            };

            foreach (string json in hostile)
            {
                CubeSpecResult r = null;
                Assert.That(() => r = CubeSpecReader.Read(json), Throws.Nothing,
                    "a spec is untrusted input and must never throw out of the reader");
                Assert.That(r.Ok, Is.False);
                Assert.That(r.Problems, Is.Not.Empty);
            }
        }

        [Test]
        public void AProblemInsideAnArrayNamesWhichElement()
        {
            // "asset: required, but missing" does not say which of a dozen props.
            CubeSpecResult r = CubeSpecReader.Read(SpecFixtures.With("props",
                @"[{ ""name"": ""a"", ""asset"": ""g"", ""position"": [0,0,0] },
                    { ""name"": ""b"", ""position"": [0,0,0] }]"));

            Assert.That(r.Ok, Is.False);
            Assert.That(r.Problems[0].Field, Does.Contain("props[1]"), r.Describe());
            Assert.That(r.Problems[0].Field, Does.Contain("asset"));
        }
    }
}
