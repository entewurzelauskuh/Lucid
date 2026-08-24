using System.Collections.Generic;

namespace Lucid.Core
{
    /// <summary>
    /// Everything the rules read. Passed in rather than held, so the rules stay
    /// pure functions of their input (docs/CORE-API.md §5).
    /// </summary>
    public sealed record RuleContext(
        Lattice Lattice,
        Derived Derived,
        CubeRegistry Registry,
        IReadOnlyList<SleeperState> Sleepers,
        Budget Budget,
        RoundSettings Settings);
}
