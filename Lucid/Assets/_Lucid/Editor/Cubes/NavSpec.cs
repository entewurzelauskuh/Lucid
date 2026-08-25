using Newtonsoft.Json;

namespace Lucid.Editor.Cubes
{
    public sealed class NavSpec
    {
        [JsonProperty("agentRadius")] public float AgentRadius { get; set; } = 0.4f;
        [JsonProperty("links")] public bool Links { get; set; } = true;
        [JsonProperty("exclude")] public string[] Exclude { get; set; }
    }
}
