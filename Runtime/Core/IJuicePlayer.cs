using System.Collections.Generic;
using UnityEngine;

namespace JuiceVFX
{
    /// <summary>
    /// Component responsible for playing Juice Feedbacks.
    /// Note that this component manages the lifecycle of all active JuiceEffectRunners.
    /// </summary>
    public interface IJuicePlayer
    {
        /// <summary>
        /// Plays the specified feedback.
        /// </summary>
        /// <param name="feedback">The feedback configuration to play.</param>
        /// <param name="contactPoint">The contact point in world space (optional).</param>
        /// <param name="rotation">The rotation to apply to the feedback effects (optional).</param>
        void Play(JuiceFeedback feedback, Vector3? contactPoint = null, Quaternion? rotation = null);

        /// <summary>
        /// Plays a collection of effects directly.
        /// </summary>
        void Play(IEnumerable<JuiceEffectData> effects, Vector3? contactPoint = null, Quaternion? rotation = null);

        public void StopAll();
    }
}