using System;
using System.Collections.Generic;
using Lucid.Core;

namespace Lucid.Netcode
{
    /// <summary>
    /// Core's records to §11's wire shapes and back. Type ids become their
    /// index in the registry's order, which contentHash guarantees is the
    /// same on every machine; a skin index of zero is the pack's default
    /// ("*"), the only skin M0 has.
    /// </summary>
    public sealed class Codec
    {
        public const ushort DefaultSkin = 0;
        public const string DefaultSkinId = "*";

        readonly CubeRegistry _registry;
        readonly Dictionary<string, ushort> _index = new Dictionary<string, ushort>(StringComparer.Ordinal);

        public Codec(CubeRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            for (int i = 0; i < registry.All.Count; i++) _index[registry.All[i].Id] = checked((ushort)i);
        }

        public CubeRegistry Registry => _registry;

        public ushort TypeIndex(string typeId)
        {
            if (typeId != null && _index.TryGetValue(typeId, out ushort i)) return i;
            throw new ArgumentException($"'{typeId}' is not in the registry", nameof(typeId));
        }

        public bool TryTypeId(ushort index, out string typeId)
        {
            if (index < _registry.All.Count) { typeId = _registry.All[index].Id; return true; }
            typeId = null;
            return false;
        }

        public static ushort SkinIndex(string skinId) =>
            skinId == null || skinId == DefaultSkinId ? DefaultSkin
                : throw new ArgumentException($"skin '{skinId}': skins arrive with M1", nameof(skinId));

        public static string SkinId(ushort index) =>
            index == DefaultSkin ? DefaultSkinId : throw new ArgumentException($"skin index {index}: skins arrive with M1", nameof(index));

        public LatticeEventMsg Encode(LatticeEvent e, ulong postHash)
        {
            switch (e)
            {
                case CubePlaced p:
                    return new LatticeEventMsg
                    {
                        Seq = checked((uint)p.Seq), Kind = (byte)EventKind.Placed, Cube = WireCoord.From(p.Cube),
                        TypeIndex = TypeIndex(p.TypeId), Rotation = (byte)p.Rotation, SkinIndex = SkinIndex(p.SkinId), PostHash = postHash,
                    };
                case CubeExplored x:
                    return new LatticeEventMsg
                    {
                        Seq = checked((uint)x.Seq), Kind = (byte)EventKind.Explored, Cube = WireCoord.From(x.Cube),
                        SleeperId = checked((byte)x.SleeperId), PostHash = postHash,
                    };
                default:
                    throw new ArgumentException($"cannot encode {e?.GetType().Name ?? "null"}", nameof(e));
            }
        }

        /// <summary>Null when the message names a type the registry has not got — a guard, since contentHash makes it impossible.</summary>
        public LatticeEvent Decode(LatticeEventMsg m)
        {
            switch ((EventKind)m.Kind)
            {
                case EventKind.Placed:
                    if (!TryTypeId(m.TypeIndex, out string typeId)) return null;
                    return new CubePlaced(m.Seq, m.Cube.ToCoord(), typeId, (Rotation)m.Rotation, SkinId(m.SkinIndex));
                case EventKind.Explored:
                    return new CubeExplored(m.Seq, m.Cube.ToCoord(), m.SleeperId);
                default:
                    return null;
            }
        }

        public PlaceRequestMsg Encode(PlaceRequest r, ushort reqId) => new PlaceRequestMsg
        {
            ReqId = reqId, TargetCube = WireCoord.From(r.Target.Cube), TargetFace = (byte)r.Target.Face,
            TypeIndex = TypeIndex(r.TypeId), Rotation = (byte)r.Rotation, SkinIndex = SkinIndex(r.SkinId),
        };

        /// <summary>Null for an unknown type index, which the host answers with <see cref="PlaceError.UnknownType"/> (§14).</summary>
        public PlaceRequest Decode(PlaceRequestMsg m)
        {
            if (!TryTypeId(m.TypeIndex, out string typeId)) return null;
            return new PlaceRequest(new ConnectorRef(m.TargetCube.ToCoord(), (Face)m.TargetFace), typeId, (Rotation)m.Rotation, SkinId(m.SkinIndex));
        }
    }
}
