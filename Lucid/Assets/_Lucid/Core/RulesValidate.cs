namespace Lucid.Core
{
    /// <summary>
    /// The validation half of the rules: whether a move is legal
    /// (docs/CORE-API.md §5 and §6). Pure — nothing here mutates the lattice
    /// or spends budget, so the host can ask freely and the Nightmare's client
    /// can preview a ghost without committing to anything.
    /// </summary>
    public static partial class Rules
    {
        /// <summary>
        /// Checks in the order given by §5 and returns the first failure, so a
        /// rejected placement always names the most fundamental reason rather
        /// than whichever check happened to run first.
        /// </summary>
        public static PlaceVerdict ValidatePlace(RuleContext ctx, PlaceRequest req)
        {
            // 1. The type must exist, and the start cube is never placeable.
            if (req.TypeId == null || !ctx.Registry.TryGet(req.TypeId, out CubeType type))
                return new PlaceVerdict(PlaceError.UnknownType);
            if (type.Category == CubeCategory.Start)
                return new PlaceVerdict(PlaceError.UnknownType);

            Coord from = req.Target.Cube;
            Face face = req.Target.Face;

            // 2. The target must be a door on an existing cube, and not already
            //    a passage.
            if (!ctx.Lattice.Has(from)) return new PlaceVerdict(PlaceError.NotADoor);
            if (!ctx.Lattice.HasConnector(from, face, ctx.Registry))
                return new PlaceVerdict(PlaceError.NotADoor);

            ConnectorState state = ctx.Derived.StateOf(req.Target);
            if (state == ConnectorState.Attached) return new PlaceVerdict(PlaceError.NotADoor);

            // 3. A door a Sleeper has already closed is closed for good.
            if (state == ConnectorState.Solid) return new PlaceVerdict(PlaceError.DoorIsSolid);

            // 4. Guard: unreachable once 2 and 3 pass, but cheap insurance
            //    against a future rule change.
            Coord at = from.Offset(face);
            if (ctx.Lattice.Has(at)) return new PlaceVerdict(PlaceError.DoorOccupied);

            // 5. The dream has edges.
            Limits limits = ctx.Settings?.EffectiveLimits ?? Limits.Default;
            if (!limits.Contains(at))
                return new PlaceVerdict(PlaceError.OutOfBounds);

            // 6. Fit: connector to connector, wall to wall, nothing else.
            FaceMask mask = Faces.Rotate(type.Connectors, req.Rotation);
            if (!Faces.Has(mask, Faces.Opposite(face)))
                return new PlaceVerdict(PlaceError.DoesNotFit);

            foreach (Face f in Faces.All)
            {
                Coord neighbour = at.Offset(f);
                if (!ctx.Lattice.Has(neighbour)) continue;

                bool mine = Faces.Has(mask, f);
                // HasOpenConnector, not HasConnector: a neighbour's condensed
                // door counts as wall, so the new cube must present a wall to it.
                bool theirs = ctx.Lattice.HasOpenConnector(neighbour, Faces.Opposite(f), ctx.Registry);
                if (mine != theirs) return new PlaceVerdict(PlaceError.DoesNotFit);
            }

            // 7. Affordability. The host spends; validation only asks.
            if (ctx.Budget != null && !ctx.Budget.CanAfford(type.Cost))
                return new PlaceVerdict(PlaceError.NotEnoughBudget);

            // 8. Leak: nobody may be sealed away from every exit. Checked last
            //    because it costs a clone and a derive.
            return CheckLeak(ctx, req, at, type);
        }

        static PlaceVerdict CheckLeak(RuleContext ctx, PlaceRequest req, Coord at, CubeType type)
        {
            Lattice hypothetical = ctx.Lattice.WithCube(
                at, new CubeInstance(req.TypeId, req.Rotation, req.SkinId, ctx.Lattice.Cubes.Count));
            Derived after = Deriver.Derive(
                hypothetical, ctx.Registry,
                ctx.Settings.ExitHysteresis, ctx.Derived.ExitDepth, ctx.Derived.Exits);

            // Invariant 4 has two halves (docs/CORE-API.md §11): an exit must
            // exist at all, and every Sleeper still in the dream must be able to
            // reach one. Closing the last fog door seals the dream even with
            // nobody inside it, so this check cannot be skipped when the dream
            // is empty.
            if (after.Exits.Count == 0) return new PlaceVerdict(PlaceError.WouldTrap);

            if (ctx.Sleepers == null) return PlaceVerdict.Pass;

            foreach (SleeperState s in ctx.Sleepers)
            {
                if (s.Status != SleeperStatus.InDream) continue;
                if (!Traversal.CanReachExit(hypothetical, ctx.Registry, after, s.Cube))
                    return new PlaceVerdict(PlaceError.WouldTrap, s.Id);
            }

            return PlaceVerdict.Pass;
        }

        public static ExploreError ValidateExplore(RuleContext ctx, Coord cube)
        {
            if (!ctx.Lattice.Has(cube)) return ExploreError.NoCube;
            if (cube == ctx.Lattice.Start) return ExploreError.StartCube;
            if (ctx.Lattice.IsExplored(cube)) return ExploreError.AlreadyExplored;
            return ExploreError.None;
        }
    }
}
