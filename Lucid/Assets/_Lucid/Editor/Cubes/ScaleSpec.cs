using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// `props[].scale`: the schema allows a single number or a vec3, defaulting
    /// to 1. Both forms collapse to a vector here so the builder never has to
    /// care which the author wrote.
    /// </summary>
    [JsonConverter(typeof(ScaleSpecConverter))]
    public readonly struct ScaleSpec
    {
        public static readonly ScaleSpec One = new ScaleSpec(1f, 1f, 1f);

        public ScaleSpec(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public bool IsUniform => X == Y && Y == Z;

        public override string ToString() => IsUniform ? X.ToString() : $"[{X}, {Y}, {Z}]";
    }

    /// <summary>Reads either form, and enforces the schema's exclusiveMinimum of 0.</summary>
    public sealed class ScaleSpecConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(ScaleSpec);

        public override object ReadJson(
            JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string path = reader.Path;
            JToken token = JToken.Load(reader);

            if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
            {
                float uniform = Numbers.Read(token, path, "scale");
                if (uniform <= 0)
                    throw new JsonSerializationException($"must be greater than 0, found {uniform}", path, 0, 0, null);
                return new ScaleSpec(uniform, uniform, uniform);
            }

            Vec3Spec v = Vec3SpecConverter.ReadArray(token, path);
            if (v.X <= 0 || v.Y <= 0 || v.Z <= 0)
                throw new JsonSerializationException($"every axis must be greater than 0, found {v}", path, 0, 0, null);
            return new ScaleSpec(v.X, v.Y, v.Z);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var s = (ScaleSpec)value;
            if (s.IsUniform)
            {
                writer.WriteValue(s.X);
                return;
            }
            writer.WriteStartArray();
            writer.WriteValue(s.X);
            writer.WriteValue(s.Y);
            writer.WriteValue(s.Z);
            writer.WriteEndArray();
        }
    }
}
