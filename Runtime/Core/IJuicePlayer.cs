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
        /// <param name="isCameraTarget">True if the play is addressed to a camera.</param>
        /// <param name="contactPoint">The contact point in world space (optional).</param>
        /// <param name="rotation">The rotation to apply to the feedback effects (optional).</param>
        void Play(JuiceFeedback feedback, bool isCameraTarget = false, Vector3? contactPoint = null, Quaternion? rotation = null);

        /// <summary>
        /// Plays a collection of effects directly.
        /// </summary>
        /// <param name="effects">The collection of effect data to run.</param>
        /// <param name="isCameraTarget">True if the play is addressed to a camera.</param>
        /// <param name="contactPoint">The contact point in world space (optional).</param>
        /// <param name="rotation">The rotation to apply to the feedback effects (optional).</param>
        void Play(IEnumerable<JuiceEffectData> effects, bool isCameraTarget = false, Vector3? contactPoint = null, Quaternion? rotation = null);

        public void StopAll();
    }
}