using System.Collections.Generic;

namespace Lucid.Core
{
    /// <summary>
    /// What one global effect costs and how long it lasts. Defaults from
    /// docs/SPEC.md §10; the tuning console (M0.9c) is what changes them.
    /// </summary>
    public sealed record EffectSpec(EffectKind Kind, int Cost, int DurationMs, int CooldownMs)
    {
        public static IReadOnlyList<EffectSpec> Defaults { get; } = new[]
        {
            new EffectSpec(EffectKind.Dark, 3, 8_000, 30_000),
            new EffectSpec(EffectKind.Fog, 2, 10_000, 30_000),
            new EffectSpec(EffectKind.Molasses, 4, 6_000, 30_000),
        };
    }
}
