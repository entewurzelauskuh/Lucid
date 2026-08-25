using Newtonsoft.Json;

namespace Lucid.Editor.Cubes
{
    /// <summary>What the Nightmare's T key does in this cube (docs/SPEC.md §10).</summary>
    public sealed class TriggerSpec
    {
        [JsonProperty("kind", Required = Required.Always)] public string Kind { get; set; }
        [JsonProperty("cooldownMs")] public int CooldownMs { get; set; } = 6_000;
        [JsonProperty("label")] public string Label { get; set; }
    }
}
