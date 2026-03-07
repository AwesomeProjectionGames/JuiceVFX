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

    [CreateAssetMenu(fileName = "NewMaterialPropertyEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/Material/Material Property")]
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

        [Header("Settings")]
        [Tooltip("If true, the effect value is added to the base value instead of replacing it.")]
        public bool IsRelative = false;

        [Tooltip("If true, the material property is strictly restored to its initial value when the effect stops.")]
        public bool ResetInitialValue = false;

        public override JuiceEffectRunner CreateRunner()
        {
            return new MaterialPropertyEffectRunner(this);
        }

        public override bool IsSameEffect(JuiceEffectData other)
        {
            // We consider it the "same effect" if it's trying to modify the same property on the same material.
            // For caching purposes, we cannot allow multiple instances of the same property to run simultaneously, otherwise they would conflict with each other (initial value to return to at the end for example).
            return other is MaterialPropertyEffectData otherData &&
                   PropertyType == otherData.PropertyType &&
                   PropertyName == otherData.PropertyName;
        }
    }

    public class MaterialPropertyEffectRunner : JuiceEffectRunner
    {
        private MaterialPropertyEffectData _data;
        private Renderer[]? _renderers;
        private MaterialPropertyBlock _propBlock;
        private int _propID;

        private Color[]? _initialColors;
        private Vector4[]? _initialVectors;
        private float[]? _initialFloats;

        public MaterialPropertyEffectRunner(MaterialPropertyEffectData data)
        {
            _data = data;
            _propBlock = new MaterialPropertyBlock();
        }

        public override void OnStart(JuicePlayer player)
        {
            _renderers = Context.Renderers;
            _propID = Shader.PropertyToID(_data.PropertyName);

            if (_renderers != null && (_data.IsRelative || _data.ResetInitialValue))
            {
                _initialColors = new Color[_renderers.Length];
                _initialVectors = new Vector4[_renderers.Length];
                _initialFloats = new float[_renderers.Length];

                for (int i = 0; i < _renderers.Length; i++)
                {
                    Renderer renderer = _renderers[i];
                    if (renderer == null) continue;

                    Material? mat = _data.UseMaterialInstance ? renderer.material : renderer.sharedMaterial;
                    if (mat == null) continue;

                    switch (_data.PropertyType)
                    {
                        case MaterialPropertyType.Color:
                            if (mat.HasProperty(_propID)) _initialColors[i] = mat.GetColor(_propID);
                            break;
                        case MaterialPropertyType.Vector3:
                        case MaterialPropertyType.Vector2:
                            if (mat.HasProperty(_propID)) _initialVectors[i] = mat.GetVector(_propID);
                            break;
                        case MaterialPropertyType.Float:
                        case MaterialPropertyType.Bool:
                            if (mat.HasProperty(_propID)) _initialFloats[i] = mat.GetFloat(_propID);
                            break;
                    }
                }
            }
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
            if (_data.ResetInitialValue && _renderers != null)
            {
                for (int i = 0; i < _renderers.Length; i++)
                {
                    var renderer = _renderers[i];
                    if (renderer == null) continue;

                    if (!_data.UseMaterialInstance)
                    {
                        renderer.GetPropertyBlock(_propBlock);
                        RestoreInitialValue(i, _propBlock, null);
                        renderer.SetPropertyBlock(_propBlock);
                    }
                    else
                    {
                        RestoreInitialValue(i, null, renderer.material);
                    }
                }
            }
            else
            {
                float finalIntensity = _data.IntensityCurve.Evaluate(1f);
                ApplyProperty(finalIntensity);
            }
        }

        private void ApplyProperty(float intensity)
        {
            if (_renderers == null) return;

            for (int i = 0; i < _renderers.Length; i++)
            {
                var renderer = _renderers[i];
                if (renderer == null) continue;

                if (!_data.UseMaterialInstance)
                {
                    renderer.GetPropertyBlock(_propBlock);
                    SetPropertyValue(i, intensity, _propBlock, null);
                    renderer.SetPropertyBlock(_propBlock);
                }
                else
                {
                    SetPropertyValue(i, intensity, null, renderer.material);
                }
            }
        }

        private void SetPropertyValue(int index, float intensity, MaterialPropertyBlock? block, Material? material)
        {
            switch (_data.PropertyType)
            {
                case MaterialPropertyType.Color:
                    Color colorResult = _data.ColorValue * intensity;
                    if (_data.IsRelative && _initialColors != null) colorResult += _initialColors[index];
                    if (block != null) block.SetColor(_propID, colorResult);
                    if (material != null) material.SetColor(_propID, colorResult);
                    break;

                case MaterialPropertyType.Vector3:
                    Vector4 v3Result = _data.Vector3Value * intensity;
                    if (_data.IsRelative && _initialVectors != null) v3Result += _initialVectors[index];
                    if (block != null) block.SetVector(_propID, v3Result);
                    if (material != null) material.SetVector(_propID, v3Result);
                    break;

                case MaterialPropertyType.Vector2:
                    Vector4 v2Result = _data.Vector2Value * intensity;
                    if (_data.IsRelative && _initialVectors != null) v2Result += _initialVectors[index];
                    if (block != null) block.SetVector(_propID, v2Result);
                    if (material != null) material.SetVector(_propID, v2Result);
                    break;

                case MaterialPropertyType.Float:
                    float floatResult = _data.FloatValue * intensity;
                    if (_data.IsRelative && _initialFloats != null) floatResult += _initialFloats[index];
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

        private void RestoreInitialValue(int index, MaterialPropertyBlock? block, Material? material)
        {
            switch (_data.PropertyType)
            {
                case MaterialPropertyType.Color:
                    if (_initialColors != null)
                    {
                        if (block != null) block.SetColor(_propID, _initialColors[index]);
                        if (material != null) material.SetColor(_propID, _initialColors[index]);
                    }
                    break;
                case MaterialPropertyType.Vector3:
                case MaterialPropertyType.Vector2:
                    if (_initialVectors != null)
                    {
                        if (block != null) block.SetVector(_propID, _initialVectors[index]);
                        if (material != null) material.SetVector(_propID, _initialVectors[index]);
                    }
                    break;
                case MaterialPropertyType.Float:
                case MaterialPropertyType.Bool:
                    if (_initialFloats != null)
                    {
                        if (block != null) block.SetFloat(_propID, _initialFloats[index]);
                        if (material != null) material.SetFloat(_propID, _initialFloats[index]);
                    }
                    break;
            }
        }
    }
}
