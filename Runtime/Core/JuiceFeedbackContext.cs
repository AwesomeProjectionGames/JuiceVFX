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
        public Gamepad[] Gamepads;
        public Renderer[] Renderers;
        public Transform RootTransform;
        public float Multiplier;

        public JuiceFeedbackContext(Vector3? contactPoint = null, Quaternion? rotation = null, Gamepad[] gamepads = null, Renderer[] renderers = null, Transform rootTransform = null, float multiplier = 1f)
        {
            ContactPoint = contactPoint;
            Rotation = rotation;
            Gamepads = gamepads;
            Renderers = renderers;
            RootTransform = rootTransform;
            Multiplier = multiplier;
        }
    }
}
