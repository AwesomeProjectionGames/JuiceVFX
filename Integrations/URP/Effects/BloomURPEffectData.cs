#nullable enable
#if URP

using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace JuiceVFX.Integrations.URP
{
    [CreateAssetMenu(fileName = "NewBloomURPEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/URP/Bloom")]
    public class BloomURPEffectData : URPVolumeEffectData<Bloom>
    {
        protected override URPVolumeEffectRunner<Bloom> CreateRunnerInstance()
        {
            return new BloomURPEffectRunner(this);
        }
    }

    public class BloomURPEffectRunner : URPVolumeEffectRunner<Bloom>
    {
        public BloomURPEffectRunner(BloomURPEffectData data) : base(data)
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
