namespace Lucid.Core
{
    /// <summary>
    /// FNV-1a 64. Used for every content hash Core produces, because two
    /// machines must agree bit for bit (docs/CORE-API.md §3). Integers are fed
    /// in little-endian byte order and strings as UTF-16 code units, so the
    /// result never depends on the platform.
    /// </summary>
    public static class Fnv
    {
        public const ulong Basis = 14695981039346656037UL;
        public const ulong Prime = 1099511628211UL;

        public static ulong Byte(ulong hash, byte b)
        {
            unchecked
            {
                hash ^= b;
                hash *= Prime;
                return hash;
            }
        }

        public static ulong Int32(ulong hash, int value)
        {
            unchecked
            {
                var v = (uint)value;
                hash = Byte(hash, (byte)(v & 0xFF));
                hash = Byte(hash, (byte)((v >> 8) & 0xFF));
                hash = Byte(hash, (byte)((v >> 16) & 0xFF));
                hash = Byte(hash, (byte)((v >> 24) & 0xFF));
                return hash;
            }
        }

        public static ulong String(ulong hash, string s)
        {
            if (s == null) return Byte(hash, 0);
            foreach (char c in s)
            {
                hash = Byte(hash, (byte)(c & 0xFF));
                hash = Byte(hash, (byte)((c >> 8) & 0xFF));
            }
            return Byte(hash, 0);   // terminator, so "ab"+"c" and "a"+"bc" differ
        }

        public static ulong Coord(ulong hash, Coord c)
        {
            hash = Int32(hash, c.X);
            hash = Int32(hash, c.Y);
            hash = Int32(hash, c.Z);
            return hash;
        }
    }
}
