using UnityEngine;

namespace Lucid.Runtime
{
    /// <summary>
    /// Builds a Sleeper body in code. There is no Sleeper prefab on purpose:
    /// the gauntlet scene builder, the PlayMode tests and (later) the dream
    /// runtime all want the same rig, and one factory is easier to keep honest
    /// than a prefab plus the scripts that have to match it (CLAUDE.md rule 4).
    /// </summary>
    public static class SleeperRig
    {
        public const string EyeName = "Eye";

        /// <summary>
        /// A body standing with its feet at <paramref name="feet"/>, facing
        /// <paramref name="facing"/>. Nothing drives it until an input source
        /// is added; see <see cref="SleeperInputSource"/>.
        /// </summary>
        public static SleeperMotor Create(Vector3 feet, Vector3 facing, string name = "Sleeper")
        {
            var body = new GameObject(name);
            body.transform.position = feet;
            if (facing.sqrMagnitude > 0f)
                body.transform.rotation = Quaternion.LookRotation(
                    new Vector3(facing.x, 0f, facing.z).normalized, Vector3.up);

            body.AddComponent<CharacterController>();

            var eye = new GameObject(EyeName).transform;
            eye.SetParent(body.transform, false);
            eye.gameObject.AddComponent<Camera>();

            // The motor configures the controller from its settings in Awake,
            // so it goes on after the controller exists and before the eye is
            // bound — Bind is what puts the camera at the right height.
            var motor = body.AddComponent<SleeperMotor>();
            motor.Bind(eye);

            body.AddComponent<SleeperLook>().Bind(eye);

            return motor;
        }

        public static SleeperMotor Create(Vector3 feet) => Create(feet, Vector3.forward);
    }
}
