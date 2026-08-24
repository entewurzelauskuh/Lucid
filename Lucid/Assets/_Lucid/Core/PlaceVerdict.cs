using System;

namespace Lucid.Core
{
    /// <summary>
    /// The answer to "may the Nightmare build here". <see cref="TrappedSleeper"/>
    /// names the first Sleeper the leak rule would have stranded, or -1.
    /// </summary>
    public readonly struct PlaceVerdict : IEquatable<PlaceVerdict>
    {
        public static readonly PlaceVerdict Ok = new PlaceVerdict(PlaceError.None);

        public PlaceVerdict(PlaceError error, int trappedSleeper = -1)
        {
            Error = error;
            TrappedSleeper = trappedSleeper;
        }

        public PlaceError Error { get; }
        public int TrappedSleeper { get; }

        public bool IsOk => Error == PlaceError.None;

        public bool Equals(PlaceVerdict other) =>
            Error == other.Error && TrappedSleeper == other.TrappedSleeper;

        public override bool Equals(object obj) => obj is PlaceVerdict other && Equals(other);
        public override int GetHashCode() { unchecked { return ((int)Error * 397) ^ TrappedSleeper; } }
        public static bool operator ==(PlaceVerdict a, PlaceVerdict b) => a.Equals(b);
        public static bool operator !=(PlaceVerdict a, PlaceVerdict b) => !a.Equals(b);

        public override string ToString() =>
            TrappedSleeper >= 0 ? $"{Error}(sleeper {TrappedSleeper})" : Error.ToString();
    }
}
