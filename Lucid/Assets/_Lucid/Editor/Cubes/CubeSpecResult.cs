using System.Collections.Generic;
using System.Linq;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// The outcome of reading a spec: either a usable spec, or every problem
    /// found. Reporting all of them at once matters — the build loop is read
    /// report, fix, rebuild (docs/CUBE-SPEC.md §5), and one problem per run
    /// makes that loop needlessly long.
    /// </summary>
    public sealed class CubeSpecResult
    {
        readonly List<SpecProblem> _problems;

        CubeSpecResult(CubeSpec spec, List<SpecProblem> problems)
        {
            Spec = spec;
            _problems = problems;
        }

        public CubeSpec Spec { get; }
        public IReadOnlyList<SpecProblem> Problems => _problems;
        public bool Ok => _problems.Count == 0;

        public static CubeSpecResult Success(CubeSpec spec) =>
            new CubeSpecResult(spec, new List<SpecProblem>());

        public static CubeSpecResult Failure(IEnumerable<SpecProblem> problems) =>
            new CubeSpecResult(null, problems.ToList());

        public static CubeSpecResult Failure(string field, string message) =>
            Failure(new[] { new SpecProblem(field, message) });

        public string Describe() => string.Join("\n", _problems.Select(p => "  " + p));
    }
}
