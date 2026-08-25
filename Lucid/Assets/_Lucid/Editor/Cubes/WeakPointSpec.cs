using Newtonsoft.Json;

namespace Lucid.Editor.Cubes
{
    /// <summary>The prop a Sleeper can shoot out, and what jamming it does.</summary>
    public sealed class WeakPointSpec
    {
        [JsonProperty("prop", Required = Required.Always)] public string Prop { get; set; }
        [JsonProperty("hp", Required = Required.Always)] public int Hp { get; set; }
        // The schema makes this required: a weak point with no jam effect is
        // a prop the Sleeper can shoot to no purpose.
        [JsonProperty("jamEffect", Required = Required.Always)] public string JamEffect { get; set; }
    }
}
