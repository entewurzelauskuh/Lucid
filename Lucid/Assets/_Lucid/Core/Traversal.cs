using System.Collections.Generic;

namespace Lucid.Core
{
    /// <summary>
    /// Where a Sleeper can actually get to. Movement is directed, which is what
    /// makes drops funnels: you can always fall, but you can only climb back if
    /// the cube you are leaving has something to climb (docs/CORE-API.md §4).
    /// </summary>
    public static class Traversal
    {
        public static HashSet<Coord> Reachable(Lattice l, CubeRegistry reg, Derived d, Coord from)
        {
            var seen = new HashSet<Coord>();
            if (!l.Has(from)) return seen;

            var queue = new Queue<Coord>();
            seen.Add(from);
            queue.Enqueue(from);

            while (queue.Count > 0)
            {
                Coord c = queue.Dequeue();
                foreach (Face f in Faces.Of(l.ConnectorsAt(c, reg)))
                {
                    if (d.StateOf(new ConnectorRef(c, f)) != ConnectorState.Attached) continue;
                    if (!CanPass(l, reg, c, f)) continue;

                    Coord n = c.Offset(f);
                    if (seen.Add(n)) queue.Enqueue(n);
                }
            }

            return seen;
        }

        /// <summary>
        /// True if any cube reachable from <paramref name="from"/> owns an Exit
        /// door. Exits are doors rather than cubes, so standing in the exit cube
        /// already counts.
        /// </summary>
        public static bool CanReachExit(Lattice l, CubeRegistry reg, Derived d, Coord from)
        {
            foreach (Coord c in Reachable(l, reg, d, from))
            {
                foreach (Face f in Faces.Of(l.ConnectorsAt(c, reg)))
                {
                    if (d.StateOf(new ConnectorRef(c, f)) == ConnectorState.Exit) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Whether a Sleeper standing in <paramref name="c"/> can leave through
        /// <paramref name="f"/>. Climbability belongs to the lower cube: it is
        /// the ladder in the room you are standing in that gets you out
        /// (docs/SPEC.md §7).
        /// </summary>
        static bool CanPass(Lattice l, CubeRegistry reg, Coord c, Face f)
        {
            if (f == Face.Down) return true;             // falling is always allowed
            if (f != Face.Up) return true;               // horizontal, both ways
            return reg.Get(l.At(c).TypeId).Climbable;    // climbing out needs a climbable
        }
    }
}
