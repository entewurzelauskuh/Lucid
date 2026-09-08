using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// Where the god view's camera is, as a value: a pivot on the lattice, a
    /// yaw and pitch around it, a distance, and whether it is looking straight
    /// down (docs/SPEC.md §10, docs/UI.md §8). Pure, so the camera maths has a
    /// test and the MonoBehaviour that drives a <c>Camera</c> from it does
    /// nothing but copy.
    /// </summary>
    public readonly struct GodViewPose
    {
        public const float MinPitch = 20f;
        public const float MaxPitch = 89f;
        public const float MinDistance = 8f;
        public const float MaxDistance = 96f;

        /// <summary>Straight down, or as near as a look rotation allows without flipping.</summary>
        public const float TopDownPitch = MaxPitch;

        public readonly Vector3 Pivot;
        public readonly float Yaw;
        public readonly float Pitch;
        public readonly float Distance;
        public readonly bool TopDown;

        public GodViewPose(Vector3 pivot, float yaw, float pitch, float distance, bool topDown)
        {
            Pivot = pivot;
            Yaw = yaw;
            Pitch = Mathf.Clamp(pitch, MinPitch, MaxPitch);
            Distance = Mathf.Clamp(distance, MinDistance, MaxDistance);
            TopDown = topDown;
        }

        /// <summary>Over the start cube, three-quarter view, far enough to see a few rooms.</summary>
        public static GodViewPose Default(Vector3 pivot) => new GodViewPose(pivot, 35f, 55f, 28f, false);

        /// <summary>The pitch actually used: top-down overrides the orbit pitch without losing it.</summary>
        public float EffectivePitch => TopDown ? TopDownPitch : Pitch;

        public Quaternion Rotation => Quaternion.Euler(EffectivePitch, Yaw, 0f);

        /// <summary>Where the camera sits, looking at the pivot.</summary>
        public Vector3 Position => Pivot + Rotation * new Vector3(0f, 0f, -Distance);

        public GodViewPose Orbited(float yawDelta, float pitchDelta) =>
            new GodViewPose(Pivot, Yaw + yawDelta, Pitch + pitchDelta, Distance, TopDown);

        /// <summary>Zoom is multiplicative, so a scroll feels the same near and far.</summary>
        public GodViewPose Zoomed(float steps) =>
            new GodViewPose(Pivot, Yaw, Pitch, Distance * Mathf.Pow(0.85f, steps), TopDown);

        /// <summary>
        /// Pans in the camera's own frame flattened to the ground, so "right" is
        /// right on screen whichever way the view is turned, and the pivot
        /// never leaves the layer it was on.
        /// </summary>
        public GodViewPose Panned(Vector2 screenDelta, float metresPerUnit)
        {
            Quaternion flat = Quaternion.Euler(0f, Yaw, 0f);
            Vector3 move = flat * new Vector3(screenDelta.x, 0f, screenDelta.y) * metresPerUnit;
            return new GodViewPose(Pivot + move, Yaw, Pitch, Distance, TopDown);
        }

        public GodViewPose FocusedOn(Vector3 pivot) => new GodViewPose(pivot, Yaw, Pitch, Distance, TopDown);

        public GodViewPose WithTopDown(bool topDown) => new GodViewPose(Pivot, Yaw, Pitch, Distance, topDown);
    }
}
