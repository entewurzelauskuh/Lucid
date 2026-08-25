using System.Globalization;

namespace Lucid.Runtime.Dev
{
    public enum GauntletObstacle
    {
        /// <summary>A hole in the floor of the given width, in metres.</summary>
        Gap,

        /// <summary>A step up of the given height, in metres.</summary>
        Ledge,
    }

    /// <summary>One lane of the gauntlet: a run-up, one obstacle, a landing.</summary>
    public readonly struct GauntletLane
    {
        public readonly GauntletObstacle Obstacle;

        /// <summary>Gap width or ledge height, metres.</summary>
        public readonly float Size;

        public GauntletLane(GauntletObstacle obstacle, float size)
        {
            Obstacle = obstacle;
            Size = size;
        }

        public string Name =>
            Obstacle + "-" + Size.ToString("0.0", CultureInfo.InvariantCulture);

        public override string ToString() => Name;
    }

    /// <summary>
    /// The movement test course of docs/WORKPLAN.md §4 (M0.4). It lives in
    /// Runtime rather than Editor because two very different callers need the
    /// same geometry: the editor script that saves the scene a human walks,
    /// and the PlayMode test that builds it from nothing. One description, so
    /// the test cannot pass against a course the human never sees.
    /// </summary>
    public static class GauntletLayout
    {
        /// <summary>Run-up length before every obstacle, metres.</summary>
        public const float RunUp = 10f;

        /// <summary>Landing length after every obstacle, metres.</summary>
        public const float Landing = 10f;

        public const float LaneWidth = 4f;
        public const float LaneSpacing = 6f;
        public const float SlabThickness = 1f;

        /// <summary>How far below the course the catch floor sits, metres.</summary>
        public const float CatchDepth = 20f;

        /// <summary>Where a runner starts, in metres of run-up before the obstacle.</summary>
        public const float SpawnSetback = 8f;

        /// <summary>
        /// Every obstacle is a pair astride the guideline it tests: the
        /// chicane limits are gaps ≤ 3.5 m and ledges ≤ 1.1 m (SPEC §9), so
        /// 3.5 and 1.1 must be clearable and 4.5 and 1.4 must not.
        /// </summary>
        public static readonly GauntletLane[] Standard =
        {
            new GauntletLane(GauntletObstacle.Gap, 3.0f),
            new GauntletLane(GauntletObstacle.Gap, 3.5f),
            new GauntletLane(GauntletObstacle.Gap, 4.5f),
            new GauntletLane(GauntletObstacle.Ledge, 1.0f),
            new GauntletLane(GauntletObstacle.Ledge, 1.1f),
            new GauntletLane(GauntletObstacle.Ledge, 1.4f),
        };

        /// <summary>Centre of lane <paramref name="index"/> on the x axis.</summary>
        public static float LaneX(int index) => index * LaneSpacing;
    }
}
