using System.Linq;
using Lucid.Core;
using Newtonsoft.Json.Linq;
using Lucid.Editor.Cubes;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Cubes
{
    public sealed class CubeSpecReaderTests
    {
        static CubeSpecResult Read(string json) => CubeSpecReader.Read(json);

        static void AssertRejects(string json, string expectedField)
        {
            CubeSpecResult r = Read(json);
            Assert.That(r.Ok, Is.False, "expected this spec to be rejected");
            Assert.That(r.Problems.Select(p => p.Field), Has.Some.Contains(expectedField),
                $"expected a problem naming '{expectedField}', got:\n{r.Describe()}");
        }

        // ---- The worked examples --------------------------------------------

        [Test]
        public void TheSimplestConnectorFromTheGuideParses()
        {
            CubeSpecResult r = Read(SpecFixtures.Straight);
            Assert.That(r.Ok, Is.True, r.Describe());

            Assert.That(r.Spec.Id, Is.EqualTo("core.straight"));
            Assert.That(r.Spec.Category, Is.EqualTo(SpecCategory.Connector));
            Assert.That(r.Spec.Connectors, Is.EqualTo(new[] { SpecFace.North, SpecFace.South }));
            Assert.That(r.Spec.Shell.DoorFrame, Is.EqualTo(DoorFrameStyle.Plain));
            Assert.That(r.Spec.Shell.Interior.Width, Is.EqualTo(4f));
            Assert.That(r.Spec.IntendedPaths[0].Points.Length, Is.EqualTo(2));
        }

        [Test]
        public void TheFullChicaneFromTheGuideParses()
        {
            // Exercises every optional section at once: props, chicane with raw
            // params, weak point, trigger, kill volumes, two paths, nav and
            // lighting.
            CubeSpecResult r = Read(SpecFixtures.TrapdoorPit);
            Assert.That(r.Ok, Is.True, r.Describe());

            Assert.That(r.Spec.EffectiveProps.Length, Is.EqualTo(6));
            Assert.That(r.Spec.Chicane.Component, Is.EqualTo("Trapdoor"));
            Assert.That(r.Spec.Chicane.Params["openDelayMs"].ToObject<int>(), Is.EqualTo(250));
            Assert.That(r.Spec.Chicane.Actors, Is.EqualTo(new[] { "panel" }));
            Assert.That(r.Spec.WeakPoint.Hp, Is.EqualTo(60));
            Assert.That(r.Spec.Trigger.CooldownMs, Is.EqualTo(6_000));
            Assert.That(r.Spec.KillVolumes.Length, Is.EqualTo(1));
            Assert.That(r.Spec.IntendedPaths.Length, Is.EqualTo(2));
            Assert.That(r.Spec.Nav.Exclude, Is.EqualTo(new[] { "panel" }));
            Assert.That(r.Spec.Lighting.Lights[0].Intensity, Is.EqualTo(8f));
        }

        [Test]
        public void PropTransformsSurviveTheRoundTrip()
        {
            CubeSpecResult r = Read(SpecFixtures.TrapdoorPit);
            PropSpec latch = r.Spec.EffectiveProps.Single(p => p.Name == "latch");

            Assert.That(latch.Position.X, Is.EqualTo(3.6f).Within(0.0001f));
            Assert.That(latch.Position.Y, Is.EqualTo(1.2f).Within(0.0001f));
            Assert.That(latch.Rotation.Value.Y, Is.EqualTo(-90f).Within(0.0001f));
            Assert.That(latch.Collider, Is.EqualTo(ColliderMode.Box));
            Assert.That(latch.IsStatic, Is.True, "props are static unless the spec says otherwise");
        }

        // ---- Defaults --------------------------------------------------------

        [Test]
        public void OmittedFieldsTakeTheSchemasDefaults()
        {
            CubeSpecResult r = Read(SpecFixtures.Minimal);
            Assert.That(r.Ok, Is.True, r.Describe());

            Assert.That(r.Spec.Shell.DoorFrame, Is.EqualTo(DoorFrameStyle.Plain));
            Assert.That(r.Spec.Shell.Thickness, Is.EqualTo(0.3f));
            Assert.That(r.Spec.Shell.OpenFloor, Is.False);
            Assert.That(r.Spec.Climbable, Is.False);
            Assert.That(r.Spec.EffectiveSkins, Is.EqualTo(new[] { "*" }));
            Assert.That(r.Spec.EffectiveProps, Is.Empty);
            Assert.That(new PreviewSpec().EffectiveCameras, Is.EqualTo(
                new[] { PreviewCamera.Iso, PreviewCamera.Entrance, PreviewCamera.Top }));
        }

        // ---- Malformed input names the field ---------------------------------

        [Test]
        public void AMissingRequiredFieldNamesIt()
        {
            string json = SpecFixtures.Minimal.Replace(@"""cost"": 1,", "");
            CubeSpecResult r = Read(json);

            Assert.That(r.Ok, Is.False);
            Assert.That(r.Problems[0].ToString(), Does.Contain("cost"));
            Assert.That(r.Problems[0].Message, Does.Contain("required"));
        }

        [Test]
        public void AFieldTheSchemaDoesNotDefineIsRejected()
        {
            // "additionalProperties": false. A typo in an optional field would
            // otherwise be silently ignored and the cube built wrong.
            AssertRejects(SpecFixtures.With("colour", @"""red"""), "colour");
        }

        [Test]
        public void MalformedJsonIsReportedNotThrown()
        {
            CubeSpecResult r = Read("{ this is not json");
            Assert.That(r.Ok, Is.False);
            Assert.That(r.Problems, Is.Not.Empty);
        }

        [Test]
        public void AnUnknownEnumValueIsRejected()
        {
            AssertRejects(SpecFixtures.Replacing(@"""category"": ""connector""",
                @"""category"": ""corridor"""), "category");
            AssertRejects(SpecFixtures.Replacing(@"[""north"", ""south""]",
                @"[""north"", ""sideways""]"), "connectors");
        }

        [Test]
        public void AnEmptyFileIsReported()
        {
            Assert.That(Read("null").Ok, Is.False);
        }

        // ---- Mapping into Core ------------------------------------------------

        [Test]
        public void TheSpecMapsOntoTheRulesEnginesView()
        {
            CubeSpec spec = Read(SpecFixtures.Straight).Spec;
            CubeType type = CubeSpecMapping.ToCubeType(spec);

            Assert.That(type.Id, Is.EqualTo("core.straight"));
            Assert.That(type.Category, Is.EqualTo(CubeCategory.Connector));
            Assert.That(type.Connectors, Is.EqualTo(FaceMask.North | FaceMask.South));
            Assert.That(type.Cost, Is.EqualTo(1));
            Assert.That(type.Climbable, Is.False);
        }

        [Test]
        public void EveryFaceAndCategoryMapsToItsCoreCounterpart()
        {
            // The two enums are mapped explicitly rather than by name, because
            // Core's Face order feeds the derived-state hash and must not be
            // coupled to a file format.
            Assert.That(CubeSpecMapping.ToFace(SpecFace.North), Is.EqualTo(Face.North));
            Assert.That(CubeSpecMapping.ToFace(SpecFace.East), Is.EqualTo(Face.East));
            Assert.That(CubeSpecMapping.ToFace(SpecFace.South), Is.EqualTo(Face.South));
            Assert.That(CubeSpecMapping.ToFace(SpecFace.West), Is.EqualTo(Face.West));
            Assert.That(CubeSpecMapping.ToFace(SpecFace.Up), Is.EqualTo(Face.Up));
            Assert.That(CubeSpecMapping.ToFace(SpecFace.Down), Is.EqualTo(Face.Down));

            Assert.That(CubeSpecMapping.ToCategory(SpecCategory.Start), Is.EqualTo(CubeCategory.Start));
            Assert.That(CubeSpecMapping.ToCategory(SpecCategory.Chicane), Is.EqualTo(CubeCategory.Chicane));
        }

        [Test]
        public void AParsedSpecIsAcceptedByTheRulesEngine()
        {
            // The strongest check that the two agree: the registry enforces its
            // own connector-count rule, so a spec that passes here and is
            // rejected there would mean the two disagree about what a cube is.
            var registry = new CubeRegistry();
            Assert.That(() => registry.Register(CubeSpecMapping.ToCubeType(Read(SpecFixtures.Straight).Spec)),
                Throws.Nothing);
            Assert.That(() => registry.Register(CubeSpecMapping.ToCubeType(Read(SpecFixtures.TrapdoorPit).Spec)),
                Throws.Nothing);
        }
    }
}
