using UnityEngine;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewScaleEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/Transform/Scale")]
    public class ScaleEffectData : JuiceEffectData
    {
        [Tooltip("Curve for scale multiplier over time.")]
        public AnimationCurve ScaleCurveX = AnimationCurve.EaseInOut(0, 1, 1, 1);
        public AnimationCurve ScaleCurveY = AnimationCurve.Constant(0, 1, 1);
        public AnimationCurve ScaleCurveZ = AnimationCurve.EaseInOut(0, 1, 1, 1);

        [Tooltip("Return to original scale after effect finishes?")]
        public bool ResetOnComplete = true;

        public override JuiceEffectRunner CreateRunner()
        {
            return new ScaleEffectRunner(this);
        }
    }

    public class ScaleEffectRunner : JuiceEffectRunner
    {
        private ScaleEffectData _data;
        private Vector3 _initialScale;
        private Transform _target;

        public ScaleEffectRunner(ScaleEffectData data)
        {
            _data = data;
        }

        public override void OnStart(JuicePlayer player)
        {
            _target = Context.RootTransform;
            if (_target != null)
            {
                _initialScale = _target.localScale;
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            if (_target == null) return;

            _timer += deltaTime;
            float t = Mathf.Clamp01(_timer / _data.Duration);

            // If duration is 0, we assume instant or one-shot, but for curves we need time.
            if (_data.Duration <= 0) t = 1f;

            float scaleX = Mathf.LerpUnclamped(1f, _data.ScaleCurveX.Evaluate(t), Context.Multiplier);
            float scaleY = Mathf.LerpUnclamped(1f, _data.ScaleCurveY.Evaluate(t), Context.Multiplier);
            float scaleZ = Mathf.LerpUnclamped(1f, _data.ScaleCurveZ.Evaluate(t), Context.Multiplier);

            _target.localScale = new Vector3(_initialScale.x * scaleX, _initialScale.y * scaleY, _initialScale.z * scaleZ);

            if (t >= 1f)
            {
                Stop();
            }
        }

        public override void OnStop()
        {
            if (_data.ResetOnComplete && _target != null)
            {
                _target.localScale = _initialScale;
            }
        }
    }
}
