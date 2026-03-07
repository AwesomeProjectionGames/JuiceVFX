#nullable enable

using UnityEngine;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewObjectBlinkEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/Object Blink")]
    public class ObjectBlinkEffectData : JuiceEffectData
    {
        [Tooltip("Number of times the object blinks (toggles active state) during the duration.")]
        [Min(1)]
        public int BlinkCount = 3;

        [Tooltip("The percentage of a single blink cycle during which the object is enabled. 0.5 means half enabled, half disabled.")]
        [Range(0f, 1f)]
        public float EnableRatio = 0.5f;

        public override JuiceEffectRunner CreateRunner()
        {
            return new ObjectBlinkEffectRunner(this);
        }
    }

    public class ObjectBlinkEffectRunner : JuiceEffectRunner
    {
        private ObjectBlinkEffectData _data;
        private Transform? _rootTransform;
        private bool _originalActiveState;
        private bool _isCurrentlyModified;

        public ObjectBlinkEffectRunner(ObjectBlinkEffectData data)
        {
            _data = data;
        }

        public override void OnStart(JuicePlayer player)
        {
            _rootTransform = Context.RootTransform;
            if (_rootTransform != null)
            {
                _originalActiveState = _rootTransform.gameObject.activeSelf;
            }
            _isCurrentlyModified = false;
        }

        public override void OnUpdate(float deltaTime)
        {
            if (_rootTransform == null || _data.BlinkCount <= 0 || _data.Duration <= 0)
            {
                Stop();
                return;
            }

            _timer += deltaTime;
            float t = Mathf.Clamp01(_timer / _data.Duration);

            float blinkDuration = _data.Duration / _data.BlinkCount;
            float timeInCurrentBlink = _timer % blinkDuration;
            
            bool shouldBeEnabled;
            if (_originalActiveState)
            {
                // If the object is originally enabled, start the cycle by hiding it (disabling).
                // The duration it stays disabled is (1 - EnableRatio).
                float disablePhaseDuration = blinkDuration * (1f - _data.EnableRatio);
                shouldBeEnabled = timeInCurrentBlink >= disablePhaseDuration;
            }
            else
            {
                // If the object is originally disabled, start the cycle by showing it (enabling).
                // The duration it stays enabled is EnableRatio.
                float enablePhaseDuration = blinkDuration * _data.EnableRatio;
                shouldBeEnabled = timeInCurrentBlink < enablePhaseDuration;
            }

            if (t < 1f && _rootTransform.gameObject.activeSelf != shouldBeEnabled)
            {
                _isCurrentlyModified = true;
                _rootTransform.gameObject.SetActive(shouldBeEnabled);
            }

            if (t >= 1f)
            {
                Stop();
            }
        }

        public override void OnStop()
        {
            if (_rootTransform != null && _isCurrentlyModified)
            {
                _rootTransform.gameObject.SetActive(_originalActiveState);
                _isCurrentlyModified = false;
            }
        }
    }
}
