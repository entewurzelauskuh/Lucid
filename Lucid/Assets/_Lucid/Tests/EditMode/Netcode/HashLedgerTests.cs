using Lucid.Netcode;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Netcode
{
    public sealed class HashLedgerTests
    {
        [Test]
        public void AReportIsJudgedAgainstWhatWasBroadcast()
        {
            var ledger = new HashLedger();
            ledger.Broadcast(0, 10); ledger.Broadcast(1, 11);
            Assert.That(ledger.Report(0, 10), Is.EqualTo(HashLedger.Verdict.Match));
            Assert.That(ledger.Report(1, 12), Is.EqualTo(HashLedger.Verdict.Mismatch));
            Assert.That(ledger.Report(2, 12), Is.EqualTo(HashLedger.Verdict.UnknownSeq), "a seq never broadcast is the reporter's bug, not a desync");
        }
    }
}
