using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// Reads the schema's lower-case enum values, case-sensitively.
    /// </summary>
    /// <remarks>
    /// Newtonsoft's own StringEnumConverter matches case-insensitively, which
    /// would accept "Connector" where docs/cube-spec.schema.json allows only
    /// "connector". Being strict here means the schema's enums mean exactly
    /// what they say, and it lets the failure carry the JSON path.
    /// </remarks>
    public sealed class StrictEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            (Nullable.GetUnderlyingType(objectType) ?? objectType).IsEnum;

        public override object ReadJson(
            JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Type type = Nullable.GetUnderlyingType(objectType) ?? objectType;
            string path = reader.Path;

            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonSerializationException(
                    $"expected one of {Allowed(type)}, found {reader.TokenType}", path, 0, 0, null);
            }

            var text = (string)reader.Value;
            foreach (string name in Enum.GetNames(type))
            {
                if (string.Equals(Wire(name), text, StringComparison.Ordinal)) return Enum.Parse(type, name);
            }

            throw new JsonSerializationException(
                $"'{text}' is not one of {Allowed(type)}", path, 0, 0, null);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            writer.WriteValue(Wire(value.ToString()));

        /// <summary>The schema spells enum members in camelCase, mostly lower case.</summary>
        static string Wire(string name) => char.ToLowerInvariant(name[0]) + name.Substring(1);

        static string Allowed(Type type) =>
            string.Join(", ", Enum.GetNames(type).Select(n => "'" + Wire(n) + "'"));
    }
}
