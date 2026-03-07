#nullable enable

using UnityEngine;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewCameraShake", menuName = "AwesomeProjection/JuiceVFX/Effects/Camera/Camera Shake")]
    public class CameraShakeEffectData : BaseShakeEffectData
    {
        public override JuiceEffectTarget Target => JuiceEffectTarget.Camera;

        public override JuiceEffectRunner CreateRunner()
        {
            return new CameraShakeEffectRunner(this);
        }
    }

    public class CameraShakeEffectRunner : BaseShakeEffectRunner<CameraShakeEffectData>
    {
        private Camera? _camera;
        private Vector3 _initialPos;

        public CameraShakeEffectRunner(CameraShakeEffectData data) : base(data)
        {
        }

        protected override void OnSetup(JuicePlayer player)
        {
            _camera = Context.RootTransform != null ? Context.RootTransform.GetComponentInChildren<Camera>() : null;
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_camera != null)
            {
                _initialPos = _camera.transform.localPosition;
            }
        }

        protected override bool IsTargetValid() => _camera != null;

        protected override void ApplyShake(Vector3 posOffset, float damping)
        {
            if (_camera != null)
            {
                _camera.transform.localPosition = _initialPos + posOffset;
            }
        }

        public override void OnStop()
        {
            if (_camera != null)
            {
                _camera.transform.localPosition = _initialPos;
            }
        }
    }
}
