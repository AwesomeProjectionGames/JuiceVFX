using GameFramework;
using UnityEngine;

namespace JuiceVFX
{
    public enum JuiceTargetType
    {
        Emitter,
        Target,
        ContactPoint,
        EmitterCamera,
        TargetCamera
    }
    
    /// <summary>
    /// Context information when playing one juice feedback.
    /// Used by any JuiceEffectRunner and in relation with JuiceTargetType.
    /// Heavily inspired by Unreal's GameplayEffectContext.
    /// </summary>
    public struct JuiceFeedbackContext
    {
        public IActor Emitter;
        public IActor Target;
        public Vector3? ContactPoint;
        public Quaternion? Rotation;
        
        public JuiceFeedbackContext(IActor emitter, IActor target, Vector3? contactPoint = null, Quaternion? rotation = null)
        {
            Emitter = emitter;
            Target = target;
            ContactPoint = contactPoint;
            Rotation = rotation;
        }
    }
}
