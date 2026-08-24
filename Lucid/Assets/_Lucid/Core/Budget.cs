using System;

namespace Lucid.Core
{
    /// <summary>
    /// The Nightmare's points. Trickle accumulates in integer milliseconds and
    /// keeps its remainder, so a round advanced in many small steps grants
    /// exactly as many points as one advanced in a single large step
    /// (docs/CORE-API.md §2).
    /// </summary>
    public sealed class Budget
    {
        int _sinceLastPoint;

        public Budget(int startingPoints, int trickleIntervalMs)
        {
            if (startingPoints < 0) throw new ArgumentOutOfRangeException(nameof(startingPoints));
            if (trickleIntervalMs < 0) throw new ArgumentOutOfRangeException(nameof(trickleIntervalMs));

            Points = startingPoints;
            TrickleIntervalMs = trickleIntervalMs;
        }

        public int Points { get; private set; }

        /// <summary>Zero means no trickle.</summary>
        public int TrickleIntervalMs { get; }

        public int MsUntilNextPoint =>
            TrickleIntervalMs == 0 ? 0 : TrickleIntervalMs - _sinceLastPoint;

        public void Advance(int deltaMs)
        {
            if (deltaMs < 0) throw new ArgumentOutOfRangeException(nameof(deltaMs));
            if (TrickleIntervalMs == 0 || deltaMs == 0) return;

            _sinceLastPoint += deltaMs;
            int earned = _sinceLastPoint / TrickleIntervalMs;
            if (earned > 0)
            {
                Points += earned;
                _sinceLastPoint -= earned * TrickleIntervalMs;   // the remainder survives
            }
        }

        public bool CanAfford(int cost) => cost <= Points;

        public bool TrySpend(int cost)
        {
            if (cost < 0) throw new ArgumentOutOfRangeException(nameof(cost));
            if (!CanAfford(cost)) return false;
            Points -= cost;
            return true;
        }
    }
}
