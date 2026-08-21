#nullable enable

using UnityEditor;

namespace JuiceVFX.Editor
{
    /// <summary>
    /// View-model for a single effect slot inside a <see cref="JuiceFeedback"/> asset.
    /// Tracks expansion state and whether the effect is a local sub-asset or a shared preset.
    /// </summary>
    public sealed class EffectItemViewModel
    {
        public JuiceEffectData Effect { get; }
        public int Index { get; set; }
        public bool IsExpanded { get; set; } = true;
        public bool IsSubAsset { get; }
        public string DisplayName { get; }

        public EffectItemViewModel(JuiceEffectData effect, int index, string feedbackAssetPath)
        {
            Effect = effect;
            Index = index;
            DisplayName = ObjectNames.NicifyVariableName(effect.GetType().Name.Replace("EffectData", ""));
            IsSubAsset = AssetDatabase.GetAssetPath(effect) == feedbackAssetPath;
        }
    }
}
