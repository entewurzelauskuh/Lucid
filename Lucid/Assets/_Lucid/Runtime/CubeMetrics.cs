namespace Lucid.Runtime
{
    /// <summary>
    /// The frame every cube is built to and every Sleeper moves through
    /// (docs/CUBE-SPEC.md §1).
    /// </summary>
    /// <remarks>
    /// In Runtime rather than beside the rest of the build geometry, because
    /// these particular numbers are gameplay as much as construction: a
    /// doorway's size is what a Sleeper fits through, and a fog door has to
    /// fill exactly that opening. <c>Lucid.Editor</c>'s CubeGeometry takes its
    /// values from here so there is one definition rather than two that agree
    /// until someone edits one of them.
    /// </remarks>
    public static class CubeMetrics
    {
        /// <summary>A cube is 8 m on every side.</summary>
        public const float Size = 8f;

        public const float Half = Size / 2f;

        /// <summary>Doorways are 2.5 m wide and 3 m high, at floor level.</summary>
        public const float DoorWidth = 2.5f;

        public const float DoorHeight = 3f;

        /// <summary>A vertical connector is a 2.5 m square hole.</summary>
        public const float VerticalHole = 2.5f;
    }
}
