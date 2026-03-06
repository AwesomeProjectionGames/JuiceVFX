using UnityEngine;
using UnityEngine.InputSystem;

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
        public Gamepad Gamepad;
        public Renderer[] Renderers;
        public Transform RootTransform;

        public JuiceFeedbackContext(Vector3? contactPoint = null, Quaternion? rotation = null, Gamepad gamepad = null, Renderer[] renderers = null, Transform rootTransform = null)
        {
            ContactPoint = contactPoint;
            Rotation = rotation;
            Gamepad = gamepad;
            Renderers = renderers;
            RootTransform = rootTransform;
        }
    }
}
