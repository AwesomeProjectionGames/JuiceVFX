#nullable enable
#if URP

using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace JuiceVFX.Integrations.URP
{
    [CreateAssetMenu(fileName = "NewLensDistortionURPEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/URP/Lens Distortion")]
    public class LensDistortionURPEffectData : URPVolumeEffectData<LensDistortion>
    {
        protected override URPVolumeEffectRunner<LensDistortion> CreateRunnerInstance()
        {
            return new LensDistortionURPEffectRunner(this);
        }
    }

    public class LensDistortionURPEffectRunner : URPVolumeEffectRunner<LensDistortion>
    {
        public LensDistortionURPEffectRunner(LensDistortionURPEffectData data) : base(data)
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
