using System;
using Lucid.Core;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// The one place the authoring vocabulary meets the rules engine. Kept
    /// explicit rather than relying on the two enums happening to share member
    /// names, because Core's <see cref="Face"/> order feeds the derived-state
    /// hash and must not be coupled to a file format.
    /// </summary>
    public static class CubeSpecMapping
    {
        public static Face ToFace(SpecFace f)
        {
            switch (f)
            {
                case SpecFace.North: return Face.North;
                case SpecFace.East: return Face.East;
                case SpecFace.South: return Face.South;
                case SpecFace.West: return Face.West;
                case SpecFace.Up: return Face.Up;
                case SpecFace.Down: return Face.Down;
                default: throw new ArgumentOutOfRangeException(nameof(f), f, "not a face");
            }
        }

        public static CubeCategory ToCategory(SpecCategory c)
        {
            switch (c)
            {
                case SpecCategory.Connector: return CubeCategory.Connector;
                case SpecCategory.Vertical: return CubeCategory.Vertical;
                case SpecCategory.Chicane: return CubeCategory.Chicane;
                case SpecCategory.Mob: return CubeCategory.Mob;
                case SpecCategory.Gimmick: return CubeCategory.Gimmick;
                case SpecCategory.Start: return CubeCategory.Start;
                default: throw new ArgumentOutOfRangeException(nameof(c), c, "not a category");
            }
        }

        /// <summary>The unrotated connector mask this spec declares.</summary>
        public static FaceMask ToMask(SpecFace[] connectors)
        {
            FaceMask mask = FaceMask.None;
            foreach (SpecFace f in connectors) mask |= Faces.ToMask(ToFace(f));
            return mask;
        }

        /// <summary>The rules' view of a spec, before anything is built.</summary>
        public static CubeType ToCubeType(CubeSpec spec) =>
            new CubeType(spec.Id, spec.Pack, ToCategory(spec.Category),
                ToMask(spec.Connectors), spec.Climbable, spec.Cost);
    }
}
