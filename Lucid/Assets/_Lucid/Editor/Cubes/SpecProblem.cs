namespace Lucid.Editor.Cubes
{
    /// <summary>
    /// One thing wrong with a spec. <see cref="Field"/> is the JSON path so the
    /// author is pointed at the line rather than handed a stack trace.
    /// </summary>
    public sealed class SpecProblem
    {
        public SpecProblem(string field, string message)
        {
            Field = field;
            Message = message;
        }

        public string Field { get; }
        public string Message { get; }

        public override string ToString() =>
            string.IsNullOrEmpty(Field) ? Message : $"{Field}: {Message}";
    }
}
