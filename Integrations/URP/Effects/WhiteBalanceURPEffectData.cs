using UnityEngine;
using UnityEngine.Rendering.Universal;

#nullable enable

namespace JuiceVFX.Integrations.URP
{
    public enum WhiteBalanceDrivenProperty
    {
        Temperature,
        Tint
    }

    [CreateAssetMenu(fileName = "NewWhiteBalanceURPEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/URP/White Balance")]
    public class WhiteBalanceURPEffectData : URPVolumeEffectData<WhiteBalance>
    {
        [Tooltip("The property to be driven by the intensity curve.")]
        public WhiteBalanceDrivenProperty DrivenProperty = WhiteBalanceDrivenProperty.Temperature;

        protected override URPVolumeEffectRunner<WhiteBalance> CreateRunnerInstance()
        {
            return new WhiteBalanceURPEffectRunner(this);
        }

        public override bool IsSameEffect(JuiceEffectData other)
        {
            if (other is WhiteBalanceURPEffectData otherEffect)
            {
                return DrivenProperty == otherEffect.DrivenProperty;
            }
            return false;
        }
    }

    public class WhiteBalanceURPEffectRunner : URPVolumeEffectRunner<WhiteBalance>
    {
        private new WhiteBalanceURPEffectData _data;

        public WhiteBalanceURPEffectRunner(WhiteBalanceURPEffectData data) : base(data)
        {
            _data = data;
        }

        protected override void InitializeComponentState()
        {
            if (_volumeComponent == null) return;

            switch (_data.DrivenProperty)
            {
                case WhiteBalanceDrivenProperty.Temperature:
                    _volumeComponent.temperature.overrideState = true;
                    break;
                case WhiteBalanceDrivenProperty.Tint:
                    _volumeComponent.tint.overrideState = true;
                    break;
            }

            _volumeComponent.active = true;
        }

        protected override void ApplyEffect(float intensity)
        {
            if (_volumeComponent == null) return;

            switch (_data.DrivenProperty)
            {
                case WhiteBalanceDrivenProperty.Temperature:
                    if (_data.IsRelative)
                    {
                        float baseVal = _initialComponentState != null && _initialComponentState.active
                            ? _initialComponentState.temperature.value : 0f;
                        _volumeComponent.temperature.value = baseVal + intensity;
                    }
                    else
                    {
                        _volumeComponent.temperature.value = intensity;
                    }
                    break;

                case WhiteBalanceDrivenProperty.Tint:
                    if (_data.IsRelative)
                    {
                        float baseVal = _initialComponentState != null && _initialComponentState.active
                            ? _initialComponentState.tint.value : 0f;
                        _volumeComponent.tint.value = baseVal + intensity;
                    }
                    else
                    {
                        _volumeComponent.tint.value = intensity;
                    }
                    break;
            }
        }
    }
}
