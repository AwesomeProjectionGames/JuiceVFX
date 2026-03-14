#nullable enable
#if DOTWEEN

using UnityEngine;
using DG.Tweening;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewDOTweenCameraShake", menuName = "AwesomeProjection/JuiceVFX/Effects/Camera/DOTween Camera Shake")]
    public class DOTweenCameraShakeEffectData : CameraShakeEffectData
    {
        [Header("DOTween Settings")]
        [Tooltip("DOTween ease to apply for damping. If set to INTERNAL_Custom, it will evaluate the original AnimationCurve instead.")]
        public Ease DampingEaseType = Ease.OutQuad;

        public override float EvaluateDampingCurve(float time)
        {
            if (DampingEaseType == Ease.INTERNAL_Custom) return base.EvaluateDampingCurve(time);
            return DG.Tweening.Core.Easing.EaseManager.Evaluate(DampingEaseType, null, time, 1f, DOTween.defaultEaseOvershootOrAmplitude, DOTween.defaultEasePeriod);
        }
    }
}
#endif
