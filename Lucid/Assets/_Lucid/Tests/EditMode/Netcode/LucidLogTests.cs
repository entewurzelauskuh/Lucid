using System.Collections.Generic;
using System.IO;
using Lucid.Core;
using Lucid.Runtime;
using Lucid.Tests.EditMode.Core;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Netcode
{
    /// <summary>docs/CORE-API.md §7: the runtime's header around Core's events, and a replay that hashes like the host.</summary>
    public sealed class LucidLogTests
    {
        [Test]
        public void ALogRoundTripsAndReplaysToTheHostsHash()
        {
            var host = new Round(new RoundSettings(HeadStartMs: 0, Lives: 2), TestLattice.Registry(), TestLattice.Start, Rotation.R0,
                new List<PlayerId> { new PlayerId(100) });
            Assert.That(host.TryPlace(new PlaceRequest(new ConnectorRef(new Coord(0, 0, 0), Face.North), TestLattice.Straight, Rotation.R0, "*")).Ok, Is.True);
            host.UpdateSleeperCube(0, new Coord(0, 1, 0));
            Assert.That(host.TryExplore(0, new Coord(0, 1, 0)), Is.EqualTo(ExploreError.None));

            var players = new[] { new LucidLog.Player(0, "Anna"), new LucidLog.Player(1, "Ben") };
            var ms = new MemoryStream();
            LucidLog.Write(ms, host.Settings, players, host.Registry, host.Log, "desync at seq 1");
            ms.Position = 0;
            LucidLog.Contents read = LucidLog.Read(ms);

            Assert.That(read.Settings, Is.EqualTo(host.Settings));
            Assert.That(read.Players, Is.EqualTo(players));
            Assert.That(read.RegistryIds.Count, Is.EqualTo(host.Registry.All.Count));
            Assert.That(read.Note, Is.EqualTo("desync at seq 1"));
            Assert.That(read.Events.Events, Is.EqualTo(host.Log.Events));

            (Lattice _, Derived derived) = EventLog.Replay(read.Events, host.Registry, TestLattice.Start, Rotation.R0, read.Settings);
            Assert.That(derived.Hash, Is.EqualTo(host.Derived.Hash));
            Assert.That(ms.Position, Is.EqualTo(ms.Length), "bytes left unread");
        }

        [Test]
        public void SomethingElseIsRefused()
        {
            Assert.That(() => LucidLog.Read(new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6 })), Throws.TypeOf<InvalidDataException>());
        }

        [Test]
        public void ACountFromTheFileIsDataNotAnAllocation()
        {
            // A negative player count is a bad file, and the caller catches
            // InvalidDataException — not the ArgumentOutOfRange a List ctor throws.
            var ms = new MemoryStream();
            using (var w = new System.IO.BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                w.Write(new[] { (byte)'L', (byte)'U', (byte)'C', (byte)'L' }); w.Write((ushort)1); w.Write("note");
                for (int i = 0; i < 6; i++) w.Write(1);
                w.Write(-1);
            }
            ms.Position = 0;
            Assert.That(() => LucidLog.Read(ms), Throws.TypeOf<InvalidDataException>());
        }
    }
}
