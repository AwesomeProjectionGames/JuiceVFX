using UnityEngine;
using UnityEngine.Rendering.Universal;

#nullable enable

namespace JuiceVFX.Integrations.URP
{
    public enum PaniniProjectionDrivenProperty
    {
        Distance,
        CropToFit
    }

    [CreateAssetMenu(fileName = "NewPaniniProjectionURPEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/URP/Panini Projection")]
    public class PaniniProjectionURPEffectData : URPVolumeEffectData<PaniniProjection>
    {
        [Tooltip("The property to be driven by the intensity curve.")]
        public PaniniProjectionDrivenProperty DrivenProperty = PaniniProjectionDrivenProperty.Distance;

        protected override URPVolumeEffectRunner<PaniniProjection> CreateRunnerInstance()
        {
            return new PaniniProjectionURPEffectRunner(this);
        }

        public override bool IsSameEffect(JuiceEffectData other)
        {
            if (other is PaniniProjectionURPEffectData otherEffect)
            {
                return DrivenProperty == otherEffect.DrivenProperty;
            }
            return false;
        }
    }

    public class PaniniProjectionURPEffectRunner : URPVolumeEffectRunner<PaniniProjection>
    {
        private new PaniniProjectionURPEffectData _data;

        public PaniniProjectionURPEffectRunner(PaniniProjectionURPEffectData data) : base(data)
        {
            _data = data;
        }

        protected override void InitializeComponentState()
        {
            if (_volumeComponent == null) return;

            switch (_data.DrivenProperty)
            {
                case PaniniProjectionDrivenProperty.Distance:
                    _volumeComponent.distance.overrideState = true;
                    break;
                case PaniniProjectionDrivenProperty.CropToFit:
                    _volumeComponent.cropToFit.overrideState = true;
                    break;
            }

            _volumeComponent.active = true;
        }

        protected override void ApplyEffect(float intensity)
        {
            if (_volumeComponent == null) return;

            switch (_data.DrivenProperty)
            {
                case PaniniProjectionDrivenProperty.Distance:
                    if (_data.IsRelative)
                    {
                        float baseVal = _initialComponentState != null && _initialComponentState.active
                            ? _initialComponentState.distance.value : 0f;
                        _volumeComponent.distance.value = baseVal + intensity;
                    }
                    else
                    {
                        _volumeComponent.distance.value = intensity;
                    }
                    break;

                case PaniniProjectionDrivenProperty.CropToFit:
                    if (_data.IsRelative)
                    {
                        float baseVal = _initialComponentState != null && _initialComponentState.active
                            ? _initialComponentState.cropToFit.value : 1f;
                        _volumeComponent.cropToFit.value = baseVal + intensity;
                    }
                    else
                    {
                        _volumeComponent.cropToFit.value = intensity;
                    }
                    break;
            }
        }
    }
}
