using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// Reads and checks `cube.spec.json`. The builder runs this before doing
    /// anything else (docs/WORKPLAN.md §4), so a bad spec never reaches the
    /// asset database.
    /// </summary>
    /// <remarks>
    /// This is a checker written against docs/cube-spec.schema.json, not a
    /// JSON-Schema engine running that file — no such engine is available in
    /// Unity or in the local Python. Unknown and missing members come from
    /// Newtonsoft; ranges, patterns and the schema's six cross-field rules are
    /// coded in <see cref="CubeSpecChecks"/>. See docs/DECISIONS.md.
    /// </remarks>
    public static class CubeSpecReader
    {
        static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            // "additionalProperties": false, for free and with a JSON path.
            MissingMemberHandling = MissingMemberHandling.Error,
            Converters = { new StrictEnumConverter() },
            DateParseHandling = DateParseHandling.None,
        };

        public static CubeSpecResult ReadFile(string path)
        {
            if (!File.Exists(path))
                return CubeSpecResult.Failure(path, "no cube.spec.json here");

            try
            {
                return Read(File.ReadAllText(path));
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException
                                      || e is ArgumentException || e is NotSupportedException)
            {
                return CubeSpecResult.Failure(path, "could not be read: " + e.Message);
            }
        }

        public static CubeSpecResult Read(string json)
        {
            CubeSpec spec;
            try
            {
                spec = JsonConvert.DeserializeObject<CubeSpec>(json, Settings);
            }
            catch (JsonException e)
            {
                return CubeSpecResult.Failure(PathOf(e), Explain(e));
            }
            catch (Exception e)
            {
                // A spec is untrusted input. Whatever it does to the
                // deserializer, the caller gets a report line: the build loop
                // is read report, fix, rebuild, and a stack trace out of
                // batch mode breaks it (docs/CUBE-SPEC.md §5).
                return CubeSpecResult.Failure("", "could not be read: " + e.Message);
            }

            if (spec == null) return CubeSpecResult.Failure("", "the file is empty");

            var problems = new List<SpecProblem>();
            CubeSpecChecks.Run(spec, problems);
            return problems.Count == 0 ? CubeSpecResult.Success(spec) : CubeSpecResult.Failure(problems);
        }

        /// <summary>
        /// Newtonsoft puts the offending member in the message and the
        /// enclosing object in Path, so for a missing or unknown member the
        /// message is the only place the field name appears. Prefer it.
        /// </summary>
        static string PathOf(JsonException e)
        {
            string enclosing =
                e is JsonSerializationException s && !string.IsNullOrEmpty(s.Path) ? s.Path
                : e is JsonReaderException r && !string.IsNullOrEmpty(r.Path) ? r.Path
                : "";

            // Newtonsoft names the offending member in the message and the
            // enclosing object in Path. Neither alone is enough: with a dozen
            // props, "asset: required, but missing" does not say which one.
            string named = NamedMember(e.Message);
            if (named == null) return enclosing;
            return string.IsNullOrEmpty(enclosing) ? named : enclosing + "." + named;
        }

        /// <summary>
        /// Newtonsoft's own wording leaks type names and line numbers into the
        /// author's face; these are the two cases worth rephrasing.
        /// </summary>
        static string Explain(JsonException e)
        {
            string message = e.Message;
            if (message.Contains("Could not find member"))
                return "not a field this schema defines";
            if (message.Contains("Required property"))
                return "required, but missing";
            return message;
        }

        static readonly Regex MemberInMessage = new Regex(
            @"(?:Required property|Could not find member) '([^']+)'");

        static string NamedMember(string message)
        {
            Match m = MemberInMessage.Match(message ?? string.Empty);
            return m.Success ? m.Groups[1].Value : null;
        }
    }
}
