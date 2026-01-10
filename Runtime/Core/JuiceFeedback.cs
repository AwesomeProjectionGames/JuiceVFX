using System.Collections.Generic;
using UnityEngine;

namespace JuiceVFX
{
    /// <summary>
    /// A collection of Juice Effects to be played together.
    /// </summary>
    [CreateAssetMenu(fileName = "NewJuiceFeedback", menuName = "JuiceVFX/Feedback")]
    public class JuiceFeedback : ScriptableObject
    {
        [Tooltip("List of effects to play.")]
        public List<JuiceEffectData> Effects = new List<JuiceEffectData>();
    }
}
