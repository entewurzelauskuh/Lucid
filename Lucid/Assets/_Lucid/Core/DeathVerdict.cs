using System;

namespace Lucid.Core
{
    /// <summary>What a reported death cost: a life, or the whole round.</summary>
    public readonly struct DeathVerdict : IEquatable<DeathVerdict>
    {
        public static readonly DeathVerdict Ignored = new DeathVerdict(DeathOutcome.Ignored, 0);

        public DeathVerdict(DeathOutcome outcome, int livesLeft)
        {
            Outcome = outcome;
            LivesLeft = livesLeft;
        }

        public DeathOutcome Outcome { get; }
        public int LivesLeft { get; }

        public void Deconstruct(out DeathOutcome outcome, out int livesLeft)
        {
            outcome = Outcome;
            livesLeft = LivesLeft;
        }

        public bool Equals(DeathVerdict other) => Outcome == other.Outcome && LivesLeft == other.LivesLeft;
        public override bool Equals(object obj) => obj is DeathVerdict other && Equals(other);
        public override int GetHashCode() { unchecked { return ((int)Outcome * 397) ^ LivesLeft; } }
        public static bool operator ==(DeathVerdict a, DeathVerdict b) => a.Equals(b);
        public static bool operator !=(DeathVerdict a, DeathVerdict b) => !a.Equals(b);
        public override string ToString() =>
            Outcome == DeathOutcome.LostLife ? $"LostLife({LivesLeft})" : Outcome.ToString();
    }
}
