using Newtonsoft.Json;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// Insets the shell to make a narrower or lower room. Doorways keep their
    /// standard size regardless, so cubes always join.
    /// </summary>
    public sealed class InteriorSpec
    {
        [JsonProperty("width")] public float? Width { get; set; }
        [JsonProperty("height")] public float? Height { get; set; }
    }
}
