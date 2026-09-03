using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Lucid.Editor.Scenes
{
    /// <summary>
    /// A canonical description of a hierarchy: everything a generator decides,
    /// and nothing Unity decides for it.
    /// </summary>
    /// <remarks>
    /// Saving a scene mints fresh fileIDs and orders the YAML blocks by them,
    /// so an unchanged scene is written as a wholly different file — 1296 of
    /// 2116 lines for the gauntlet (#59). Comparing text cannot see past that,
    /// because normalising the ids still leaves the blocks in a new order.
    ///
    /// <see cref="Lucid.Editor.Cubes.CubeEquivalence"/> solved the same problem
    /// for prefabs by comparing a hand-written list of fields, and says in its
    /// own remarks that the list has to grow whenever the builder learns to
    /// emit something new. That is a trap: the failure is silent, and it looks
    /// exactly like success. So this walks the serialized properties instead:
    /// a generator can emit anything it likes and be compared on all of it,
    /// and the one thing that cannot happen is a value being quietly skipped —
    /// a property type with no arm to read it stops the build rather than
    /// signing as a constant.
    ///
    /// Two kinds of value are deliberately not taken literally. A fileID is
    /// Unity's own bookkeeping and differs on every save, so a reference to
    /// another object in the same hierarchy is recorded as that object's path,
    /// and a reference to an asset as its GUID.
    /// </remarks>
    public static class SceneSignature
    {
        /// <summary>
        /// Properties skipped because they carry Unity's own bookkeeping —
        /// fileIDs and hierarchy links that differ on every save.
        /// </summary>
        /// <remarks>
        /// The three prefab entries are the exception, and a known limit
        /// rather than bookkeeping: they record which prefab an object came
        /// from, so a generator that switched from building an object in code
        /// to instantiating an identical prefab would not be noticed. No
        /// generator does — <see cref="Lucid.Runtime.SleeperRig"/> builds in
        /// code on purpose — and skipping them is what keeps a fileID out of
        /// the signature.
        /// </remarks>
        static readonly HashSet<string> Ignored = new HashSet<string>(StringComparer.Ordinal)
        {
            "m_ObjectHideFlags",
            "m_CorrespondingSourceObject",
            "m_PrefabInstance",
            "m_PrefabAsset",
            "m_GameObject",
            "m_Father",
            "m_Children",
            "m_LocalIdentfierInFile",
            "m_RootOrder",
            "m_Component",
        };

        public static string Of(IEnumerable<GameObject> roots)
        {
            return Join(Blocks(Roots(roots), t => t.name));
        }

        /// <summary>
        /// One rendered block per node, sorted by content. Sorting the text
        /// rather than the objects makes the order total: two nodes with the
        /// same name in the same place would otherwise land in whatever order
        /// an unstable sort left them.
        /// </summary>
        static List<string> Blocks(List<Transform> nodes, Func<Transform, string> path)
        {
            var blocks = new List<string>(nodes.Count);
            foreach (Transform node in nodes)
            {
                var sb = new StringBuilder();
                Append(sb, node, path(node));
                blocks.Add(sb.ToString());
            }
            blocks.Sort(StringComparer.Ordinal);
            return blocks;
        }

        static string Join(List<string> blocks) => string.Concat(blocks);

        static List<Transform> Roots(IEnumerable<GameObject> roots)
        {
            var list = new List<Transform>();
            foreach (GameObject go in roots)
                if (go != null) list.Add(go.transform);
            return list;
        }

        static void Append(StringBuilder sb, Transform t, string path)
        {
            sb.Append("N ").Append(Escape(path))
              .Append(" p").Append(V(t.localPosition))
              .Append(" r").Append(V(t.localRotation.eulerAngles))
              .Append(" s").Append(V(t.localScale))
              .Append(" active=").Append(t.gameObject.activeSelf)
              .Append(" layer=").Append(t.gameObject.layer)
              .Append('\n');

            // The GameObject's own properties: tag, static flags, icon. It is
            // not a Component, so walking the components alone never saw them.
            Append(sb, t.gameObject, path, "G");

            foreach (Component component in Components(t.gameObject))
                Append(sb, component, path, "C");

            foreach (string block in Blocks(Children(t), c => path + "/" + c.name))
                sb.Append(block);
        }

        static void Append(StringBuilder sb, UnityEngine.Object target, string path, string kind)
        {
            sb.Append(kind).Append(' ').Append(Escape(path)).Append(' ')
              .Append(target.GetType().FullName);

            // Whether a component is switched on is serialized but drawn in the
            // component header rather than as a field, so NextVisible never
            // reaches it. Measured: disabling a BoxCollider left the signature
            // identical.
            if (Enabled(target) is bool on) sb.Append(" enabled=").Append(on);
            sb.Append('\n');

            // Transform's own values are recorded on the node line, and the
            // rest of its property set is hierarchy bookkeeping. RectTransform
            // is not exempt: its anchors and sizeDelta move nothing that the
            // node line records.
            if (target.GetType() == typeof(Transform)) return;

            using var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.GetIterator();
            while (property.NextVisible(true))
            {
                if (Ignored.Contains(property.name)) continue;
                sb.Append("  ").Append(property.propertyPath).Append('=')
                  .Append(Escape(Value(property))).Append('\n');
            }
        }

        static bool? Enabled(UnityEngine.Object target)
        {
            switch (target)
            {
                case Behaviour b: return b.enabled;
                case Collider c: return c.enabled;
                case Renderer r: return r.enabled;
                default: return null;
            }
        }

        /// <summary>
        /// The value of one property, as text.
        /// </summary>
        /// <remarks>
        /// Every arm must read the value. An arm that returns something
        /// constant — the name of the type, say — hides that property from the
        /// comparison, so a generator changing it would stop rewriting the
        /// scene and leave a stale one committed. That is the silent failure
        /// this whole file exists to avoid, and the first draft walked into it:
        /// the default arm returned the type name, which made a Light's
        /// culling mask invisible. Unknown types therefore throw rather than
        /// return anything at all.
        /// </remarks>
        static string Value(SerializedProperty p)
        {
            switch (p.propertyType)
            {
                case SerializedPropertyType.ObjectReference: return Reference(p.objectReferenceValue);
                case SerializedPropertyType.ExposedReference: return Reference(p.exposedReferenceValue);
                case SerializedPropertyType.Float: return F(p.floatValue);
                case SerializedPropertyType.Vector2: return V2(p.vector2Value);
                case SerializedPropertyType.Vector3: return V(p.vector3Value);
                case SerializedPropertyType.Vector4: return V4(p.vector4Value);
                case SerializedPropertyType.Quaternion: return V(p.quaternionValue.eulerAngles);
                case SerializedPropertyType.Rect: return R(p.rectValue);
                case SerializedPropertyType.Bounds:
                    return V(p.boundsValue.center) + V(p.boundsValue.extents);
                case SerializedPropertyType.Vector2Int: return p.vector2IntValue.ToString();
                case SerializedPropertyType.Vector3Int: return p.vector3IntValue.ToString();
                case SerializedPropertyType.RectInt: return p.rectIntValue.ToString();
                case SerializedPropertyType.BoundsInt: return p.boundsIntValue.ToString();
                case SerializedPropertyType.Color: return C(p.colorValue);
                case SerializedPropertyType.Boolean: return p.boolValue.ToString();
                case SerializedPropertyType.String: return p.stringValue;
                case SerializedPropertyType.AnimationCurve: return Curve(p.animationCurveValue);
                case SerializedPropertyType.Hash128: return p.hash128Value.ToString();
                case SerializedPropertyType.ManagedReference: return p.managedReferenceFullTypename;

                // All stored as integers, and all read for their value. An
                // enum read as enumValueIndex would be the position in the
                // names array, which is not the value a [Flags] enum holds.
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.RenderingLayerMask:
                    return I(p.longValue);

                // A container: NextVisible descends into the parts that carry
                // the values, and each is rendered by an arm above.
                case SerializedPropertyType.Generic: return "";

                default:
                    throw new NotSupportedException(
                        $"SceneSignature cannot read {p.propertyType} at '{p.propertyPath}'. " +
                        "Add an arm that reads its value — returning anything constant would " +
                        "hide the property from the comparison instead.");
            }
        }

        static string Curve(AnimationCurve curve)
        {
            if (curve == null) return "null";
            var sb = new StringBuilder("curve");
            foreach (Keyframe k in curve.keys)
                sb.Append('[').Append(F(k.time)).Append(',').Append(F(k.value))
                  .Append(',').Append(F(k.inTangent)).Append(',').Append(F(k.outTangent)).Append(']');
            return sb.ToString();
        }

        /// <summary>
        /// An asset becomes its GUID and a scene object its path, because a
        /// fileID is exactly the thing that changes when nothing else has.
        /// </summary>
        static string Reference(UnityEngine.Object target)
        {
            if (target == null) return "null";

            // The GUID alone is not enough: every built-in mesh lives in one
            // file, so Cube and Sphere share a GUID and signed identically.
            // The local id is what tells them apart.
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(target, out string guid, out long localId))
                return $"asset:{guid}:{localId}";

            if (target is Component c) return "scene:" + Path(c.transform) + ":" + c.GetType().Name;
            if (target is GameObject go) return "scene:" + Path(go.transform);
            return "object:" + target.GetType().FullName;
        }

        /// <summary>
        /// Renders a value so it cannot be mistaken for the structure around
        /// it. A GameObject name may contain a slash and a string field may
        /// contain a newline, either of which could otherwise forge a path or
        /// a node line and make two different hierarchies sign alike.
        /// </summary>
        static string Escape(string value) => value == null
            ? "null"
            : value.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("/", "\\s");

        static string Path(Transform t)
        {
            var parts = new List<string>();
            for (Transform current = t; current != null; current = current.parent)
                parts.Add(current.name);
            parts.Reverse();
            return string.Join("/", parts);
        }

        static List<Component> Components(GameObject go)
        {
            var list = new List<Component>();
            foreach (Component c in go.GetComponents<Component>())
                if (c != null) list.Add(c);
            // A generator's emission order is not part of what it generated.
            list.Sort((x, y) => string.CompareOrdinal(x.GetType().FullName, y.GetType().FullName));
            return list;
        }

        static List<Transform> Children(Transform t)
        {
            var list = new List<Transform>(t.childCount);
            foreach (Transform child in t) list.Add(child);
            return list;
        }

        static string V(Vector3 v) => $"({F(v.x)},{F(v.y)},{F(v.z)})";

        static string V2(Vector2 v) => $"({F(v.x)},{F(v.y)})";

        static string V4(Vector4 v) => $"({F(v.x)},{F(v.y)},{F(v.z)},{F(v.w)})";

        static string R(Rect r) => $"({F(r.x)},{F(r.y)},{F(r.width)},{F(r.height)})";

        static string C(Color c) => $"({F(c.r)},{F(c.g)},{F(c.b)},{F(c.a)})";

        static string I(long v) => v.ToString(CultureInfo.InvariantCulture);

        // Rounded, so a rebuild is not defeated by the last bit of a float.
        static string F(float f) => Mathf.Abs(f) < 1e-4f
            ? "0"
            : f.ToString("0.0000", CultureInfo.InvariantCulture);
    }
}
