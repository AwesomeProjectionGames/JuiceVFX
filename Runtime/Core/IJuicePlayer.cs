using GameFramework;
using UnityEngine;

namespace JuiceVFX
{
    /// <summary>
    /// Component responsible for playing Juice Feedbacks.
    /// Note that this component should manages the lifecycle of all active JuiceEffectRunners.
    /// You should not need to add this to every Actor that are targetted by JuiceFeedbacks, just have one in the scene (like a GameManager or a DI container).
    /// </summary>
    public interface IJuicePlayer
    {
        /// <summary>
        /// Plays the specified feedback.
        /// </summary>
        /// <param name="feedback">The feedback configuration to play.</param>
        /// <param name="target">The targeted actor receiving the feedback.</param>
        /// <param name="emitter">The actor emitting the feedback (optional).</param>
        /// <param name="contactPoint">The contact point in world space (optional).</param>
        /// <param name="rotation">The rotation to apply to the feedback effects (optional).</param>
        void Play(JuiceFeedback feedback, IActor target, IActor? emitter = null, Vector3? contactPoint = null, Quaternion? rotation = null);

        public void StopAll();
    }
}