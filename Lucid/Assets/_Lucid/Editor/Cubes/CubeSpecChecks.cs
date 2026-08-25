using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// The rules of docs/cube-spec.schema.json that Newtonsoft cannot enforce
    /// on its own: ranges, patterns, uniqueness, and the six cross-field rules
    /// in the schema's `allOf`. Every one is listed in docs/CUBE-SPEC.md §2.
    /// </summary>
    public static class CubeSpecChecks
    {
        // These four mirror docs/cube-spec.schema.json exactly. Diverging by
        // a character means rejecting a spec the schema allows.
        static readonly Regex IdPattern = new Regex(@"^[a-z0-9]+\.[a-z0-9_]+$");
        static readonly Regex PackPattern = new Regex(@"^[a-z0-9]+$");
        static readonly Regex PropNamePattern = new Regex(@"^[a-z][a-z0-9_]*$");
        static readonly Regex ComponentPattern = new Regex(@"^[A-Z][A-Za-z0-9]+$");
        static readonly Regex ColourPattern = new Regex(@"^(skin|#[0-9A-Fa-f]{6})$");

        public static void Run(CubeSpec spec, List<SpecProblem> problems)
        {
            Scalars(spec, problems);
            Connectors(spec, problems);
            CrossFieldRules(spec, problems);
            Props(spec, problems);
            Paths(spec, problems);
            Lighting(spec, problems);
        }

        static void Scalars(CubeSpec spec, List<SpecProblem> problems)
        {
            if (spec.SpecVersion != CubeSpec.CurrentSpecVersion)
                Add(problems, "specVersion",
                    $"must be {CubeSpec.CurrentSpecVersion}, found {spec.SpecVersion}");

            if (spec.Id == null || !IdPattern.IsMatch(spec.Id))
                Add(problems, "id", "must look like pack.cube, lower case, e.g. core.straight");
            else if (spec.Pack != null && !spec.Id.StartsWith(spec.Pack + "."))
                Add(problems, "id", $"must start with the pack name, '{spec.Pack}.'");

            if (spec.Pack == null || !PackPattern.IsMatch(spec.Pack))
                Add(problems, "pack", "must be lower case letters and digits only");

            if (spec.Name != null && spec.Name.Length > 40)
                Add(problems, "name", $"at most 40 characters, found {spec.Name.Length}");

            if (spec.Cost < 0 || spec.Cost > 20)
                Add(problems, "cost", $"must be between 0 and 20, found {spec.Cost}");

            if (spec.Shell != null)
            {
                if (spec.Shell.Thickness < 0.1f || spec.Shell.Thickness > 1.0f)
                    Add(problems, "shell.thickness",
                        $"must be between 0.1 and 1.0 m, found {spec.Shell.Thickness}");

                InteriorSpec interior = spec.Shell.Interior;
                if (interior?.Width != null && (interior.Width < 2.5f || interior.Width > 8f))
                    Add(problems, "shell.interior.width",
                        $"must be between 2.5 and 8 m, found {interior.Width}");
                if (interior?.Height != null && (interior.Height < 3f || interior.Height > 8f))
                    Add(problems, "shell.interior.height",
                        $"must be between 3 and 8 m, found {interior.Height}");
            }

            if (spec.Nav != null && (spec.Nav.AgentRadius < 0.2f || spec.Nav.AgentRadius > 1.0f))
                Add(problems, "nav.agentRadius",
                    $"must be between 0.2 and 1.0 m, found {spec.Nav.AgentRadius}");

            if (spec.Chicane?.Component != null && !ComponentPattern.IsMatch(spec.Chicane.Component))
                Add(problems, "chicane.component",
                    "must be a component name in PascalCase, e.g. Trapdoor");

            if (spec.WeakPoint != null && (spec.WeakPoint.Hp < 10 || spec.WeakPoint.Hp > 300))
                Add(problems, "weakPoint.hp",
                    $"must be between 10 and 300, found {spec.WeakPoint.Hp}");

            if (spec.Trigger != null)
            {
                if (spec.Trigger.CooldownMs < 1_000 || spec.Trigger.CooldownMs > 60_000)
                    Add(problems, "trigger.cooldownMs",
                        $"must be between 1000 and 60000 ms, found {spec.Trigger.CooldownMs}");
                if (spec.Trigger.Label != null && spec.Trigger.Label.Length > 24)
                    Add(problems, "trigger.label",
                        $"at most 24 characters, found {spec.Trigger.Label.Length}");
            }
        }

        static void Connectors(CubeSpec spec, List<SpecProblem> problems)
        {
            if (spec.Connectors == null || spec.Connectors.Length == 0)
            {
                Add(problems, "connectors", "a cube needs at least one doorway");
                return;
            }

            var seen = new HashSet<SpecFace>();
            foreach (SpecFace f in spec.Connectors)
            {
                if (!seen.Add(f)) Add(problems, "connectors", $"'{f.ToString().ToLowerInvariant()}' is listed twice");
            }
        }

        /// <summary>The schema's `allOf`, rule for rule.</summary>
        static void CrossFieldRules(CubeSpec spec, List<SpecProblem> problems)
        {
            int connectors = spec.Connectors?.Length ?? 0;

            if (spec.Category == SpecCategory.Start)
            {
                // The start cube is exempt from the rules and has one way out.
                if (connectors > 1)
                    Add(problems, "connectors",
                        $"a start cube has exactly one doorway, found {connectors}");
                if (spec.Cost != 0)
                    Add(problems, "cost", $"a start cube costs nothing, found {spec.Cost}");
            }
            else if (connectors < 2)
            {
                // Two connectors is what stops a placement ever sealing the
                // dream (docs/SPEC.md §7).
                Add(problems, "connectors",
                    $"every cube but the start cube needs at least two doorways, found {connectors}");
            }

            if (spec.Climbable && !HasConnector(spec, SpecFace.Up))
                Add(problems, "climbable", "needs 'up' in connectors: there is nothing to climb to");

            bool needsChicane = spec.Category == SpecCategory.Chicane || spec.Category == SpecCategory.Mob;
            if (needsChicane && spec.Chicane == null)
                Add(problems, "chicane", $"a {spec.Category.ToString().ToLowerInvariant()} cube needs a chicane block");
            if (needsChicane && spec.IntendedPaths == null)
                Add(problems, "intendedPaths", $"a {spec.Category.ToString().ToLowerInvariant()} cube needs intended paths");

            if (spec.Category == SpecCategory.Vertical && spec.IntendedPaths == null)
                Add(problems, "intendedPaths", "a vertical cube needs intended paths");

            if (spec.Trigger != null && spec.Chicane == null)
                Add(problems, "trigger", "there is nothing to trigger without a chicane");
            if (spec.WeakPoint != null && spec.Chicane == null)
                Add(problems, "weakPoint", "there is nothing to jam without a chicane");
        }

        static void Props(CubeSpec spec, List<SpecProblem> problems)
        {
            var names = new HashSet<string>();
            PropSpec[] props = spec.EffectiveProps;

            for (int i = 0; i < props.Length; i++)
            {
                PropSpec prop = props[i];
                string at = $"props[{i}]";

                if (prop == null)
                {
                    Add(problems, at, "must be an object, found null");
                    continue;
                }

                if (prop.Name == null || !PropNamePattern.IsMatch(prop.Name))
                    Add(problems, at + ".name", "must be lower case letters, digits and underscores");
                else if (!names.Add(prop.Name))
                    Add(problems, at + ".name", $"'{prop.Name}' is used by more than one prop");
            }

            // Everything that points at a prop has to find one.
            foreach (KillVolumeSpec volume in spec.EffectiveKillVolumes)
            {
                if (volume == null) Add(problems, "killVolumes", "must be an object, found null");
            }

            if (spec.WeakPoint?.Prop != null && !names.Contains(spec.WeakPoint.Prop))
                Add(problems, "weakPoint.prop", $"no prop named '{spec.WeakPoint.Prop}'");

            if (spec.Chicane?.Actors != null)
            {
                foreach (string actor in spec.Chicane.Actors)
                {
                    if (!names.Contains(actor))
                        Add(problems, "chicane.actors", $"no prop named '{actor}'");
                }
            }

            if (spec.Nav?.Exclude != null)
            {
                foreach (string excluded in spec.Nav.Exclude)
                {
                    if (!names.Contains(excluded))
                        Add(problems, "nav.exclude", $"no prop named '{excluded}'");
                }
            }
        }

        static void Paths(CubeSpec spec, List<SpecProblem> problems)
        {
            if (spec.IntendedPaths == null) return;

            for (int i = 0; i < spec.IntendedPaths.Length; i++)
            {
                IntendedPathSpec path = spec.IntendedPaths[i];
                string at = $"intendedPaths[{i}]";

                if (path == null)
                {
                    Add(problems, at, "must be an object, found null");
                    continue;
                }

                if (path.Points == null || path.Points.Length < 2)
                    Add(problems, at + ".points", "a path needs at least two points");

                if (!HasConnector(spec, path.From))
                    Add(problems, at + ".from",
                        $"'{path.From.ToString().ToLowerInvariant()}' is not a doorway of this cube");
                if (!HasConnector(spec, path.To))
                    Add(problems, at + ".to",
                        $"'{path.To.ToString().ToLowerInvariant()}' is not a doorway of this cube");
            }
        }

        static void Lighting(CubeSpec spec, List<SpecProblem> problems)
        {
            LightSpec[] lights = spec.Lighting?.Lights;
            if (lights == null) return;

            if (lights.Length > 4)
                Add(problems, "lighting.lights", $"at most 4 lights per cube, found {lights.Length}");

            for (int i = 0; i < lights.Length; i++)
            {
                string at = $"lighting.lights[{i}]";
                if (lights[i] == null)
                {
                    Add(problems, at, "must be an object, found null");
                    continue;
                }

                if (lights[i].Intensity != null && lights[i].Intensity < 0)
                    Add(problems, at + ".intensity", "cannot be negative");
                if (lights[i].Color != null && !ColourPattern.IsMatch(lights[i].Color))
                    Add(problems, at + ".color", "must be 'skin' or a #RRGGBB hex colour");
            }
        }

        static bool HasConnector(CubeSpec spec, SpecFace face)
        {
            if (spec.Connectors == null) return false;
            foreach (SpecFace f in spec.Connectors)
            {
                if (f == face) return true;
            }
            return false;
        }

        static void Add(List<SpecProblem> problems, string field, string message) =>
            problems.Add(new SpecProblem(field, message));
    }
}
