using System.Collections.Generic;

namespace Lucid.Core
{
    /// <summary>
    /// Points at the end of a round (docs/SPEC.md §12). Waking early is worth
    /// more than waking late, so a Sleeper who runs is rewarded over one who
    /// waits out the clock.
    /// </summary>
    public static class Scoring
    {
        public const int WakeBonus = 100;
        public const int PerConsumedSleeper = 100;

        public static IReadOnlyDictionary<PlayerId, int> Compute(Round r, PlayerId nightmare)
        {
            var scores = new Dictionary<PlayerId, int> { [nightmare] = 0 };

            foreach (SleeperState s in r.Sleepers)
            {
                switch (s.Status)
                {
                    case SleeperStatus.Awake:
                        Add(scores, s.Player, WakeBonus + RemainingSeconds(r, s));
                        break;

                    case SleeperStatus.Consumed:
                        Add(scores, s.Player, 0);
                        Add(scores, nightmare, PerConsumedSleeper);
                        break;

                    default:
                        // Still running or dropped out: no points either way.
                        Add(scores, s.Player, 0);
                        break;
                }
            }

            return scores;
        }

        /// <summary>
        /// Accumulates rather than assigns, so a PlayerId appearing twice — two
        /// Sleepers sharing an id, or a Nightmare who is also listed as one —
        /// cannot silently drop one of the two scores.
        /// </summary>
        static void Add(IDictionary<PlayerId, int> scores, PlayerId player, int points)
        {
            scores[player] = scores.TryGetValue(player, out int existing) ? existing + points : points;
        }

        /// <summary>
        /// Whole seconds left on the clock at the moment of waking. Integer
        /// division, because Core does no floating point.
        /// </summary>
        static int RemainingSeconds(Round r, SleeperState s)
        {
            if (s.WokeAtMs < 0) return 0;
            int remainingMs = r.Settings.RoundLengthMs - s.WokeAtMs;
            return remainingMs > 0 ? remainingMs / 1000 : 0;
        }
    }
}
