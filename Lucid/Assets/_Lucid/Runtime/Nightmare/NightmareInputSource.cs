using UnityEngine;
using UnityEngine.InputSystem;

namespace Lucid.Runtime
{
    /// <summary>What the Nightmare did this frame (docs/UI.md §8, Keys).</summary>
    public struct NightmareInput
    {
        public Vector2 Point;
        public Vector2 Look;
        public Vector2 Pan;
        public float Zoom;
        public bool PlacePressed;
        public bool OrbitHeld;
        public bool OrbitReleased;
        public bool PanHeld;
        public bool RotatePressed;
        /// <summary>1–9 for the palette slot pressed this frame, else 0.</summary>
        public int SelectSlot;
        public bool LayerUpPressed;
        public bool LayerDownPressed;
        public bool TopDownPressed;
        public bool FocusPressed;
    }

    /// <summary>
    /// Reads the <c>Nightmare</c> action map. The same shape as
    /// <see cref="SleeperInputSource"/>: bound by the scene, polled each frame,
    /// and nothing else ever enables the map.
    /// </summary>
    public sealed class NightmareInputSource : MonoBehaviour
    {
        public const string MapName = "Nightmare";

        [SerializeField] InputActionAsset _actions;

        InputActionMap _map;
        InputAction _point, _look, _pan, _zoom, _place, _orbit, _panHold, _rotate, _select,
            _layerUp, _layerDown, _topDown, _focus;

        void Awake() => Resolve();

        void OnEnable()
        {
            Resolve();
            _map?.Enable();
        }

        void OnDisable() => _map?.Disable();

        internal void Bind(InputActionAsset actions)
        {
            _actions = actions;
            _map = null;
            Resolve();
            if (isActiveAndEnabled) _map?.Enable();
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
            _point = _map.FindAction("Point"); _look = _map.FindAction("Look"); _pan = _map.FindAction("Pan");
            _zoom = _map.FindAction("Zoom"); _place = _map.FindAction("Place"); _orbit = _map.FindAction("OrbitHold");
            _panHold = _map.FindAction("PanHold"); _rotate = _map.FindAction("Rotate"); _select = _map.FindAction("Select");
            _layerUp = _map.FindAction("LayerUp"); _layerDown = _map.FindAction("LayerDown");
            _topDown = _map.FindAction("ToggleTopDown"); _focus = _map.FindAction("FocusStart");
        }

        public NightmareInput Read()
        {
            if (_map == null) return default;

            int slot = 0;
            if (_select != null && _select.WasPressedThisFrame() && _select.activeControl != null)
                int.TryParse(_select.activeControl.name, out slot);

            return new NightmareInput
            {
                Point = _point?.ReadValue<Vector2>() ?? Vector2.zero,
                Look = _look?.ReadValue<Vector2>() ?? Vector2.zero,
                Pan = _pan?.ReadValue<Vector2>() ?? Vector2.zero,
                Zoom = _zoom?.ReadValue<float>() ?? 0f,
                PlacePressed = _place != null && _place.WasPressedThisFrame(),
                OrbitHeld = _orbit != null && _orbit.IsPressed(),
                OrbitReleased = _orbit != null && _orbit.WasReleasedThisFrame(),
                PanHeld = _panHold != null && _panHold.IsPressed(),
                RotatePressed = _rotate != null && _rotate.WasPressedThisFrame(),
                SelectSlot = slot,
                LayerUpPressed = _layerUp != null && _layerUp.WasPressedThisFrame(),
                LayerDownPressed = _layerDown != null && _layerDown.WasPressedThisFrame(),
                TopDownPressed = _topDown != null && _topDown.WasPressedThisFrame(),
                FocusPressed = _focus != null && _focus.WasPressedThisFrame(),
            };
        }
    }
}
