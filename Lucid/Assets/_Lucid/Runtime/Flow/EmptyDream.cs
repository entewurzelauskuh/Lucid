using Lucid.Core;
using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// Stands the bare lattice up — the bedroom and nothing else — from an
    /// empty event log, as soon as the scene starts.
    /// </summary>
    /// <remarks>
    /// The Title's backdrop (docs/UI.md §3: "the start cube, a bedroom at
    /// night") and the Sandbox's starting point are the same thing, and this is
    /// it. M0.7 replaces the empty log with whatever the Nightmare builds.
    /// </remarks>
    [RequireComponent(typeof(DreamInstance))]
    public sealed class EmptyDream : MonoBehaviour
    {
        public DreamInstance Dream { get; private set; }

        void Awake() => Ensure();

        /// <summary>
        /// Builds once, whoever asks first. Awake normally does; a sibling's
        /// Start may ask before it if the components are ever reordered, and
        /// correctness should not rest on the order a builder happened to add
        /// them in.
        /// </summary>
        public DreamInstance Ensure()
        {
            if (Dream != null) return Dream;
            Dream = GetComponent<DreamInstance>();
            Dream.Build(new EventLog());
            return Dream;
        }
    }
}
