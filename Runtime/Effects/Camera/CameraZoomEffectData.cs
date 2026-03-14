#nullable enable

using UnityEngine;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewCameraZoomEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/Camera/Camera Zoom")]
    public class CameraZoomEffectData : JuiceEffectData
    {
        public override JuiceEffectTarget Target => JuiceEffectTarget.Camera;

        [Header("Effect Settings")]
        [Tooltip("Target zoom multiplier when curve is at 1. (e.g., 2 = 2x zoom = half FOV, 0.5 = 0.5x zoom = double FOV)")]
        public float TargetZoomMultiplier = 2f;

        [Tooltip("Intensity curve of the effect over time. 0 means no zoom (1x), 1 means TargetZoomMultiplier.")]
        public AnimationCurve IntensityCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        public virtual float EvaluateIntensityCurve(float time) => IntensityCurve.Evaluate(time);

        [Tooltip("If true, the camera property is strictly restored to its initial value when the effect stops.")]
        public bool ResetInitialValue = true;

        [Tooltip("If true, the effect never completes automatically and holds its value until explicitly stopped.")]
        public bool HoldUntilStopped = false;

        public override JuiceEffectRunner CreateRunner()
        {
            return new CameraZoomEffectRunner(this);
        }

        public override bool IsSameEffect(JuiceEffectData other)
        {
            return other is CameraZoomEffectData;
        }
    }

    public class CameraZoomEffectRunner : JuiceEffectRunner
    {
        private CameraZoomEffectData _data;
        private Camera? _camera;
        private float _initialValue;

        public CameraZoomEffectRunner(CameraZoomEffectData data)
        {
            _data = data;
        }

        public override void OnStart(JuicePlayer player)
        {
            _camera = Context.RootTransform != null ? Context.RootTransform.GetComponentInChildren<Camera>() : null;
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_camera != null)
            {
                _initialValue = _camera.orthographic ? _camera.orthographicSize : _camera.fieldOfView;
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            if (_camera == null) return;

            _timer += deltaTime;
            float t = Mathf.Clamp01(_timer / _data.Duration);
            if (_data.Duration <= 0) t = 1f;

            float intensity = _data.EvaluateIntensityCurve(t) * Context.Multiplier;
            ApplyValue(intensity);

            if (t >= 1f && !_data.HoldUntilStopped)
            {
                Stop();
            }
        }

        public override void OnStop()
        {
            if (_camera == null) return;

            if (_data.ResetInitialValue)
            {
                ApplyRawValue(_initialValue);
            }
            else
            {
                float finalIntensity = _data.EvaluateIntensityCurve(1f);
                ApplyValue(finalIntensity);
            }
        }

        private void ApplyValue(float intensity)
        {
            // Calculate the current zoom multiplier based on intensity.
            // When intensity is 0, zoom multiplier is 1 (no zoom).
            // When intensity is 1, zoom multiplier is TargetZoomMultiplier.
            float currentZoomMultiplier = Mathf.LerpUnclamped(1f, _data.TargetZoomMultiplier, intensity);

            // Prevent division by zero or negative FOV/Size
            float multiplierToApply = Mathf.Max(0.001f, currentZoomMultiplier);

            // Zooming in (multiplier > 1) reduces the FOV/size.
            float result = _initialValue / multiplierToApply;

            ApplyRawValue(result);
        }

        private void ApplyRawValue(float value)
        {
            if (_camera == null) return;

            if (_camera.orthographic)
                _camera.orthographicSize = value;
            else
                _camera.fieldOfView = value;
        }
    }
}
