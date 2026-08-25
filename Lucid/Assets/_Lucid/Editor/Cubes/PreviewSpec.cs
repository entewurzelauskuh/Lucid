using Newtonsoft.Json;

namespace Lucid.Editor.Cubes
{
    /// <summary>Which previews to render. These are what a reviewer looks at.</summary>
    public sealed class PreviewSpec
    {
        public static readonly PreviewCamera[] DefaultCameras =
            { PreviewCamera.Iso, PreviewCamera.Entrance, PreviewCamera.Top };

        [JsonProperty("cameras")] public PreviewCamera[] Cameras { get; set; }
        [JsonProperty("custom")] public CustomCameraSpec Custom { get; set; }

        public PreviewCamera[] EffectiveCameras => Cameras ?? DefaultCameras;
    }
}
