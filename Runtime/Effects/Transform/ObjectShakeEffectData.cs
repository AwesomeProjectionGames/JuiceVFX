#nullable enable

using UnityEngine;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewObjectShake", menuName = "AwesomeProjection/JuiceVFX/Effects/Object Shake")]
    public class ObjectShakeEffectData : BaseShakeEffectData
    {
        [Tooltip("Maximum offset for rotation shake (degrees).")]
        public Vector3 RotationStrength = new Vector3(5f, 5f, 5f);

        public override JuiceEffectRunner CreateRunner()
        {
            return new ObjectShakeEffectRunner(this);
        }
    }

    public class ObjectShakeEffectRunner : BaseShakeEffectRunner<ObjectShakeEffectData>
    {
        private Transform? _target;
        private Vector3 _initialPos;
        private Quaternion _initialRot;

        public ObjectShakeEffectRunner(ObjectShakeEffectData data) : base(data)
        {
        }

        protected override void OnSetup(JuicePlayer player)
        {
            _target = player.transform;
            _initialPos = _target.localPosition;
            _initialRot = _target.localRotation;
        }

        protected override bool IsTargetValid() => _target != null;

        protected override void ApplyShake(Vector3 posOffset, float damping)
        {
            if (_target == null) return;

            float rotNoiseX = (Mathf.PerlinNoise(_seed + 3, Time.time * _baseData.Frequency) - 0.5f) * 2f;
            float rotNoiseY = (Mathf.PerlinNoise(_seed + 4, Time.time * _baseData.Frequency) - 0.5f) * 2f;
            float rotNoiseZ = (Mathf.PerlinNoise(_seed + 5, Time.time * _baseData.Frequency) - 0.5f) * 2f;

            Vector3 rotOffset = new Vector3(
                rotNoiseX * _baseData.RotationStrength.x,
                rotNoiseY * _baseData.RotationStrength.y,
                rotNoiseZ * _baseData.RotationStrength.z
            ) * damping;

            _target.localPosition = _initialPos + posOffset;
            _target.localRotation = _initialRot * Quaternion.Euler(rotOffset);
        }

        public override void OnStop()
        {
            if (_target != null)
            {
                _target.localPosition = _initialPos;
                _target.localRotation = _initialRot;
            }
        }
    }
}
