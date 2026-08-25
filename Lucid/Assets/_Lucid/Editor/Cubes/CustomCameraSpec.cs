using Newtonsoft.Json;

namespace Lucid.Editor.Cubes
{
    public sealed class CustomCameraSpec
    {
        [JsonProperty("position")] public Vec3Spec? Position { get; set; }
        [JsonProperty("lookAt")] public Vec3Spec? LookAt { get; set; }
    }
}
