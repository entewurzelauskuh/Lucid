using System.Globalization;

namespace Lucid.Runtime.Dev
{
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
}
