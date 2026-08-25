using System;

namespace Lucid.Core
{
    /// <summary>
    /// The answer to "may the Nightmare build here". <see cref="TrappedSleeper"/>
    /// names the first Sleeper the leak rule would have stranded, or -1.
    /// </summary>
    public readonly struct PlaceVerdict : IEquatable<PlaceVerdict>
    {
        /// <summary>A passing verdict. Named Pass so the instance predicate
        /// can keep the name Ok that docs/CORE-API.md §5 and §10 use.</summary>
        public static readonly PlaceVerdict Pass = new PlaceVerdict(PlaceError.None);

        public PlaceVerdict(PlaceError error, int trappedSleeper = -1)
        {
            Error = error;
            TrappedSleeper = trappedSleeper;
        }

        public PlaceError Error { get; }
        public int TrappedSleeper { get; }

        public bool Ok => Error == PlaceError.None;

        public void Deconstruct(out PlaceError error, out int trappedSleeper)
        {
            error = Error;
            trappedSleeper = TrappedSleeper;
        }

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
