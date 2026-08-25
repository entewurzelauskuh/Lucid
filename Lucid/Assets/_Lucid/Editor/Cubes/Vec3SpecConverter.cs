using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// Reads the schema's three-number array form. Length and element type are
    /// both checked here, so a malformed vector is reported with its JSON path
    /// rather than thrown out of the reader.
    /// </summary>
    public sealed class Vec3SpecConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            objectType == typeof(Vec3Spec) || objectType == typeof(Vec3Spec?);

        public override object ReadJson(
            JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Capture the path before loading: Load advances the reader, and an
            // exception thrown from a converter does not get a path filled in
            // for it the way a serializer-level failure does.
            string path = reader.Path;
            return ReadArray(JToken.Load(reader), path);
        }

        internal static Vec3Spec ReadArray(JToken token, string path)
        {
            if (token.Type != JTokenType.Array)
                throw new JsonSerializationException(
                    $"expected [x, y, z], found {token.Type.ToString().ToLowerInvariant()}", path, 0, 0, null);

            var array = (JArray)token;
            if (array.Count != 3)
                throw new JsonSerializationException(
                    $"expected 3 numbers, found {array.Count}", path, 0, 0, null);

            return new Vec3Spec(
                Numbers.Read(array[0], path, "x"),
                Numbers.Read(array[1], path, "y"),
                Numbers.Read(array[2], path, "z"));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var v = (Vec3Spec)value;
            writer.WriteStartArray();
            writer.WriteValue(v.X);
            writer.WriteValue(v.Y);
            writer.WriteValue(v.Z);
            writer.WriteEndArray();
        }
    }
}
