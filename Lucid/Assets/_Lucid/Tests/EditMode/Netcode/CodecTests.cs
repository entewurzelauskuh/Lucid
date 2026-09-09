using System;
using Lucid.Core;
using Lucid.Netcode;
using Lucid.Tests.EditMode.Core;
using NUnit.Framework;

namespace Lucid.Tests.EditMode.Netcode
{
    /// <summary>Core's records to §11's shapes and back, by registry order.</summary>
    public sealed class CodecTests
    {
        static Codec NewCodec() => new Codec(TestLattice.Registry());

        [Test]
        public void TypeIndicesAreTheRegistrysOrder()
        {
            Codec c = NewCodec();
            CubeRegistry reg = c.Registry;
            for (int i = 0; i < reg.All.Count; i++)
            {
                Assert.That(c.TypeIndex(reg.All[i].Id), Is.EqualTo((ushort)i));
                Assert.That(c.TryTypeId((ushort)i, out string id) && id == reg.All[i].Id, Is.True);
            }
            Assert.That(c.TryTypeId((ushort)reg.All.Count, out _), Is.False);
            Assert.That(() => c.TypeIndex("core.nothing"), Throws.ArgumentException);
        }

        [Test]
        public void BothEventKindsRoundTrip()
        {
            Codec c = NewCodec();
            var placed = new CubePlaced(4, new Coord(0, 1, 0), TestLattice.Corner, Rotation.R270, "*");
            LatticeEventMsg m = c.Encode(placed, 99);
            Assert.That((m.Seq, (EventKind)m.Kind, m.PostHash), Is.EqualTo((4u, EventKind.Placed, 99ul)));
            Assert.That(c.Decode(m), Is.EqualTo(placed));

            var explored = new CubeExplored(5, new Coord(0, 1, 0), 3);
            Assert.That(c.Decode(c.Encode(explored, 1)), Is.EqualTo(explored));
        }

        [Test]
        public void AnUnknownTypeIndexDecodesToNothing()
        {
            Codec c = NewCodec();
            var m = new LatticeEventMsg { Seq = 0, Kind = (byte)EventKind.Placed, TypeIndex = 9999 };
            Assert.That(c.Decode(m), Is.Null);
            Assert.That(c.Decode(new PlaceRequestMsg { TypeIndex = 9999 }), Is.Null, "docs/NETCODE.md §14: UnknownType, not a throw");
            Assert.That(c.Decode(new PlaceRequestMsg { TypeIndex = 0, SkinIndex = 1 }), Is.Null, "a skin M0 has not got is unknown, not a throw");
            Assert.That(c.Decode(new LatticeEventMsg { Kind = (byte)EventKind.Placed, TypeIndex = 0, SkinIndex = 7 }), Is.Null);
            Assert.That(c.Decode(new LatticeEventMsg { Kind = 7 }), Is.Null);
        }

        [Test]
        public void APlaceRequestRoundTripsAndOnlyTheDefaultSkinExists()
        {
            Codec c = NewCodec();
            var r = new PlaceRequest(new ConnectorRef(new Coord(2, -1, 1), Face.West), TestLattice.Tee, Rotation.R90, "*");
            PlaceRequestMsg m = c.Encode(r, 7);
            Assert.That(m.ReqId, Is.EqualTo((ushort)7));
            Assert.That(c.Decode(m), Is.EqualTo(r));
            Assert.That(() => Codec.SkinIndex("wood"), Throws.ArgumentException, "skins are M1's");
            Assert.That(() => Codec.SkinId(1), Throws.ArgumentException);
        }
    }
}
