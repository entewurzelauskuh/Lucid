namespace Lucid.Tests.EditMode.Cubes
{
    /// <summary>
    /// The two worked examples from docs/CUBE-SPEC.md, copied verbatim. If the
    /// guide and the reader ever disagree, these fail — which is the point:
    /// the guide is what a cube author reads.
    /// </summary>
    internal static class SpecFixtures
    {
        /// <summary>docs/CUBE-SPEC.md §3, "the simplest connector".</summary>
        public const string Straight = @"{
  ""specVersion"": 1,
  ""id"": ""core.straight"",
  ""name"": ""Straight"",
  ""pack"": ""core"",
  ""category"": ""connector"",
  ""cost"": 1,
  ""connectors"": [""north"", ""south""],
  ""shell"": {
    ""materials"": { ""wall"": ""wall"", ""floor"": ""floor"", ""ceiling"": ""ceiling"", ""trim"": ""trim"" },
    ""doorFrame"": ""plain"",
    ""interior"": { ""width"": 4, ""height"": 4 }
  },
  ""intendedPaths"": [
    { ""from"": ""south"", ""to"": ""north"", ""points"": [[0, 0, -4], [0, 0, 4]] }
  ],
  ""skins"": [""*""],
  ""notes"": ""A 4 m wide corridor inside the 8 m cube. No props; the skin provides all the character.""
}";

        /// <summary>docs/CUBE-SPEC.md §4, "a full chicane".</summary>
        public const string TrapdoorPit = @"{
  ""specVersion"": 1,
  ""id"": ""core.trapdoor_pit"",
  ""name"": ""Trapdoor"",
  ""pack"": ""core"",
  ""category"": ""chicane"",
  ""cost"": 3,
  ""connectors"": [""south"", ""north""],
  ""shell"": {
    ""materials"": { ""wall"": ""wall"", ""floor"": ""floor"", ""ceiling"": ""ceiling"", ""trim"": ""metal"" },
    ""doorFrame"": ""industrial"",
    ""openFloor"": true
  },
  ""props"": [
    { ""name"": ""floor_south"", ""asset"": ""generated:box"", ""position"": [0, -0.15, -3], ""size"": [8, 0.3, 2], ""material"": ""floor"" },
    { ""name"": ""floor_north"", ""asset"": ""generated:box"", ""position"": [0, -0.15, 3], ""size"": [8, 0.3, 2], ""material"": ""floor"" },
    { ""name"": ""ledge_west"", ""asset"": ""generated:box"", ""position"": [-3.5, -0.15, 0], ""size"": [1, 0.3, 4], ""material"": ""floor"" },
    { ""name"": ""panel"", ""asset"": ""generated:box"", ""position"": [0, -0.15, 0], ""size"": [7, 0.3, 4], ""material"": ""metal"", ""static"": false },
    { ""name"": ""latch"", ""asset"": ""assets/latch_box.fbx"", ""position"": [3.6, 1.2, 0], ""rotation"": [0, -90, 0], ""collider"": ""box"" },
    { ""name"": ""pit_walls"", ""asset"": ""generated:box"", ""position"": [0, -4, 0], ""size"": [8, 8, 8], ""material"": ""wall"", ""collider"": ""none"" }
  ],
  ""chicane"": {
    ""component"": ""Trapdoor"",
    ""params"": { ""variant"": ""pit"", ""openDelayMs"": 250, ""resetMs"": 4000, ""hinge"": ""south"" },
    ""actors"": [""panel""]
  },
  ""weakPoint"": { ""prop"": ""latch"", ""hp"": 60, ""jamEffect"": ""lockShut"" },
  ""trigger"": { ""kind"": ""dropNow"", ""cooldownMs"": 6000, ""label"": ""Drop"" },
  ""killVolumes"": [
    { ""center"": [0, -3.5, 0], ""size"": [7, 1, 4], ""cause"": ""pit"" }
  ],
  ""intendedPaths"": [
    { ""from"": ""south"", ""to"": ""north"", ""points"": [[0, 0, -4], [-3.5, 0, -2], [-3.5, 0, 2], [0, 0, 4]], ""notes"": ""Safe route along the west ledge."" },
    { ""from"": ""south"", ""to"": ""north"", ""points"": [[0, 0, -4], [0, 0, -2], [0, 0, 2], [0, 0, 4]], ""notes"": ""Straight across the panel; only safe once the latch is shot or if you are fast."" }
  ],
  ""nav"": { ""agentRadius"": 0.4, ""links"": true, ""exclude"": [""panel""] },
  ""lighting"": {
    ""preset"": ""skin"",
    ""lights"": [ { ""type"": ""spot"", ""position"": [3.2, 3, 0], ""direction"": [0, -1, 0], ""role"": ""accent"", ""intensity"": 8, ""color"": ""skin"" } ]
  },
  ""skins"": [""*""],
  ""notes"": ""The pit variant kills; the drop variant is the same spec with variant: drop, openFloor kept, no killVolumes, and a down connector.""
}";

        /// <summary>A minimal valid connector, for tests that vary one field.</summary>
        public const string Minimal = @"{
  ""specVersion"": 1,
  ""id"": ""core.minimal"",
  ""name"": ""Minimal"",
  ""pack"": ""core"",
  ""category"": ""connector"",
  ""cost"": 1,
  ""connectors"": [""north"", ""south""],
  ""shell"": { ""materials"": { ""wall"": ""wall"", ""floor"": ""floor"", ""ceiling"": ""ceiling"" } }
}";

        /// <summary>The minimal spec with one field replaced.</summary>
        public static string With(string field, string json) =>
            Minimal.TrimEnd().TrimEnd('}').TrimEnd().TrimEnd(',') + $",\n  \"{field}\": {json}\n}}";

        /// <summary>The minimal spec with one existing value swapped out.</summary>
        public static string Replacing(string find, string replace) => Minimal.Replace(find, replace);
    }
}
