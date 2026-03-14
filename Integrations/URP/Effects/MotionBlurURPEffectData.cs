#nullable enable
#if URP

using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace JuiceVFX.Integrations.URP
{
    [CreateAssetMenu(fileName = "NewMotionBlurURPEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/URP/Motion Blur")]
    public class MotionBlurURPEffectData : URPVolumeEffectData<MotionBlur>
    {
        protected override URPVolumeEffectRunner<MotionBlur> CreateRunnerInstance()
        {
            return new MotionBlurURPEffectRunner(this);
        }
    }

    public class MotionBlurURPEffectRunner : URPVolumeEffectRunner<MotionBlur>
    {
        public MotionBlurURPEffectRunner(MotionBlurURPEffectData data) : base(data)
        {
        }

        protected override void InitializeComponentState()
        {
            if (_volumeComponent == null) return;
            
            _volumeComponent.intensity.overrideState = true;
            _volumeComponent.active = true;
        }

        protected override void ApplyEffect(float intensity)
        {
            if (_volumeComponent == null) return;

            if (_data.IsRelative)
            {
                float baseIntensity = _initialComponentState != null && _initialComponentState.active 
                    ? _initialComponentState.intensity.value : 0f;
                _volumeComponent.intensity.value = baseIntensity + intensity;
            }
            else
            {
                _volumeComponent.intensity.value = intensity;
            }
        }
    }
}
#endif
