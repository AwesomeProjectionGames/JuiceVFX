#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace JuiceVFX
{
    /// <summary>
    /// Base class for components responsible for playing Juice Feedbacks.
    /// Manages the lifecycle of active JuiceEffectRunners.
    /// </summary>
    public abstract class AbstractJuicePlayer : MonoBehaviour, IJuicePlayer
    {
        private readonly List<JuiceEffectRunner> activeRunners = new List<JuiceEffectRunner>();
        private readonly List<JuiceEffectRunner> runnersToRemove = new List<JuiceEffectRunner>();

#if UNITY_EDITOR
        /// <summary>
        /// Gets the list of currently active effect runners managed by this player.
        /// </summary>
        public IReadOnlyList<JuiceEffectRunner> ActiveRunners => activeRunners;
#endif

        protected virtual void Awake()
        {
        }

        protected virtual void Start()
        {
        }

        protected virtual void OnEnable()
        {
        }

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

        protected virtual void OnDestroy()
        {
        }

        /// <summary>
        /// Plays the specified feedback.
        /// </summary>
        public void Play(JuiceFeedback feedback, bool isCameraTarget = false, Vector3? contactPoint = null,
            Quaternion? rotation = null, float multiplier = 1f, float? duration = null)
        {
            if (feedback == null) return;
            Play(feedback.Effects, isCameraTarget, contactPoint, rotation, multiplier, duration);
        }

        /// <summary>
        /// Plays a collection of effects directly.
        /// </summary>
        public abstract void Play(IEnumerable<JuiceEffectData> effects, bool isCameraTarget = false,
            Vector3? contactPoint = null, Quaternion? rotation = null, float multiplier = 1f, float? duration = null);

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

        protected void RemoveExistingDuplicateRunners(JuiceEffectData effectData)
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

        protected void StartNewEffectRunner(JuiceEffectData effectData, JuiceFeedbackContext context,
            float? durationOverride = null)
        {
            var runner = effectData.CreateRunner();
            runner.EffectData = effectData;
            runner.Initialize(this, context);
            runner.Start(effectData.Delay, durationOverride);
            activeRunners.Add(runner);

#if UNITY_EDITOR
            JuiceDebugger.RecordEffect(this, effectData, context, durationOverride, runner);
#endif
        }
    }
}

