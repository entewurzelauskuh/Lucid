using Newtonsoft.Json;

namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// One thing wrong with a built cube. <see cref="Rule"/> names the
    /// validator rule so the report groups by cause rather than by object.
    /// </summary>
    public sealed class ValidationProblem
    {
        public ValidationProblem(string rule, string message)
        {
            Rule = rule;
            Message = message;
        }

        [JsonProperty("rule")] public string Rule { get; }
        [JsonProperty("message")] public string Message { get; }

        public override string ToString() => $"{Rule}: {Message}";
    }
}
