using Lucid.Runtime.UI;

namespace Lucid.Netcode
{
    /// <summary>What the host expects of a Hello, and how many seats it has.</summary>
    public sealed record Expectation(string BuildId, ulong ContentHash, int Capacity, int Connected, ushort ProtocolVersion = Hello.CurrentProtocol);

    /// <summary>The host's answer: approved, or a §14 reason the client shows.</summary>
    public readonly struct Decision
    {
        public readonly bool Approved;
        public readonly string Reason;
        public Decision(bool approved, string reason) { Approved = approved; Reason = reason; }
        public static readonly Decision Yes = new Decision(true, null);
        public static Decision No(string reason) => new Decision(false, reason);
        public override string ToString() => Approved ? "approved" : $"rejected: {Reason}";
    }

    /// <summary>
    /// docs/NETCODE.md §2, as a pure function: protocol first (a client that
    /// cannot even parse the rest is told so in words it can parse), then the
    /// build and its content, then the seats. Same-steamId resume and the
    /// round-in-progress case are M1.3–M1.4's.
    /// </summary>
    public static class Approval
    {
        public static Decision Decide(Hello hello, Expectation expect)
        {
            if (hello == null || hello.ProtocolVersion != expect.ProtocolVersion) return Decision.No(LucidStrings.ProtocolMismatch);
            if (hello.BuildId != expect.BuildId || hello.ContentHash != expect.ContentHash) return Decision.No(LucidStrings.DifferentVersion);
            if (expect.Connected >= expect.Capacity) return Decision.No(LucidStrings.LobbyFull);
            return Decision.Yes;
        }
    }
}
