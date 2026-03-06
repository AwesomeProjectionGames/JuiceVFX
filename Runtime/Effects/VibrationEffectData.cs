using UnityEngine;
using UnityEngine.InputSystem;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewVibrationEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/Vibration")]
    public class VibrationEffectData : JuiceEffectData
    {
        [Tooltip("Low Frequency Motor speed curve.")]
        public AnimationCurve LowFrequencyMotor = AnimationCurve.Linear(0, 0.5f, 1, 0);

        [Tooltip("High Frequency Motor speed curve.")]
        public AnimationCurve HighFrequencyMotor = AnimationCurve.Linear(0, 0.5f, 1, 0);

        public override JuiceEffectRunner CreateRunner()
        {
            return new VibrationEffectRunner(this);
        }
    }

    public class VibrationEffectRunner : JuiceEffectRunner
    {
        private VibrationEffectData _data;
        private Gamepad _gamepad;

        public VibrationEffectRunner(VibrationEffectData data)
        {
            _data = data;
        }

        public override void OnStart(JuicePlayer player)
        {
            _gamepad = Context.Gamepad;
        }

        public override void OnUpdate(float deltaTime)
        {
            if (_gamepad == null) return;

            _timer += deltaTime;
            float t = Mathf.Clamp01(_timer / _data.Duration);
            if (_data.Duration <= 0) t = 1f;

            float low = _data.LowFrequencyMotor.Evaluate(t);
            float high = _data.HighFrequencyMotor.Evaluate(t);

            _gamepad.SetMotorSpeeds(low, high);

            if (t >= 1f)
            {
                Stop();
            }
        }

        public override void OnStop()
        {
            if (_gamepad != null)
            {
                _gamepad.SetMotorSpeeds(0, 0);
            }
        }
    }
}
