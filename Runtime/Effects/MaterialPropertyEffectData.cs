#nullable enable

using UnityEngine;

namespace JuiceVFX
{
    public enum MaterialPropertyType
    {
        Color,
        Vector3,
        Vector2,
        Float,
        Bool
    }

    [CreateAssetMenu(fileName = "NewMaterialPropertyEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/Material Property")]
    public class MaterialPropertyEffectData : JuiceEffectData
    {
        [Header("Property Definition")]
        [Tooltip("Type of property to modify.")]
        public MaterialPropertyType PropertyType = MaterialPropertyType.Float;

        [Tooltip("Name of the property in the shader (e.g. _BaseColor, _Cutoff).")]
        public string PropertyName = "_BaseColor";

        [Tooltip("Intensity curve of the effect over time. Acts as a multiplier or threshold lerp parameter.")]
        public AnimationCurve IntensityCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Tooltip("If true, it will modify the material directly (cloning it). Default is PropertyBlock.")]
        public bool UseMaterialInstance = false;

        [Header("Target Values")]
        [Tooltip("Target color value when curve is at 1. Supports HDR.")]
        [ColorUsage(true, true)]
        public Color ColorValue = Color.white;
        
        [Tooltip("Target Vector3 value when curve is at 1.")]
        public Vector3 Vector3Value = Vector3.one;
        
        [Tooltip("Target Vector2 value when curve is at 1.")]
        public Vector2 Vector2Value = Vector2.one;
        
        [Tooltip("Target float value when curve is at 1.")]
        public float FloatValue = 1f;
        
        [Tooltip("Target bool value when curve crosses 0.5 threshold.")]
        public bool BoolValue = true;

        public override JuiceEffectRunner CreateRunner()
        {
            return new MaterialPropertyEffectRunner(this);
        }
    }

    public class MaterialPropertyEffectRunner : JuiceEffectRunner
    {
        private MaterialPropertyEffectData _data;
        private Renderer[]? _renderers;
        private MaterialPropertyBlock _propBlock;
        private int _propID;

        public MaterialPropertyEffectRunner(MaterialPropertyEffectData data)
        {
            _data = data;
            _propBlock = new MaterialPropertyBlock();
        }

        public override void OnStart(JuicePlayer player)
        {
            _renderers = Context.Renderers;
            _propID = Shader.PropertyToID(_data.PropertyName);
        }

        public override void OnUpdate(float deltaTime)
        {
            _timer += deltaTime;
            float t = Mathf.Clamp01(_timer / _data.Duration);
            if (_data.Duration <= 0) t = 1f;

            float intensity = _data.IntensityCurve.Evaluate(t);

            ApplyProperty(intensity);

            if (t >= 1f)
            {
                Stop();
            }
        }

        public override void OnStop()
        {
            float finalIntensity = _data.IntensityCurve.Evaluate(1f);
            ApplyProperty(finalIntensity);
        }

        private void ApplyProperty(float intensity)
        {
            if (_renderers == null) return;

            foreach (var renderer in _renderers)
            {
                if (renderer == null) continue;

                if (!_data.UseMaterialInstance)
                {
                    renderer.GetPropertyBlock(_propBlock);
                    SetPropertyValue(intensity, _propBlock, null);
                    renderer.SetPropertyBlock(_propBlock);
                }
                else
                {
                    SetPropertyValue(intensity, null, renderer.material);
                }
            }
        }

        private void SetPropertyValue(float intensity, MaterialPropertyBlock? block, Material? material)
        {
            switch (_data.PropertyType)
            {
                case MaterialPropertyType.Color:
                    Color colorResult = _data.ColorValue * intensity;
                    if (block != null) block.SetColor(_propID, colorResult);
                    if (material != null) material.SetColor(_propID, colorResult);
                    break;

                case MaterialPropertyType.Vector3:
                    Vector4 v3Result = _data.Vector3Value * intensity;
                    if (block != null) block.SetVector(_propID, v3Result);
                    if (material != null) material.SetVector(_propID, v3Result);
                    break;

                case MaterialPropertyType.Vector2:
                    Vector4 v2Result = _data.Vector2Value * intensity;
                    if (block != null) block.SetVector(_propID, v2Result);
                    if (material != null) material.SetVector(_propID, v2Result);
                    break;

                case MaterialPropertyType.Float:
                    float floatResult = _data.FloatValue * intensity;
                    if (block != null) block.SetFloat(_propID, floatResult);
                    if (material != null) material.SetFloat(_propID, floatResult);
                    break;

                case MaterialPropertyType.Bool:
                    // Treat a curve intensity > 0.5 as "Active", activating the target bool state
                    bool isActive = intensity >= 0.5f;
                    float boolResult = (isActive ? _data.BoolValue : !_data.BoolValue) ? 1f : 0f;
                    if (block != null) block.SetFloat(_propID, boolResult);
                    if (material != null) material.SetFloat(_propID, boolResult);
                    break;
            }
        }
    }
}
