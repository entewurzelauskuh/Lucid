using System.Collections.Generic;
using Lucid.Core;
using Lucid.Netcode;
using Lucid.Tests.EditMode.Core;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Netcode
{
    /// <summary>
    /// docs/NETCODE.md §5: a client applying the host's events in order
    /// derives the host's hash after every one — CORE-API §11's sixth
    /// invariant, on the wire.
    /// </summary>
    public sealed class LatticeMirrorTests
    {
        static Round Host() => new Round(new RoundSettings(HeadStartMs: 0), TestLattice.Registry(), TestLattice.Start, Rotation.R0,
            new List<PlayerId> { new PlayerId(100) });

        static LatticeMirror Mirror(Round host) =>
            new LatticeMirror(new Codec(host.Registry), TestLattice.Start, Rotation.R0, host.Settings);

        /// <summary>Drives the host through a short round and returns each broadcast as it would leave the wire.</summary>
        static List<LatticeEventMsg> Broadcasts(Round host)
        {
            var codec = new Codec(host.Registry);
            var sent = new List<LatticeEventMsg>();
            void Place(Coord on, Face face, string type, Rotation rot)
            {
                PlaceVerdict v = host.TryPlace(new PlaceRequest(new ConnectorRef(on, face), type, rot, "*"));
                Assert.That(v.Ok, Is.True, $"{type} on {on}/{face}: {v}");
                sent.Add(codec.Encode(host.Log.Events[host.Log.Events.Count - 1], host.Derived.Hash));
            }
            void Explore(Coord cube)
            {
                host.UpdateSleeperCube(0, cube);
                Assert.That(host.TryExplore(0, cube), Is.EqualTo(ExploreError.None));
                sent.Add(codec.Encode(host.Log.Events[host.Log.Events.Count - 1], host.Derived.Hash));
            }
            Place(new Coord(0, 0, 0), Face.North, TestLattice.Tee, Rotation.R0);       // N,E,S → S meets the bedroom; N and E open
            Place(new Coord(0, 1, 0), Face.North, TestLattice.Straight, Rotation.R0);  // deeper: (0,2,0)
            Explore(new Coord(0, 1, 0));                                               // the T's east door is fog, hardens
            Place(new Coord(0, 2, 0), Face.North, TestLattice.Corner, Rotation.R180); // S,W at (0,3,0)
            return sent;
        }

        [Test]
        public void TheMirrorDerivesTheHostsHashAfterEveryEvent()
        {
            Round host = Host();
            LatticeMirror mirror = Mirror(host);
            foreach (LatticeEventMsg m in Broadcasts(host))
            {
                MirrorResult r = mirror.Apply(m);
                Assert.That(r.Applied, Is.True, r.ToString());
                Assert.That(r.InSync, Is.True, $"seq {m.Seq}: {r}");
            }
            Assert.That(mirror.Derived.Hash, Is.EqualTo(host.Derived.Hash));
            Assert.That(mirror.Lattice.Cubes.Count, Is.EqualTo(host.Lattice.Cubes.Count));
            Assert.That(mirror.Derived.Exits, Is.EqualTo(host.Derived.Exits));
            Assert.That(mirror.Log.NextSeq, Is.EqualTo(host.Log.NextSeq));
        }

        [Test]
        public void AGapIsRefusedAndNothingIsApplied()
        {
            Round host = Host();
            LatticeMirror mirror = Mirror(host);
            List<LatticeEventMsg> sent = Broadcasts(host);
            Assert.That(mirror.Apply(sent[0]).Applied, Is.True);
            MirrorResult gap = mirror.Apply(sent[2]);
            Assert.That(gap.Gap, Is.True);
            Assert.That(gap.Applied, Is.False);
            Assert.That(mirror.Log.NextSeq, Is.EqualTo(1), "a gap moved the log");
            Assert.That(mirror.Apply(sent[1]).InSync, Is.True, "the right one after a gap still applies");
        }

        [Test]
        public void ACorruptedHashIsADesyncAndAnUnknownTypeIsNot()
        {
            Round host = Host();
            LatticeMirror mirror = Mirror(host);
            List<LatticeEventMsg> sent = Broadcasts(host);
            LatticeEventMsg bad = sent[0];
            bad.PostHash ^= 1;
            MirrorResult r = mirror.Apply(bad);
            Assert.That(r.Applied, Is.True, "the event itself is fine and is applied");
            Assert.That(r.InSync, Is.False);
            Assert.That(r.Hash, Is.EqualTo(host.Log.Events.Count > 0 ? sent[0].PostHash : 0ul));

            LatticeEventMsg unknown = sent[1];
            unknown.TypeIndex = 9999;
            MirrorResult u = mirror.Apply(unknown);
            Assert.That(u.Unknown, Is.True);
            Assert.That(u.Applied, Is.False);
        }
    }
}
