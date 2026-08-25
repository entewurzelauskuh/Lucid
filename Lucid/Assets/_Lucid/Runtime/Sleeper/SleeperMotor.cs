using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// The Sleeper's body: run, jump, crouch, and nothing else (SPEC §9 — no
    /// sprint, no double jump, no mantle, no wall-jump).
    /// </summary>
    /// <remarks>
    /// Movement is plain local gameplay on the Sleeper's own machine (SPEC
    /// §14), so floats are fine here; the determinism rule is about
    /// <c>Lucid.Core</c>. <see cref="Tick"/> is public and takes its own dt so
    /// a test can run a whole jump in one frame instead of waiting for one.
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class SleeperMotor : MonoBehaviour
    {
        [SerializeField] SleeperMotorSettings _settings = new SleeperMotorSettings();
        [SerializeField] Transform _eye;

        CharacterController _controller;
        ISleeperInputSource _source;
        readonly Collider[] _overlap = new Collider[8];
        float _vertical;
        bool _crouched;

        public SleeperMotorSettings Settings => _settings;
        public bool IsGrounded => _controller != null && _controller.isGrounded;
        public bool IsCrouched => _crouched;

        /// <summary>Capsule height right now, m. 1.8 standing, 1.0 crouched.</summary>
        public float Height => _controller != null ? _controller.height : 0f;

        /// <summary>Vertical speed, m/s. Positive is up.</summary>
        public float VerticalSpeed => _vertical;

        /// <summary>Where the feet are. The transform origin is the capsule's base.</summary>
        public Vector3 Feet => transform.position;

        public Transform Eye => _eye;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _source = GetComponent<ISleeperInputSource>();
            ApplySettings();
        }

        void Update()
        {
            if (_source != null) Tick(_source.Read(), Time.deltaTime);
        }

        /// <summary>
        /// Advance the body by one step. Called by <c>Update</c> for a human
        /// and directly by the PlayMode tests, which is why dt is a parameter.
        /// </summary>
        public void Tick(SleeperInput input, float dt)
        {
            if (dt <= 0f) return;
            if (_controller == null) Awake();

            ResolveStance(input.Crouch);

            Vector3 wish = transform.right * input.Move.x + transform.forward * input.Move.y;
            if (wish.sqrMagnitude > 1f) wish.Normalize();
            float speed = _crouched ? _settings.CrouchSpeed : _settings.RunSpeed;

            bool grounded = _controller.isGrounded;
            // Hold the body against the floor while it is on it, so walking over
            // a lip does not launch it and isGrounded stays honest.
            if (grounded && _vertical <= 0f) _vertical = -_settings.GroundStick;
            if (grounded && input.JumpPressed) _vertical = _settings.JumpSpeed;

            // Exact for constant acceleration: dy = v·dt − ½·g·dt². Stepping
            // velocity first and multiplying (the usual shortcut) loses height
            // in proportion to dt — at 60 Hz a 1.2 m jump peaks at 1.14 m,
            // which is under the 1.1 m ledges the chicane guideline promises
            // are clearable. This form makes the apex the spec's number at any
            // frame rate.
            float dy = _vertical * dt - 0.5f * _settings.Gravity * dt * dt;
            _vertical -= _settings.Gravity * dt;

            Vector3 step = wish * (speed * dt);
            if (!grounded && step != Vector3.zero && WouldMantle(step, dy)) step = Vector3.zero;

            CollisionFlags flags = _controller.Move(step + Vector3.up * dy);

            // A ceiling ends the climb; without this the body would hang there
            // burning off velocity it never got to spend.
            if ((flags & CollisionFlags.Above) != 0 && _vertical > 0f) _vertical = 0f;
        }

        /// <summary>Put the body somewhere without the controller fighting it.</summary>
        public void Warp(Vector3 feet)
        {
            bool wasEnabled = _controller != null && _controller.enabled;
            if (wasEnabled) _controller.enabled = false;
            transform.position = feet;
            if (wasEnabled) _controller.enabled = true;
            _vertical = 0f;
        }

        internal void Bind(Transform eye)
        {
            _eye = eye;
            PlaceEye();
        }

        void ResolveStance(bool wantsCrouch)
        {
            if (wantsCrouch == _crouched) return;
            // Standing up inside a crawl space would push the body through the
            // ceiling, so a held crouch that cannot be released simply stays.
            if (!wantsCrouch && !HasRoomToStand()) return;

            _crouched = wantsCrouch;
            float height = _crouched ? _settings.CrouchHeight : _settings.StandHeight;
            _controller.height = height;
            _controller.center = new Vector3(0f, height * 0.5f, 0f);
            PlaceEye();
        }

        /// <summary>
        /// True when this step would climb something the feet are still below.
        /// </summary>
        /// <remarks>
        /// SPEC §9 grants no mantle, but a capsule mantles all by itself: its
        /// foot is a hemisphere, so once the sphere's centre rises past a lip
        /// the corner is outside the capsule and the body slides on and over
        /// it. Measured on the gauntlet, a 1.2 m jump mounted the 1.4 m ledge
        /// the acceptance requires it to fail, arriving 0.2 m higher than it
        /// ever rose. Refusing to move into anything standing above the feet
        /// gives the body flat feet, which is what the chicane guideline
        /// assumes when it promises ledges up to 1.1 m and no more.
        ///
        /// The second probe matters: a body already inside the check volume —
        /// jumping along a wall, say — must keep its ordinary freedom to slide,
        /// so only a step from clear ground into an obstruction is refused.
        /// </remarks>
        bool WouldMantle(Vector3 step, float dy)
        {
            Vector3 destination = transform.position + step + Vector3.up * dy;
            if (HasFootRoomAt(destination)) return false;
            return HasFootRoomAt(transform.position);
        }

        bool HasFootRoomAt(Vector3 feet)
        {
            float r = _controller.radius;
            // Exactly the hemisphere's worth of height, which is the band in
            // which the rounded foot would otherwise find purchase.
            Vector3 centre = feet + Vector3.up * (r * 0.5f);
            var half = new Vector3(r * 0.9f, r * 0.5f, r * 0.9f);

            int hits = Physics.OverlapBoxNonAlloc(
                centre, half, _overlap, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits; i++)
                if (_overlap[i] != null && _overlap[i].transform != transform) return false;
            return true;
        }

        bool HasRoomToStand()
        {
            float r = _controller.radius;
            // Only the volume standing would newly occupy: from the crouched
            // capsule's top sphere to the standing one's. Starting lower would
            // find the floor the body is already resting on.
            Vector3 bottom = transform.position + Vector3.up * (_settings.CrouchHeight - r);
            Vector3 top = transform.position + Vector3.up * (_settings.StandHeight - r);

            int hits = Physics.OverlapCapsuleNonAlloc(
                bottom, top, r, _overlap, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits; i++)
                if (_overlap[i] != null && _overlap[i].transform != transform) return false;
            return true;
        }

        void PlaceEye()
        {
            if (_eye == null) return;
            float height = _crouched ? _settings.CrouchHeight : _settings.StandHeight;
            _eye.localPosition = new Vector3(0f, height - _settings.EyeDrop, 0f);
        }

        void ApplySettings()
        {
            _controller.radius = _settings.Radius;
            _controller.skinWidth = _settings.SkinWidth;
            _controller.stepOffset = _settings.StepOffset;
            _controller.slopeLimit = _settings.SlopeLimit;
            // The default 0.001 m swallows short steps, which at a high frame
            // rate is most of them.
            _controller.minMoveDistance = 0f;
            _controller.height = _settings.StandHeight;
            _controller.center = new Vector3(0f, _settings.StandHeight * 0.5f, 0f);
            _crouched = false;
            PlaceEye();
        }
    }
}
