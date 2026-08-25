using Newtonsoft.Json;

namespace Lucid.Editor.Cubes
{
    public sealed class LightingSpec
    {
        [JsonProperty("preset")] public LightingPreset Preset { get; set; } = LightingPreset.Skin;
        [JsonProperty("lights")] public LightSpec[] Lights { get; set; }
    }
}
