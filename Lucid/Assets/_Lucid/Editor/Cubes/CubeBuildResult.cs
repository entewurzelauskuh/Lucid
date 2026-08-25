using System.Collections.Generic;
using System.Linq;
using Lucid.Runtime;

namespace Lucid.Editor.Cubes
{
    /// <summary>What one cube build did, or why it did nothing.</summary>
    public sealed class CubeBuildResult
    {
        CubeBuildResult(string specPath, string prefabPath, CubeDefinition definition,
                        bool packChanged, bool prefabChanged, IReadOnlyList<SpecProblem> problems)
        {
            PrefabChanged = prefabChanged;
            SpecPath = specPath;
            PrefabPath = prefabPath;
            Definition = definition;
            PackChanged = packChanged;
            Problems = problems;
        }

        public string SpecPath { get; }
        public string PrefabPath { get; }
        public CubeDefinition Definition { get; }
        public bool PackChanged { get; }

        /// <summary>False when the prefab on disk was already correct.</summary>
        public bool PrefabChanged { get; }
        public IReadOnlyList<SpecProblem> Problems { get; }

        public bool Ok => Problems.Count == 0;

        public static CubeBuildResult Built(
            string specPath, string prefabPath, CubeDefinition definition,
            bool packChanged, bool prefabChanged) =>
            new CubeBuildResult(specPath, prefabPath, definition, packChanged, prefabChanged,
                new SpecProblem[0]);

        public static CubeBuildResult Rejected(string specPath, IEnumerable<SpecProblem> problems) =>
            new CubeBuildResult(specPath, null, null, false, false, problems.ToList());

        public string Describe() =>
            Ok ? (PrefabChanged ? $"built {PrefabPath}" : $"unchanged {PrefabPath}")
               : $"{SpecPath}\n" + string.Join("\n", Problems.Select(p => "  " + p));
    }
}
