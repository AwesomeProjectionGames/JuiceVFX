using System.Collections.Generic;
using GameFramework;
using UnityEngine;

namespace JuiceVFX
{
    /// <summary>
    /// Component responsible for playing Juice Feedbacks.
    /// </summary>
    public class JuicePlayer : MonoBehaviour, ITickable
    {
        private List<JuiceEffectRunner> _activeRunners = new List<JuiceEffectRunner>();
        private List<JuiceEffectRunner> _runnersToRemove = new List<JuiceEffectRunner>();

        /// <summary>
        /// Plays the specified feedback.
        /// </summary>
        /// <param name="feedback">The feedback configuration to play.</param>
        public void Play(JuiceFeedback feedback)
        {
            if (feedback == null) return;

            foreach (var effectData in feedback.Effects)
            {
                if (effectData == null) continue;

                var runner = effectData.CreateRunner();
                runner.Initialize(this);
                runner.Start(effectData.Delay);
                _activeRunners.Add(runner);
            }
        }

        public void Tick(float deltaTime)
        {
            if (_activeRunners.Count == 0) return;

            _runnersToRemove.Clear();

            foreach (var runner in _activeRunners)
            {
                runner.Update(deltaTime);
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

        // Ideally, ITickable is called by a central manager. 
        // If not, we can use Update as a fallback, but we should ensure we don't double tick if a system is calling Tick.
        // For simplicity in a standalone context, we can call Tick in Update if not managed externally.
        // However, GameFramework usually implies a manager via IGameInstance or similar.
        // Since we don't know the exact setup, we will use Update but respect the ITickable contract.
        
        private void Update()
        {
            // If the GameFramework handles ITickable, this might be redundant or handled elsewhere.
            // Assuming standard MonoBehaviour usage for now.
            Tick(Time.deltaTime);
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
