using System;
using Lucid.Core;
using Lucid.Netcode;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;

namespace Lucid.Tests.EditMode.Netcode
{
    /// <summary>docs/NETCODE.md §11: fixed layouts that survive the wire byte for byte, and the sizes it promises.</summary>
    public sealed class WireTests
    {
        static T RoundTrip<T>(T value, out int bytes) where T : struct, INetworkSerializable
        {
            using (var w = new FastBufferWriter(256, Allocator.Temp))
            {
                w.WriteNetworkSerializable(in value);
                bytes = w.Length;
                using (var r = new FastBufferReader(w, Allocator.Temp))
                {
                    r.ReadNetworkSerializable(out T back);
                    return back;
                }
            }
        }

        [Test]
        public void ACoordIsThreeSignedBytesAndRefusesMore()
        {
            WireCoord c = WireCoord.From(new Coord(-48, 3, -3));
            Assert.That(RoundTrip(c, out int bytes).ToCoord(), Is.EqualTo(new Coord(-48, 3, -3)));
            Assert.That(bytes, Is.EqualTo(3));
            Assert.That(() => WireCoord.From(new Coord(128, 0, 0)), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => WireCoord.From(new Coord(0, -129, 0)), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TelemetryIsSixteenBytes()
        {
            var t = new TelemetryMsg { Seq = 65535, Cube = WireCoord.From(new Coord(1, 2, -1)), LocalX = 1024, LocalY = 3, LocalZ = 2047, Yaw = 200, Health = 100, Lives = 2, Status = 1, Flags = 7 };
            TelemetryMsg back = RoundTrip(t, out int bytes);
            Assert.That(bytes, Is.EqualTo(16), "docs/NETCODE.md §6: 16 bytes");
            Assert.That(back.Seq, Is.EqualTo(t.Seq)); Assert.That(back.Cube, Is.EqualTo(t.Cube));
            Assert.That((back.LocalX, back.LocalY, back.LocalZ), Is.EqualTo((t.LocalX, t.LocalY, t.LocalZ)));
            Assert.That((back.Yaw, back.Health, back.Lives, back.Status, back.Flags), Is.EqualTo((t.Yaw, t.Health, t.Lives, t.Status, t.Flags)));
        }

        [Test]
        public void EveryReliableShapeSurvivesTheWire()
        {
            var start = new RoundStartMsg { HeadStartMs = 1, RoundLengthMs = 2, Lives = 3, StartingBudget = 4, TrickleIntervalMs = 5, ExitHysteresis = 6, Seed = 7, NightmareClientId = 8, SleeperCount = 2, StartTypeIndex = 9, StartRotation = 2, RegistryHash = 0xDEADBEEFUL, RoundStartServerTime = 12.5 };
            start.SetSleeper(0, 10); start.SetSleeper(1, 11);
            RoundStartMsg s2 = RoundTrip(start, out _);
            Assert.That(s2.Settings, Is.EqualTo(new RoundSettings(1, 2, 3, 4, 5, 6)));
            Assert.That((s2.Seed, s2.NightmareClientId, s2.SleeperCount, s2.SleeperClient(0), s2.SleeperClient(1)), Is.EqualTo((7u, 8ul, (byte)2, 10ul, 11ul)));
            Assert.That((s2.StartTypeIndex, s2.StartRotation, s2.RegistryHash, s2.RoundStartServerTime), Is.EqualTo(((ushort)9, (byte)2, 0xDEADBEEFUL, 12.5)));

            var ev = new LatticeEventMsg { Seq = 3, Kind = (byte)EventKind.Placed, Cube = WireCoord.From(new Coord(0, 1, 0)), TypeIndex = 2, Rotation = 1, SkinIndex = 0, SleeperId = 0, PostHash = 0x1234 };
            LatticeEventMsg e2 = RoundTrip(ev, out _);
            Assert.That((e2.Seq, e2.Kind, e2.Cube, e2.TypeIndex, e2.Rotation, e2.PostHash), Is.EqualTo((3u, (byte)1, ev.Cube, (ushort)2, (byte)1, 0x1234ul)));

            Assert.That(RoundTrip(new HashReportMsg { Seq = 5, Hash = 6 }, out _).Hash, Is.EqualTo(6ul));
            Assert.That(RoundTrip(new DesyncNoticeMsg { Seq = 5, ClientId = 9 }, out _).ClientId, Is.EqualTo(9ul));
            Assert.That(RoundTrip(new PhaseChangedMsg { Phase = 2, AtServerTime = 3.5 }, out _).AtServerTime, Is.EqualTo(3.5));
            SleeperStatusMsg st = RoundTrip(new SleeperStatusMsg { DreamId = 1, Status = 2, LivesLeft = 0, AtClockMs = 77, Cause = 1 }, out _);
            Assert.That((st.DreamId, st.Status, st.AtClockMs), Is.EqualTo(((sbyte)1, (byte)2, 77u)));
            Assert.That(RoundTrip(new DreamReadyMsg { DreamId = 2 }, out _).DreamId, Is.EqualTo((sbyte)2));
            Assert.That(RoundTrip(new ExploredMsg { Cube = ev.Cube }, out _).Cube, Is.EqualTo(ev.Cube));
            TouchedExitMsg te = RoundTrip(new TouchedExitMsg { Cube = ev.Cube, Face = 3 }, out _);
            Assert.That((te.Cube, te.Face), Is.EqualTo((ev.Cube, (byte)3)));
            WakeVerdictMsg wv = RoundTrip(new WakeVerdictMsg { Accepted = false, Reason = 1 }, out _);
            Assert.That((wv.Accepted, wv.Reason), Is.EqualTo((false, (byte)1)));
            PlaceRequestMsg pr = RoundTrip(new PlaceRequestMsg { ReqId = 40, TargetCube = ev.Cube, TargetFace = 1, TypeIndex = 2, Rotation = 3, SkinIndex = 0 }, out _);
            Assert.That((pr.ReqId, pr.TargetFace, pr.TypeIndex, pr.Rotation), Is.EqualTo(((ushort)40, (byte)1, (ushort)2, (byte)3)));
            PlaceReplyMsg rp = RoundTrip(new PlaceReplyMsg { ReqId = 40, Verdict = 4, TrappedDreamId = 2 }, out _);
            Assert.That((rp.ReqId, rp.Verdict, rp.TrappedDreamId), Is.EqualTo(((ushort)40, (byte)4, (sbyte)2)));
            BudgetStateMsg bs = RoundTrip(new BudgetStateMsg { Points = 11, MsUntilNextPoint = 1234, CooldownCount = 0, PossessionActive = true }, out _);
            Assert.That((bs.Points, bs.MsUntilNextPoint, bs.PossessionActive), Is.EqualTo((11, 1234, true)));
        }
    }
}
