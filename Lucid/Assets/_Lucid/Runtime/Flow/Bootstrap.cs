using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// The Boot scene's one job: install <see cref="Services"/>, then go to
    /// the Title. Everything after that is a transition the flow owns.
    /// </summary>
    [RequireComponent(typeof(GameFlow))]
    public sealed class Bootstrap : MonoBehaviour
    {
        /// <summary>
        /// Off in tests that want to drive the flow themselves from Boot.
        /// </summary>
        [SerializeField] bool _goToTitle = true;

        Services _installed;

        void Awake() => _installed = Services.Install(GetComponent<GameFlow>());

        void Start()
        {
            // Awake threw on a second Boot and left this null; whether Unity
            // then runs Start is not something to depend on either way.
            if (_installed == null) return;
            if (_goToTitle) _installed.Flow.Go(FlowState.Title);
        }

        // Boot is never unloaded in play, so this is for the tests that load and
        // unload it, and for a domain reload: a stale Current would make the
        // next Install throw about a second Boot scene that is not there.
        void OnDestroy() => Services.Uninstall(_installed);
    }
}
