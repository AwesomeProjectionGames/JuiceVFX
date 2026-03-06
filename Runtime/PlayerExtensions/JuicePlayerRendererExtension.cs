#nullable enable

using UnityEngine;

namespace JuiceVFX
{
    public class JuicePlayerRendererExtension : JuicePlayerExtension
    {
        [Tooltip("If true, searches for Renderers in all children of this GameObject. If false, only looks for a Renderer on this GameObject.")]
        public bool searchInChildren = false;

        private void OnEnable()
        {
            UpdateTargetRenderers();
        }

        public void UpdateTargetRenderers()
        {
            if (_juicePlayer == null) return;

            if (searchInChildren)
            {
                _juicePlayer.targetRenderers = GetComponentsInChildren<Renderer>();
            }
            else
            {
                var singleRenderer = GetComponent<Renderer>();
                _juicePlayer.targetRenderers = singleRenderer != null ? new[] { singleRenderer } : System.Array.Empty<Renderer>();
            }
        }
    }
}
