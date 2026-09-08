using Lucid.Core;

namespace Lucid.Runtime
{
    /// <summary>
    /// The layer slider (docs/SPEC.md §10, docs/UI.md §8): everything above the
    /// chosen layer is cut away so the Nightmare can see into the lattice.
    /// Pure; the god view applies it to renderers and nothing else, because a
    /// cube that is hidden is still there for the Sleeper walking through it.
    /// </summary>
    public readonly struct LayerCutaway
    {
        public readonly int Layer;
        public readonly Limits Limits;

        public LayerCutaway(int layer, Limits limits)
        {
            Limits = limits ?? Limits.Default;
            Layer = Clamp(layer, Limits);
        }

        /// <summary>Nothing cut away: the top of the dream is the slider's default.</summary>
        public static LayerCutaway ShowAll(Limits limits) => new LayerCutaway((limits ?? Limits.Default).LayerMax, limits);

        public bool IsVisible(Coord cube) => cube.Z <= Layer;

        public LayerCutaway Up() => new LayerCutaway(Layer + 1, Limits);
        public LayerCutaway Down() => new LayerCutaway(Layer - 1, Limits);

        static int Clamp(int layer, Limits limits) =>
            layer < limits.LayerMin ? limits.LayerMin : layer > limits.LayerMax ? limits.LayerMax : layer;
    }
}
