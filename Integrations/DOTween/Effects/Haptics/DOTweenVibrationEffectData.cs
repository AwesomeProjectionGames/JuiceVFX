#nullable enable
#if DOTWEEN

using UnityEngine;
using DG.Tweening;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewDOTweenVibration", menuName = "AwesomeProjection/JuiceVFX/Effects/Haptics/DOTween Vibration")]
    public class DOTweenVibrationEffectData : VibrationEffectData
    {
        [Header("DOTween Settings")]
        [Tooltip("DOTween ease to apply for the low frequency motor. If set to INTERNAL_Custom, it will evaluate the original AnimationCurve instead.")]
        public Ease LowFrequencyEaseType = Ease.OutQuad;
        [Tooltip("DOTween ease to apply for the high frequency motor. If set to INTERNAL_Custom, it will evaluate the original AnimationCurve instead.")]
        public Ease HighFrequencyEaseType = Ease.OutQuad;

        public override float EvaluateLowFrequencyMotor(float time)
        {
            if (LowFrequencyEaseType == Ease.INTERNAL_Custom) return base.EvaluateLowFrequencyMotor(time);
            return DG.Tweening.Core.Easing.EaseManager.Evaluate(LowFrequencyEaseType, null, time, 1f, DOTween.defaultEaseOvershootOrAmplitude, DOTween.defaultEasePeriod);
        }

        public override float EvaluateHighFrequencyMotor(float time)
        {
            if (HighFrequencyEaseType == Ease.INTERNAL_Custom) return base.EvaluateHighFrequencyMotor(time);
            return DG.Tweening.Core.Easing.EaseManager.Evaluate(HighFrequencyEaseType, null, time, 1f, DOTween.defaultEaseOvershootOrAmplitude, DOTween.defaultEasePeriod);
        }
    }
}
#endif
