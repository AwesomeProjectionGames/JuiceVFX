using UnityEngine;
using UnityEngine.Rendering.Universal;

#nullable enable

namespace JuiceVFX.Integrations.URP
{
    public enum ColorAdjustmentsDrivenProperty
    {
        PostExposure,
        Contrast,
        HueShift,
        Saturation
    }

    [CreateAssetMenu(fileName = "NewColorAdjustmentsURPEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/URP/Color Adjustments")]
    public class ColorAdjustmentsURPEffectData : URPVolumeEffectData<ColorAdjustments>
    {
        [Tooltip("The property to be driven by the intensity curve.")]
        public ColorAdjustmentsDrivenProperty DrivenProperty = ColorAdjustmentsDrivenProperty.PostExposure;

        protected override URPVolumeEffectRunner<ColorAdjustments> CreateRunnerInstance()
        {
            return new ColorAdjustmentsURPEffectRunner(this);
        }

        public override bool IsSameEffect(JuiceEffectData other)
        {
            if (other is ColorAdjustmentsURPEffectData otherEffect)
            {
                return DrivenProperty == otherEffect.DrivenProperty;
            }
            return false;
        }
    }

    public class ColorAdjustmentsURPEffectRunner : URPVolumeEffectRunner<ColorAdjustments>
    {
        private new ColorAdjustmentsURPEffectData _data;

        public ColorAdjustmentsURPEffectRunner(ColorAdjustmentsURPEffectData data) : base(data)
        {
            _data = data;
        }

        protected override void InitializeComponentState()
        {
            if (_volumeComponent == null) return;

            switch (_data.DrivenProperty)
            {
                case ColorAdjustmentsDrivenProperty.PostExposure:
                    _volumeComponent.postExposure.overrideState = true;
                    break;
                case ColorAdjustmentsDrivenProperty.Contrast:
                    _volumeComponent.contrast.overrideState = true;
                    break;
                case ColorAdjustmentsDrivenProperty.HueShift:
                    _volumeComponent.hueShift.overrideState = true;
                    break;
                case ColorAdjustmentsDrivenProperty.Saturation:
                    _volumeComponent.saturation.overrideState = true;
                    break;
            }

            _volumeComponent.active = true;
        }

        protected override void ApplyEffect(float intensity)
        {
            if (_volumeComponent == null) return;

            switch (_data.DrivenProperty)
            {
                case ColorAdjustmentsDrivenProperty.PostExposure:
                    if (_data.IsRelative)
                    {
                        float baseVal = _initialComponentState != null && _initialComponentState.active
                            ? _initialComponentState.postExposure.value : 0f;
                        _volumeComponent.postExposure.value = baseVal + intensity;
                    }
                    else
                    {
                        _volumeComponent.postExposure.value = intensity;
                    }
                    break;

                case ColorAdjustmentsDrivenProperty.Contrast:
                    if (_data.IsRelative)
                    {
                        float baseVal = _initialComponentState != null && _initialComponentState.active
                            ? _initialComponentState.contrast.value : 0f;
                        _volumeComponent.contrast.value = baseVal + intensity;
                    }
                    else
                    {
                        _volumeComponent.contrast.value = intensity;
                    }
                    break;

                case ColorAdjustmentsDrivenProperty.HueShift:
                    if (_data.IsRelative)
                    {
                        float baseVal = _initialComponentState != null && _initialComponentState.active
                            ? _initialComponentState.hueShift.value : 0f;
                        _volumeComponent.hueShift.value = baseVal + intensity;
                    }
                    else
                    {
                        _volumeComponent.hueShift.value = intensity;
                    }
                    break;

                case ColorAdjustmentsDrivenProperty.Saturation:
                    if (_data.IsRelative)
                    {
                        float baseVal = _initialComponentState != null && _initialComponentState.active
                            ? _initialComponentState.saturation.value : 0f;
                        _volumeComponent.saturation.value = baseVal + intensity;
                    }
                    else
                    {
                        _volumeComponent.saturation.value = intensity;
                    }
                    break;
            }
        }
    }
}
