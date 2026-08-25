using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lucid.Core;
using Lucid.Runtime;
using UnityEditor;
using UnityEngine;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// Turns a `cube.spec.json` into a prefab, a <see cref="CubeDefinition"/>
    /// and a place in the pack's <see cref="DreamPack"/> (docs/SPEC.md §17).
    /// </summary>
    /// <remarks>
    /// Nothing here is authored by hand. The point of the pipeline is that the
    /// spec is the source and the prefab is output: rebuilding after a template
    /// change is one command, and reviewing a cube means reading a diff of JSON
    /// rather than of GUIDs.
    /// </remarks>
    public static class CubeBuilder
    {
        public const string PacksRoot = "Assets/_Lucid/Packs";

        /// <summary>Builds one cube. Returns what happened, never throws on a bad spec.</summary>
        public static CubeBuildResult BuildFromSpec(string specPath)
        {
            CubeSpecResult read = CubeSpecReader.ReadFile(specPath);
            if (!read.Ok) return CubeBuildResult.Rejected(specPath, read.Problems);

            CubeSpec spec = read.Spec;
            string folder = Path.GetDirectoryName(specPath).Replace('\\', '/');

            GameObject template = AssetDatabase.LoadAssetAtPath<GameObject>(
                CubeTemplateBuilder.TemplatePath);
            if (template == null)
            {
                return CubeBuildResult.Rejected(specPath, new[]
                {
                    new SpecProblem(CubeTemplateBuilder.TemplatePath,
                        "no cube template; run Lucid/Rebuild Cube Template"),
                });
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(template);
            CubeDefinition definition;
            bool packChanged;
            string prefabPath;
            bool prefabChanged;
            try
            {
                PrefabUtility.UnpackPrefabInstance(
                    instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                instance.name = Path.GetFileName(folder);

                Configure(instance, spec);

                prefabPath = $"{folder}/{instance.name}.prefab";

                // Only write when the cube actually changed. Saving
                // unconditionally would rewrite the file on every build, because
                // Unity mints new fileIDs each time and orders the YAML by them.
                var onDisk = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                prefabChanged = !CubeEquivalence.Matches(instance, onDisk);
                if (prefabChanged) PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);

                definition = WriteDefinition(folder, spec, prefabPath);
                packChanged = Register(spec, definition);
            }
            finally
            {
                // Destroyed before anything renders. The preview renderer puts
                // its own copy in the scene, and a second one standing in the
                // same place renders straight through the cut-away.
                Object.DestroyImmediate(instance);
            }

            // Validate and preview the prefab as saved, not the working
            // instance: what a reviewer looks at has to be what shipped.
            var saved = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            // Render first: the validator checks that every camera the spec
            // asked for actually produced a file.
            List<string> previews = CubePreviewRenderer.Render(saved, spec, folder);
            ValidationReport report = CubeValidator.Validate(saved, spec, folder, previews);
            WriteReport(folder, report);

            AssetDatabase.SaveAssets();
            return new CubeBuildResult.Builder(
                specPath, prefabPath, definition, packChanged, prefabChanged)
                {
                    Ignored = Unhandled(spec),
                    DefinitionChanged = _definitionChanged,
                    Report = report,
                }.Result;
        }

        /// <summary>Shell, sockets and doors, from the spec's connector mask.</summary>
        static void Configure(GameObject instance, CubeSpec spec)
        {
            FaceMask mask = CubeSpecMapping.ToMask(spec.Connectors);

            Transform shell = instance.transform.Find("Shell");
            ShellBuilder.Build(shell, spec);

            Transform sockets = instance.transform.Find("Sockets");
            foreach (Face face in Faces.All)
            {
                Transform socket = sockets.Find(face.ToString());
                var connector = socket.GetComponent<Connector>();
                connector.Configure(face, Faces.Has(mask, face), connector.Door);
            }
        }

        /// <summary>Writes report.json beside the previews, when it changed.</summary>
        static void WriteReport(string folder, ValidationReport report)
        {
            string previews = Path.Combine(folder, "Previews");
            Directory.CreateDirectory(previews);

            string path = Path.Combine(previews, "report.json");
            string json = report.ToJson();
            if (File.Exists(path) && File.ReadAllText(path) == json) return;

            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Sections the spec carries that this milestone's builder does not act
        /// on. Reporting them keeps a bare shell from reading as a finished
        /// cube (docs/SPEC.md §17 lists the full step order).
        /// </summary>
        static string[] Unhandled(CubeSpec spec)
        {
            var ignored = new List<string>();
            if (spec.EffectiveProps.Length > 0) ignored.Add("props");
            if (spec.Chicane != null) ignored.Add("chicane");
            if (spec.WeakPoint != null) ignored.Add("weakPoint");
            if (spec.Trigger != null) ignored.Add("trigger");
            if (spec.EffectiveKillVolumes.Length > 0) ignored.Add("killVolumes");
            if (spec.Nav != null) ignored.Add("nav");
            if (spec.Lighting != null) ignored.Add("lighting");
            if (spec.Preview?.Custom != null) ignored.Add("preview.custom");
            return ignored.ToArray();
        }

        /// <summary>Set by <see cref="WriteDefinition"/>; read straight back by the caller.</summary>
        static bool _definitionChanged;

        static CubeDefinition WriteDefinition(string folder, CubeSpec spec, string prefabPath)
        {
            string path = $"{folder}/{Path.GetFileName(folder)}.asset";
            var definition = AssetDatabase.LoadAssetAtPath<CubeDefinition>(path);

            bool fresh = definition == null;
            if (fresh) definition = ScriptableObject.CreateInstance<CubeDefinition>();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            FaceMask connectors = CubeSpecMapping.ToMask(spec.Connectors);
            CubeCategory category = CubeSpecMapping.ToCategory(spec.Category);

            // Only mark it dirty when something actually differs, for the same
            // reason the prefab is only written on change: an unconditional
            // SetDirty re-serialises the asset on every build of the pack.
            _definitionChanged = false;
            bool changed = fresh
                || definition.Id != spec.Id
                || definition.Pack != spec.Pack
                || definition.DisplayName != spec.Name
                || definition.Category != category
                || definition.Connectors != connectors
                || definition.Climbable != spec.Climbable
                || definition.Cost != spec.Cost
                || definition.Prefab != prefab
                || !definition.Skins.SequenceEqual(spec.EffectiveSkins);

            _definitionChanged = changed;
            if (!changed) return definition;

            definition.Configure(
                spec.Id, spec.Pack, spec.Name, category, connectors,
                spec.Climbable, spec.Cost, prefab, spec.EffectiveSkins);

            if (fresh) AssetDatabase.CreateAsset(definition, path);
            else EditorUtility.SetDirty(definition);

            return definition;
        }

        /// <summary>
        /// Adds the cube to its pack. The pack is created on first use so a new
        /// pack needs no setup step.
        /// </summary>
        static bool Register(CubeSpec spec, CubeDefinition definition)
        {
            string packFolder = $"{PacksRoot}/{spec.Pack}";
            string path = $"{packFolder}/{spec.Pack}.asset";

            var pack = AssetDatabase.LoadAssetAtPath<DreamPack>(path);
            if (pack == null)
            {
                Directory.CreateDirectory(packFolder);
                AssetDatabase.Refresh();
                pack = ScriptableObject.CreateInstance<DreamPack>();
                pack.Configure(spec.Pack);
                AssetDatabase.CreateAsset(pack, path);
            }

            bool changed = pack.AddOrReplace(definition);
            if (changed) EditorUtility.SetDirty(pack);
            return changed;
        }
    }
}
