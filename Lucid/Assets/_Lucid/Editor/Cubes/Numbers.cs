using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// Number reading that reports rather than throws. `ToObject&lt;float&gt;()`
    /// on a string or a null raises a FormatException or an ArgumentException,
    /// neither of which is a JsonException — so it would escape the reader's
    /// catch and reach the caller as a stack trace instead of a report line.
    /// </summary>
    internal static class Numbers
    {
        public static float Read(JToken token, string path, string what)
        {
            if (token.Type != JTokenType.Integer && token.Type != JTokenType.Float)
            {
                throw new JsonSerializationException(
                    $"{what} must be a number, found {Describe(token)}", path, 0, 0, null);
            }

            return token.ToObject<float>();
        }

        static string Describe(JToken token) =>
            token.Type == JTokenType.Null ? "null" : token.Type.ToString().ToLowerInvariant();
    }
}
