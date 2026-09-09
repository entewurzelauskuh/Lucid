using System.Collections.Generic;

namespace Lucid.Netcode
{
    /// <summary>
    /// The host's half of the sync check (docs/NETCODE.md §5): the hash after
    /// every event it broadcast, and each client's report against it. A
    /// report for a seq the host never broadcast is a bug in the reporter,
    /// not a desync, and is told apart.
    /// </summary>
    public sealed class HashLedger
    {
        public enum Verdict { Match, Mismatch, UnknownSeq }

        readonly Dictionary<uint, ulong> _postHash = new Dictionary<uint, ulong>();

        public void Broadcast(uint seq, ulong postHash) => _postHash[seq] = postHash;

        public Verdict Report(uint seq, ulong hash)
        {
            if (!_postHash.TryGetValue(seq, out ulong expected)) return Verdict.UnknownSeq;
            return hash == expected ? Verdict.Match : Verdict.Mismatch;
        }

        public bool TryExpected(uint seq, out ulong postHash) => _postHash.TryGetValue(seq, out postHash);
        public int Count => _postHash.Count;
    }
}
