using System;
using System.IO;
using System.Text;

namespace Lucid.Netcode
{
    /// <summary>001: the approval payload (docs/NETCODE.md §2). The one message with strings.</summary>
    public sealed record Hello(string BuildId, ulong ContentHash, ulong SteamId, string DisplayName, ushort ProtocolVersion)
    {
        public const ushort CurrentProtocol = 1;

        public byte[] ToBytes()
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms, Encoding.UTF8))
            {
                w.Write(ProtocolVersion);
                w.Write(BuildId ?? string.Empty);
                w.Write(ContentHash);
                w.Write(SteamId);
                w.Write(DisplayName ?? string.Empty);
                w.Flush();
                return ms.ToArray();
            }
        }

        /// <summary>Null for bytes that are not a Hello at all — an old build, or not Lucid.</summary>
        public static Hello FromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 2 + 1 + 8 + 8 + 1) return null;
            try
            {
                using (var r = new BinaryReader(new MemoryStream(bytes), Encoding.UTF8))
                {
                    ushort protocol = r.ReadUInt16();
                    string build = r.ReadString();
                    ulong content = r.ReadUInt64();
                    ulong steam = r.ReadUInt64();
                    string name = r.ReadString();
                    return new Hello(build, content, steam, name, protocol);
                }
            }
            catch (Exception e) when (e is IOException || e is FormatException || e is ArgumentException)
            {
                return null;
            }
        }
    }
}
