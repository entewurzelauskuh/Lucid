using Lucid.Core;
using Lucid.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Lucid.Tests.EditMode.Nightmare
{
    public sealed class GodViewPoseTests
    {
        static readonly Vector3 Pivot = new Vector3(0f, 4f, 0f);
        static readonly System.Collections.Generic.IEqualityComparer<Vector3> Near = new NearVector();

        sealed class NearVector : System.Collections.Generic.IEqualityComparer<Vector3>
        {
            public bool Equals(Vector3 a, Vector3 b) => (a - b).sqrMagnitude < 1e-6f;
            public int GetHashCode(Vector3 v) => 0;
        }

        [Test]
        public void TheCameraLooksAtThePivotFromItsDistance()
        {
            GodViewPose pose = GodViewPose.Default(Pivot);

            Vector3 toPivot = Pivot - pose.Position;
            Assert.That(toPivot.magnitude, Is.EqualTo(pose.Distance).Within(1e-3f));
            // The camera's forward is the direction to the pivot.
            Assert.That(Vector3.Dot((pose.Rotation * Vector3.forward).normalized, toPivot.normalized),
                Is.EqualTo(1f).Within(1e-4f));
            Assert.That(pose.Position.y, Is.GreaterThan(Pivot.y), "the god view is above the lattice");
        }

        [Test]
        public void TopDownOverridesThePitchWithoutLosingIt()
        {
            GodViewPose orbit = GodViewPose.Default(Pivot);
            GodViewPose down = orbit.WithTopDown(true);

            Assert.That(down.EffectivePitch, Is.EqualTo(GodViewPose.TopDownPitch));
            Assert.That(down.Pitch, Is.EqualTo(orbit.Pitch), "V should toggle, not destroy the orbit");
            Assert.That(down.WithTopDown(false).EffectivePitch, Is.EqualTo(orbit.Pitch));

            // Straight enough down that x and z of the camera sit over the pivot.
            Assert.That(down.Position.x, Is.EqualTo(Pivot.x).Within(0.6f));
            Assert.That(down.Position.z, Is.EqualTo(Pivot.z).Within(0.6f));
        }

        [Test]
        public void PitchAndDistanceAreClamped()
        {
            GodViewPose pose = GodViewPose.Default(Pivot).Orbited(0f, 400f);
            Assert.That(pose.Pitch, Is.EqualTo(GodViewPose.MaxPitch));
            pose = pose.Orbited(0f, -400f);
            Assert.That(pose.Pitch, Is.EqualTo(GodViewPose.MinPitch), "below the horizon is not a god view");

            Assert.That(GodViewPose.Default(Pivot).Zoomed(50f).Distance, Is.EqualTo(GodViewPose.MinDistance));
            Assert.That(GodViewPose.Default(Pivot).Zoomed(-50f).Distance, Is.EqualTo(GodViewPose.MaxDistance));
        }

        [Test]
        public void ZoomIsMultiplicativeSoAStepFeelsTheSameNearAndFar()
        {
            GodViewPose far = new GodViewPose(Pivot, 0f, 45f, 64f, false);
            GodViewPose near = new GodViewPose(Pivot, 0f, 45f, 16f, false);
            Assert.That(far.Zoomed(1f).Distance / far.Distance, Is.EqualTo(near.Zoomed(1f).Distance / near.Distance).Within(1e-5f));
        }

        [Test]
        public void PanningFollowsTheScreenAndStaysOnTheLayer()
        {
            // Facing north, screen right is world +x and screen up world +z.
            GodViewPose north = new GodViewPose(Pivot, 0f, 45f, 20f, false);
            Assert.That(north.Panned(new Vector2(1f, 0f), 2f).Pivot, Is.EqualTo(Pivot + new Vector3(2f, 0f, 0f)).Using(Near));
            Assert.That(north.Panned(new Vector2(0f, 1f), 2f).Pivot, Is.EqualTo(Pivot + new Vector3(0f, 0f, 2f)).Using(Near));

            // Turned a quarter clockwise, the camera faces east: screen right
            // is world -z — the sign is the point, a flipped pan passes a
            // test that only asks for movement along z.
            GodViewPose east = new GodViewPose(Pivot, 90f, 45f, 20f, false);
            GodViewPose panned = east.Panned(new Vector2(1f, 0f), 2f);
            Assert.That(panned.Pivot, Is.EqualTo(Pivot + new Vector3(0f, 0f, -2f)).Using(Near));
            Assert.That(panned.Pivot.y, Is.EqualTo(Pivot.y), "a pan never changes the layer");
        }

        [Test]
        public void FocusMovesThePivotAndNothingElse()
        {
            GodViewPose pose = GodViewPose.Default(Pivot).Orbited(20f, 5f).Zoomed(2f);
            Vector3 there = DreamSpace.Centre(new Coord(3, -2, 1));
            GodViewPose focused = pose.FocusedOn(there);

            Assert.That(focused.Pivot, Is.EqualTo(there));
            Assert.That(focused.Yaw, Is.EqualTo(pose.Yaw));
            Assert.That(focused.Pitch, Is.EqualTo(pose.Pitch));
            Assert.That(focused.Distance, Is.EqualTo(pose.Distance));
        }
    }

    public sealed class LayerCutawayTests
    {
        [Test]
        public void ShowAllSitsAtTheTopAndHidesNothing()
        {
            LayerCutaway cut = LayerCutaway.ShowAll(Limits.Default);
            Assert.That(cut.Layer, Is.EqualTo(Limits.Default.LayerMax));
            for (int z = Limits.Default.LayerMin; z <= Limits.Default.LayerMax; z++)
                Assert.That(cut.IsVisible(new Coord(0, 0, z)), Is.True, z.ToString());
        }

        [Test]
        public void EverythingAboveTheLayerIsCutAway()
        {
            LayerCutaway cut = new LayerCutaway(0, Limits.Default);
            Assert.That(cut.IsVisible(new Coord(0, 0, 0)), Is.True, "the chosen layer stays");
            Assert.That(cut.IsVisible(new Coord(0, 0, -1)), Is.True, "below stays");
            Assert.That(cut.IsVisible(new Coord(0, 0, 1)), Is.False, "above goes");
        }

        [Test]
        public void TheSliderStopsAtTheDreamsLimits()
        {
            LayerCutaway cut = LayerCutaway.ShowAll(Limits.Default).Up().Up();
            Assert.That(cut.Layer, Is.EqualTo(Limits.Default.LayerMax));
            cut = new LayerCutaway(Limits.Default.LayerMin, Limits.Default).Down();
            Assert.That(cut.Layer, Is.EqualTo(Limits.Default.LayerMin));
        }
    }
}
