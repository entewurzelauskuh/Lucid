using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Lucid.Core;

namespace Lucid.Runtime
{
    /// <summary>
    /// A .lucidlog (docs/CORE-API.md §7): a header the runtime owns — format
    /// version, the round's settings, its players, the registry's ids in
    /// order — followed by Core's own event encoding. Saved by the host on a
    /// desync (docs/NETCODE.md §5, §14) and read by M0.9c's report.
    /// </summary>
    public static class LucidLog
    {
        static readonly byte[] Magic = { (byte)'L', (byte)'U', (byte)'C', (byte)'L' };
        const ushort Version = 1;

        public sealed record Player(int Id, string Name);

        public sealed record Contents(RoundSettings Settings, IReadOnlyList<Player> Players, IReadOnlyList<string> RegistryIds, EventLog Events, string Note);

        public static void Write(Stream s, RoundSettings settings, IReadOnlyList<Player> players, CubeRegistry registry, EventLog events, string note)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            if (events == null) throw new ArgumentNullException(nameof(events));

            using (var w = new BinaryWriter(s, Encoding.UTF8, leaveOpen: true))
            {
                w.Write(Magic, 0, Magic.Length);
                w.Write(Version);
                w.Write(note ?? string.Empty);
                w.Write(settings.HeadStartMs); w.Write(settings.RoundLengthMs); w.Write(settings.Lives);
                w.Write(settings.StartingBudget); w.Write(settings.TrickleIntervalMs); w.Write(settings.ExitHysteresis);
                w.Write(players?.Count ?? 0);
                if (players != null) foreach (Player p in players) { w.Write(p.Id); w.Write(p.Name ?? string.Empty); }
                w.Write(registry.All.Count);
                foreach (CubeType t in registry.All) w.Write(t.Id);
            }
            events.Write(s);
        }

        /// <summary>A count from the file is data: negative or absurd is a bad file, not a bad allocation.</summary>
        static int Count(BinaryReader r, string what)
        {
            int n = r.ReadInt32();
            if (n < 0 || n > 1 << 16) throw new InvalidDataException($".lucidlog: {n} {what}");
            return n;
        }

        public static Contents Read(Stream s)
        {
            using (var r = new BinaryReader(s, Encoding.UTF8, leaveOpen: true))
            {
                byte[] magic = r.ReadBytes(Magic.Length);
                if (magic.Length != Magic.Length) throw new InvalidDataException("not a .lucidlog");
                for (int i = 0; i < Magic.Length; i++) if (magic[i] != Magic[i]) throw new InvalidDataException("not a .lucidlog");
                ushort version = r.ReadUInt16();
                if (version != Version) throw new InvalidDataException($".lucidlog version {version}, expected {Version}");
                string note = r.ReadString();
                var settings = new RoundSettings(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32());
                int n = Count(r, "players");
                var players = new List<Player>(n);
                for (int i = 0; i < n; i++) players.Add(new Player(r.ReadInt32(), r.ReadString()));
                int m = Count(r, "registry ids");
                var ids = new List<string>(m);
                for (int i = 0; i < m; i++) ids.Add(r.ReadString());
                EventLog events = EventLog.Read(s);
                return new Contents(settings, players, ids, events, note);
            }
        }
    }
}
