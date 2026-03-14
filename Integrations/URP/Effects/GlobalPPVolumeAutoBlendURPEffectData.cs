#nullable enable
#if URP

using UnityEngine;
using UnityEngine.Rendering;

namespace JuiceVFX.Integrations.URP
{
    [CreateAssetMenu(fileName = "NewGlobalPPVolumeAutoBlendURPEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/URP/Global PP Volume Auto Blend")]
    public class GlobalPPVolumeAutoBlendURPEffectData : JuiceEffectData
    {
        [Tooltip("The Target volume whose weight will be modified. If null, attempts to find one on the JuicePlayer or Camera.")]
        public Volume? TargetVolume;

        [Tooltip("Curve driving the weight parameter over time.")]
        public AnimationCurve WeightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("Multiplier applied to the curve.")]
        public float WeightMultiplier = 1f;

        [Tooltip("If true, resets strictly to what the weight was before the effect started.")]
        public bool ResetStrictlyInitialValue = true;

        public override JuiceEffectRunner CreateRunner()
        {
            return new GlobalPPVolumeAutoBlendURPEffectRunner(this);
        }

        public override bool IsSameEffect(JuiceEffectData other)
        {
            if (other is GlobalPPVolumeAutoBlendURPEffectData otherBlend)
            {
                return TargetVolume == otherBlend.TargetVolume;
            }
            return false;
        }
    }

    public class GlobalPPVolumeAutoBlendURPEffectRunner : JuiceEffectRunner
    {
        private GlobalPPVolumeAutoBlendURPEffectData _data;
        private Volume? _targetVolume;
        private float _initialWeight;

        public GlobalPPVolumeAutoBlendURPEffectRunner(GlobalPPVolumeAutoBlendURPEffectData data)
        {
            _data = data;
        }

        public override void OnStart(JuicePlayer player)
        {
            _targetVolume = _data.TargetVolume;

            // Try resolving if null
            if (_targetVolume == null)
            {
                if (_data.Target == JuiceEffectTarget.Camera)
                {
                    if (Camera.main != null)
                    {
                        _targetVolume = Camera.main.GetComponentInChildren<Volume>();
                    }
                }
                else
                {
                    _targetVolume = player.GetComponentInChildren<Volume>();
                }
            }

            if (_targetVolume != null)
            {
                _initialWeight = _targetVolume.weight;
                ApplyWeight(0f);
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            if (_targetVolume == null)
            {
                Stop();
                return;
            }

            _timer += deltaTime;
            if (_timer >= _data.Duration)
            {
                Stop();
                return;
            }

            float rawProgress = _timer / _data.Duration;
            ApplyWeight(rawProgress);
        }

        private void ApplyWeight(float rawProgress)
        {
            if (_targetVolume == null) return;

            float weight = _data.WeightCurve.Evaluate(rawProgress) * _data.WeightMultiplier * Context.Multiplier;
            _targetVolume.weight = weight;
        }

        public override void OnStop()
        {
            if (_targetVolume != null && _data.ResetStrictlyInitialValue)
            {
                _targetVolume.weight = _initialWeight;
            }
        }
    }
}
#endif
