using Lucid.Core;
using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// How each connector state looks, as the numbers the mist shader takes.
    /// </summary>
    /// <remarks>
    /// docs/UI.md §1: "Door states never depend on hue alone. Fog is dark and
    /// matte, Exit is bright and radiant, Solid is a wall, Attached is an
    /// opening." So the states are separated by density and brightness as much
    /// as by colour — a viewer who cannot tell the two hues apart still sees a
    /// dense dark sheet against a thin radiant one. The hatch on fog and the
    /// rays on exits that §11 asks for belong to the accessibility set, which
    /// §16 schedules for M3.
    /// </remarks>
    public readonly struct FogDoorLook
    {
        public readonly Color Tint;
        public readonly float Brightness;
        public readonly float Density;
        public readonly float Drift;

        /// <summary>How much of the mist has been eaten away, 0 to 1.</summary>
        public readonly float Dissolve;

        public FogDoorLook(Color tint, float brightness, float density, float drift, float dissolve)
        {
            Tint = tint;
            Brightness = brightness;
            Density = density;
            Drift = drift;
            Dissolve = dissolve;
        }

        /// <summary>Dark, dense and slow: a dead end for now.</summary>
        public static FogDoorLook Fog =>
            new FogDoorLook(new Color(0.42f, 0.45f, 0.52f), 0.55f, 0.92f, 0.10f, 0f);

        /// <summary>Bright, thin and quick: the way out.</summary>
        public static FogDoorLook Exit =>
            new FogDoorLook(new Color(0.96f, 0.97f, 1f), 2.6f, 0.45f, 0.45f, 0f);

        /// <summary>Gone. The Nightmare built here.</summary>
        public static FogDoorLook Attached =>
            new FogDoorLook(new Color(0.42f, 0.45f, 0.52f), 0.55f, 0f, 0.10f, 1f);

        /// <summary>
        /// Condensed into the wall: still opaque, but no longer drifting. The
        /// cube's own wall material takes the doorway once M0.6 builds cubes
        /// from a log; until then the mist simply stops moving and darkens,
        /// which is the tell that matters — a wall does not breathe.
        /// </summary>
        public static FogDoorLook Solid =>
            new FogDoorLook(new Color(0.20f, 0.20f, 0.23f), 0.30f, 1f, 0f, 0f);

        public static FogDoorLook For(ConnectorState state)
        {
            switch (state)
            {
                case ConnectorState.Fog: return Fog;
                case ConnectorState.Exit: return Exit;
                case ConnectorState.Attached: return Attached;
                case ConnectorState.Solid: return Solid;
                default: return Fog;
            }
        }

        public static FogDoorLook Lerp(FogDoorLook a, FogDoorLook b, float t) =>
            new FogDoorLook(
                Color.Lerp(a.Tint, b.Tint, t),
                Mathf.Lerp(a.Brightness, b.Brightness, t),
                Mathf.Lerp(a.Density, b.Density, t),
                Mathf.Lerp(a.Drift, b.Drift, t),
                Mathf.Lerp(a.Dissolve, b.Dissolve, t));
    }
}
