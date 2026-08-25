using UnityEngine;
using UnityEngine.InputSystem;

namespace Lucid.Runtime
{
    /// <summary>
    /// Reads the <c>Sleeper</c> action map. Keyboard and mouse only for now
    /// (docs/WORKPLAN.md §4, M0.4); the gamepad bindings SPEC §9 promises come
    /// with the rest of the input work.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SleeperInputSource : MonoBehaviour, ISleeperInputSource
    {
        public const string MapName = "Sleeper";

        [SerializeField] InputActionAsset _actions;

        InputActionMap _map;
        InputAction _move, _look, _jump, _crouch;

        void Awake() => Resolve();

        void OnEnable()
        {
            Resolve();
            _map?.Enable();
        }

        void OnDisable() => _map?.Disable();

        public SleeperInput Read()
        {
            if (_map == null) return default;
            return new SleeperInput
            {
                Move = _move?.ReadValue<Vector2>() ?? Vector2.zero,
                Look = _look?.ReadValue<Vector2>() ?? Vector2.zero,
                JumpPressed = _jump != null && _jump.WasPressedThisFrame(),
                Crouch = _crouch != null && _crouch.IsPressed(),
            };
        }

        /// <summary>
        /// Points the source at an action asset. Internal because only the
        /// scene builders wire this up; a running game gets it from the scene.
        /// </summary>
        internal void Bind(InputActionAsset actions)
        {
            _actions = actions;
            _map = null;
            Resolve();
        }

        void Resolve()
        {
            if (_map != null || _actions == null) return;

            _map = _actions.FindActionMap(MapName, throwIfNotFound: false);
            if (_map == null)
            {
                Debug.LogError($"{name}: '{_actions.name}' has no '{MapName}' action map.", this);
                return;
            }

            _move = _map.FindAction("Move");
            _look = _map.FindAction("Look");
            _jump = _map.FindAction("Jump");
            _crouch = _map.FindAction("Crouch");
        }
    }
}
