#nullable enable

using UnityEngine;

namespace JuiceVFX
{
    public enum CameraPropertyType
    {
        Zoom,
        FieldOfView,
        OrthographicSize,
        NearClipPlane,
        FarClipPlane
    }

    [CreateAssetMenu(fileName = "NewCameraPropertyEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/Camera/Camera Property")]
    public class CameraPropertyEffectData : JuiceEffectData
    {
        public override JuiceEffectTarget Target => JuiceEffectTarget.Camera;

        [Header("Camera Property Definition")]
        [Tooltip("Type of property to modify on the camera.")]
        public CameraPropertyType PropertyType = CameraPropertyType.Zoom;

        [Header("Effect Settings")]
        [Tooltip("Target float value when curve is at 1.")]
        public float TargetValue = 60f;

        [Tooltip("Intensity curve of the effect over time. Acts as a multiplier for the Target Value.")]
        public AnimationCurve IntensityCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        public virtual float EvaluateIntensityCurve(float time) => IntensityCurve.Evaluate(time);

        [Tooltip("If true, the effect value is added to the base value instead of replacing it.")]
        public bool IsRelative = false;

        [Tooltip("If true, the camera property is strictly restored to its initial value when the effect stops.")]
        public bool ResetInitialValue = true;

        [Tooltip("If true, the effect never completes automatically and holds its value until explicitly stopped.")]
        public bool HoldUntilStopped = false;

        public override JuiceEffectRunner CreateRunner()
        {
            return new CameraPropertyEffectRunner(this);
        }

        public override bool IsSameEffect(JuiceEffectData other)
        {
            return other is CameraPropertyEffectData otherData &&
                   PropertyType == otherData.PropertyType;
        }
    }

    public class CameraPropertyEffectRunner : JuiceEffectRunner
    {
        private CameraPropertyEffectData _data;
        private Camera? _camera;
        private float _initialValue;

        public CameraPropertyEffectRunner(CameraPropertyEffectData data)
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
                switch (_data.PropertyType)
                {
                    case CameraPropertyType.Zoom:
                        _initialValue = _camera.orthographic ? _camera.orthographicSize : _camera.fieldOfView;
                        break;
                    case CameraPropertyType.FieldOfView:
                        _initialValue = _camera.fieldOfView;
                        break;
                    case CameraPropertyType.OrthographicSize:
                        _initialValue = _camera.orthographicSize;
                        break;
                    case CameraPropertyType.NearClipPlane:
                        _initialValue = _camera.nearClipPlane;
                        break;
                    case CameraPropertyType.FarClipPlane:
                        _initialValue = _camera.farClipPlane;
                        break;
                }
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
            float result = _data.TargetValue * intensity;
            if (_data.IsRelative)
            {
                result += _initialValue;
            }

            ApplyRawValue(result);
        }

        private void ApplyRawValue(float value)
        {
            if (_camera == null) return;

            switch (_data.PropertyType)
            {
                case CameraPropertyType.Zoom:
                    if (_camera.orthographic)
                        _camera.orthographicSize = value;
                    else
                        _camera.fieldOfView = value;
                    break;
                case CameraPropertyType.FieldOfView:
                    _camera.fieldOfView = value;
                    break;
                case CameraPropertyType.OrthographicSize:
                    _camera.orthographicSize = value;
                    break;
                case CameraPropertyType.NearClipPlane:
                    _camera.nearClipPlane = value;
                    break;
                case CameraPropertyType.FarClipPlane:
                    _camera.farClipPlane = value;
                    break;
            }
        }
    }
}
