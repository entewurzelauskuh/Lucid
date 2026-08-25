using Newtonsoft.Json;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// `cube.spec.json`: the only thing a cube author writes by hand. Mirrors
    /// docs/cube-spec.schema.json; the builder turns it into a prefab and a
    /// CubeDefinition, and prefab YAML is never edited (docs/SPEC.md §17).
    /// </summary>
    public sealed class CubeSpec
    {
        public const int CurrentSpecVersion = 1;

        [JsonProperty("specVersion", Required = Required.Always)] public int SpecVersion { get; set; }
        [JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
        [JsonProperty("name", Required = Required.Always)] public string Name { get; set; }
        [JsonProperty("pack", Required = Required.Always)] public string Pack { get; set; }
        [JsonProperty("category", Required = Required.Always)] public SpecCategory Category { get; set; }
        [JsonProperty("cost", Required = Required.Always)] public int Cost { get; set; }
        [JsonProperty("connectors", Required = Required.Always)] public SpecFace[] Connectors { get; set; }
        [JsonProperty("shell", Required = Required.Always)] public ShellSpec Shell { get; set; }

        [JsonProperty("climbable")] public bool Climbable { get; set; }
        [JsonProperty("props")] public PropSpec[] Props { get; set; }
        [JsonProperty("chicane")] public ChicaneSpec Chicane { get; set; }
        [JsonProperty("weakPoint")] public WeakPointSpec WeakPoint { get; set; }
        [JsonProperty("trigger")] public TriggerSpec Trigger { get; set; }
        [JsonProperty("killVolumes")] public KillVolumeSpec[] KillVolumes { get; set; }
        [JsonProperty("intendedPaths")] public IntendedPathSpec[] IntendedPaths { get; set; }
        [JsonProperty("nav")] public NavSpec Nav { get; set; }
        [JsonProperty("lighting")] public LightingSpec Lighting { get; set; }
        [JsonProperty("skins")] public string[] Skins { get; set; }
        [JsonProperty("preview")] public PreviewSpec Preview { get; set; }
        [JsonProperty("notes")] public string Notes { get; set; }

        public string[] EffectiveSkins => Skins ?? new[] { "*" };
        public PropSpec[] EffectiveProps => Props ?? new PropSpec[0];
        public KillVolumeSpec[] EffectiveKillVolumes => KillVolumes ?? new KillVolumeSpec[0];
    }
}
