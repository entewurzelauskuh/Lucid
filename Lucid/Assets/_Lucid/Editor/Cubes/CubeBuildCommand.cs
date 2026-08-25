using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// The batch-mode entry point behind `tools/build-cube.sh`. Reads
    /// `-cubeTarget &lt;pack&gt;[/&lt;cube&gt;]` and exits non-zero if anything
    /// failed, so the shell can tell.
    /// </summary>
    public static class CubeBuildCommand
    {
        public static void Run()
        {
            int code = 1;
            try
            {
                code = Execute(Argument("-cubeTarget"));
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("build-cube: " + e);
            }
            finally
            {
                EditorApplication.Exit(code);
            }
        }

        static int Execute(string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                Console.Error.WriteLine("build-cube: no -cubeTarget given");
                return 2;
            }

            // The template is generated, so a fresh clone has none until
            // something asks for it (CLAUDE.md rule 4). It is also rebuilt when
            // it no longer matches the code, because otherwise a change to the
            // socket layout would silently produce every cube from the stale
            // one — and CLAUDE.md forbids opening the editor to fix it by hand.
            if (CubeTemplateBuilder.IsStale())
            {
                Console.WriteLine("rebuilt " + CubeTemplateBuilder.Build());
            }

            List<string> specs = FindSpecs(target);
            if (specs.Count == 0)
            {
                Console.Error.WriteLine($"build-cube: no cube.spec.json under '{target}'");
                return 2;
            }

            var failures = new List<CubeBuildResult>();
            foreach (string spec in specs)
            {
                CubeBuildResult result = CubeBuilder.BuildFromSpec(spec);
                Console.WriteLine(result.Describe());
                if (!result.Ok) failures.Add(result);
            }

            AssetDatabase.SaveAssets();
            Console.WriteLine($"build-cube: {specs.Count - failures.Count}/{specs.Count} built");
            return failures.Count == 0 ? 0 : 1;
        }

        /// <summary>`&lt;pack&gt;` builds a whole pack; `&lt;pack&gt;/&lt;cube&gt;` builds one.</summary>
        static List<string> FindSpecs(string target)
        {
            // The target reaches a path, so it may not climb out of the packs
            // root: "core/../../.." would otherwise build every spec in the
            // project.
            if (target.Contains("..") || target.StartsWith("/") || target.Contains("\\"))
            {
                Console.Error.WriteLine($"build-cube: '{target}' is not a pack or cube name");
                return new List<string>();
            }

            string root = $"{CubeBuilder.PacksRoot}/{target.Split('/')[0]}/Cubes";
            if (target.Contains("/")) root += "/" + target.Substring(target.IndexOf('/') + 1);

            if (!Directory.Exists(root)) return new List<string>();

            return Directory.GetFiles(root, "cube.spec.json", SearchOption.AllDirectories)
                .Select(p => p.Replace('\\', '/'))
                .OrderBy(p => p, StringComparer.Ordinal)
                .ToList();
        }

        static string Argument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name) return args[i + 1];
            }
            return null;
        }
    }
}
