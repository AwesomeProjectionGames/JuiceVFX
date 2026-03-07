#nullable enable

using UnityEngine;

namespace JuiceVFX
{
    public abstract class BaseShakeEffectData : JuiceEffectData
    {
        [Tooltip("Maximum offset for position shake.")]
        public Vector3 PositionStrength = new Vector3(0.5f, 0.5f, 0.5f);

        [Tooltip("Frequency of the shake.")]
        public float Frequency = 10f;

        [Tooltip("Damping curve (1 at start, 0 at end usually).")]
        public AnimationCurve DampingCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Tooltip("Random seed for noise.")]
        public bool Randomize = true;
    }

    public abstract class BaseShakeEffectRunner<T> : JuiceEffectRunner where T : BaseShakeEffectData
    {
        protected T _baseData;
        protected float _seed;

        public BaseShakeEffectRunner(T data)
        {
            _baseData = data;
        }

        public override void OnStart(JuicePlayer player)
        {
            _seed = _baseData.Randomize ? Random.Range(0f, 100f) : 0f;
            OnSetup(player);
        }

        protected abstract void OnSetup(JuicePlayer player);

        public override void OnUpdate(float deltaTime)
        {
            if (!IsTargetValid()) return;

            _timer += deltaTime;
            float t = Mathf.Clamp01(_timer / _baseData.Duration);
            if (_baseData.Duration <= 0) t = 1f;

            float damping = _baseData.DampingCurve.Evaluate(t);

            // Perlin noise based shake
            float noiseX = (Mathf.PerlinNoise(_seed, Time.time * _baseData.Frequency) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(_seed + 1, Time.time * _baseData.Frequency) - 0.5f) * 2f;
            float noiseZ = (Mathf.PerlinNoise(_seed + 2, Time.time * _baseData.Frequency) - 0.5f) * 2f;

            Vector3 posOffset = new Vector3(
                noiseX * _baseData.PositionStrength.x,
                noiseY * _baseData.PositionStrength.y,
                noiseZ * _baseData.PositionStrength.z
            ) * damping * Context.Multiplier;

            ApplyShake(posOffset, damping);

            if (t >= 1f)
            {
                Stop();
            }
        }

        protected abstract bool IsTargetValid();
        protected abstract void ApplyShake(Vector3 posOffset, float damping);
    }
}
