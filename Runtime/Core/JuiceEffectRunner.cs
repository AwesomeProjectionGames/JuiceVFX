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
        protected JuicePlayer Player = null!;
        protected JuiceFeedbackContext Context;
        protected float _timer;
        protected float _delayTimer;
        public bool IsFinished { get; protected set; }
        public bool IsPlaying { get; protected set; }

        public abstract void OnStart(JuicePlayer player);
        public abstract void OnUpdate(float deltaTime);
        public abstract void OnStop();

        /// <summary>
        /// Initializes the runner with the player owner.
        /// </summary>
        /// <param name="player">The JuicePlayer that triggered this effect.</param>
        public void Initialize(JuicePlayer player, JuiceFeedbackContext context)
        {
            Player = player;
            Context = context;
            IsFinished = false;
            IsPlaying = false;
        }

        /// <summary>
        /// Starts the effect, handling delay.
        /// </summary>
        public void Start(float delay)
        {
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
                    return Context.ContactPoint ?? Player.transform.position;
                case JuiceTargetType.Target:
                default:
                    return Player.transform.position;
            }
        }

        protected Quaternion GetTargetRotation(JuiceTargetType type)
        {
            switch (type)
            {
                case JuiceTargetType.ContactPoint:
                    return Context.Rotation ?? Player.transform.rotation;
                case JuiceTargetType.Target:
                default:
                    return Player.transform.rotation;
            }
        }
    }
}
