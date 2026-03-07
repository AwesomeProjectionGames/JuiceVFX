using UnityEngine;
using UnityEngine.Rendering.Universal;

#nullable enable

namespace JuiceVFX.Integrations.URP
{
    [CreateAssetMenu(fileName = "NewVignetteURPEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/URP/Vignette")]
    public class VignetteURPEffectData : URPVolumeEffectData<Vignette>
    {
        [Tooltip("Optional override for the vignette color. Leave as white or fully transparent to not affect it.")]
        public Color ColorOverride = Color.black;
        
        [Tooltip("If true, color override is applied. Otherwise, relies purely on intensity.")]
        public bool ApplyColorOverride = false;

        protected override URPVolumeEffectRunner<Vignette> CreateRunnerInstance()
        {
            return new VignetteURPEffectRunner(this);
        }
    }

    public class VignetteURPEffectRunner : URPVolumeEffectRunner<Vignette>
    {
        private new VignetteURPEffectData _data;

        public VignetteURPEffectRunner(VignetteURPEffectData data) : base(data)
        {
            _data = data;
        }

        protected override void InitializeComponentState()
        {
            if (_volumeComponent == null) return;
            
            _volumeComponent.intensity.overrideState = true;
            if (_data.ApplyColorOverride)
            {
                _volumeComponent.color.overrideState = true;
                _volumeComponent.color.value = _data.ColorOverride;
            }

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
