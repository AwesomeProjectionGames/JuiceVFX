#nullable enable

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JuiceVFX
{
    public class JuicePlayer : MonoBehaviour, IJuicePlayer
    {
        public Gamepad[] TargetGamepads;
        public Renderer[] targetRenderers = { };
        public Transform targetRoot;

        private List<JuiceEffectRunner> _activeRunners = new List<JuiceEffectRunner>();
        private List<JuiceEffectRunner> _runnersToRemove = new List<JuiceEffectRunner>();

        public void Play(JuiceFeedback feedback, Vector3? contactPoint = null, Quaternion? rotation = null)
        {
            if (feedback == null) return;

            var gamepads = (TargetGamepads != null && TargetGamepads.Length > 0) ? TargetGamepads : (Gamepad.current != null ? new[] { Gamepad.current } : System.Array.Empty<Gamepad>());
            var root = targetRoot != null ? targetRoot : transform;
            var context = new JuiceFeedbackContext(contactPoint, rotation, gamepads, targetRenderers, root);

            foreach (var effectData in feedback.Effects)
            {
                if (effectData == null) continue;

                var runner = effectData.CreateRunner();
                runner.Initialize(this, context);
                runner.Start(effectData.Delay);
                _activeRunners.Add(runner);
            }
        }


        private void Update()
        {
            if (_activeRunners.Count == 0) return;

            _runnersToRemove.Clear();

            foreach (var runner in _activeRunners)
            {
                runner.Update(Time.deltaTime);
                if (runner.IsFinished)
                {
                    _runnersToRemove.Add(runner);
                }
            }

            foreach (var runner in _runnersToRemove)
            {
                _activeRunners.Remove(runner);
            }
        }

        private void OnDisable()
        {
            StopAll();
        }

        public void StopAll()
        {
            foreach (var runner in _activeRunners)
            {
                runner.Stop();
            }
            _activeRunners.Clear();
        }
    }
}
