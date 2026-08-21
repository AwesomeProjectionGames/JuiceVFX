#nullable enable

using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace JuiceVFX
{
    /// <summary>
    /// Component responsible for playing Juice Feedbacks.
    /// Note that this component manages the lifecycle of all active JuiceEffectRunners.
    /// </summary>
    public class JuicePlayer : AbstractJuicePlayer
    {
        [Header("Targeting Settings")]
        public Renderer[] targetRenderers = System.Array.Empty<Renderer>();
        public Transform? targetRoot;

#if ENABLE_INPUT_SYSTEM
        [Header("JuicePlayer Input Settings")]
        public Gamepad[] targetGamepads = System.Array.Empty<Gamepad>();
        public bool takeCurrentGamepadAsDefault = true;

        /// <summary>
        /// Whether the current active gamepad should be considered if no targets are specified.
        /// </summary>
        public bool TakeCurrentGamepadAsDefault { get => takeCurrentGamepadAsDefault; set => takeCurrentGamepadAsDefault = value; }
#endif

        [Header("Camera Settings")]
        public JuicePlayer? cameraPlayer;

        /// <summary>
        /// The player component located on the main camera to redirect camera effects.
        /// </summary>
        public IJuicePlayer? CameraPlayer { get => cameraPlayer; set => cameraPlayer = value is JuicePlayer jp ? jp : null; }
        
        /// <summary>
        /// Plays a collection of effects directly.
        /// </summary>
        public override void Play(IEnumerable<JuiceEffectData> effects, bool isCameraTarget = false, Vector3? contactPoint = null, Quaternion? rotation = null, float multiplier = 1f, float? duration = null)
        {
            if (effects == null) return;

            var context = CreateFeedbackContext(contactPoint, rotation, multiplier);
            List<JuiceEffectData>? cameraEffects = null;

            foreach (var effectData in effects)
            {
                if (effectData == null) continue;

                if (!isCameraTarget && effectData.Target == JuiceEffectTarget.Camera)
                {
                    RedirectEffectToCamera(effectData, ref cameraEffects);
                    continue;
                }

                RemoveExistingDuplicateRunners(effectData);
                StartNewEffectRunner(effectData, context, duration);
            }

            PlayCameraEffects(cameraEffects, contactPoint, rotation, multiplier, duration);
        }
        
        private void RedirectEffectToCamera(JuiceEffectData effectData, ref List<JuiceEffectData>? cameraEffects)
        {
            if (cameraEffects == null) cameraEffects = new List<JuiceEffectData>();
            cameraEffects.Add(effectData);
        }
        
        protected JuiceFeedbackContext CreateFeedbackContext(Vector3? contactPoint, Quaternion? rotation, float multiplier)
        {
            var root = targetRoot != null ? targetRoot : transform;

#if ENABLE_INPUT_SYSTEM
            var gamepads = (targetGamepads != null && targetGamepads.Length > 0)
                ? targetGamepads
                : (TakeCurrentGamepadAsDefault && Gamepad.current != null ? new[] { Gamepad.current } : System.Array.Empty<Gamepad>());

            return new JuiceFeedbackContext(contactPoint, rotation, gamepads, targetRenderers, root, multiplier);
#else
            return new JuiceFeedbackContext(contactPoint, rotation, targetRenderers, root, multiplier);
#endif
        }

        protected virtual void PlayCameraEffects(List<JuiceEffectData>? cameraEffects, Vector3? contactPoint, Quaternion? rotation, float multiplier, float? duration = null)
        {
            if (cameraEffects != null && cameraEffects.Count > 0)
            {
                var camPlayer = CameraPlayer;

                if (camPlayer == null && Camera.main != null)
                {
                    camPlayer = Camera.main.GetComponent<IJuicePlayer>();
                }

                if (camPlayer != null && (object)camPlayer != this)
                {
                    camPlayer.Play(cameraEffects, true, contactPoint, rotation, multiplier, duration);
                }
            }
        }
    }
}
