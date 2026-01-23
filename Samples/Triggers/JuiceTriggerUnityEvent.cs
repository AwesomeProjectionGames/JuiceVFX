using UnityEngine;

namespace JuiceVFX
{
    public class JuiceTriggerUnityEvent : MonoBehaviour
    {
        [Tooltip("The JuicePlayer to control.")]
        public JuicePlayer targetPlayer;

        [Tooltip("The feedback to play when triggered.")]
        public JuiceFeedback feedback;

        public void Trigger()
        {
            if (targetPlayer != null && feedback != null)
            {
                targetPlayer.Play(feedback);
            }
        }
    }
}
