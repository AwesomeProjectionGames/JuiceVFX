using UnityEngine;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewFlashEffect", menuName = "JuiceVFX/Effects/Flash")]
    public class FlashEffectData : JuiceEffectData
    {
        [Tooltip("Color of the flash.")]
        public Color FlashColor = Color.white;
        
        [Tooltip("Intensity curve of the flash over time.")]
        public AnimationCurve IntensityCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Tooltip("Name of the color property in the shader (e.g. _EmissionColor, _BaseColor).")]
        public string ColorPropertyName = "_EmissionColor";

        [Tooltip("If true, it will modify the material directly (cloning it). Use carefully. Default is PropertyBlock.")]
        public bool UseMaterialInstance = false;

        public override JuiceEffectRunner CreateRunner()
        {
            return new FlashEffectRunner(this);
        }
    }

    public class FlashEffectRunner : JuiceEffectRunner
    {
        private FlashEffectData _data;
        private Renderer[] _renderers;
        private MaterialPropertyBlock _propBlock;
        private int _colorPropID;

        public FlashEffectRunner(FlashEffectData data)
        {
            _data = data;
        }

        public override void OnStart(JuicePlayer player)
        {
            _renderers = player.GetComponentsInChildren<Renderer>();
            _colorPropID = Shader.PropertyToID(_data.ColorPropertyName);
            _propBlock = new MaterialPropertyBlock();
        }

        public override void OnUpdate(float deltaTime)
        {
            _timer += deltaTime;
            float t = Mathf.Clamp01(_timer / _data.Duration);
            if (_data.Duration <= 0) t = 1f;

            float intensity = _data.IntensityCurve.Evaluate(t);
            Color finalColor = _data.FlashColor * intensity;

            foreach (var renderer in _renderers)
            {
                if (renderer == null) continue;

                if (_data.UseMaterialInstance)
                {
                     // Not recommended but supported
                     renderer.material.SetColor(_colorPropID, finalColor);
                }
                else
                {
                    renderer.GetPropertyBlock(_propBlock);
                    _propBlock.SetColor(_colorPropID, finalColor);
                    renderer.SetPropertyBlock(_propBlock);
                }
            }

            if (t >= 1f)
            {
                Stop();
            }
        }

        public override void OnStop()
        {
            // Reset to black/zero emission or handle cleanup? 
            // Property blocks persist. We should probably reset the color to black/transparent if it's emission.
            // But if it's BaseColor, we might override the texture.
            // Flash usually implies additive emission or overlay.
            // If it modifies BaseColor, we need to store original. 
            // For simplicity, let's assume Emission for now or that the curve goes to 0 (default color).
            
            // To be safe, if we used PropertyBlock, we should probably clear the property if possible, 
            // or set it to 0 interaction if we assume it was 0 before.
            // A better approach for Flash is usually setting Emission.
            
            float finalIntensity = _data.IntensityCurve.Evaluate(1f); 
            Color finalColor = _data.FlashColor * finalIntensity; // Should be black if curve ends at 0

             foreach (var renderer in _renderers)
            {
                if (renderer == null) continue;

                if (!_data.UseMaterialInstance)
                {
                    renderer.GetPropertyBlock(_propBlock);
                    _propBlock.SetColor(_colorPropID, finalColor);
                    renderer.SetPropertyBlock(_propBlock);
                }
                else
                {
                    renderer.material.SetColor(_colorPropID, finalColor);
                }
            }
        }
    }
}
