using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// A component from the chicane library plus its parameters. The parameter
    /// shape belongs to the component, not to this schema, so it stays as raw
    /// JSON until the component validates it (docs/CHICANES.md §8).
    /// </summary>
    public sealed class ChicaneSpec
    {
        [JsonProperty("component", Required = Required.Always)] public string Component { get; set; }
        [JsonProperty("params")] public JObject Params { get; set; }
        [JsonProperty("actors")] public string[] Actors { get; set; }
    }
}
