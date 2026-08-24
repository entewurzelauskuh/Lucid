using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lucid.Core
{
    /// <summary>
    /// The ordered record of everything that changed the lattice. Replaying it
    /// must reproduce the host's <see cref="Derived.Hash"/> exactly; that is
    /// what the netcode's per-event sync check rests on (docs/NETCODE.md §5),
    /// and what makes a .lucidlog a faithful bug report.
    /// </summary>
    public sealed class EventLog
    {
        // Written and read as four bytes, so the header reads "LUCE" in a hex
        // dump regardless of the platform's endianness. Writing it as a uint
        // put "ECUL" on disk.
        static readonly byte[] Magic = { (byte)'L', (byte)'U', (byte)'C', (byte)'E' };
        const ushort Version = 1;

        readonly List<LatticeEvent> _events = new List<LatticeEvent>();

        public IReadOnlyList<LatticeEvent> Events => _events;

        public long NextSeq => _events.Count;

        public void Append(LatticeEvent e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (e.Seq != NextSeq)
                throw new ArgumentException($"expected seq {NextSeq}, got {e.Seq}", nameof(e));
            _events.Add(e);
        }

        /// <summary>
        /// Rebuild the lattice from nothing. No validation and no budget: the
        /// events already happened, and re-judging them could only disagree
        /// with the host that accepted them.
        /// </summary>
        public static (Lattice lattice, Derived derived) Replay(
            EventLog log, CubeRegistry reg, string startTypeId, Rotation startRotation, RoundSettings s)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));

            RoundSettings settings = s ?? new RoundSettings();
            Lattice lattice = Lattice.New(reg, startTypeId, startRotation);
            Derived derived = Deriver.Derive(lattice, reg, settings.ExitHysteresis);

            foreach (LatticeEvent e in log.Events)
            {
                var ctx = new RuleContext(lattice, derived, reg, Array.Empty<SleeperState>(), null, settings);
                switch (e)
                {
                    case CubePlaced p:
                        (lattice, derived) = Rules.PlaceAt(ctx, p.Cube, p.TypeId, p.Rotation, p.SkinId, p.Seq);
                        break;
                    case CubeExplored x:
                        (lattice, derived) = Rules.ApplyExplore(ctx, x.Cube, x.Seq);
                        break;
                    default:
                        throw new InvalidDataException($"unknown event kind {e.GetType().Name}");
                }
            }

            return (lattice, derived);
        }

        /// <summary>
        /// Compact binary, little-endian. The runtime wraps this in a .lucidlog
        /// header and optional telemetry; Core owns only the event stream.
        /// </summary>
        public void Write(Stream s)
        {
            using (var w = new BinaryWriter(s, Encoding.UTF8, leaveOpen: true))
            {
                w.Write(Magic, 0, Magic.Length);
                w.Write(Version);
                w.Write(_events.Count);

                foreach (LatticeEvent e in _events)
                {
                    switch (e)
                    {
                        case CubePlaced p:
                            w.Write((byte)1);
                            w.Write(p.Seq);
                            WriteCoord(w, p.Cube);
                            w.Write(p.TypeId ?? string.Empty);
                            w.Write((byte)p.Rotation);
                            w.Write(p.SkinId ?? string.Empty);
                            break;
                        case CubeExplored x:
                            w.Write((byte)2);
                            w.Write(x.Seq);
                            WriteCoord(w, x.Cube);
                            w.Write(x.SleeperId);
                            break;
                        default:
                            throw new InvalidDataException($"cannot write event kind {e.GetType().Name}");
                    }
                }
            }
        }

        public static EventLog Read(Stream s)
        {
            var log = new EventLog();
            using (var r = new BinaryReader(s, Encoding.UTF8, leaveOpen: true))
            {
byte[] magic = r.ReadBytes(Magic.Length);
                if (magic.Length != Magic.Length) throw new InvalidDataException("not a Lucid event log");
                for (int i = 0; i < Magic.Length; i++)
                {
                    if (magic[i] != Magic[i]) throw new InvalidDataException("not a Lucid event log");
                }

                ushort version = r.ReadUInt16();
                if (version != Version)
                    throw new InvalidDataException($"event log version {version}, expected {Version}");

                int count = r.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    byte kind = r.ReadByte();
                    long seq = r.ReadInt64();
                    Coord cube = ReadCoord(r);
                    switch (kind)
                    {
                        case 1:
                        {
                            string typeId = r.ReadString();
                            var rotation = (Rotation)r.ReadByte();
                            string skinId = r.ReadString();
                            log.Append(new CubePlaced(seq, cube, typeId,
                                rotation, skinId.Length == 0 ? null : skinId));
                            break;
                        }
                        case 2:
                            log.Append(new CubeExplored(seq, cube, r.ReadInt32()));
                            break;
                        default:
                            throw new InvalidDataException($"unknown event kind byte {kind}");
                    }
                }
            }
            return log;
        }

        static void WriteCoord(BinaryWriter w, Coord c)
        {
            w.Write(c.X);
            w.Write(c.Y);
            w.Write(c.Z);
        }

        static Coord ReadCoord(BinaryReader r) => new Coord(r.ReadInt32(), r.ReadInt32(), r.ReadInt32());
    }
}
