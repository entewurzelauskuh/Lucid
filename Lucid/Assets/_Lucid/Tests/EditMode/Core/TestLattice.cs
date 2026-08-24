using Lucid.Core;

namespace Lucid.Tests.EditMode.Core
{
    /// <summary>
    /// Shared fixtures. Ids mirror the MVP cube set in docs/SPEC.md §8 so the
    /// tests read like the design rather than like scaffolding.
    /// </summary>
    internal static class TestLattice
    {
        public const string Start = "core.start";
        public const string Straight = "core.straight";   // North-South corridor
        public const string Corner = "core.corner";       // North-East
        public const string Tee = "core.tee";             // North-East-South
        public const string Cross = "core.cross";         // all four horizontals
        public const string Drop = "core.drop";           // North plus Down, not climbable
        public const string Ladder = "core.ladder";       // North plus Up, climbable

        public static CubeRegistry Registry()
        {
            var reg = new CubeRegistry();
            reg.Register(new CubeType(Start, "core", CubeCategory.Start, FaceMask.North, false, 0));
            reg.Register(new CubeType(Straight, "core", CubeCategory.Connector,
                FaceMask.North | FaceMask.South, false, 1));
            reg.Register(new CubeType(Corner, "core", CubeCategory.Connector,
                FaceMask.North | FaceMask.East, false, 1));
            reg.Register(new CubeType(Tee, "core", CubeCategory.Connector,
                FaceMask.North | FaceMask.East | FaceMask.South, false, 2));
            reg.Register(new CubeType(Cross, "core", CubeCategory.Connector,
                FaceMask.North | FaceMask.East | FaceMask.South | FaceMask.West, false, 2));
            reg.Register(new CubeType(Drop, "core", CubeCategory.Vertical,
                FaceMask.North | FaceMask.Down, false, 1));
            reg.Register(new CubeType(Ladder, "core", CubeCategory.Vertical,
                FaceMask.North | FaceMask.Up, true, 1));
            return reg;
        }

        public static RuleContext Context(Lattice l, Derived d, CubeRegistry reg, RoundSettings s = null) =>
            new RuleContext(l, d, reg, new SleeperState[0], null, s ?? new RoundSettings());

        /// <summary>Start at (0,0,0) with its single North door open.</summary>
        public static (Lattice, Derived, CubeRegistry) Fresh(RoundSettings s = null)
        {
            CubeRegistry reg = Registry();
            Lattice l = Lattice.New(reg, Start, Rotation.R0);
            Derived d = Deriver.Derive(l, reg, (s ?? new RoundSettings()).ExitHysteresis);
            return (l, d, reg);
        }

        /// <summary>Place a cube through a door and return the new state.</summary>
        public static (Lattice, Derived) Place(
            Lattice l, Derived d, CubeRegistry reg, Coord from, Face face,
            string typeId, Rotation rot = Rotation.R0, RoundSettings s = null)
        {
            RuleContext ctx = Context(l, d, reg, s);
            var req = new PlaceRequest(new ConnectorRef(from, face), typeId, rot, null);
            return Rules.ApplyPlace(ctx, req, l.Cubes.Count);
        }

        public static (Lattice, Derived) Explore(
            Lattice l, Derived d, CubeRegistry reg, Coord cube, RoundSettings s = null) =>
            Rules.ApplyExplore(Context(l, d, reg, s), cube, l.Cubes.Count);
    }
}
