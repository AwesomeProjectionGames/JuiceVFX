using UnityEngine;

namespace JuiceVFX
{
    /// <summary>
    /// Base class for all Juice Effect data assets.
    /// Defines the configuration for a specific effect.
    /// </summary>
    public abstract class JuiceEffectData : ScriptableObject
    {
        [Tooltip("Duration of the effect in seconds.")]
        public float Duration = 0.5f;

        [Tooltip("Delay before the effect starts.")]
        public float Delay = 0f;

        [Tooltip("What target should this effect apply to?")]
        public JuiceTargetType TargetType = JuiceTargetType.Target;


        /// <summary>
        /// Creates the runner instance for this effect.
        /// </summary>
        public abstract JuiceEffectRunner CreateRunner();
    }
}
