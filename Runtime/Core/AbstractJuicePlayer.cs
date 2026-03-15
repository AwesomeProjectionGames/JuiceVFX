#nullable enable

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JuiceVFX
{
    /// <summary>
    /// Base class for components responsible for playing Juice Feedbacks.
    /// Manages the lifecycle of active JuiceEffectRunners.
    /// </summary>
    public abstract class AbstractJuicePlayer : MonoBehaviour, IJuicePlayer
    {
        [Header("Targeting Settings")]
        public Renderer[] targetRenderers = System.Array.Empty<Renderer>();
        public Transform? targetRoot;

        /// <summary>
        /// The target renderers affected by the feedback effects.
        /// </summary>
        public virtual Renderer[] TargetRenderers { get => targetRenderers; set => targetRenderers = value; }

        /// <summary>
        /// The root transform used for playing effects. Let empty to use this GameObject's transform.
        /// </summary>
        public virtual Transform? TargetRoot { get => targetRoot; set => targetRoot = value; }

        /// <summary>
        /// The specific gamepads targeted by this player.
        /// </summary>
        public abstract Gamepad[] TargetGamepads { get; set; }

        /// <summary>
        /// The player component used to redirect camera effects.
        /// </summary>
        public abstract IJuicePlayer? CameraPlayer { get; set; }

        private readonly List<JuiceEffectRunner> activeRunners = new List<JuiceEffectRunner>();
        private readonly List<JuiceEffectRunner> runnersToRemove = new List<JuiceEffectRunner>();

        protected virtual void Awake() { }

        protected virtual void Start() { }

        protected virtual void OnEnable() { }

        protected virtual void Update()
        {
            if (activeRunners.Count == 0) return;

            runnersToRemove.Clear();

            foreach (var runner in activeRunners)
            {
                runner.Update(Time.deltaTime);

                if (runner.IsFinished)
                {
                    runnersToRemove.Add(runner);
                }
            }

            foreach (var runner in runnersToRemove)
            {
                activeRunners.Remove(runner);
            }
        }

        protected virtual void OnDisable()
        {
            StopAll();
        }

        protected virtual void OnDestroy() { }

        /// <summary>
        /// Plays the specified feedback.
        /// </summary>
        public void Play(JuiceFeedback feedback, bool isCameraTarget = false, Vector3? contactPoint = null, Quaternion? rotation = null, float multiplier = 1f)
        {
            if (feedback == null) return;
            Play(feedback.Effects, isCameraTarget, contactPoint, rotation, multiplier);
        }

        /// <summary>
        /// Plays a collection of effects directly.
        /// </summary>
        public void Play(IEnumerable<JuiceEffectData> effects, bool isCameraTarget = false, Vector3? contactPoint = null, Quaternion? rotation = null, float multiplier = 1f)
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
                StartNewEffectRunner(effectData, context);
            }

            PlayCameraEffects(cameraEffects, contactPoint, rotation, multiplier);
        }

        /// <summary>
        /// Stops all currently active effect runners.
        /// </summary>
        public void StopAll()
        {
            foreach (var runner in activeRunners)
            {
                runner.Stop();
            }
            activeRunners.Clear();
        }

        protected virtual JuiceFeedbackContext CreateFeedbackContext(Vector3? contactPoint, Quaternion? rotation, float multiplier)
        {
            var gamepads = TargetGamepads;
            var root = TargetRoot != null ? TargetRoot : transform;

            return new JuiceFeedbackContext(contactPoint, rotation, gamepads, TargetRenderers, root, multiplier);
        }

        private void RedirectEffectToCamera(JuiceEffectData effectData, ref List<JuiceEffectData>? cameraEffects)
        {
            if (cameraEffects == null) cameraEffects = new List<JuiceEffectData>();
            cameraEffects.Add(effectData);
        }

        private void RemoveExistingDuplicateRunners(JuiceEffectData effectData)
        {
            for (int i = activeRunners.Count - 1; i >= 0; i--)
            {
                var existingRunner = activeRunners[i];
                if (existingRunner.EffectData != null && effectData.IsSameEffect(existingRunner.EffectData))
                {
                    existingRunner.Stop();
                    activeRunners.RemoveAt(i);
                }
            }
        }

        private void StartNewEffectRunner(JuiceEffectData effectData, JuiceFeedbackContext context)
        {
            var runner = effectData.CreateRunner();
            runner.EffectData = effectData;
            runner.Initialize(this, context);
            runner.Start(effectData.Delay);
            activeRunners.Add(runner);
        }

        protected virtual void PlayCameraEffects(List<JuiceEffectData>? cameraEffects, Vector3? contactPoint, Quaternion? rotation, float multiplier)
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
                    camPlayer.Play(cameraEffects, true, contactPoint, rotation, multiplier);
                }
            }
        }
    }
}

