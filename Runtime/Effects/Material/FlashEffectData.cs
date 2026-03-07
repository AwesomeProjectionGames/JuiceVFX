#nullable enable

using UnityEngine;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewFlashEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/Material/Flash")]
    public class FlashEffectData : JuiceEffectData
    {
        [Tooltip("The material to apply during the flash.")]
        public Material? FlashMaterial;

        [Tooltip("Number of times it blinks (swaps to the flash material) during the effect duration.")]
        [Min(1)]
        public int BlinkCount = 3;

        public override JuiceEffectRunner CreateRunner()
        {
            return new FlashEffectRunner(this);
        }
    }

    public class FlashEffectRunner : JuiceEffectRunner
    {
        private FlashEffectData _data;
        private Renderer[]? _renderers;
        private Material[][]? _originalMaterials;
        private bool _isCurrentlyFlashing;

        public FlashEffectRunner(FlashEffectData data)
        {
            _data = data;
        }

        public override void OnStart(JuicePlayer player)
        {
            _renderers = Context.Renderers;

            if (_renderers != null)
            {
                _originalMaterials = new Material[_renderers.Length][];
                for (int i = 0; i < _renderers.Length; i++)
                {
                    if (_renderers[i] != null)
                    {
                        _originalMaterials[i] = _renderers[i].sharedMaterials;
                    }
                }
            }

            _isCurrentlyFlashing = false;
        }

        public override void OnUpdate(float deltaTime)
        {
            // If data is invalid, stop immediately
            if (_renderers == null || _data.FlashMaterial == null || _data.BlinkCount <= 0 || _data.Duration <= 0)
            {
                Stop();
                return;
            }

            _timer += deltaTime;
            float t = Mathf.Clamp01(_timer / _data.Duration);

            float blinkDuration = _data.Duration / _data.BlinkCount;
            float phaseDuration = blinkDuration / 2f;

            int currentPhase = Mathf.FloorToInt(_timer / phaseDuration);

            // Even phases (0, 2, 4...) are ON, odd phases (1, 3, 5...) are OFF
            bool shouldFlash = (currentPhase % 2) == 0 && t < 1f;

            if (shouldFlash != _isCurrentlyFlashing)
            {
                _isCurrentlyFlashing = shouldFlash;
                ApplyMaterials(shouldFlash);
            }

            if (t >= 1f)
            {
                Stop();
            }
        }

        public override void OnStop()
        {
            if (_isCurrentlyFlashing)
            {
                ApplyMaterials(false);
            }
        }

        private void ApplyMaterials(bool flash)
        {
            if (_renderers == null || _originalMaterials == null || _data.FlashMaterial == null) return;

            for (int i = 0; i < _renderers.Length; i++)
            {
                var renderer = _renderers[i];
                if (renderer == null) continue;

                if (flash)
                {
                    var materials = renderer.sharedMaterials;
                    var flashMaterials = new Material[materials.Length];
                    for (int j = 0; j < materials.Length; j++)
                    {
                        flashMaterials[j] = _data.FlashMaterial;
                    }
                    renderer.sharedMaterials = flashMaterials;
                }
                else
                {
                    if (_originalMaterials[i] != null)
                    {
                        renderer.sharedMaterials = _originalMaterials[i];
                    }
                }
            }
        }
    }
}
