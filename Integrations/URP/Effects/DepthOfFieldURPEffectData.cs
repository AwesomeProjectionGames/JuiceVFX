using UnityEngine;
using UnityEngine.Rendering.Universal;

#nullable enable

namespace JuiceVFX.Integrations.URP
{
    public enum DepthOfFieldDrivenProperty
    {
        FocusDistance,
        FocalLength
    }

    [CreateAssetMenu(fileName = "NewDepthOfFieldURPEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/URP/Depth Of Field")]
    public class DepthOfFieldURPEffectData : URPVolumeEffectData<DepthOfField>
    {
        [Tooltip("The property to be driven by the intensity curve.")]
        public DepthOfFieldDrivenProperty DrivenProperty = DepthOfFieldDrivenProperty.FocusDistance;

        protected override URPVolumeEffectRunner<DepthOfField> CreateRunnerInstance()
        {
            return new DepthOfFieldURPEffectRunner(this);
        }

        public override bool IsSameEffect(JuiceEffectData other)
        {
            if (other is DepthOfFieldURPEffectData otherEffect)
            {
                return DrivenProperty == otherEffect.DrivenProperty;
            }
            return false;
        }
    }

    public class DepthOfFieldURPEffectRunner : URPVolumeEffectRunner<DepthOfField>
    {
        private new DepthOfFieldURPEffectData _data;

        public DepthOfFieldURPEffectRunner(DepthOfFieldURPEffectData data) : base(data)
        {
            _data = data;
        }

        protected override void InitializeComponentState()
        {
            if (_volumeComponent == null) return;

            switch (_data.DrivenProperty)
            {
                case DepthOfFieldDrivenProperty.FocusDistance:
                    _volumeComponent.focusDistance.overrideState = true;
                    break;
                case DepthOfFieldDrivenProperty.FocalLength:
                    _volumeComponent.focalLength.overrideState = true;
                    break;
            }

            // Ensure mode is set
            _volumeComponent.mode.overrideState = true;
            _volumeComponent.mode.value = DepthOfFieldMode.Bokeh;

            _volumeComponent.active = true;
        }

        protected override void ApplyEffect(float intensity)
        {
            if (_volumeComponent == null) return;

            if (_data.DrivenProperty == DepthOfFieldDrivenProperty.FocusDistance)
            {
                if (_data.IsRelative)
                {
                    float baseVal = _initialComponentState != null && _initialComponentState.active
                        ? _initialComponentState.focusDistance.value : 10f;
                    _volumeComponent.focusDistance.value = baseVal + intensity;
                }
                else
                {
                    _volumeComponent.focusDistance.value = intensity;
                }
            }
            else if (_data.DrivenProperty == DepthOfFieldDrivenProperty.FocalLength)
            {
                if (_data.IsRelative)
                {
                    float baseVal = _initialComponentState != null && _initialComponentState.active
                        ? _initialComponentState.focalLength.value : 50f;
                    _volumeComponent.focalLength.value = baseVal + intensity;
                }
                else
                {
                    _volumeComponent.focalLength.value = intensity;
                }
            }
        }
    }
}
