#nullable enable

using UnityEngine;

namespace JuiceVFX
{
    /// <summary>
    /// Runtime runner for a Juice Effect.
    /// Handles the actual logic and state of the effect.
    /// </summary>
    public abstract class JuiceEffectRunner
    {
        public JuiceEffectData EffectData { get; internal set; } = null!;
        protected IJuicePlayer Player = null!;
        protected JuiceFeedbackContext Context;
        protected float _timer;
        protected float _delayTimer;
        public float Duration { get; protected set; }
        public bool IsFinished { get; protected set; }
        public bool IsPlaying { get; protected set; }
#if UNITY_EDITOR
        public float ElapsedTime => _timer;
        public float DelayRemaining => _delayTimer;
        public JuiceFeedbackContext FeedbackContext => Context;
        public IJuicePlayer PlayerInstance => Player;
        public float Progress => Duration > 0f ? Mathf.Clamp01(_timer / Duration) : 1f;
#endif

        public abstract void OnStart(IJuicePlayer player);
        public abstract void OnUpdate(float deltaTime);
        public abstract void OnStop();

        /// <summary>
        /// Initializes the runner with the player owner.
        /// </summary>
        /// <param name="player">The JuicePlayer that triggered this effect.</param>
        public void Initialize(IJuicePlayer player, JuiceFeedbackContext context)
        {
            Player = player;
            Context = context;
            IsFinished = false;
            IsPlaying = false;
            Duration = EffectData.Duration;
        }

        /// <summary>
        /// Starts the effect, handling delay.
        /// </summary>
        public void Start(float delay, float? durationOverride = null)
        {
            if (durationOverride.HasValue && EffectData.AllowDurationOverride)
            {
                Duration = durationOverride.Value;
            }

            _delayTimer = delay;
            _timer = 0f;
            IsFinished = false;

            if (_delayTimer <= 0)
            {
                BeginEffect();
            }
            else
            {
                IsPlaying = true; // Playing but waiting for delay
            }
        }

        protected virtual void BeginEffect()
        {
            IsPlaying = true;
            OnStart(Player);
        }

        /// <summary>
        /// Updates the effect logic.
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!IsPlaying || IsFinished) return;

            if (_delayTimer > 0)
            {
                _delayTimer -= deltaTime;
                if (_delayTimer <= 0)
                {
                    BeginEffect();
                }
                return;
            }

            OnUpdate(deltaTime);
        }

        /// <summary>
        /// Stops the effect immediately.
        /// </summary>
        public void Stop()
        {
            if (IsPlaying && !IsFinished)
            {
                OnStop();
            }
            IsPlaying = false;
            IsFinished = true;
        }

        protected Vector3 GetTargetPosition(JuiceTargetType type)
        {
            switch (type)
            {
                case JuiceTargetType.ContactPoint:
                    return Context.ContactPoint ?? Context.RootTransform.position;
                case JuiceTargetType.Target:
                default:
                    return Context.RootTransform.position;
            }
        }

        protected Quaternion GetTargetRotation(JuiceTargetType type)
        {
            switch (type)
            {
                case JuiceTargetType.ContactPoint:
                    return Context.Rotation ?? Context.RootTransform.rotation;
                case JuiceTargetType.Target:
                default:
                    return Context.RootTransform.rotation;
            }
        }
    }
}
