using System;
using Lucid.Core;
using Unity.Netcode;

namespace Lucid.Netcode
{
    /// <summary>
    /// docs/NETCODE.md §11: fixed layouts, no strings after Hello and
    /// RoundStart, registry indices for types and skins, three int8 for a
    /// coord. Every struct here is one row of §12's table.
    /// </summary>
    public struct WireCoord : INetworkSerializable, IEquatable<WireCoord>
    {
        public sbyte X, Y, Z;

        /// <summary>Footprint half ≤ 48, layers ±3 (§5): anything outside is a bug, not a coord.</summary>
        public static WireCoord From(Coord c)
        {
            if (c.X < sbyte.MinValue || c.X > sbyte.MaxValue || c.Y < sbyte.MinValue || c.Y > sbyte.MaxValue
                || c.Z < sbyte.MinValue || c.Z > sbyte.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(c), c, "a coord on the wire is three int8");
            return new WireCoord { X = (sbyte)c.X, Y = (sbyte)c.Y, Z = (sbyte)c.Z };
        }

        public Coord ToCoord() => new Coord(X, Y, Z);

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref X); s.SerializeValue(ref Y); s.SerializeValue(ref Z);
        }

        public bool Equals(WireCoord o) => X == o.X && Y == o.Y && Z == o.Z;
        public override bool Equals(object obj) => obj is WireCoord o && Equals(o);
        public override int GetHashCode() => (X * 397 ^ Y) * 397 ^ Z;
        public override string ToString() => $"({X},{Y},{Z})";
    }

    /// <summary>101. The settings snapshot is RoundSettings' six knobs; Limits stay the default until a lobby can change them.</summary>
    public struct RoundStartMsg : INetworkSerializable
    {
        public int HeadStartMs, RoundLengthMs, Lives, StartingBudget, TrickleIntervalMs, ExitHysteresis;
        public uint Seed;
        public ulong NightmareClientId;
        public byte SleeperCount;
        public ulong Sleeper0, Sleeper1, Sleeper2, Sleeper3;   // client id by dreamId
        public ushort StartTypeIndex;
        public byte StartRotation;
        public ulong RegistryHash;
        public double RoundStartServerTime;

        public RoundSettings Settings => new RoundSettings(HeadStartMs, RoundLengthMs, Lives, StartingBudget, TrickleIntervalMs, ExitHysteresis);

        public ulong SleeperClient(int dreamId)
        {
            switch (dreamId) { case 0: return Sleeper0; case 1: return Sleeper1; case 2: return Sleeper2; case 3: return Sleeper3; }
            throw new ArgumentOutOfRangeException(nameof(dreamId));
        }

        public void SetSleeper(int dreamId, ulong clientId)
        {
            switch (dreamId) { case 0: Sleeper0 = clientId; break; case 1: Sleeper1 = clientId; break; case 2: Sleeper2 = clientId; break; case 3: Sleeper3 = clientId; break; default: throw new ArgumentOutOfRangeException(nameof(dreamId)); }
        }

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref HeadStartMs); s.SerializeValue(ref RoundLengthMs); s.SerializeValue(ref Lives);
            s.SerializeValue(ref StartingBudget); s.SerializeValue(ref TrickleIntervalMs); s.SerializeValue(ref ExitHysteresis);
            s.SerializeValue(ref Seed); s.SerializeValue(ref NightmareClientId); s.SerializeValue(ref SleeperCount);
            s.SerializeValue(ref Sleeper0); s.SerializeValue(ref Sleeper1); s.SerializeValue(ref Sleeper2); s.SerializeValue(ref Sleeper3);
            s.SerializeValue(ref StartTypeIndex); s.SerializeValue(ref StartRotation); s.SerializeValue(ref RegistryHash);
            s.SerializeValue(ref RoundStartServerTime);
        }
    }

    /// <summary>102.</summary>
    public struct PhaseChangedMsg : INetworkSerializable
    {
        public byte Phase;
        public double AtServerTime;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter { s.SerializeValue(ref Phase); s.SerializeValue(ref AtServerTime); }
    }

    /// <summary>103.</summary>
    public struct SleeperStatusMsg : INetworkSerializable
    {
        public sbyte DreamId;
        public byte Status, LivesLeft, Cause;
        public uint AtClockMs;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref DreamId); s.SerializeValue(ref Status); s.SerializeValue(ref LivesLeft); s.SerializeValue(ref AtClockMs); s.SerializeValue(ref Cause);
        }
    }

    public enum EventKind : byte { Placed = 1, Explored = 2 }

    /// <summary>201. One shape for both kinds; the fields the other kind does not use are zero.</summary>
    public struct LatticeEventMsg : INetworkSerializable
    {
        public uint Seq;
        public byte Kind;
        public WireCoord Cube;
        public ushort TypeIndex;
        public byte Rotation;
        public ushort SkinIndex;
        public byte SleeperId;
        public ulong PostHash;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref Seq); s.SerializeValue(ref Kind); s.SerializeValue(ref Cube); s.SerializeValue(ref TypeIndex);
            s.SerializeValue(ref Rotation); s.SerializeValue(ref SkinIndex); s.SerializeValue(ref SleeperId); s.SerializeValue(ref PostHash);
        }
    }

    /// <summary>202.</summary>
    public struct HashReportMsg : INetworkSerializable
    {
        public uint Seq; public ulong Hash;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter { s.SerializeValue(ref Seq); s.SerializeValue(ref Hash); }
    }

    /// <summary>203.</summary>
    public struct DesyncNoticeMsg : INetworkSerializable
    {
        public uint Seq; public ulong ClientId;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter { s.SerializeValue(ref Seq); s.SerializeValue(ref ClientId); }
    }

    /// <summary>301.</summary>
    public struct DreamReadyMsg : INetworkSerializable
    {
        public sbyte DreamId;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter { s.SerializeValue(ref DreamId); }
    }

    /// <summary>302: sixteen bytes, unreliable, sequenced by <see cref="Seq"/>.</summary>
    public struct TelemetryMsg : INetworkSerializable
    {
        public ushort Seq;
        public WireCoord Cube;
        public ushort LocalX, LocalY, LocalZ;   // 1/256 m inside the cube
        public byte Yaw, Health, Lives, Status, Flags;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref Seq); s.SerializeValue(ref Cube); s.SerializeValue(ref LocalX); s.SerializeValue(ref LocalY); s.SerializeValue(ref LocalZ);
            s.SerializeValue(ref Yaw); s.SerializeValue(ref Health); s.SerializeValue(ref Lives); s.SerializeValue(ref Status); s.SerializeValue(ref Flags);
        }
    }

    /// <summary>303.</summary>
    public struct ExploredMsg : INetworkSerializable
    {
        public WireCoord Cube;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter { s.SerializeValue(ref Cube); }
    }

    /// <summary>304.</summary>
    public struct TouchedExitMsg : INetworkSerializable
    {
        public WireCoord Cube; public byte Face;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter { s.SerializeValue(ref Cube); s.SerializeValue(ref Face); }
    }

    /// <summary>311.</summary>
    public struct WakeVerdictMsg : INetworkSerializable
    {
        public bool Accepted; public byte Reason;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter { s.SerializeValue(ref Accepted); s.SerializeValue(ref Reason); }
    }

    /// <summary>401.</summary>
    public struct PlaceRequestMsg : INetworkSerializable
    {
        public ushort ReqId;
        public WireCoord TargetCube;
        public byte TargetFace;
        public ushort TypeIndex;
        public byte Rotation;
        public ushort SkinIndex;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref ReqId); s.SerializeValue(ref TargetCube); s.SerializeValue(ref TargetFace);
            s.SerializeValue(ref TypeIndex); s.SerializeValue(ref Rotation); s.SerializeValue(ref SkinIndex);
        }
    }

    /// <summary>402.</summary>
    public struct PlaceReplyMsg : INetworkSerializable
    {
        public ushort ReqId; public byte Verdict; public sbyte TrappedDreamId;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter { s.SerializeValue(ref ReqId); s.SerializeValue(ref Verdict); s.SerializeValue(ref TrappedDreamId); }
    }

    /// <summary>408. Effect cooldowns arrive with the powers (M1.9); the count is carried so the layout does not change then.</summary>
    public struct BudgetStateMsg : INetworkSerializable
    {
        public int Points, MsUntilNextPoint;
        public byte CooldownCount;
        public bool PossessionActive;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref Points); s.SerializeValue(ref MsUntilNextPoint); s.SerializeValue(ref CooldownCount); s.SerializeValue(ref PossessionActive);
        }
    }
}
