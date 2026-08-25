using Newtonsoft.Json;
using UnityEngine;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// A `vec3` from the schema: exactly three numbers, in the cube-local frame
    /// (x east, y up, z north; docs/CUBE-SPEC.md §1).
    /// </summary>
    [JsonConverter(typeof(Vec3SpecConverter))]
    public readonly struct Vec3Spec
    {
        public Vec3Spec(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public Vector3 ToVector3() => new Vector3(X, Y, Z);

        public override string ToString() => $"[{X}, {Y}, {Z}]";
    }
}
