using Newtonsoft.Json;

namespace Lucid.Editor.Cubes
{
    public sealed class KillVolumeSpec
    {
        [JsonProperty("center", Required = Required.Always)] public Vec3Spec Center { get; set; }
        [JsonProperty("size", Required = Required.Always)] public Vec3Spec Size { get; set; }
        [JsonProperty("cause")] public KillCause Cause { get; set; } = KillCause.Pit;
    }
}
