#nullable enable

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JuiceVFX
{
    /// <summary>
    /// Component responsible for playing Juice Feedbacks.
    /// Note that this component manages the lifecycle of all active JuiceEffectRunners.
    /// </summary>
    public class JuicePlayer : AbstractJuicePlayer
    {
        [Header("JuicePlayer Settings")]
        public Gamepad[] targetGamepads = System.Array.Empty<Gamepad>();
        public bool takeCurrentGamepadAsDefault = true;
        public JuicePlayer? cameraPlayer;

        /// <summary>
        /// The specific gamepads targeted by this player.
        /// </summary>
        public override Gamepad[] TargetGamepads { get => targetGamepads; set => targetGamepads = value; }

        /// <summary>
        /// Whether the current active gamepad should be considered if no targets are specified.
        /// </summary>
        public bool TakeCurrentGamepadAsDefault { get => takeCurrentGamepadAsDefault; set => takeCurrentGamepadAsDefault = value; }

        /// <summary>
        /// The player component located on the main camera to redirect camera effects.
        /// </summary>
        public override IJuicePlayer? CameraPlayer { get => cameraPlayer; set => cameraPlayer = value is JuicePlayer jp ? jp : null; }

        protected override JuiceFeedbackContext CreateFeedbackContext(Vector3? contactPoint, Quaternion? rotation, float multiplier)
        {
            var gamepads = (TargetGamepads != null && TargetGamepads.Length > 0)
                ? TargetGamepads
                : (TakeCurrentGamepadAsDefault && Gamepad.current != null ? new[] { Gamepad.current } : System.Array.Empty<Gamepad>());

            var root = TargetRoot != null ? TargetRoot : transform;

            return new JuiceFeedbackContext(contactPoint, rotation, gamepads, TargetRenderers, root, multiplier);
        }
    }
}
