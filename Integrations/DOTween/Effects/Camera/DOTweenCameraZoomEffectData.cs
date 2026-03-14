#nullable enable
#if DOTWEEN

using UnityEngine;
using DG.Tweening;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewDOTweenCameraZoom", menuName = "AwesomeProjection/JuiceVFX/Effects/Camera/DOTween Camera Zoom")]
    public class DOTweenCameraZoomEffectData : CameraZoomEffectData
    {
        [Header("DOTween Settings")]
        [Tooltip("DOTween ease to apply. If set to INTERNAL_Custom, it will evaluate the original AnimationCurve instead.")]
        public Ease IntensityEaseType = Ease.OutQuad;

        public override float EvaluateIntensityCurve(float time)
        {
            if (IntensityEaseType == Ease.INTERNAL_Custom) return base.EvaluateIntensityCurve(time);
            return DG.Tweening.Core.Easing.EaseManager.Evaluate(IntensityEaseType, null, time, 1f, DOTween.defaultEaseOvershootOrAmplitude, DOTween.defaultEasePeriod);
        }
    }
}
#endif
