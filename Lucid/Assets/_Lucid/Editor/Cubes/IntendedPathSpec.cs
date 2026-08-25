using Newtonsoft.Json;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// How a Sleeper is meant to cross. The validator samples these against the
    /// kit limits — gap 3.5 m, rise 1.1 m, clearance 1.1 m — which is why the
    /// safe route goes first (docs/CUBE-SPEC.md §5).
    /// </summary>
    public sealed class IntendedPathSpec
    {
        [JsonProperty("from", Required = Required.Always)] public SpecFace From { get; set; }
        [JsonProperty("to", Required = Required.Always)] public SpecFace To { get; set; }
        [JsonProperty("points", Required = Required.Always)] public Vec3Spec[] Points { get; set; }
        [JsonProperty("notes")] public string Notes { get; set; }
    }
}
