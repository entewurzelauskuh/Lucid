using System;
using Lucid.Core;

namespace Lucid.Netcode
{
    /// <summary>What applying one broadcast event came to.</summary>
    public readonly struct MirrorResult
    {
        public readonly bool Applied;
        /// <summary>The message's seq is not the next one: a lost reliable message, which cannot happen, so a bug (§5, §10).</summary>
        public readonly bool Gap;
        /// <summary>The message named a type or a kind this registry has not got.</summary>
        public readonly bool Unknown;
        /// <summary>The event could not be applied — a cube on an occupied coord, an invariant broken. The host's log and this lattice have parted; a report has to say so.</summary>
        public readonly bool Faulted;
        public readonly ulong Hash;
        public readonly ulong Expected;
        public bool InSync => Applied && Hash == Expected;

        public MirrorResult(bool applied, bool gap, bool unknown, ulong hash, ulong expected, bool faulted = false)
        {
            Applied = applied; Gap = gap; Unknown = unknown; Hash = hash; Expected = expected; Faulted = faulted;
        }

        public override string ToString() =>
            Applied ? (InSync ? $"applied, hash {Hash:x16}" : $"applied, DESYNC {Hash:x16} != {Expected:x16}") : Gap ? "gap" : Faulted ? "faulted" : "unknown";
    }

    /// <summary>
    /// A client's lattice (docs/NETCODE.md §5, §13): every broadcast event is
    /// applied in seq order through the same functions Replay uses, so the
    /// hash after each one is the host's if and only if the two machines
    /// derive alike — which is the whole check. The log it keeps is what a
    /// .lucidlog holds when the check fails.
    /// </summary>
    public sealed class LatticeMirror
    {
        readonly Codec _codec;
        readonly RoundSettings _settings;

        public LatticeMirror(Codec codec, string startTypeId, Rotation startRotation, RoundSettings settings)
        {
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
            _settings = settings ?? new RoundSettings();
            Log = new EventLog();
            Lattice = Lattice.New(codec.Registry, startTypeId, startRotation);
            Derived = Deriver.Derive(Lattice, codec.Registry, _settings.ExitHysteresis);
        }

        public EventLog Log { get; }
        public Lattice Lattice { get; private set; }
        public Derived Derived { get; private set; }
        public RoundSettings Settings => _settings;
        public uint NextSeq => (uint)Log.NextSeq;

        public MirrorResult Apply(LatticeEventMsg m)
        {
            if (m.Seq != Log.NextSeq) return new MirrorResult(false, true, false, Derived.Hash, m.PostHash);

            LatticeEvent e = _codec.Decode(m);
            if (e == null) return new MirrorResult(false, false, true, Derived.Hash, m.PostHash);

            var ctx = new RuleContext(Lattice, Derived, _codec.Registry, Array.Empty<SleeperState>(), null, _settings);
            try
            {
                switch (e)
                {
                    case CubePlaced p:
                        (Lattice, Derived) = Rules.PlaceAt(ctx, p.Cube, p.TypeId, p.Rotation, p.SkinId, p.Seq);
                        break;
                    case CubeExplored x:
                        (Lattice, Derived) = Rules.ApplyExplore(ctx, x.Cube, x.Seq);
                        break;
                }
            }
            catch (Exception ex) when (ex is LatticeInvariantViolation || ex is ArgumentException || ex is InvalidOperationException)
            {
                // An honest host never sends this; a corrupted one or a bug
                // has. Nothing is applied, the seq does not advance, and the
                // caller reports a hash that cannot match so the host notices
                // rather than waiting on a report that never comes.
                return new MirrorResult(false, false, false, Derived.Hash, m.PostHash, faulted: true);
            }
            Log.Append(e);
            return new MirrorResult(true, false, false, Derived.Hash, m.PostHash);
        }
    }
}
