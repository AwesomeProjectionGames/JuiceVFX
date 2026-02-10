using UnityEngine;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewObjectShake", menuName = "AwesomeProjection/JuiceVFX/Effects/Object Shake")]
    public class ObjectShakeEffectData : JuiceEffectData
    {
        [Tooltip("Maximum offset for position shake.")]
        public Vector3 PositionStrength = new Vector3(0.5f, 0.5f, 0.5f);
        
        [Tooltip("Maximum offset for rotation shake (degrees).")]
        public Vector3 RotationStrength = new Vector3(5f, 5f, 5f);

        [Tooltip("Frequency of the shake.")]
        public float Frequency = 10f;

        [Tooltip("Damping curve (1 at start, 0 at end usually).")]
        public AnimationCurve DampingCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Tooltip("Random seed for noise.")]
        public bool Randomize = true;

        public override JuiceEffectRunner CreateRunner()
        {
            return new ObjectShakeEffectRunner(this);
        }
    }

    public class ObjectShakeEffectRunner : JuiceEffectRunner
    {
        private ObjectShakeEffectData _data;
        private Transform _target;
        private Vector3 _initialPos;
        private Quaternion _initialRot;
        private float _seed;

        public ObjectShakeEffectRunner(ObjectShakeEffectData data)
        {
            _data = data;
        }

        public override void OnStart(JuicePlayer player)
        {
            _target = GetTargetTransform(_data.TargetType);
            
            if (_target == null)
            {
                // Fallback to player if specific target not found, or just do nothing?
                // The base helper GetTargetTransform falls back to null if not found (except UseBinding/Target which tries context).
                // Let's fallback to player if null, or just stop. 
                // If I configured "Target" and there is no target, I probably don't want to shake the player (self).
                // But GetTargetTransform returns null if not found.
                // Let's try one fallback: if everything fails, use player.
                _target = player.transform;
            }

            if (_target != null)
            {
                _initialPos = _target.localPosition;
                _initialRot = _target.localRotation;
            }
            
            _seed = _data.Randomize ? Random.Range(0f, 100f) : 0f;
        }

        public override void OnUpdate(float deltaTime)
        {
            _timer += deltaTime;
            float t = Mathf.Clamp01(_timer / _data.Duration);
            if (_data.Duration <= 0) t = 1f;

            float damping = _data.DampingCurve.Evaluate(t);
            
            // Perlin noise based shake
            float noiseX = (Mathf.PerlinNoise(_seed, Time.time * _data.Frequency) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(_seed + 1, Time.time * _data.Frequency) - 0.5f) * 2f;
            float noiseZ = (Mathf.PerlinNoise(_seed + 2, Time.time * _data.Frequency) - 0.5f) * 2f;

            float rotNoiseX = (Mathf.PerlinNoise(_seed + 3, Time.time * _data.Frequency) - 0.5f) * 2f;
            float rotNoiseY = (Mathf.PerlinNoise(_seed + 4, Time.time * _data.Frequency) - 0.5f) * 2f;
            float rotNoiseZ = (Mathf.PerlinNoise(_seed + 5, Time.time * _data.Frequency) - 0.5f) * 2f;

            Vector3 posOffset = new Vector3(
                noiseX * _data.PositionStrength.x,
                noiseY * _data.PositionStrength.y,
                noiseZ * _data.PositionStrength.z
            ) * damping;

            Vector3 rotOffset = new Vector3(
                rotNoiseX * _data.RotationStrength.x,
                rotNoiseY * _data.RotationStrength.y,
                rotNoiseZ * _data.RotationStrength.z
            ) * damping;

            _target.localPosition = _initialPos + posOffset;
            _target.localRotation = _initialRot * Quaternion.Euler(rotOffset);

            if (t >= 1f)
            {
                Stop();
            }
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
