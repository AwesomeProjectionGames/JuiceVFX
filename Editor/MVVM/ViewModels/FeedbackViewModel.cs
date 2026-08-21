#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace JuiceVFX.Editor
{
    /// <summary>
    /// ViewModel for the <see cref="JuiceFeedback"/> custom inspector.
    /// Manages the effects list, CRUD operations, sub-asset lifecycle,
    /// and preset save/clone workflows.
    /// </summary>
    public sealed class FeedbackViewModel : IDisposable
    {
        private readonly JuiceFeedback _feedback;
        private readonly SerializedObject _serializedObject;

        // ═══════════════════════════════════════════════════════
        //  State
        // ═══════════════════════════════════════════════════════

        public List<EffectItemViewModel> EffectItems { get; } = new();
        public int EffectCount => _feedback.Effects.Count;
        public string AssetPath { get; }

        // ═══════════════════════════════════════════════════════
        //  Events
        // ═══════════════════════════════════════════════════════

        /// <summary>Raised when the effects list structure changes (add/remove/reorder).</summary>
        public event Action? EffectsChanged;

        // ═══════════════════════════════════════════════════════
        //  Lifecycle
        // ═══════════════════════════════════════════════════════

        public FeedbackViewModel(JuiceFeedback feedback, SerializedObject serializedObject)
        {
            _feedback = feedback;
            _serializedObject = serializedObject;
            AssetPath = AssetDatabase.GetAssetPath(_feedback);

            CleanUpNullEffects();
            RebuildEffectItems();
        }

        public void Dispose()
        {
            // Nothing to unsubscribe currently; placeholder for future needs.
        }

        // ═══════════════════════════════════════════════════════
        //  Commands
        // ═══════════════════════════════════════════════════════

        /// <summary>Adds a new effect of the specified type as a local sub-asset.</summary>
        public void AddEffect(Type effectType)
        {
            var effect = (JuiceEffectData)ScriptableObject.CreateInstance(effectType);
            effect.name = effectType.Name;
            AssetDatabase.AddObjectToAsset(effect, _feedback);
            _feedback.Effects.Add(effect);
            MarkDirtyAndSave();
            RebuildEffectItems();
            EffectsChanged?.Invoke();
        }

        /// <summary>Drops an existing preset reference into the list.</summary>
        public void DropPreset(JuiceEffectData preset)
        {
            if (preset == null) return;
            _feedback.Effects.Add(preset);
            MarkDirtyAndSave();
            RebuildEffectItems();
            EffectsChanged?.Invoke();
        }

        /// <summary>Removes an effect at the given index, cleaning up sub-assets if necessary.</summary>
        public void RemoveEffect(int index)
        {
            if (index < 0 || index >= _feedback.Effects.Count) return;
            var effect = _feedback.Effects[index];
            _feedback.Effects.RemoveAt(index);

            if (effect != null && AssetDatabase.GetAssetPath(effect) == AssetPath)
            {
                AssetDatabase.RemoveObjectFromAsset(effect);
                UnityEngine.Object.DestroyImmediate(effect, true);
            }

            MarkDirtyAndSave();
            RebuildEffectItems();
            EffectsChanged?.Invoke();
        }

        /// <summary>Swaps the effect at <paramref name="index"/> with the one above it.</summary>
        public void MoveUp(int index)
        {
            if (index <= 0 || index >= _feedback.Effects.Count) return;
            (_feedback.Effects[index], _feedback.Effects[index - 1]) =
                (_feedback.Effects[index - 1], _feedback.Effects[index]);
            MarkDirtyAndSave();
            RebuildEffectItems();
            EffectsChanged?.Invoke();
        }

        /// <summary>Swaps the effect at <paramref name="index"/> with the one below it.</summary>
        public void MoveDown(int index)
        {
            if (index < 0 || index >= _feedback.Effects.Count - 1) return;
            (_feedback.Effects[index], _feedback.Effects[index + 1]) =
                (_feedback.Effects[index + 1], _feedback.Effects[index]);
            MarkDirtyAndSave();
            RebuildEffectItems();
            EffectsChanged?.Invoke();
        }

        /// <summary>Clones a shared preset into a local sub-asset copy.</summary>
        public void CloneToLocal(int index)
        {
            if (index < 0 || index >= _feedback.Effects.Count) return;
            var original = _feedback.Effects[index];
            if (original == null) return;

            var clone = UnityEngine.Object.Instantiate(original);
            clone.name = original.name + " (Local)";
            AssetDatabase.AddObjectToAsset(clone, _feedback);
            _feedback.Effects[index] = clone;
            MarkDirtyAndSave();
            RebuildEffectItems();
            EffectsChanged?.Invoke();
        }

        /// <summary>Saves a local sub-asset as a standalone preset asset.</summary>
        public void SaveAsPreset(int index)
        {
            if (index < 0 || index >= _feedback.Effects.Count) return;
            var effect = _feedback.Effects[index];
            if (effect == null) return;

            string? defaultFolder = Path.GetDirectoryName(AssetPath);
            if (!string.IsNullOrEmpty(defaultFolder))
                defaultFolder = defaultFolder!.Replace("\\", "/");

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Effect as Preset",
                effect.GetType().Name + "_Preset",
                "asset",
                "Please enter a file name to save the effect preset.",
                defaultFolder ?? "Assets");

            if (string.IsNullOrEmpty(path)) return;

            var copy = UnityEngine.Object.Instantiate(effect);
            copy.name = Path.GetFileNameWithoutExtension(path);

            AssetDatabase.RemoveObjectFromAsset(effect);
            UnityEngine.Object.DestroyImmediate(effect, true);
            AssetDatabase.CreateAsset(copy, path);

            _feedback.Effects[index] = copy;
            MarkDirtyAndSave();
            RebuildEffectItems();
            EffectsChanged?.Invoke();
        }

        /// <summary>Replaces the preset reference at the given index.</summary>
        public void ReplacePreset(int index, JuiceEffectData newEffect)
        {
            if (index < 0 || index >= _feedback.Effects.Count) return;
            if (newEffect == null) return;
            _feedback.Effects[index] = newEffect;
            MarkDirtyAndSave();
            RebuildEffectItems();
            EffectsChanged?.Invoke();
        }

        /// <summary>Toggles the expanded state for the item at the given index.</summary>
        public void ToggleExpanded(int index)
        {
            if (index < 0 || index >= EffectItems.Count) return;
            EffectItems[index].IsExpanded = !EffectItems[index].IsExpanded;
        }

        // ═══════════════════════════════════════════════════════
        //  Private
        // ═══════════════════════════════════════════════════════

        private void RebuildEffectItems()
        {
            // Preserve expansion state by effect reference
            var prevExpanded = new Dictionary<JuiceEffectData, bool>();
            foreach (var item in EffectItems)
                prevExpanded[item.Effect] = item.IsExpanded;

            EffectItems.Clear();
            for (int i = 0; i < _feedback.Effects.Count; i++)
            {
                var effect = _feedback.Effects[i];
                if (effect == null) continue;

                var vm = new EffectItemViewModel(effect, i, AssetPath);
                if (prevExpanded.TryGetValue(effect, out bool expanded))
                    vm.IsExpanded = expanded;

                EffectItems.Add(vm);
            }
        }

        private void CleanUpNullEffects()
        {
            bool modified = false;
            for (int i = _feedback.Effects.Count - 1; i >= 0; i--)
            {
                if (_feedback.Effects[i] == null)
                {
                    _feedback.Effects.RemoveAt(i);
                    modified = true;
                }
            }

            if (modified)
                MarkDirtyAndSave();
        }

        private void MarkDirtyAndSave()
        {
            EditorUtility.SetDirty(_feedback);
            AssetDatabase.SaveAssets();
        }
    }
}
