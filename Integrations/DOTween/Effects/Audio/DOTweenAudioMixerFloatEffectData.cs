#nullable enable
#if DOTWEEN

using UnityEngine;
using DG.Tweening;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewDOTweenAudioMixerFloat", menuName = "AwesomeProjection/JuiceVFX/Effects/Audio/DOTween Audio Mixer Float")]
    public class DOTweenAudioMixerFloatEffectData : AudioMixerFloatEffectData
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
