using System.Collections.Generic;

namespace Lucid.Core
{
    /// <summary>
    /// Turns a lattice into connector states, depths and exits. Pure, ordered
    /// and free of floating point, so every machine derives an identical
    /// <see cref="Derived.Hash"/> (docs/CORE-API.md §3).
    /// </summary>
    public static class Deriver
    {
        public static Derived Derive(
            Lattice l,
            CubeRegistry reg,
            int exitHysteresis = 0,
            int previousExitDepth = -1,
            IReadOnlyList<ConnectorRef> previousExits = null)
        {
            var connectors = new Dictionary<ConnectorRef, ConnectorState>();

            // 1. Attachment.
            foreach (Coord c in l.CoordsInOrder())
            {
                FaceMask mask = l.ConnectorsAt(c, reg);
                foreach (Face f in Faces.Of(mask))
                {
                    var door = new ConnectorRef(c, f);
                    Coord n = c.Offset(f);

                    if (!l.Has(n))
                    {
                        connectors[door] = l.IsSolidified(door) ? ConnectorState.Solid : ConnectorState.Fog;
                        continue;
                    }

                    if (!l.HasConnector(n, Faces.Opposite(f), reg))
                    {
                        throw new LatticeInvariantViolation(
                            $"doorway {door} faces the wall of {n}; the fit rule should have prevented this");
                    }

                    connectors[door] = ConnectorState.Attached;
                }
            }

            // 2. Depth: breadth-first from Start over attached doors, both ways.
            var depth = new Dictionary<Coord, int> { [l.Start] = 0 };
            var queue = new Queue<Coord>();
            queue.Enqueue(l.Start);
            while (queue.Count > 0)
            {
                Coord c = queue.Dequeue();
                int next = depth[c] + 1;
                foreach (Face f in Faces.Of(l.ConnectorsAt(c, reg)))
                {
                    if (connectors[new ConnectorRef(c, f)] != ConnectorState.Attached) continue;
                    Coord n = c.Offset(f);
                    if (depth.ContainsKey(n)) continue;
                    depth[n] = next;
                    queue.Enqueue(n);
                }
            }

            if (depth.Count != l.Cubes.Count)
            {
                throw new LatticeInvariantViolation(
                    $"{l.Cubes.Count - depth.Count} cube(s) are unreachable from the start; " +
                    "placement is frontier-only, so this cannot happen");
            }

            // 3 and 4. Exits: the deepest cubes that still have a fog door.
            int deepest = -1;
            foreach (KeyValuePair<ConnectorRef, ConnectorState> kv in connectors)
            {
                if (kv.Value != ConnectorState.Fog) continue;
                int d = depth[kv.Key.Cube];
                if (d > deepest) deepest = d;
            }

            int exitDepth = deepest;
            if (exitHysteresis > 0 && previousExitDepth >= 0 && deepest >= 0 &&
                deepest < previousExitDepth + exitHysteresis &&
                StillHasFog(previousExits, connectors))
            {
                // The new frontier is not decisively deeper, so leave the exits
                // where they are. Keeps the Nightmare from flipping the light
                // between two branches (docs/SPEC.md §7).
                exitDepth = previousExitDepth;
            }

            var exits = new List<ConnectorRef>();
            if (exitDepth >= 0)
            {
                foreach (Coord c in l.CoordsInOrder())
                {
                    if (depth[c] != exitDepth) continue;
                    foreach (Face f in Faces.Of(l.ConnectorsAt(c, reg)))
                    {
                        var door = new ConnectorRef(c, f);
                        if (connectors[door] == ConnectorState.Fog)
                        {
                            connectors[door] = ConnectorState.Exit;
                            exits.Add(door);
                        }
                    }
                }
            }

            if (exits.Count == 0) exitDepth = -1;

            // 5. Hash: depths then connector states, both in (Z, Y, X) order.
            ulong hash = Fnv.Basis;
            foreach (Coord c in l.CoordsInOrder())
            {
                hash = Fnv.Coord(hash, c);
                hash = Fnv.Int32(hash, depth[c]);
            }
            foreach (Coord c in l.CoordsInOrder())
            {
                foreach (Face f in Faces.All)
                {
                    var door = new ConnectorRef(c, f);
                    if (!connectors.TryGetValue(door, out ConnectorState state)) continue;
                    hash = Fnv.Coord(hash, c);
                    hash = Fnv.Int32(hash, (int)f);
                    hash = Fnv.Int32(hash, (int)state);
                }
            }

            return new Derived(depth, connectors, exits, exitDepth, hash);
        }

        static bool StillHasFog(
            IReadOnlyList<ConnectorRef> previousExits,
            Dictionary<ConnectorRef, ConnectorState> connectors)
        {
            if (previousExits == null || previousExits.Count == 0) return false;
            foreach (ConnectorRef door in previousExits)
            {
                if (connectors.TryGetValue(door, out ConnectorState s) && s == ConnectorState.Fog) return true;
            }
            return false;
        }
    }
}
