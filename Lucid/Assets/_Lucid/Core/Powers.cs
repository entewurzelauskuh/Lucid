using System;
using System.Collections.Generic;

namespace Lucid.Core
{
    /// <summary>
    /// Budget and cooldowns for the Nightmare's live powers. What a trigger or
    /// effect actually does inside a dream is runtime code; Core only answers
    /// "may the Nightmare do this now, and what does it cost"
    /// (docs/CORE-API.md §9).
    /// </summary>
    public sealed class Powers
    {
        readonly Dictionary<EffectKind, EffectSpec> _effects = new Dictionary<EffectKind, EffectSpec>();
        readonly Dictionary<EffectKind, int[]> _effectReadyAt = new Dictionary<EffectKind, int[]>();
        // Cooldowns are per trap per dream (docs/SPEC.md §10); a trap is
        // addressed by the cube it lives in.
        readonly Dictionary<Coord, int[]> _triggerReadyAt = new Dictionary<Coord, int[]>();

        readonly int _dreamCount;

        public Powers(IReadOnlyList<EffectSpec> effects, int triggerCooldownMs, int dreamCount)
        {
            if (effects == null) throw new ArgumentNullException(nameof(effects));
            if (dreamCount <= 0) throw new ArgumentOutOfRangeException(nameof(dreamCount));
            if (triggerCooldownMs < 0) throw new ArgumentOutOfRangeException(nameof(triggerCooldownMs));

            _dreamCount = dreamCount;
            TriggerCooldownMs = triggerCooldownMs;

            foreach (EffectSpec spec in effects)
            {
                if (spec == null) throw new ArgumentException("null effect spec", nameof(effects));
                _effects[spec.Kind] = spec;
                _effectReadyAt[spec.Kind] = new int[dreamCount];
            }
        }

        public int TriggerCooldownMs { get; }

        /// <summary>
        /// While the Nightmare is inside a dream, nothing else is available:
        /// no placement, no triggers, no effects (docs/SPEC.md §10). The maze
        /// stops growing for everyone, which is what makes possession a gamble.
        /// </summary>
        public bool PossessionActive { get; set; }

        public PowerError ValidateEffect(EffectKind k, PowerTarget t, Budget b, int clockMs)
        {
            // Possession first: while it is active nothing else is available,
            // whatever else might also be wrong (docs/SPEC.md §10).
            if (PossessionActive) return PowerError.Possessed;
            if (!_effects.TryGetValue(k, out EffectSpec spec)) return PowerError.Disabled;
            if (!IsTargetable(t)) return PowerError.NoSuchDream;

            foreach (int dream in Dreams(t))
            {
                if (_effectReadyAt[k][dream] > clockMs) return PowerError.OnCooldown;
            }

            // Flat cost per use, however many dreams it lands in.
            if (b != null && !b.CanAfford(spec.Cost)) return PowerError.NotEnoughBudget;

            return PowerError.None;
        }

        /// <summary>
        /// Fires the effect. Returns false and changes nothing if it is not
        /// currently allowed.
        /// </summary>
        /// <remarks>
        /// Returns a bool rather than the void of docs/CORE-API.md §9 because
        /// the spend can fail even after a passing verdict: the host loop
        /// validates, then may spend budget on a placement, then applies
        /// (§10). The old signature had no way to say so, and started the
        /// cooldown regardless — an effect the Nightmare could not afford fired
        /// for free. See docs/DECISIONS.md.
        /// </remarks>
        public bool ApplyEffect(EffectKind k, PowerTarget t, Budget b, int clockMs)
        {
            if (ValidateEffect(k, t, b, clockMs) != PowerError.None) return false;

            EffectSpec spec = _effects[k];
            if (b != null && !b.TrySpend(spec.Cost)) return false;

            foreach (int dream in Dreams(t))
            {
                _effectReadyAt[k][dream] = clockMs + spec.CooldownMs;
            }
            return true;
        }

        public PowerError ValidateTrigger(Coord cube, PowerTarget t, int clockMs)
        {
            if (PossessionActive) return PowerError.Possessed;
            if (!IsTargetable(t)) return PowerError.NoSuchDream;

            int[] readyAt = TriggerSlots(cube, create: false);
            if (readyAt != null)
            {
                foreach (int dream in Dreams(t))
                {
                    if (readyAt[dream] > clockMs) return PowerError.OnCooldown;
                }
            }

            // Triggers cost no budget; their cost is the Nightmare's attention
            // (docs/SPEC.md §10).
            return PowerError.None;
        }

        /// <summary>Fires the trap. Returns false and changes nothing if refused.</summary>
        public bool ApplyTrigger(Coord cube, PowerTarget t, int clockMs)
        {
            if (ValidateTrigger(cube, t, clockMs) != PowerError.None) return false;

            int[] readyAt = TriggerSlots(cube, create: true);
            foreach (int dream in Dreams(t))
            {
                readyAt[dream] = clockMs + TriggerCooldownMs;
            }
            return true;
        }

        public int CooldownRemainingMs(EffectKind k, int dreamId, int clockMs)
        {
            if (!_effectReadyAt.TryGetValue(k, out int[] readyAt)) return 0;
            if (dreamId < 0 || dreamId >= _dreamCount) return 0;
            return Math.Max(0, readyAt[dreamId] - clockMs);
        }

        public int TriggerCooldownRemainingMs(Coord cube, int dreamId, int clockMs)
        {
            int[] readyAt = TriggerSlots(cube, create: false);
            if (readyAt == null || dreamId < 0 || dreamId >= _dreamCount) return 0;
            return Math.Max(0, readyAt[dreamId] - clockMs);
        }

        bool IsTargetable(PowerTarget t) =>
            t.IsAll || (t.DreamId >= 0 && t.DreamId < _dreamCount);

        IEnumerable<int> Dreams(PowerTarget t)
        {
            if (!t.IsAll)
            {
                yield return t.DreamId;
                yield break;
            }
            for (int i = 0; i < _dreamCount; i++) yield return i;
        }

        int[] TriggerSlots(Coord cube, bool create)
        {
            if (_triggerReadyAt.TryGetValue(cube, out int[] slots)) return slots;
            if (!create) return null;

            slots = new int[_dreamCount];
            _triggerReadyAt[cube] = slots;
            return slots;
        }

    }
}
