#nullable enable
#if DOTWEEN

using UnityEngine;
using DG.Tweening;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewDOTweenFreezeFrame", menuName = "AwesomeProjection/JuiceVFX/Effects/Time/DOTween Freeze Frame")]
    public class DOTweenFreezeFrameEffectData : FreezeFrameEffectData
    {
        [Header("DOTween Settings")]
        [Tooltip("DOTween ease to apply for recovery. If set to INTERNAL_Custom, it will evaluate the original AnimationCurve instead.")]
        public Ease RecoveryEaseType = Ease.Linear;

        public override float EvaluateRecoveryCurve(float time)
        {
            if (RecoveryEaseType == Ease.INTERNAL_Custom) return base.EvaluateRecoveryCurve(time);
            return DG.Tweening.Core.Easing.EaseManager.Evaluate(RecoveryEaseType, null, time, 1f, DOTween.defaultEaseOvershootOrAmplitude, DOTween.defaultEasePeriod);
        }
    }
}
#endif
