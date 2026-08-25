using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// What the validator found, written to `Previews/report.json` beside the
    /// previews. This is the file the pipeline's caller reads and acts on
    /// (docs/SPEC.md §17), so it is JSON rather than log lines.
    /// </summary>
    public sealed class ValidationReport
    {
        [JsonProperty("cube")] public string Cube { get; set; }
        [JsonProperty("spec")] public string Spec { get; set; }
        [JsonProperty("prefab")] public string Prefab { get; set; }
        [JsonProperty("ok")] public bool Ok => Problems.Count == 0;
        [JsonProperty("problems")] public List<ValidationProblem> Problems { get; set; }
            = new List<ValidationProblem>();
        [JsonProperty("previews")] public List<string> Previews { get; set; } = new List<string>();
        [JsonProperty("triangles")] public int Triangles { get; set; }

        public void Add(string rule, string message) =>
            Problems.Add(new ValidationProblem(rule, message));

        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);

        public string Describe() =>
            Ok ? $"validated {Cube}"
               : $"{Cube}\n" + string.Join("\n", Problems.Select(p => "  " + p));
    }
}
