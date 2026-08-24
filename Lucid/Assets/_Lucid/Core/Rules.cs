using System.Collections.Generic;

namespace Lucid.Core
{
    /// <summary>
    /// The rules of the dream. This file holds the state transitions; the
    /// validation half — fit, frontier, leak and the exploration checks —
    /// arrives with docs/CORE-API.md §5 and §6.
    /// </summary>
    /// <remarks>
    /// Apply assumes a passing verdict and never consults the budget: the host
    /// spends it, so validation stays free of side effects.
    /// </remarks>
    public static partial class Rules
    {
        /// <summary>Build on a door. The new cube lands through that face.</summary>
        public static (Lattice lattice, Derived derived) ApplyPlace(
            RuleContext ctx, PlaceRequest req, long seq) =>
            PlaceAt(ctx, req.Target.Cube.Offset(req.Target.Face),
                    req.TypeId, req.Rotation, req.SkinId, seq);

        /// <summary>
        /// Place by coord. Replay works from <see cref="CubePlaced"/>, which
        /// records where the cube went rather than which door it covered.
        /// </summary>
        internal static (Lattice lattice, Derived derived) PlaceAt(
            RuleContext ctx, Coord at, string typeId, Rotation rotation, string skinId, long seq)
        {
            Lattice next = ctx.Lattice.WithCube(at, new CubeInstance(typeId, rotation, skinId, seq));
            return (next, Rederive(ctx, next));
        }

        public static (Lattice lattice, Derived derived) ApplyExplore(
            RuleContext ctx, Coord cube, long seq)
        {
            // Only Fog doors condense into wall. Exit doors never solidify and
            // Attached ones are already passages (docs/SPEC.md §7).
            var solidify = new List<ConnectorRef>();
            foreach (Face f in Faces.Of(ctx.Lattice.ConnectorsAt(cube, ctx.Registry)))
            {
                var door = new ConnectorRef(cube, f);
                if (ctx.Derived.StateOf(door) == ConnectorState.Fog) solidify.Add(door);
            }

            Lattice next = ctx.Lattice.WithExplored(cube, solidify);
            return (next, Rederive(ctx, next));
        }

        /// <summary>
        /// Carries the previous exit depth forward, which is what lets
        /// hysteresis compare against where the light actually was.
        /// </summary>
        static Derived Rederive(RuleContext ctx, Lattice next) =>
            Deriver.Derive(
                next,
                ctx.Registry,
                ctx.Settings?.ExitHysteresis ?? 0,
                ctx.Derived?.ExitDepth ?? -1,
                ctx.Derived?.Exits);
    }
}
