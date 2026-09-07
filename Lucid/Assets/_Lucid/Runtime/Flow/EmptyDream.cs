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

        void Awake()
        {
            Dream = GetComponent<DreamInstance>();
            Dream.Build(new EventLog());
        }
    }
}
