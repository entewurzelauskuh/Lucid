using Newtonsoft.Json;

namespace Lucid.Editor.Cubes
{
    public sealed class LightSpec
    {
        [JsonProperty("type", Required = Required.Always)] public LightKind Type { get; set; }
        [JsonProperty("position", Required = Required.Always)] public Vec3Spec Position { get; set; }
        [JsonProperty("direction")] public Vec3Spec? Direction { get; set; }
        [JsonProperty("role")] public LightRole Role { get; set; } = LightRole.Accent;
        [JsonProperty("intensity")] public float? Intensity { get; set; }
        [JsonProperty("color")] public string Color { get; set; } = "skin";
    }
}
