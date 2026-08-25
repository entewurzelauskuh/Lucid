using System.Collections.Generic;
using System.Linq;
using Lucid.Runtime;
using UnityEngine;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// Whether a freshly generated cube is the same cube as the one already on
    /// disk.
    /// </summary>
    /// <remarks>
    /// The builder cannot simply write every time and call that idempotent.
    /// Unity assigns fresh random fileIDs on each `SaveAsPrefabAsset` and
    /// orders the YAML blocks by them, so an identical cube produces a
    /// completely different file — hundreds of changed lines, a useless diff,
    /// and needless git churn on every rebuild of a pack.
    ///
    /// Comparing the text does not work either: normalising the ids still
    /// leaves the blocks in a different order. So the comparison is semantic —
    /// walk both hierarchies and compare what the builder actually generates.
    /// Anything the builder does not set is not compared, which is why the list
    /// below has to grow whenever the builder learns to emit something new.
    /// </remarks>
    public static class CubeEquivalence
    {
        const float Epsilon = 1e-4f;

        public static bool Matches(GameObject built, GameObject existing) =>
            existing != null && SameNode(built.transform, existing.transform);

        static bool SameNode(Transform a, Transform b)
        {
            if (a.name != b.name) return false;

            if (!Near(a.localPosition, b.localPosition)) return false;
            if (!Near(a.localScale, b.localScale)) return false;
            if (Quaternion.Angle(a.localRotation, b.localRotation) > 0.01f) return false;

            if (!SameComponents(a.gameObject, b.gameObject)) return false;

            if (a.childCount != b.childCount) return false;

            // By name, so a reordered hierarchy still compares equal — the
            // builder's emission order is not part of the cube.
            List<Transform> ca = Children(a), cb = Children(b);
            for (int i = 0; i < ca.Count; i++)
            {
                if (!SameNode(ca[i], cb[i])) return false;
            }
            return true;
        }

        /// <summary>
        /// The component types, and the fields the builder sets on them.
        /// </summary>
        static bool SameComponents(GameObject a, GameObject b)
        {
            string[] ta = TypeNames(a), tb = TypeNames(b);
            if (!ta.SequenceEqual(tb)) return false;

            var roleA = a.GetComponent<MaterialRole>();
            var roleB = b.GetComponent<MaterialRole>();
            if ((roleA == null) != (roleB == null)) return false;
            if (roleA != null && roleA.Role != roleB.Role) return false;

            var conA = a.GetComponent<Connector>();
            var conB = b.GetComponent<Connector>();
            if ((conA == null) != (conB == null)) return false;
            if (conA != null && (conA.Face != conB.Face || conA.IsDoorway != conB.IsDoorway)) return false;
            if (conA != null && (conA.Door == null) != (conB.Door == null)) return false;

            var doorA = a.GetComponent<FogDoor>();
            var doorB = b.GetComponent<FogDoor>();
            if ((doorA == null) != (doorB == null)) return false;
            if (doorA != null && (doorA.Face != doorB.Face || doorA.State != doorB.State)) return false;

            var boundsA = a.GetComponent<CubeBounds>();
            var boundsB = b.GetComponent<CubeBounds>();
            if ((boundsA == null) != (boundsB == null)) return false;
            if (boundsA != null &&
                (Mathf.Abs(boundsA.Size - boundsB.Size) > Epsilon ||
                 Mathf.Abs(boundsA.FloorDrop - boundsB.FloorDrop) > Epsilon)) return false;

            // The mesh a primitive carries is decided by its type, which the
            // type list above already pins.
            return true;
        }

        static string[] TypeNames(GameObject go) =>
            go.GetComponents<Component>()
                .Where(c => c != null)
                .Select(c => c.GetType().FullName)
                .OrderBy(n => n, System.StringComparer.Ordinal)
                .ToArray();

        static List<Transform> Children(Transform t)
        {
            var list = new List<Transform>(t.childCount);
            foreach (Transform child in t) list.Add(child);
            list.Sort((x, y) => string.CompareOrdinal(x.name, y.name));
            return list;
        }

        static bool Near(Vector3 a, Vector3 b) => (a - b).sqrMagnitude <= Epsilon * Epsilon;
    }
}
