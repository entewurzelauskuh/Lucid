using System;

namespace Lucid.Core
{
    /// <summary>An opaque player handle assigned by the lobby.</summary>
    public readonly struct PlayerId : IEquatable<PlayerId>
    {
        public static readonly PlayerId None = new PlayerId(-1);

        public int Value { get; }

        public PlayerId(int value) => Value = value;

        public bool Equals(PlayerId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is PlayerId other && Equals(other);
        public override int GetHashCode() => Value;
        public static bool operator ==(PlayerId a, PlayerId b) => a.Equals(b);
        public static bool operator !=(PlayerId a, PlayerId b) => !a.Equals(b);
        public override string ToString() => $"P{Value}";
    }
}
