using System;

namespace Lucid.Core
{
    /// <summary>
    /// Thrown when the lattice contradicts an invariant from
    /// docs/CORE-API.md §11 — a doorway facing a wall, or a cube unreachable
    /// from the start. The placement rules make these unreachable, so this
    /// signals a bug in Core rather than bad input.
    /// </summary>
    public sealed class LatticeInvariantViolation : Exception
    {
        public LatticeInvariantViolation(string message) : base(message) { }
    }
}
