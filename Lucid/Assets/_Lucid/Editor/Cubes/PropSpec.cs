using Newtonsoft.Json;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// Dressing or a mechanism part: a committed asset, a manifest asset, or a
    /// builder primitive (`generated:box`). Every prop needs a stable
    /// <see cref="Name"/>, because the chicane, weak point and nav sections
    /// refer to it (docs/CUBE-SPEC.md §5).
    /// </summary>
    public sealed class PropSpec
    {
        [JsonProperty("name", Required = Required.Always)] public string Name { get; set; }
        [JsonProperty("asset", Required = Required.Always)] public string Asset { get; set; }
        [JsonProperty("position", Required = Required.Always)] public Vec3Spec Position { get; set; }
        [JsonProperty("rotation")] public Vec3Spec? Rotation { get; set; }
        [JsonProperty("scale")] public ScaleSpec? Scale { get; set; }
        [JsonProperty("size")] public Vec3Spec? Size { get; set; }
        [JsonProperty("material")] public string Material { get; set; }
        [JsonProperty("collider")] public ColliderMode Collider { get; set; } = ColliderMode.Auto;
        [JsonProperty("static")] public bool IsStatic { get; set; } = true;

        /// <summary>
        /// Only instantiated for skins declaring this tag; lets one cube carry
        /// alternative dressing.
        /// </summary>
        [JsonProperty("skinTag")] public string SkinTag { get; set; }

        /// <summary>The schema's defaults, so no consumer has to re-derive them.</summary>
        public Vec3Spec EffectiveRotation => Rotation ?? new Vec3Spec(0, 0, 0);
        public ScaleSpec EffectiveScale => Scale ?? ScaleSpec.One;
    }
}
