#nullable enable

using UnityEngine;
using UnityEngine.Audio;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewAudioMixerFloatEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/Audio/Audio Mixer Float")]
    public class AudioMixerFloatEffectData : JuiceEffectData
    {
        [Header("Audio Mixer Definition")]
        [Tooltip("The Audio Mixer containing the exposed parameter.")]
        public AudioMixer? AudioMixer;

        [Tooltip("The exposed parameter name on the Audio Mixer (e.g., MasterVolume, DistortionLevel).")]
        public string ParameterName = "Volume";

        [Header("Effect Settings")]
        [Tooltip("Target float value when curve is at 1.")]
        public float TargetValue = 1f;

        [Tooltip("Intensity curve of the effect over time. Acts as a multiplier for the Target Value.")]
        public AnimationCurve IntensityCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        public virtual float EvaluateIntensityCurve(float time) => IntensityCurve.Evaluate(time);

        [Tooltip("If true, the effect value is added to the base value instead of replacing it.")]
        public bool IsRelative = false;

        [Tooltip("If true, the audio mixer parameter is strictly restored to its initial value when the effect stops.")]
        public bool ResetInitialValue = true;

        public override JuiceEffectRunner CreateRunner()
        {
            return new AudioMixerFloatEffectRunner(this);
        }

        public override bool IsSameEffect(JuiceEffectData other)
        {
            return other is AudioMixerFloatEffectData otherData &&
                   AudioMixer == otherData.AudioMixer &&
                   ParameterName == otherData.ParameterName;
        }
    }

    public class AudioMixerFloatEffectRunner : JuiceEffectRunner
    {
        private AudioMixerFloatEffectData _data;
        private float _initialValue;
        private bool _canReadInitialValue;

        public AudioMixerFloatEffectRunner(AudioMixerFloatEffectData data)
        {
            _data = data;
        }

        public override void OnStart(JuicePlayer player)
        {
            if (_data.AudioMixer != null)
            {
                _canReadInitialValue = _data.AudioMixer.GetFloat(_data.ParameterName, out _initialValue);
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            if (_data.AudioMixer == null) return;

            _timer += deltaTime;
            float t = Mathf.Clamp01(_timer / _data.Duration);
            if (_data.Duration <= 0) t = 1f;

            float intensity = _data.EvaluateIntensityCurve(t) * Context.Multiplier;
            ApplyValue(intensity);

            if (t >= 1f)
            {
                Stop();
            }
        }

        public override void OnStop()
        {
            if (_data.AudioMixer == null) return;

            if (_data.ResetInitialValue && _canReadInitialValue)
            {
                _data.AudioMixer.SetFloat(_data.ParameterName, _initialValue);
            }
            else
            {
                float finalIntensity = _data.EvaluateIntensityCurve(1f);
                ApplyValue(finalIntensity);
            }
        }

        private void ApplyValue(float intensity)
        {
            if (_data.AudioMixer == null) return;

            float result = _data.TargetValue * intensity;
            if (_data.IsRelative && _canReadInitialValue)
            {
                result += _initialValue;
            }

            _data.AudioMixer.SetFloat(_data.ParameterName, result);
        }
    }
}
