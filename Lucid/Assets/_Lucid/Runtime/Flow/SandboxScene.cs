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
        public const string BackMap = "UI";
        public const string BackAction = "Back";

        [SerializeField] InputActionAsset _actions;

        InputAction _back;
        SleeperMotor _sleeper;

        public SleeperMotor Sleeper => _sleeper;

        internal void Configure(InputActionAsset actions) => _actions = actions;

        void Start()
        {
            DreamInstance dream = GetComponent<EmptyDream>().Dream;

            _sleeper = SleeperRig.Create(dream.SpawnPoint, dream.SpawnFacing);
            _sleeper.transform.SetParent(transform, true);
            if (_actions != null)
            {
                _sleeper.gameObject.AddComponent<SleeperInputSource>().Bind(_actions);

                _back = _actions.FindActionMap(BackMap, throwIfNotFound: false)
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
            if (_back != null) _back.performed -= OnBack;
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
