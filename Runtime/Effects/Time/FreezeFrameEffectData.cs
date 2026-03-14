using UnityEngine;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewFreezeFrame", menuName = "AwesomeProjection/JuiceVFX/Effects/Time/Freeze Frame")]
    public class FreezeFrameEffectData : JuiceEffectData
    {
        [Tooltip("Time scale during the freeze.")]
        public float TimeScale = 0f;

        [Tooltip("Curve to blend back to normal time scale (optional).")]
        public AnimationCurve RecoveryCurve = AnimationCurve.Linear(0, 0, 1, 1);

        public virtual float EvaluateRecoveryCurve(float time) => RecoveryCurve.Evaluate(time);

        public override JuiceEffectRunner CreateRunner()
        {
            return new FreezeFrameEffectRunner(this);
        }
    }

    public class FreezeFrameEffectRunner : JuiceEffectRunner
    {
        private FreezeFrameEffectData _data;
        private float _originalTimeScale;

        public FreezeFrameEffectRunner(FreezeFrameEffectData data)
        {
            _data = data;
        }

        public override void OnStart(JuicePlayer player)
        {
            _originalTimeScale = Time.timeScale;
            Time.timeScale = _data.TimeScale;
        }

        public override void OnUpdate(float deltaTime)
        {
            // We use unscaled time for duration because timeScale might be 0
            _timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(_timer / _data.Duration);
            if (_data.Duration <= 0) t = 1f;

            if (_data.Duration > 0)
            {
                // Blending back
                float recoverT = _data.EvaluateRecoveryCurve(t);
                Time.timeScale = Mathf.Lerp(_data.TimeScale, 1f, recoverT); // Assuming 1f is normal
            }

            if (t >= 1f)
            {
                Stop();
            }
        }

        public override void OnStop()
        {
            Time.timeScale = 1f; // Force reset to 1
        }
    }
}
