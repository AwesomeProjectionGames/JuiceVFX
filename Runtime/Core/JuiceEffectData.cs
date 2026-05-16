using UnityEngine;

namespace JuiceVFX
{
    public enum JuiceEffectTarget
    {
        Emitter,
        Anywhere,
        Camera
    }

    /// <summary>
    /// Base class for all Juice Effect data assets.
    /// Defines the configuration for a specific effect.
    /// </summary>
    public abstract class JuiceEffectData : ScriptableObject
    {
        public virtual JuiceEffectTarget Target => JuiceEffectTarget.Emitter;

        [Tooltip("Duration of the effect in seconds.")]
        public float Duration = 1.0f;

        [Tooltip("If true, this effect's duration can be overridden dynamically by the JuicePlayer.")]
        public bool AllowDurationOverride = true;

        [Tooltip("Delay before the effect starts.")]
        public float Delay = 0f;

        /// <summary>
        /// Creates the runner instance for this effect.
        /// </summary>
        public abstract JuiceEffectRunner CreateRunner();

        /// <summary>
        /// Determines if another effect is considered equal, allowing the runner to stop
        /// existing similar effects when a new one is played to prevent race conditions.
        /// By default, it considers them equal if they are of the exact same data type.
        /// </summary>
        public virtual bool IsSameEffect(JuiceEffectData other)
        {
            if (other == null) return false;
            return this.GetType() == other.GetType();
        }
    }
}
