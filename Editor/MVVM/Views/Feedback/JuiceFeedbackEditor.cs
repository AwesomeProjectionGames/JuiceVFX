#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace JuiceVFX.Editor
{
    /// <summary>
    /// UIToolkit-based custom inspector for <see cref="JuiceFeedback"/> ScriptableObjects.
    /// Uses the <see cref="FeedbackViewModel"/> to manage effects list CRUD operations.
    /// </summary>
    [CustomEditor(typeof(JuiceFeedback))]
    public sealed class JuiceFeedbackEditor : UnityEditor.Editor
    {
        private FeedbackViewModel? _vm;
        private VisualElement? _root;
        private VisualElement? _effectsContainer;
        private Label? _countLabel;

        // Cached editors per effect for embedded property rendering
        private readonly Dictionary<JuiceEffectData, UnityEditor.Editor> _editors = new();

        private void OnDisable()
        {
            _vm?.Dispose();
            _vm = null;

            foreach (var editor in _editors.Values)
            {
                if (editor != null) DestroyImmediate(editor);
            }
            _editors.Clear();
        }

        public override VisualElement CreateInspectorGUI()
        {
            _vm = new FeedbackViewModel((JuiceFeedback)target, serializedObject);

            _root = new VisualElement();
            _root.AddToClassList("feedback-editor");

            // ── Load stylesheet ──
            var styleSheet = LoadStyleSheet();
            if (styleSheet != null)
                _root.styleSheets.Add(styleSheet);

            // ── Header ──
            var header = new VisualElement();
            header.AddToClassList("feedback-editor__header");

            var title = new Label("Juice Effects");
            title.AddToClassList("feedback-editor__title");
            header.Add(title);

            _countLabel = new Label();
            _countLabel.AddToClassList("feedback-editor__count");
            header.Add(_countLabel);

            _root.Add(header);

            // ── Effects container ──
            _effectsContainer = new VisualElement();
            _root.Add(_effectsContainer);

            // ── Add button ──
            var addBtn = new Button(OnAddEffectClicked) { text = "Add Juice Effect..." };
            addBtn.AddToClassList("feedback-editor__add-btn");
            _root.Add(addBtn);

            // ── Drop zone ──
            var dropZone = new VisualElement();
            dropZone.AddToClassList("feedback-editor__drop-zone");

            var dropLabel = new Label("Or drop an existing preset:");
            dropLabel.AddToClassList("feedback-editor__drop-label");
            dropZone.Add(dropLabel);

            var dropField = new UnityEditor.UIElements.ObjectField
            {
                objectType = typeof(JuiceEffectData),
                allowSceneObjects = false
            };
            dropField.style.width = 200;
            dropField.RegisterValueChangedCallback(OnPresetDropped);
            dropZone.Add(dropField);

            _root.Add(dropZone);

            // ── Bind ──
            _vm.EffectsChanged += RebuildEffectsUI;
            RebuildEffectsUI();

            return _root;
        }

        // ═══════════════════════════════════════════════════════
        //  UI Rebuild
        // ═══════════════════════════════════════════════════════

        private void RebuildEffectsUI()
        {
            if (_effectsContainer == null || _vm == null) return;

            _effectsContainer.Clear();
            _countLabel!.text = $"{_vm.EffectCount} Effect(s)";

            for (int i = 0; i < _vm.EffectItems.Count; i++)
            {
                var itemVm = _vm.EffectItems[i];
                var effectView = new EffectItemView(itemVm, _vm, this);
                _effectsContainer.Add(effectView);
            }
        }

        // ═══════════════════════════════════════════════════════
        //  Add / Drop
        // ═══════════════════════════════════════════════════════

        private void OnAddEffectClicked()
        {
            if (_root == null || _vm == null) return;

            // Use the existing AdvancedDropdown (IMGUI-based popup — best UX for hierarchical type selection)
            var dropdown = new EffectTypeDropdown(new AdvancedDropdownState());
            dropdown.OnItemSelected += type => _vm.AddEffect(type);

            // Show at the bottom of the add button
            var addBtn = _root.Q<Button>(className: "feedback-editor__add-btn");
            if (addBtn != null)
            {
                var worldBound = addBtn.worldBound;
                dropdown.Show(new Rect(worldBound.x, worldBound.y, worldBound.width, worldBound.height));
            }
        }

        private void OnPresetDropped(ChangeEvent<UnityEngine.Object> evt)
        {
            if (evt.newValue is JuiceEffectData preset && _vm != null)
            {
                _vm.DropPreset(preset);
            }
        }

        // ═══════════════════════════════════════════════════════
        //  Cached Editor Access (for embedded property rendering)
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Returns a cached <see cref="UnityEditor.Editor"/> for the given effect data,
        /// creating one if necessary.
        /// </summary>
        internal UnityEditor.Editor GetOrCreateEditor(JuiceEffectData effect)
        {
            if (!_editors.TryGetValue(effect, out var editor) || editor == null)
            {
                editor = CreateEditor(effect);
                _editors[effect] = editor;
            }
            return editor;
        }

        // ═══════════════════════════════════════════════════════
        //  Stylesheet Loading
        // ═══════════════════════════════════════════════════════

        private static StyleSheet? LoadStyleSheet()
        {
            string[] guids = AssetDatabase.FindAssets("JuiceVFXEditor t:StyleSheet");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("JuiceVFXEditor.uss"))
                    return AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
            }
            return null;
        }
    }
}
