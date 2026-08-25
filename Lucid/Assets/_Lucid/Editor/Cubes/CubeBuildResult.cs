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

        /// <summary>Assembles a successful result, including what was ignored.</summary>
        public sealed class Builder
        {
            readonly CubeBuildResult _result;

            public Builder(string specPath, string prefabPath, CubeDefinition definition,
                           bool packChanged, bool prefabChanged) =>
                _result = Built(specPath, prefabPath, definition, packChanged, prefabChanged);

            public IReadOnlyList<string> Ignored
            {
                set => _result.Ignored = value;
            }

            public bool DefinitionChanged
            {
                set => _result.DefinitionChanged = value;
            }

            public CubeBuildResult Result => _result;
        }

        public static CubeBuildResult Rejected(string specPath, IEnumerable<SpecProblem> problems) =>
            new CubeBuildResult(specPath, null, null, false, false, problems.ToList());

        /// <summary>Spec sections the builder parsed and did not act on.</summary>
        public IReadOnlyList<string> Ignored { get; set; } = new string[0];

        /// <summary>False when the CubeDefinition on disk was already correct.</summary>
        public bool DefinitionChanged { get; set; }

        public string Describe()
        {
            if (!Ok) return $"{SpecPath}\n" + string.Join("\n", Problems.Select(p => "  " + p));

            string line = PrefabChanged ? $"built {PrefabPath}" : $"unchanged {PrefabPath}";

            // Saying so beats a green line over a cube whose trap was dropped.
            return Ignored.Count == 0 ? line : line + "\n  ignored (not yet implemented): "
                                              + string.Join(", ", Ignored);
        }
    }
}
