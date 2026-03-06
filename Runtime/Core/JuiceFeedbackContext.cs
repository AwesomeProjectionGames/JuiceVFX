using UnityEngine;

namespace JuiceVFX
{
    public enum JuiceTargetType
    {
        Target,
        ContactPoint
    }

    /// <summary>
    /// Context information when playing one juice feedback.
    /// Used by any JuiceEffectRunner and in relation with JuiceTargetType.
    /// Heavily inspired by Unreal's GameplayEffectContext.
    /// </summary>
    public struct JuiceFeedbackContext
    {
        public Vector3? ContactPoint;
        public Quaternion? Rotation;

        public JuiceFeedbackContext(Vector3? contactPoint = null, Quaternion? rotation = null)
        {
            ContactPoint = contactPoint;
            Rotation = rotation;
        }
    }
}
