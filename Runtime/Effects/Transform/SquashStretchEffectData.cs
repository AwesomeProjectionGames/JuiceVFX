#nullable enable

using UnityEngine;

namespace JuiceVFX
{
    public enum MassConservationMode
    {
        Uniform,
        KeepX,
        KeepY,
        KeepZ
    }

    public enum SquashAxis
    {
        X, Y, Z
    }

    [CreateAssetMenu(fileName = "NewSquashStretch", menuName = "AwesomeProjection/JuiceVFX/Effects/Transform/Squash and Stretch")]
    public class SquashStretchEffectData : JuiceEffectData
    {
        [Tooltip("The main axis to apply the scale curve on.")]
        public SquashAxis MainAxis = SquashAxis.Y;

        [Tooltip("Curve for scale multiplier over time on the main axis.")]
        public AnimationCurve ScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);

        [Tooltip("How should the scale be modified on the other axes to conserve volume/mass?")]
        public MassConservationMode ConservationMode = MassConservationMode.Uniform;

        [Tooltip("Return to original scale after effect finishes?")]
        public bool ResetOnComplete = true;

        public virtual float EvaluateScaleCurve(float time)
        {
            return ScaleCurve.Evaluate(time);
        }

        public override JuiceEffectRunner CreateRunner()
        {
            return new SquashStretchEffectRunner(this);
        }
    }

    public class SquashStretchEffectRunner : JuiceEffectRunner
    {
        private SquashStretchEffectData _data;
        private Vector3 _initialScale;
        private Transform? _target;

        public SquashStretchEffectRunner(SquashStretchEffectData data)
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

            float curveVal = _data.EvaluateScaleCurve(t);
            float mainScale = Mathf.LerpUnclamped(1f, curveVal, Context.Multiplier);
            // Avoid division by zero
            if (mainScale <= 0.0001f) mainScale = 0.0001f;

            float otherScaleUniform = 1f / Mathf.Sqrt(mainScale);
            float otherScaleSingle = 1f / mainScale;

            float scaleX = 1f;
            float scaleY = 1f;
            float scaleZ = 1f;

            switch (_data.MainAxis)
            {
                case SquashAxis.X: scaleX = mainScale; break;
                case SquashAxis.Y: scaleY = mainScale; break;
                case SquashAxis.Z: scaleZ = mainScale; break;
            }

            switch (_data.ConservationMode)
            {
                case MassConservationMode.Uniform:
                    if (_data.MainAxis != SquashAxis.X) scaleX = otherScaleUniform;
                    if (_data.MainAxis != SquashAxis.Y) scaleY = otherScaleUniform;
                    if (_data.MainAxis != SquashAxis.Z) scaleZ = otherScaleUniform;
                    break;
                case MassConservationMode.KeepX:
                    // X is kept constant (multiplier = 1).
                    if (_data.MainAxis != SquashAxis.X)
                    {
                        if (_data.MainAxis == SquashAxis.Y) scaleZ = otherScaleSingle;
                        if (_data.MainAxis == SquashAxis.Z) scaleY = otherScaleSingle;
                    }
                    break;
                case MassConservationMode.KeepY:
                    if (_data.MainAxis != SquashAxis.Y)
                    {
                        if (_data.MainAxis == SquashAxis.X) scaleZ = otherScaleSingle;
                        if (_data.MainAxis == SquashAxis.Z) scaleX = otherScaleSingle;
                    }
                    break;
                case MassConservationMode.KeepZ:
                    if (_data.MainAxis != SquashAxis.Z)
                    {
                        if (_data.MainAxis == SquashAxis.X) scaleY = otherScaleSingle;
                        if (_data.MainAxis == SquashAxis.Y) scaleX = otherScaleSingle;
                    }
                    break;
            }

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
