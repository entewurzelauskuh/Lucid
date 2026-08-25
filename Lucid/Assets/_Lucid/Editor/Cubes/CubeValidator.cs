using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lucid.Core;
using Lucid.Runtime;
using UnityEngine;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// Checks a built cube against the rules `docs/WORKPLAN.md` §4 sets for
    /// M0.3: bounds, connectors at the standard positions with fog doors,
    /// shell collision, and the licence ledger.
    /// </summary>
    /// <remarks>
    /// Navmesh, intended-path measurement and the triangle budget are
    /// `docs/CUBE-SPEC.md` §2 rules that arrive with M1.8; the triangle count
    /// is reported here as information rather than enforced.
    /// </remarks>
    public static class CubeValidator
    {
        public static ValidationReport Validate(GameObject prefab, CubeSpec spec, string cubeFolder)
        {
            var report = new ValidationReport
            {
                Cube = spec.Id,
                Spec = $"{cubeFolder}/cube.spec.json",
                Prefab = $"{cubeFolder}/{Path.GetFileName(cubeFolder)}.prefab",
            };

            if (prefab == null)
            {
                report.Add("prefab", "no prefab was built");
                return report;
            }

            Bounds(prefab, spec, report);
            Connectors(prefab, spec, report);
            ShellCollision(prefab, report);
            Licences(cubeFolder, report);

            report.Triangles = prefab.GetComponentsInChildren<MeshFilter>()
                .Where(f => f.sharedMesh != null)
                .Sum(f => f.sharedMesh.triangles.Length / 3);

            return report;
        }

        /// <summary>
        /// Everything inside the 8 m the cube owns. Not y in [0, 8]: the floor
        /// slab hangs below the origin so the walkable surface is the origin
        /// plane, and the ceiling stops the same distance short of the top
        /// (docs/CUBE-SPEC.md §1, docs/DECISIONS.md).
        /// </summary>
        static void Bounds(GameObject prefab, CubeSpec spec, ValidationReport report)
        {
            float t = CubeGeometry.Thickness(spec.Shell);
            float half = CubeGeometry.Half;
            float low = -t, high = CubeGeometry.Size - t;
            const float slack = 1e-3f;

            foreach (Renderer r in prefab.GetComponentsInChildren<Renderer>())
            {
                Bounds b = LocalBounds(prefab.transform, r);
                if (b.min.x < -half - slack || b.max.x > half + slack ||
                    b.min.z < -half - slack || b.max.z > half + slack ||
                    b.min.y < low - slack || b.max.y > high + slack)
                {
                    report.Add("bounds",
                        $"'{PathOf(prefab.transform, r.transform)}' reaches {Describe(b)}, " +
                        $"outside x,z [{-half}, {half}] and y [{low}, {high}]");
                }
            }
        }

        /// <summary>
        /// Every connector the spec declares is a socket at its standard
        /// position carrying a fog door, and there are at least two of them —
        /// the rule that stops a placement ever sealing the dream
        /// (docs/SPEC.md §7). The start cube is the sole exception.
        /// </summary>
        static void Connectors(GameObject prefab, CubeSpec spec, ValidationReport report)
        {
            Transform sockets = prefab.transform.Find("Sockets");
            if (sockets == null)
            {
                report.Add("connectors", "the prefab has no Sockets");
                return;
            }

            int doorways = 0;
            foreach (Face face in Faces.All)
            {
                Transform socket = sockets.Find(face.ToString());
                if (socket == null)
                {
                    report.Add("connectors", $"no socket for {face}");
                    continue;
                }

                Vector3 expected = CubeGeometry.Centre(face);
                if ((socket.localPosition - expected).sqrMagnitude > 1e-6f)
                {
                    report.Add("connectors",
                        $"{face} socket is at {socket.localPosition}, not {expected}");
                }

                var connector = socket.GetComponent<Connector>();
                if (connector == null)
                {
                    report.Add("connectors", $"{face} socket has no Connector");
                    continue;
                }

                if (connector.Door == null)
                    report.Add("connectors", $"{face} socket has no FogDoor");

                if (connector.IsDoorway) doorways++;
            }

            bool isStart = spec.Category == SpecCategory.Start;
            if (isStart && doorways != 1)
                report.Add("connectors", $"a start cube has one doorway, found {doorways}");
            else if (!isStart && doorways < 2)
                report.Add("connectors",
                    $"every cube but the start cube needs two doorways, found {doorways}");
        }

        /// <summary>
        /// A Sleeper has to be able to stand on it. A shell with meshes and no
        /// colliders looks correct in a preview and is walked straight through.
        /// </summary>
        static void ShellCollision(GameObject prefab, ValidationReport report)
        {
            Transform shell = prefab.transform.Find("Shell");
            if (shell == null)
            {
                report.Add("collision", "the prefab has no Shell");
                return;
            }

            var missing = new List<string>();
            foreach (MeshFilter filter in shell.GetComponentsInChildren<MeshFilter>())
            {
                if (filter.GetComponent<Collider>() == null)
                    missing.Add(filter.name);
            }

            if (missing.Count > 0)
                report.Add("collision", "no collider on " + string.Join(", ", missing));

            if (shell.GetComponentsInChildren<MeshFilter>().Length == 0)
                report.Add("collision", "the shell is empty");
        }

        /// <summary>
        /// Only CC0 and CC-BY assets may be committed, each with a line in the
        /// cube's ledger (CLAUDE.md rule 5). The pre-commit hook enforces the
        /// same rule; running it here means an author finds out at build time
        /// rather than at commit time.
        /// </summary>
        static void Licences(string cubeFolder, ValidationReport report)
        {
            string assets = Path.Combine(cubeFolder, "assets");
            if (!Directory.Exists(assets)) return;

            string ledgerPath = Path.Combine(assets, "LICENSES.md");
            string ledger = File.Exists(ledgerPath) ? File.ReadAllText(ledgerPath) : null;

            foreach (string file in Directory.GetFiles(assets))
            {
                string name = Path.GetFileName(file);
                if (name == "LICENSES.md" || name.EndsWith(".meta")) continue;

                if (ledger == null)
                {
                    report.Add("licences", $"'{name}' is committed but there is no assets/LICENSES.md");
                    continue;
                }

                string line = ledger.Split('\n').FirstOrDefault(l => Names(l, name));
                if (line == null)
                    report.Add("licences", $"'{name}' has no line in assets/LICENSES.md");
                else if (!System.Text.RegularExpressions.Regex.IsMatch(line, @"\bCC0\b|\bCC-?BY\b",
                             System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    report.Add("licences",
                        $"'{name}' is not CC0 or CC-BY, so it cannot be committed -> {line.Trim()}");
            }
        }

        /// <summary>Whole-token match, so a.png does not match aa.png.</summary>
        static bool Names(string line, string name) =>
            System.Text.RegularExpressions.Regex.IsMatch(
                line, $@"(?<![\w.\-]){System.Text.RegularExpressions.Regex.Escape(name)}(?![\w.\-])");

        static Bounds LocalBounds(Transform root, Renderer r)
        {
            var mesh = r.GetComponent<MeshFilter>();
            Bounds local = mesh != null && mesh.sharedMesh != null
                ? mesh.sharedMesh.bounds
                : new Bounds(Vector3.zero, Vector3.one);

            Matrix4x4 toRoot = root.worldToLocalMatrix * r.transform.localToWorldMatrix;
            var result = new Bounds(toRoot.MultiplyPoint3x4(local.center), Vector3.zero);

            Vector3 e = local.extents;
            for (int i = 0; i < 8; i++)
            {
                var corner = new Vector3(
                    (i & 1) == 0 ? -e.x : e.x,
                    (i & 2) == 0 ? -e.y : e.y,
                    (i & 4) == 0 ? -e.z : e.z);
                result.Encapsulate(toRoot.MultiplyPoint3x4(local.center + corner));
            }
            return result;
        }

        static string Describe(Bounds b) =>
            $"({b.min.x:0.##}, {b.min.y:0.##}, {b.min.z:0.##})-({b.max.x:0.##}, {b.max.y:0.##}, {b.max.z:0.##})";

        static string PathOf(Transform root, Transform t)
        {
            var parts = new List<string>();
            for (Transform c = t; c != null && c != root; c = c.parent) parts.Add(c.name);
            parts.Reverse();
            return string.Join("/", parts);
        }
    }
}
