using UnityEngine;
using GameFramework;

namespace JuiceVFX
{
    public class JuiceTriggerHealth : MonoBehaviour
    {
        [Tooltip("The JuicePlayer to control.")]
        public JuicePlayer TargetPlayer;

        [Tooltip("Feedback to play when damage is taken.")]
        public JuiceFeedback DamageFeedback;

        [Tooltip("Feedback to play when health reaches zero.")]
        public JuiceFeedback DeathFeedback;

        [Tooltip("Feedback to play when healed.")]
        public JuiceFeedback HealFeedback;

        private IHealth _health;

        private void Awake()
        {
            _health = GetComponent<IHealth>();
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.OnCurrentHealthChanged += OnHealthChanged;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnCurrentHealthChanged -= OnHealthChanged;
            }
        }

        private void OnHealthChanged(IHealth healthComp, int delta)
        {
            if (TargetPlayer == null) return;

            if (delta < 0)
            {
                // Damage
                if (DamageFeedback != null)
                    TargetPlayer.Play(DamageFeedback);
            }
            else if (delta > 0)
            {
                // Heal
                if (HealFeedback != null)
                    TargetPlayer.Play(HealFeedback);
            }

            if (healthComp.CurrentHealth <= 0 && delta < 0)
            {
                // Death (only if damage caused it)
                if (DeathFeedback != null)
                    TargetPlayer.Play(DeathFeedback);
            }
        }
    }
}
