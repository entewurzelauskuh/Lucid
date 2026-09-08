using System.Collections.Generic;
using Lucid.Core;
using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// The god view: a camera driven by a <see cref="GodViewPose"/>, and the
    /// layer slider driven by a <see cref="LayerCutaway"/>. Holds the two
    /// values and copies them onto the scene; every decision is in the values.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class GodViewCamera : MonoBehaviour
    {
        /// <summary>Metres the pivot moves per screen pixel of drag, at distance 1.</summary>
        const float PanPerPixelPerMetre = 0.0016f;

        /// <summary>Degrees per pixel of orbit drag.</summary>
        const float OrbitPerPixel = 0.25f;

        [SerializeField] DreamInstance _dream;

        Camera _camera;

        public GodViewPose Pose { get; private set; }
        public LayerCutaway Cutaway { get; private set; }
        public Camera Camera => _camera != null ? _camera : (_camera = GetComponent<Camera>());

        internal void Configure(DreamInstance dream) => _dream = dream;

        void Awake()
        {
            _camera = GetComponent<Camera>();
            Limits limits = Limits.Default;
            Cutaway = LayerCutaway.ShowAll(limits);
            Pose = GodViewPose.Default(DreamSpace.Centre(new Coord(0, 0, 0)));
            Apply();
        }

        void Start()
        {
            if (_dream != null) FocusStart();
        }

        public void Set(GodViewPose pose)
        {
            Pose = pose;
            Apply();
        }

        public void Orbit(Vector2 pixels) => Set(Pose.Orbited(pixels.x * OrbitPerPixel, -pixels.y * OrbitPerPixel));

        public void Pan(Vector2 pixels) => Set(Pose.Panned(pixels, PanPerPixelPerMetre * Pose.Distance));

        public void Zoom(float steps) => Set(Pose.Zoomed(steps));

        public void ToggleTopDown() => Set(Pose.WithTopDown(!Pose.TopDown));

        /// <summary>Home: the start cube, the one place a Nightmare always knows.</summary>
        public void FocusStart()
        {
            Coord start = _dream != null ? _dream.Start : new Coord(0, 0, 0);
            Set(Pose.FocusedOn(DreamSpace.Centre(start)));
        }

        public void LayerUp() => SetCutaway(Cutaway.Up());
        public void LayerDown() => SetCutaway(Cutaway.Down());

        public void SetCutaway(LayerCutaway cutaway)
        {
            Cutaway = cutaway;
            ApplyCutaway();
        }

        /// <summary>Re-applies the slider to the cubes standing now; call after the lattice grows.</summary>
        public void ApplyCutaway()
        {
            if (_dream == null) return;
            foreach (KeyValuePair<Coord, DreamCube> pair in _dream.Cubes)
            {
                if (pair.Value != null) pair.Value.SetCutAway(!Cutaway.IsVisible(pair.Key));
            }
        }

        void Apply()
        {
            Transform t = transform;
            t.position = Pose.Position;
            t.rotation = Pose.Rotation;
        }
    }
}
