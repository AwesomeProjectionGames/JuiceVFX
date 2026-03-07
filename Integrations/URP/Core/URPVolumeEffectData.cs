using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using JuiceVFX;

#nullable enable

namespace JuiceVFX.Integrations.URP
{
    /// <summary>
    /// Base class for URP Volume effects.
    /// Manages a temporary Volume override for a specific VolumeComponent.
    /// </summary>
    /// <typeparam name="T">The type of VolumeComponent to override.</typeparam>
    public abstract class URPVolumeEffectData<T> : JuiceEffectData where T : VolumeComponent, IPostProcessComponent
    {
        [Header("URP Volume Settings")]
        [Tooltip("If true, the effect is added to the initial value. If false, it overrides it absolutely.")]
        public bool IsRelative = false;

        [Tooltip("Curve that drives the main intensity of the effect over time.")]
        public AnimationCurve IntensityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("Multiplier applied to the evaluated Curve value.")]
        public float Multiplier = 1f;

        public override JuiceEffectRunner CreateRunner()
        {
            return CreateRunnerInstance();
        }

        protected abstract URPVolumeEffectRunner<T> CreateRunnerInstance();
    }

    /// <summary>
    /// Base runner for URP Volume effects.
    /// Creates a temporary global volume to override properties without permanently altering the scene's base volume.
    /// </summary>
    public abstract class URPVolumeEffectRunner<T> : JuiceEffectRunner where T : VolumeComponent, IPostProcessComponent
    {
        protected URPVolumeEffectData<T> _data;
        private GameObject? _volumeGameObject;
        private Volume? _volume;
        protected T? _volumeComponent;

        // Base/initial state taken from the current active volume stack
        protected T? _initialComponentState;

        public URPVolumeEffectRunner(URPVolumeEffectData<T> data)
        {
            _data = data;
        }

        public override void OnStart(JuicePlayer player)
        {
            // 1. Capture initial state from the VolumeManager
            var stack = VolumeManager.instance.stack;
            _initialComponentState = stack.GetComponent<T>();

            // 2. Create a temporary volume GameObject
            _volumeGameObject = new GameObject($"JuiceURPVolume_{typeof(T).Name}_{_data.name}");
            _volumeGameObject.transform.SetParent(player.transform);

            _volume = _volumeGameObject.AddComponent<Volume>();
            _volume.isGlobal = true;
            _volume.priority = 100; // High priority to override existing
            _volume.weight = 1f;

            // 3. Create a profile and add our specific component to override
            _volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
            _volume.profile.name = $"JuiceTempProfile_{typeof(T).Name}";

            _volumeComponent = _volume.profile.Add<T>();

            // Intentionally don't override everything by default, let subclass decide what to override active state for
            InitializeComponentState();

            UpdateEffectValue(0f);
        }

        protected abstract void InitializeComponentState();

        public override void OnUpdate(float deltaTime)
        {
            _timer += deltaTime;
            if (_timer >= _data.Duration)
            {
                Stop();
                return;
            }

            float rawProgress = _timer / _data.Duration;
            UpdateEffectValue(rawProgress);
        }

        private void UpdateEffectValue(float rawProgress)
        {
            if (_volumeComponent == null) return;

            float intensity = _data.IntensityCurve.Evaluate(rawProgress) * _data.Multiplier * Context.Multiplier;
            ApplyEffect(intensity);
        }

        /// <summary>
        /// Applies the evaluated intensity to the VolumeComponent. 
        /// Implementations should handle IsRelative logic if applicable.
        /// </summary>
        protected abstract void ApplyEffect(float intensity);

        public override void OnStop()
        {
            if (_volumeGameObject != null)
            {
                if (_volume != null && _volume.profile != null)
                {
                    UnityEngine.Object.Destroy(_volume.profile);
                }
                UnityEngine.Object.Destroy(_volumeGameObject);
            }
        }
    }
}
