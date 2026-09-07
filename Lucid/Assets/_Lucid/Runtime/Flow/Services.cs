using System;

namespace Lucid.Runtime
{
    /// <summary>
    /// The one static in Runtime (CLAUDE.md, Conventions): what the scene
    /// bootstrap made and every screen may ask for.
    /// </summary>
    /// <remarks>
    /// Installed by <see cref="Bootstrap"/> in the Boot scene and never
    /// replaced while that scene is up. Everything else is loaded additively
    /// beside Boot, so this instance is the same object on every screen — which
    /// is what M0.6b's acceptance asserts across a Title → Sandbox → Title
    /// round trip.
    /// </remarks>
    public sealed class Services
    {
        public static Services Current { get; private set; }

        public GameFlow Flow { get; }

        Services(GameFlow flow) => Flow = flow;

        internal static Services Install(GameFlow flow)
        {
            if (flow == null) throw new ArgumentNullException(nameof(flow));
            if (Current != null)
                throw new InvalidOperationException(
                    "Services are already installed: a second Boot scene is loaded, or the " +
                    "first was never torn down");

            Current = new Services(flow);
            return Current;
        }

        internal static void Uninstall(Services which)
        {
            if (ReferenceEquals(Current, which)) Current = null;
        }
    }
}
