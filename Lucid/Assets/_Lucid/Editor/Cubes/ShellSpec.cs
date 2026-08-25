using Newtonsoft.Json;

namespace Lucid.Editor.Cubes
{
    /// <summary>The generated walls, floor, ceiling and door frames.</summary>
    public sealed class ShellSpec
    {
        [JsonProperty("materials", Required = Required.Always)]
        public MaterialRolesSpec Materials { get; set; }

        [JsonProperty("doorFrame")] public DoorFrameStyle DoorFrame { get; set; } = DoorFrameStyle.Plain;
        [JsonProperty("thickness")] public float Thickness { get; set; } = 0.3f;
        [JsonProperty("interior")] public InteriorSpec Interior { get; set; }
        [JsonProperty("openFloor")] public bool OpenFloor { get; set; }
        [JsonProperty("openCeiling")] public bool OpenCeiling { get; set; }
    }
}
