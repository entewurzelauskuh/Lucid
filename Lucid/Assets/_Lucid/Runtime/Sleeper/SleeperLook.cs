using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// Mouse look: yaw turns the body, pitch tilts the eye. Runs ahead of
    /// <see cref="SleeperMotor"/> so a step goes where the player is already
    /// looking rather than where they looked last frame.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10)]
    public sealed class SleeperLook : MonoBehaviour
    {
        [SerializeField] Transform _eye;
        [SerializeField] float _degreesPerCount = 0.08f;
        [SerializeField] float _pitchLimit = 89f;
        [SerializeField] bool _invertY;

        ISleeperInputSource _source;
        float _pitch;

        /// <summary>Current pitch in degrees; negative is up.</summary>
        public float Pitch => _pitch;

        void Awake() => _source = GetComponent<ISleeperInputSource>();

        void Update()
        {
            if (_source != null) Tick(_source.Read().Look);
        }

        public void Tick(Vector2 look)
        {
            if (look == Vector2.zero) return;

            transform.Rotate(0f, look.x * _degreesPerCount, 0f, Space.Self);

            float dy = look.y * _degreesPerCount * (_invertY ? 1f : -1f);
            _pitch = Mathf.Clamp(_pitch + dy, -_pitchLimit, _pitchLimit);
            if (_eye != null) _eye.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        internal void Bind(Transform eye) => _eye = eye;
    }
}
