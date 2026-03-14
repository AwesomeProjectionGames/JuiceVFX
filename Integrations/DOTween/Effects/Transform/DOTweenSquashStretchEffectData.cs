#nullable enable
#if DOTWEEN

using UnityEngine;
using DG.Tweening;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewDOTweenSquashStretch", menuName = "AwesomeProjection/JuiceVFX/Effects/Transform/DOTween Squash and Stretch")]
    public class DOTweenSquashStretchEffectData : SquashStretchEffectData
    {
        [Header("DOTween Settings")]
        [Tooltip("DOTween ease to apply. If set to INTERNAL_Custom, it will evaluate the original AnimationCurve instead.")]
        public Ease EaseType = Ease.OutQuad;

        public override float EvaluateScaleCurve(float time)
        {
            if (EaseType == Ease.INTERNAL_Custom) return base.EvaluateScaleCurve(time);
            return DG.Tweening.Core.Easing.EaseManager.Evaluate(EaseType, null, time, 1f, DOTween.defaultEaseOvershootOrAmplitude, DOTween.defaultEasePeriod);
        }
    }
}
#endif
