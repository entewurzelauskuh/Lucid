using System;

namespace Lucid.Core
{
    /// <summary>
    /// Which dream a power lands in. All is the efficient choice and targeting
    /// is the precise one: cooldowns are per power per dream, and All consumes
    /// them everywhere (docs/SPEC.md §10).
    /// </summary>
    public readonly struct PowerTarget : IEquatable<PowerTarget>
    {
        public const int AllDreams = -1;

        public PowerTarget(int dreamId) => DreamId = dreamId;

        public int DreamId { get; }

        public static PowerTarget All => new PowerTarget(AllDreams);

        public bool IsAll => DreamId == AllDreams;

        public void Deconstruct(out int dreamId) => dreamId = DreamId;

        public bool Equals(PowerTarget other) => DreamId == other.DreamId;
        public override bool Equals(object obj) => obj is PowerTarget other && Equals(other);
        public override int GetHashCode() => DreamId;
        public static bool operator ==(PowerTarget a, PowerTarget b) => a.Equals(b);
        public static bool operator !=(PowerTarget a, PowerTarget b) => !a.Equals(b);
        public override string ToString() => IsAll ? "All" : $"Dream {DreamId}";
    }
}
