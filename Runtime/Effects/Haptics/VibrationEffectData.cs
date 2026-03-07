using UnityEngine;
using UnityEngine.InputSystem;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewVibrationEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/Haptics/Vibration")]
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
        private Gamepad[] _gamepads;

        public VibrationEffectRunner(VibrationEffectData data)
        {
            _data = data;
        }

        public override void OnStart(JuicePlayer player)
        {
            _gamepads = Context.Gamepads ?? System.Array.Empty<Gamepad>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (_gamepads == null || _gamepads.Length == 0) return;

            _timer += deltaTime;
            float t = Mathf.Clamp01(_timer / _data.Duration);
            if (_data.Duration <= 0) t = 1f;

            float low = _data.LowFrequencyMotor.Evaluate(t);
            float high = _data.HighFrequencyMotor.Evaluate(t);

            foreach (var gamepad in _gamepads)
            {
                if (gamepad == null) continue;
                gamepad.SetMotorSpeeds(low, high);
            }

            if (t >= 1f)
            {
                Stop();
            }
        }

        public override void OnStop()
        {
            if (_gamepads != null)
            {
                foreach (var gamepad in _gamepads)
                {
                    if (gamepad == null) continue;
                    gamepad.SetMotorSpeeds(0, 0);
                }
            }
        }
    }
}
