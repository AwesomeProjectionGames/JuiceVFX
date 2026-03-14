#nullable enable
#if DOTWEEN

using UnityEngine;
using DG.Tweening;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewDOTweenScaleEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/Transform/DOTween Scale")]
    public class DOTweenScaleEffectData : ScaleEffectData
    {
        [Header("DOTween Settings")]
        [Tooltip("DOTween ease to apply. If set to INTERNAL_Custom, it will evaluate the original AnimationCurve instead.")]
        public Ease EaseTypeX = Ease.OutQuad;
        public Ease EaseTypeY = Ease.OutQuad;
        public Ease EaseTypeZ = Ease.OutQuad;

        public override float EvaluateScaleCurveX(float time)
        {
            if (EaseTypeX == Ease.INTERNAL_Custom) return base.EvaluateScaleCurveX(time);
            return DG.Tweening.Core.Easing.EaseManager.Evaluate(EaseTypeX, null, time, 1f, DOTween.defaultEaseOvershootOrAmplitude, DOTween.defaultEasePeriod);
        }

        public override float EvaluateScaleCurveY(float time)
        {
            if (EaseTypeY == Ease.INTERNAL_Custom) return base.EvaluateScaleCurveY(time);
            return DG.Tweening.Core.Easing.EaseManager.Evaluate(EaseTypeY, null, time, 1f, DOTween.defaultEaseOvershootOrAmplitude, DOTween.defaultEasePeriod);
        }

        public override float EvaluateScaleCurveZ(float time)
        {
            if (EaseTypeZ == Ease.INTERNAL_Custom) return base.EvaluateScaleCurveZ(time);
            return DG.Tweening.Core.Easing.EaseManager.Evaluate(EaseTypeZ, null, time, 1f, DOTween.defaultEaseOvershootOrAmplitude, DOTween.defaultEasePeriod);
        }
    }
}
#endif
