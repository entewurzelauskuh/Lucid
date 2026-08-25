using System;
using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// The movement kit of SPEC §9, and the arithmetic that turns it into a
    /// gravity. Every chicane in the game is designed against these numbers,
    /// so they are the tunables and everything else is derived from them.
    /// </summary>
    /// <remarks>
    /// The spec asks for a 1.2 m rise and a gap of about 4 m at a 6 m/s run.
    /// Those two are not independent: a jump that rises 1.2 m under Earth
    /// gravity hangs for 0.99 s and carries almost 6 m, half again the gap the
    /// spec wants. Fixing the rise and the reach therefore fixes gravity, and
    /// it lands at 23.9 m/s² — about 2.4 g. The dream is heavy on purpose;
    /// this is worth a look in the M0 play-test rather than a surprise.
    ///
    /// <see cref="JumpTravel"/> is the flight of the capsule's *centre* over
    /// level ground. The gap a player actually crosses is a little longer,
    /// because the rounded foot finds a few centimetres of purchase at each
    /// lip: measured on the gauntlet, 3.8 m of centre travel crosses gaps up
    /// to about 4.15 m. That is the "~4 m" of SPEC §9, and it is why a 3.5 m
    /// gap is always makeable and a 4.5 m one never is.
    ///
    /// That purchase is not the mantle SPEC §9 forbids and
    /// <see cref="SleeperMotor"/> suppresses, though both come from the same
    /// rounded foot. Catching the lip of a floor you are already falling past
    /// never carries you above the height you jumped; climbing a ledge taller
    /// than your apex does. So a ledge gets no bonus at all, and the tallest
    /// one is a shade under the rise (docs/DECISIONS.md).
    /// </remarks>
    [Serializable]
    public sealed class SleeperMotorSettings
    {
        [Header("SPEC §9 kit")]
        [SerializeField] float _runSpeed = 6f;
        [SerializeField] float _crouchSpeed = 2.5f;
        [SerializeField] float _jumpRise = 1.2f;
        [SerializeField] float _jumpTravel = 3.8f;
        [SerializeField] float _standHeight = 1.8f;
        [SerializeField] float _crouchHeight = 1f;

        [Header("Capsule")]
        [SerializeField] float _radius = 0.4f;
        [SerializeField] float _skinWidth = 0.04f;
        [SerializeField] float _stepOffset = 0.1f;
        [SerializeField] float _slopeLimit = 45f;

        [Header("Feel")]
        [SerializeField] float _eyeDrop = 0.2f;
        [SerializeField] float _groundStick = 2f;

        /// <summary>Ground speed while standing, m/s.</summary>
        public float RunSpeed => _runSpeed;

        /// <summary>Ground speed while crouched, m/s.</summary>
        public float CrouchSpeed => _crouchSpeed;

        /// <summary>How far the feet rise at the apex of a jump, m.</summary>
        public float JumpRise => _jumpRise;

        /// <summary>Flight of the capsule centre over level ground, m.</summary>
        public float JumpTravel => _jumpTravel;

        public float StandHeight => _standHeight;
        public float CrouchHeight => _crouchHeight;
        public float Radius => _radius;
        public float SkinWidth => _skinWidth;
        /// <summary>
        /// How high a lip the controller walks over. Small on purpose: PhysX
        /// lifts the capsule onto anything within this of its feet whether it
        /// is standing or mid-air, so this is added to the jump's reach. At the
        /// usual 0.3 m the body stepped from a 1.2 m apex onto a 1.4 m ledge,
        /// which the M0.4 acceptance requires it to fail.
        /// </summary>
        public float StepOffset => _stepOffset;
        public float SlopeLimit => _slopeLimit;

        /// <summary>How far below the capsule's top the camera sits, m.</summary>
        public float EyeDrop => _eyeDrop;

        /// <summary>
        /// Downward speed held while grounded so the controller keeps contact
        /// over lips and slopes instead of skipping into the air.
        /// </summary>
        public float GroundStick => _groundStick;

        /// <summary>Seconds from take-off to landing on level ground.</summary>
        public float Airtime => _jumpTravel / _runSpeed;

        /// <summary>
        /// Derived so the rise and the reach both come out exactly right:
        /// with apex at T/2, rise = ½·g·(T/2)² gives g = 8·rise / T².
        /// </summary>
        public float Gravity
        {
            get
            {
                float t = Airtime;
                return 8f * _jumpRise / (t * t);
            }
        }

        /// <summary>Take-off speed, g·T/2 = 4·rise / T.</summary>
        public float JumpSpeed => 4f * _jumpRise / Airtime;
    }
}
