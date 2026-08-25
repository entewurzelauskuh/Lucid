using Newtonsoft.Json;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// Role names a SkinSet resolves, or an explicit path under Assets/ to pin
    /// the cube to one look (docs/CUBE-SPEC.md §2).
    /// </summary>
    public sealed class MaterialRolesSpec
    {
        [JsonProperty("wall", Required = Required.Always)] public string Wall { get; set; }
        [JsonProperty("floor", Required = Required.Always)] public string Floor { get; set; }
        [JsonProperty("ceiling", Required = Required.Always)] public string Ceiling { get; set; }
        [JsonProperty("trim")] public string Trim { get; set; }
    }
}
