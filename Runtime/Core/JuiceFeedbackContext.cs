#nullable enable

using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
#if ENABLE_INPUT_SYSTEM
        public Gamepad[]? Gamepads;
#endif
        public Renderer[]? Renderers;
        public Transform? RootTransform;
        public float Multiplier;

#if ENABLE_INPUT_SYSTEM
        public JuiceFeedbackContext(Vector3? contactPoint = null, Quaternion? rotation = null, Gamepad[]? gamepads = null, Renderer[]? renderers = null, Transform? rootTransform = null, float multiplier = 1f)
        {
            ContactPoint = contactPoint;
            Rotation = rotation;
            Gamepads = gamepads;
            Renderers = renderers;
            RootTransform = rootTransform;
            Multiplier = multiplier;
        }
#else
        public JuiceFeedbackContext(Vector3? contactPoint = null, Quaternion? rotation = null, Renderer[]? renderers = null, Transform? rootTransform = null, float multiplier = 1f)
        {
            ContactPoint = contactPoint;
            Rotation = rotation;
            Renderers = renderers;
            RootTransform = rootTransform;
            Multiplier = multiplier;
        }
#endif
    }
}
