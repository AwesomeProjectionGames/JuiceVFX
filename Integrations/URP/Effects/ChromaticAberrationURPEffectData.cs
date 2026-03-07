using UnityEngine;
using UnityEngine.Rendering.Universal;

#nullable enable

namespace JuiceVFX.Integrations.URP
{
    [CreateAssetMenu(fileName = "NewChromaticAberrationURPEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/URP/Chromatic Aberration")]
    public class ChromaticAberrationURPEffectData : URPVolumeEffectData<ChromaticAberration>
    {
        protected override URPVolumeEffectRunner<ChromaticAberration> CreateRunnerInstance()
        {
            return new ChromaticAberrationURPEffectRunner(this);
        }
    }

    public class ChromaticAberrationURPEffectRunner : URPVolumeEffectRunner<ChromaticAberration>
    {
        public ChromaticAberrationURPEffectRunner(ChromaticAberrationURPEffectData data) : base(data)
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
