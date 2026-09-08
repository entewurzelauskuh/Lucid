using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Lucid.Runtime
{
    /// <summary>
    /// The Dream scene while the Sandbox is a shell (docs/WORKPLAN.md §4,
    /// M0.6b): a Sleeper standing in the bedroom, and Esc to go back.
    /// </summary>
    /// <remarks>
    /// M0.7 puts the god view here and M0.9b the F5 switch between the two;
    /// the state they fill is this one. The Sleeper is created at run time
    /// rather than saved in the scene because its spawn point comes from the
    /// lattice, which does not exist until <see cref="EmptyDream"/> has built
    /// it.
    /// </remarks>
    [RequireComponent(typeof(EmptyDream))]
    public sealed class SandboxScene : MonoBehaviour
    {
        /// <summary>
        /// Not "UI": that is the map name the Input System's InputForUI bridge
        /// looks for on the project-wide asset to drive UI Toolkit, and a map
        /// by that name with only Back in it would, the day this asset became
        /// project-wide, silently take every click away from the Title.
        /// </summary>
        public const string BackMap = "Flow";
        public const string BackAction = "Back";

        [SerializeField] InputActionAsset _actions;

        InputActionAsset _own;
        InputAction _back;
        SleeperMotor _sleeper;

        public SleeperMotor Sleeper => _sleeper;

        /// <summary>The Back action as bound, for a test to inspect each link.</summary>
        internal InputAction Back => _back;

        internal void Configure(InputActionAsset actions) => _actions = actions;

        void Start()
        {
            DreamInstance dream = GetComponent<EmptyDream>().Ensure();

            _sleeper = SleeperRig.Create(dream.SpawnPoint, dream.SpawnFacing);
            _sleeper.transform.SetParent(transform, true);
            if (_actions != null)
            {
                // A copy of the asset for this scene's lifetime, not the shared
                // asset itself. An InputActionAsset carries its resolved state
                // with it, so a map enabled here would still be enabled — and
                // still bound to whatever devices it last saw — after this
                // scene is gone. The dev scenes bind the shared asset directly
                // and that state was what the Esc test tripped over when they
                // ran first. A copy is born after everything else and dies
                // with the scene.
                _own = Instantiate(_actions);
                _sleeper.gameObject.AddComponent<SleeperInputSource>().Bind(_own);

                _back = _own.FindActionMap(BackMap, throwIfNotFound: false)
                    ?.FindAction(BackAction, throwIfNotFound: false);
                if (_back != null)
                {
                    _back.performed += OnBack;
                    _back.Enable();
                }
            }
        }

        void OnDestroy()
        {
            if (_back != null)
            {
                _back.performed -= OnBack;
                _back.Disable();
            }
            if (_own != null) Destroy(_own);
        }

        void OnBack(InputAction.CallbackContext _) => Leave();

        /// <summary>Back to the Title. Public so a test can press Esc without a keyboard.</summary>
        public void Leave()
        {
            Services services = Services.Current;
            if (services == null)
                throw new InvalidOperationException(
                    $"{name}: no Services — the Dream scene was loaded without the Boot scene");
            if (!services.Flow.IsTransitioning) services.Flow.Go(FlowState.Title);
        }
    }
}
