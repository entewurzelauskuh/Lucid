using System.Collections.Generic;
using UnityEngine;

namespace Lucid.Runtime.Dev
{
    /// <summary>A built gauntlet and the handful of facts a runner needs about it.</summary>
    public sealed class Gauntlet
    {
        /// <summary>Every obstacle begins here, so every lane is comparable.</summary>
        public const float ObstacleZ = 0f;

        /// <summary>
        /// Runners start this far above the floor. A capsule spawned exactly on
        /// a surface starts interpenetrating it and the controller spends its
        /// first steps pushing itself out.
        /// </summary>
        public const float SpawnClearance = 0.1f;

        readonly GauntletLane[] _lanes;

        internal Gauntlet(GameObject root, GauntletLane[] lanes)
        {
            Root = root;
            _lanes = lanes;
        }

        public GameObject Root { get; }
        public IReadOnlyList<GauntletLane> Lanes => _lanes;

        /// <summary>Where a runner's feet start, facing +Z down the lane.</summary>
        public Vector3 SpawnFor(int index) => new Vector3(
            GauntletLayout.LaneX(index), SpawnClearance,
            ObstacleZ - GauntletLayout.SpawnSetback);

        /// <summary>The floor height a runner ends on if it beats lane <paramref name="index"/>.</summary>
        public float LandingHeightFor(int index) =>
            _lanes[index].Obstacle == GauntletObstacle.Ledge ? _lanes[index].Size : 0f;

        public int IndexOf(GauntletObstacle obstacle, float size)
        {
            for (int i = 0; i < _lanes.Length; i++)
                if (_lanes[i].Obstacle == obstacle && Mathf.Approximately(_lanes[i].Size, size))
                    return i;
            throw new KeyNotFoundException($"no {obstacle} lane of {size} m in this gauntlet");
        }
    }
}
