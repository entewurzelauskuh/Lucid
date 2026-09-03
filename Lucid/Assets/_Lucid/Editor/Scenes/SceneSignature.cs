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
    /// exactly like success. So this walks the serialized properties instead,
    /// which means a generator can emit anything at all and still be compared
    /// correctly.
    ///
    /// Two kinds of value are deliberately not taken literally. A fileID is
    /// Unity's own bookkeeping and differs on every save, so a reference to
    /// another object in the same hierarchy is recorded as that object's path,
    /// and a reference to an asset as its GUID.
    /// </remarks>
    public static class SceneSignature
    {
        /// <summary>Properties that carry Unity's bookkeeping rather than content.</summary>
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
            sb.Append("N ").Append(path)
              .Append(" p").Append(V(t.localPosition))
              .Append(" r").Append(V(t.localRotation.eulerAngles))
              .Append(" s").Append(V(t.localScale))
              .Append(" active=").Append(t.gameObject.activeSelf)
              .Append(" layer=").Append(t.gameObject.layer)
              .Append('\n');

            foreach (Component component in Components(t.gameObject))
                Append(sb, component, path);

            foreach (string block in Blocks(Children(t), c => path + "/" + c.name))
                sb.Append(block);
        }

        static void Append(StringBuilder sb, Component component, string path)
        {
            sb.Append("C ").Append(path).Append(' ').Append(component.GetType().FullName).Append('\n');

            // Transform's own values are already recorded above, and its
            // property set is full of hierarchy bookkeeping.
            if (component is Transform) return;

            var serialized = new SerializedObject(component);
            SerializedProperty property = serialized.GetIterator();
            while (property.NextVisible(true))
            {
                if (Ignored.Contains(property.name)) continue;
                sb.Append("  ").Append(property.propertyPath).Append('=')
                  .Append(Value(property)).Append('\n');
            }
        }

        static string Value(SerializedProperty p)
        {
            switch (p.propertyType)
            {
                case SerializedPropertyType.ObjectReference: return Reference(p.objectReferenceValue);
                case SerializedPropertyType.Float: return F(p.floatValue);
                case SerializedPropertyType.Vector3: return V(p.vector3Value);
                case SerializedPropertyType.Vector2: return F(p.vector2Value.x) + "," + F(p.vector2Value.y);
                case SerializedPropertyType.Quaternion: return V(p.quaternionValue.eulerAngles);
                case SerializedPropertyType.Color: return p.colorValue.ToString();
                case SerializedPropertyType.Integer: return p.longValue.ToString(CultureInfo.InvariantCulture);
                case SerializedPropertyType.Boolean: return p.boolValue.ToString();
                case SerializedPropertyType.String: return p.stringValue;
                case SerializedPropertyType.Enum: return p.enumValueIndex.ToString(CultureInfo.InvariantCulture);
                case SerializedPropertyType.Generic: return "";      // its children are visited
                default: return p.propertyType.ToString();
            }
        }

        /// <summary>
        /// An asset becomes its GUID and a scene object its path, because a
        /// fileID is exactly the thing that changes when nothing else has.
        /// </summary>
        static string Reference(UnityEngine.Object target)
        {
            if (target == null) return "null";

            string assetPath = AssetDatabase.GetAssetPath(target);
            if (!string.IsNullOrEmpty(assetPath))
                return "asset:" + AssetDatabase.AssetPathToGUID(assetPath);

            if (target is Component c) return "scene:" + Path(c.transform) + ":" + c.GetType().Name;
            if (target is GameObject go) return "scene:" + Path(go.transform);
            return "object:" + target.GetType().FullName;
        }

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

        // Rounded, so a rebuild is not defeated by the last bit of a float.
        static string F(float f) => Mathf.Abs(f) < 1e-4f
            ? "0"
            : f.ToString("0.0000", CultureInfo.InvariantCulture);
    }
}
