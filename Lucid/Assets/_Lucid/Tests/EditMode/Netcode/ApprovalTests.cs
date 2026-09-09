using Lucid.Netcode;
using Lucid.Runtime.UI;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Netcode
{
    /// <summary>docs/NETCODE.md §2: the payload, and the three ways to be turned away, in the order they are checked.</summary>
    public sealed class ApprovalTests
    {
        static readonly Hello Good = new Hello("1.0.0+abc", 0xC0FFEE, 7654321, "Anna", Hello.CurrentProtocol);
        static readonly Expectation Open = new Expectation("1.0.0+abc", 0xC0FFEE, Capacity: 5, Connected: 1);

        [Test]
        public void HelloSurvivesItsBytes()
        {
            Assert.That(Hello.FromBytes(Good.ToBytes()), Is.EqualTo(Good));
            Assert.That(Hello.FromBytes(null), Is.Null);
            Assert.That(Hello.FromBytes(new byte[] { 1, 2, 3 }), Is.Null);
        }

        [Test]
        public void TheThreeRefusalsAndTheirOrder()
        {
            Assert.That(Approval.Decide(Good, Open).Approved, Is.True);
            Assert.That(Approval.Decide(Good with { ProtocolVersion = 99 }, Open).Reason, Is.EqualTo(LucidStrings.ProtocolMismatch));
            Assert.That(Approval.Decide(null, Open).Reason, Is.EqualTo(LucidStrings.ProtocolMismatch), "bytes that are not a Hello");
            Assert.That(Approval.Decide(Good with { BuildId = "1.0.1" }, Open).Reason, Is.EqualTo(LucidStrings.DifferentVersion));
            Assert.That(Approval.Decide(Good with { ContentHash = 1 }, Open).Reason, Is.EqualTo(LucidStrings.DifferentVersion), "packs are part of the build");
            Assert.That(Approval.Decide(Good, Open with { Connected = 5 }).Reason, Is.EqualTo(LucidStrings.LobbyFull));
            // A wrong build in a full lobby is told about the build: it could not join a lobby with room either.
            Assert.That(Approval.Decide(Good with { BuildId = "x" }, Open with { Connected = 5 }).Reason, Is.EqualTo(LucidStrings.DifferentVersion));
        }
    }
}
