namespace Lucid.Core
{
    /// <summary>
    /// The host's knobs for one round. Every duration is integer milliseconds;
    /// the tuning console (M0.9c) is the only thing that changes them.
    /// </summary>
    public sealed record RoundSettings(
        int HeadStartMs = 30_000,
        int RoundLengthMs = 300_000,
        int Lives = 1,
        int StartingBudget = 12,
        int TrickleIntervalMs = 4_000,
        int ExitHysteresis = 0,
        Limits Limits = null)
    {
        // Fully qualified: the Limits property shadows the Limits type here.
        public Limits EffectiveLimits => Limits ?? Lucid.Core.Limits.Default;
    }
}
