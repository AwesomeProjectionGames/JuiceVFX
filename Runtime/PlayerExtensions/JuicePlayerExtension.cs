#nullable enable

using UnityEngine;

namespace JuiceVFX
{
    [RequireComponent(typeof(JuicePlayer))]
    public abstract class JuicePlayerExtension : MonoBehaviour
    {
        protected JuicePlayer _juicePlayer = null!;

        protected virtual void Awake()
        {
            _juicePlayer = GetComponent<JuicePlayer>();
        }
    }
}
