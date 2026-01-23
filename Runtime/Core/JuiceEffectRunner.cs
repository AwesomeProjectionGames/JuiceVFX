#nullable enable

using UnityEngine;
using GameFramework;

namespace JuiceVFX
{
    /// <summary>
    /// Runtime runner for a Juice Effect.
    /// Handles the actual logic and state of the effect.
    /// </summary>
    public abstract class JuiceEffectRunner
    {
        protected JuicePlayer _player;
        protected JuiceFeedbackContext _context;
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
            _player = player;
            _context = context;
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
            OnStart(_player);
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
            // Optimization: Try to get transform first
            Transform t = GetTargetTransform(type);
            if (t != null) return t.position;

            // Handle non-transform cases (ContactPoint)
            if (type == JuiceTargetType.ContactPoint)
            {
                if (_context.ContactPoint.HasValue) return _context.ContactPoint.Value;
                // Fallback to target or emitter
                if (_context.Target != null) return _context.Target.Transform.position;
                if (_context.Emitter != null) return _context.Emitter.Transform.position;
            }

            return _player.transform.position;
        }

        protected Quaternion GetTargetRotation(JuiceTargetType type)
        {
            // Handle non-transform cases first
            if (type == JuiceTargetType.ContactPoint)
            {
                if (_context.Rotation.HasValue) return _context.Rotation.Value;
            }
            
            Transform? t = GetTargetTransform(type);
            return t != null ? t.rotation : Quaternion.identity;
        }

        protected Transform? GetTargetTransform(JuiceTargetType type)
        {
            switch (type)
            {
                case JuiceTargetType.Target:
                    return _context.Target?.Transform;
                case JuiceTargetType.Emitter:
                    return _context.Emitter?.Transform;
                case JuiceTargetType.EmitterCamera:
                    return ResolveCamera(_context.Emitter);
                case JuiceTargetType.TargetCamera:
                    return ResolveCamera(_context.Target);
            }
            return null;
        }

        private Transform? ResolveCamera(IActor actor)
        {
            // Try to get the camera from the actor's controller
            return actor.Controller?.SpectateController?.CurrentCamera?.Transform;
        }
    }
}
