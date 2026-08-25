using System.Linq;
using Lucid.Editor.Cubes;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Cubes
{
    /// <summary>
    /// The cross-field rules docs/CUBE-SPEC.md §2 lists, and the schema's
    /// numeric bounds. Each is the reason a whole class of broken cube cannot
    /// reach the asset database.
    /// </summary>
    public sealed class CubeSpecChecksTests
    {
        static void Rejects(string json, string field)
        {
            CubeSpecResult r = CubeSpecReader.Read(json);
            Assert.That(r.Ok, Is.False, "expected rejection");
            Assert.That(r.Problems.Select(p => p.Field), Has.Some.Contains(field),
                $"expected a problem naming '{field}', got:\n{r.Describe()}");
        }

        static void Accepts(string json)
        {
            CubeSpecResult r = CubeSpecReader.Read(json);
            Assert.That(r.Ok, Is.True, r.Describe());
        }

        // ---- at least two connectors unless the category is start ----------

        [Test]
        public void AOneConnectorCubeIsRejected()
        {
            // Two connectors is what stops a placement sealing the dream
            // (docs/SPEC.md §7).
            Rejects(SpecFixtures.Replacing(@"[""north"", ""south""]", @"[""north""]"), "connectors");
        }

        [Test]
        public void TheStartCubeHasExactlyOneConnectorAndCostsNothing()
        {
            string start = SpecFixtures.Minimal
                .Replace(@"""category"": ""connector""", @"""category"": ""start""")
                .Replace(@"""cost"": 1", @"""cost"": 0")
                .Replace(@"[""north"", ""south""]", @"[""north""]");
            Accepts(start);

            Rejects(start.Replace(@"[""north""]", @"[""north"", ""south""]"), "connectors");
            Rejects(start.Replace(@"""cost"": 0", @"""cost"": 2"), "cost");
        }

        // ---- climbable needs up ---------------------------------------------

        [Test]
        public void ClimbableWithoutAnUpConnectorIsRejected()
        {
            Rejects(SpecFixtures.With("climbable", "true"), "climbable");

            Accepts(SpecFixtures
                .Replacing(@"[""north"", ""south""]", @"[""north"", ""up""]")
                .TrimEnd().TrimEnd('}').TrimEnd() + @",
  ""climbable"": true
}");
        }

        // ---- chicane and mob cubes need a chicane block and paths ------------

        [Test]
        public void AChicaneCubeNeedsAChicaneBlockAndIntendedPaths()
        {
            string chicane = SpecFixtures.Replacing(
                @"""category"": ""connector""", @"""category"": ""chicane""");
            CubeSpecResult r = CubeSpecReader.Read(chicane);

            Assert.That(r.Ok, Is.False);
            Assert.That(r.Problems.Select(p => p.Field), Has.Some.Contains("chicane"));
            Assert.That(r.Problems.Select(p => p.Field), Has.Some.Contains("intendedPaths"));
        }

        [Test]
        public void AMobCubeHasTheSameRequirement()
        {
            Rejects(SpecFixtures.Replacing(@"""category"": ""connector""", @"""category"": ""mob"""),
                "chicane");
        }

        [Test]
        public void AVerticalCubeNeedsIntendedPaths()
        {
            Rejects(SpecFixtures.Replacing(@"""category"": ""connector""", @"""category"": ""vertical"""),
                "intendedPaths");
        }

        // ---- a trigger or weak point needs a chicane -------------------------

        [Test]
        public void ATriggerWithoutAChicaneIsRejected()
        {
            Rejects(SpecFixtures.With("trigger", @"{ ""kind"": ""dropNow"" }"), "trigger");
        }

        [Test]
        public void AWeakPointWithoutAChicaneIsRejected()
        {
            // The prop has to exist, or this passes on "no prop named 'latch'"
            // rather than the rule it names: Has.Some.Contains("weakPoint")
            // also matches "weakPoint.prop".
            string spec = SpecFixtures.With("props",
                @"[{ ""name"": ""latch"", ""asset"": ""generated:box"", ""position"": [0,0,0] }]")
                .TrimEnd().TrimEnd('}').TrimEnd() + ",\n  \"weakPoint\": { \"prop\": \"latch\", \"hp\": 60, \"jamEffect\": \"lockShut\" }\n}";

            CubeSpecResult r = CubeSpecReader.Read(spec);
            Assert.That(r.Ok, Is.False);
            Assert.That(r.Problems.Select(p => p.Field), Has.Some.EqualTo("weakPoint"), r.Describe());
        }

        // ---- names, references and ranges ------------------------------------

        [Test]
        public void SpecVersionMustBeCurrent()
        {
            Rejects(SpecFixtures.Replacing(@"""specVersion"": 1", @"""specVersion"": 2"), "specVersion");
        }

        [Test]
        public void TheIdMustMatchThePack()
        {
            Rejects(SpecFixtures.Replacing(@"""id"": ""core.minimal""", @"""id"": ""other.minimal"""), "id");
        }

        [Test]
        public void TheIdMustMatchTheSchemasPattern()
        {
            // Separate from the pack-prefix rule above, which would otherwise
            // catch every case and leave the pattern itself untested. These all
            // keep the 'core.' prefix intact.
            Rejects(SpecFixtures.Replacing(@"""id"": ""core.minimal""", @"""id"": ""core.Minimal"""), "id");
            Rejects(SpecFixtures.Replacing(@"""id"": ""core.minimal""", @"""id"": ""core.min-imal"""), "id");

            // The schema allows a digit to open either segment; the first
            // version of this regex did not.
            Accepts(SpecFixtures.Replacing(@"""id"": ""core.minimal""", @"""id"": ""core.9lives"""));
        }

        [Test]
        public void ThePackMustMatchTheSchemasPattern()
        {
            Rejects(SpecFixtures.Minimal
                .Replace(@"""pack"": ""core""", @"""pack"": ""my_pack""")
                .Replace(@"""id"": ""core.minimal""", @"""id"": ""my_pack.minimal"""), "pack");
        }

        [Test]
        public void ADuplicateConnectorIsRejected()
        {
            Rejects(SpecFixtures.Replacing(@"[""north"", ""south""]", @"[""north"", ""north""]"),
                "connectors");
        }

        [Test]
        public void CostIsBounded()
        {
            Rejects(SpecFixtures.Replacing(@"""cost"": 1", @"""cost"": 21"), "cost");
            Rejects(SpecFixtures.Replacing(@"""cost"": 1", @"""cost"": -1"), "cost");
        }

        [Test]
        public void ShellDimensionsAreBounded()
        {
            Rejects(SpecFixtures.Replacing(
                @"""ceiling"": ""ceiling"" } }",
                @"""ceiling"": ""ceiling"" }, ""thickness"": 2.0 }"), "shell.thickness");

            Rejects(SpecFixtures.Replacing(
                @"""ceiling"": ""ceiling"" } }",
                @"""ceiling"": ""ceiling"" }, ""interior"": { ""width"": 1.0 } }"), "shell.interior.width");
        }

        [Test]
        public void AnInteriorThatLeavesNoWallOrNoCeilingIsRejected()
        {
            // The schema permits width and height up to 8, which describes a
            // cube with no walls and no ceiling. Both used to build cleanly and
            // report success: an empty shell with four floating door frames.
            Rejects(SpecFixtures.Replacing(@"""ceiling"": ""ceiling"" } }", @"""ceiling"": ""ceiling"" }, ""interior"": { ""width"": 8 } }"), "shell.interior.width");
            Rejects(SpecFixtures.Replacing(@"""ceiling"": ""ceiling"" } }", @"""ceiling"": ""ceiling"" }, ""interior"": { ""height"": 8 } }"), "shell.interior.height");

            // And the largest interior that still leaves both is accepted.
            Accepts(SpecFixtures.Replacing(@"""ceiling"": ""ceiling"" } }", @"""ceiling"": ""ceiling"" }, ""interior"": { ""width"": 7.4, ""height"": 7.4 } }"));
        }

        [Test]
        public void PropNamesMustBeUniqueAndWellFormed()
        {
            string twice = SpecFixtures.With("props", @"[
                { ""name"": ""box"", ""asset"": ""generated:box"", ""position"": [0,0,0] },
                { ""name"": ""box"", ""asset"": ""generated:box"", ""position"": [1,0,0] }]");
            Rejects(twice, "props[1].name");

            string shouty = SpecFixtures.With("props",
                @"[{ ""name"": ""Box"", ""asset"": ""generated:box"", ""position"": [0,0,0] }]");
            Rejects(shouty, "props[0].name");
        }

        [Test]
        public void EverythingPointingAtAPropMustFindOne()
        {
            // chicane.actors, weakPoint.prop and nav.exclude all name props by
            // string; a typo would otherwise build a cube whose trap does
            // nothing and give no sign of it.
            const string spec = @"{
  ""specVersion"": 1,
  ""id"": ""core.typo"",
  ""name"": ""Typo"",
  ""pack"": ""core"",
  ""category"": ""chicane"",
  ""cost"": 3,
  ""connectors"": [""north"", ""south""],
  ""shell"": { ""materials"": { ""wall"": ""wall"", ""floor"": ""floor"", ""ceiling"": ""ceiling"" } },
  ""props"": [ { ""name"": ""panel"", ""asset"": ""generated:box"", ""position"": [0, 0, 0] } ],
  ""chicane"": { ""component"": ""Trapdoor"", ""actors"": [""pannel""] },
  ""weakPoint"": { ""prop"": ""ltach"", ""hp"": 60, ""jamEffect"": ""lockShut"" },
  ""nav"": { ""exclude"": [""pnael""] },
  ""intendedPaths"": [ { ""from"": ""south"", ""to"": ""north"", ""points"": [[0,0,-4],[0,0,4]] } ]
}";
            CubeSpecResult r = CubeSpecReader.Read(spec);

            Assert.That(r.Ok, Is.False);
            Assert.That(r.Problems.Select(p => p.Field), Has.Some.Contains("chicane.actors"));
            Assert.That(r.Problems.Select(p => p.Field), Has.Some.Contains("weakPoint.prop"));
            Assert.That(r.Problems.Select(p => p.Field), Has.Some.Contains("nav.exclude"));
        }

        [Test]
        public void AnIntendedPathMustStartAndEndAtRealDoorways()
        {
            string spec = SpecFixtures.With("intendedPaths",
                @"[{ ""from"": ""south"", ""to"": ""east"", ""points"": [[0,0,-4],[4,0,0]] }]");
            Rejects(spec, "intendedPaths[0].to");
        }

        [Test]
        public void APathNeedsTwoPoints()
        {
            Rejects(SpecFixtures.With("intendedPaths",
                @"[{ ""from"": ""south"", ""to"": ""north"", ""points"": [[0,0,-4]] }]"),
                "intendedPaths[0].points");
        }

        [Test]
        public void LightingIsBounded()
        {
            string five = SpecFixtures.With("lighting", @"{ ""lights"": [
                { ""type"": ""point"", ""position"": [0,1,0] },
                { ""type"": ""point"", ""position"": [0,2,0] },
                { ""type"": ""point"", ""position"": [0,3,0] },
                { ""type"": ""point"", ""position"": [0,4,0] },
                { ""type"": ""point"", ""position"": [0,5,0] }] }");
            Rejects(five, "lighting.lights");

            Rejects(SpecFixtures.With("lighting",
                @"{ ""lights"": [{ ""type"": ""laser"", ""position"": [0,1,0] }] }"),
                "lighting.lights[0].type");

            Rejects(SpecFixtures.With("lighting",
                @"{ ""lights"": [{ ""type"": ""point"", ""position"": [0,1,0], ""color"": ""puce"" }] }"),
                "lighting.lights[0].color");
        }

        [Test]
        public void InteriorHeightIsBounded()
        {
            Rejects(SpecFixtures.Replacing(
                @"""ceiling"": ""ceiling"" } }",
                @"""ceiling"": ""ceiling"" }, ""interior"": { ""height"": 1.0 } }"),
                "shell.interior.height");
        }

        [Test]
        public void AgentRadiusIsBounded()
        {
            Rejects(SpecFixtures.With("nav", @"{ ""agentRadius"": 5.0 }"), "nav.agentRadius");
            Rejects(SpecFixtures.With("nav", @"{ ""agentRadius"": 0.05 }"), "nav.agentRadius");
        }

        [Test]
        public void ALightsIntensityCannotBeNegative()
        {
            Rejects(SpecFixtures.With("lighting",
                @"{ ""lights"": [{ ""type"": ""point"", ""position"": [0,1,0], ""intensity"": -1 }] }"),
                "lighting.lights[0].intensity");
        }

        [Test]
        public void APathMustStartAtARealDoorwayToo()
        {
            // The 'to' direction was covered; 'from' was not, so a mutant that
            // dropped the 'from' check survived.
            Rejects(SpecFixtures.With("intendedPaths",
                @"[{ ""from"": ""east"", ""to"": ""north"", ""points"": [[4,0,0],[0,0,4]] }]"),
                "intendedPaths[0].from");
        }

        [Test]
        public void AVec3MustHaveThreeNumbers()
        {
            Rejects(SpecFixtures.With("intendedPaths",
                @"[{ ""from"": ""south"", ""to"": ""north"", ""points"": [[0,0],[0,0,4]] }]"),
                "intendedPaths");
        }

        [Test]
        public void EveryProblemIsReportedNotJustTheFirst()
        {
            // The loop is read report, fix, rebuild (docs/CUBE-SPEC.md §5); one
            // problem per run makes that loop needlessly long.
            string bad = SpecFixtures.Minimal
                .Replace(@"""cost"": 1", @"""cost"": 99")
                .Replace(@"[""north"", ""south""]", @"[""north""]");

            CubeSpecResult r = CubeSpecReader.Read(bad);
            Assert.That(r.Problems.Count, Is.GreaterThanOrEqualTo(2), r.Describe());
        }
    }
}
