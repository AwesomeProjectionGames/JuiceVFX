#nullable enable

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace JuiceVFX.Editor
{
    /// <summary>
    /// View for a single effect slot inside the <see cref="JuiceFeedbackEditor"/>.
    /// Renders a foldout header with reorder/action/delete buttons, and an expandable body
    /// containing the effect's serialized properties.
    /// </summary>
    public sealed class EffectItemView : VisualElement
    {
        private readonly EffectItemViewModel _itemVm;
        private readonly FeedbackViewModel _feedbackVm;
        private readonly JuiceFeedbackEditor _editor;

        private readonly VisualElement _header;
        private readonly Label _arrow;
        private readonly VisualElement _body;

        public EffectItemView(EffectItemViewModel itemVm, FeedbackViewModel feedbackVm, JuiceFeedbackEditor editor)
        {
            _itemVm = itemVm;
            _feedbackVm = feedbackVm;
            _editor = editor;

            AddToClassList("effect-item");

            // ═══════════════════════════════════════════════════════
            //  Header
            // ═══════════════════════════════════════════════════════

            _header = new VisualElement();
            _header.AddToClassList("effect-item__header");
            if (_itemVm.IsExpanded)
                _header.AddToClassList("effect-item__header--expanded");

            // Foldout arrow
            _arrow = new Label(_itemVm.IsExpanded ? "▼" : "▶");
            _arrow.AddToClassList("effect-item__foldout-arrow");
            _header.Add(_arrow);

            // Effect name
            var nameLabel = new Label(_itemVm.DisplayName);
            nameLabel.AddToClassList("effect-item__name");
            _header.Add(nameLabel);

            // Shared badge
            if (!_itemVm.IsSubAsset)
            {
                var badge = new Label("(Shared)");
                badge.AddToClassList("effect-item__badge");
                _header.Add(badge);
            }

            // ── Action buttons ──

            if (_itemVm.IsSubAsset)
            {
                var savePresetBtn = new Button(() => OnSavePreset()) { text = "Save Preset" };
                savePresetBtn.AddToClassList("effect-item__btn");
                savePresetBtn.AddToClassList("effect-item__btn--action");
                _header.Add(savePresetBtn);
            }
            else
            {
                var cloneBtn = new Button(() => OnCloneToLocal()) { text = "Clone to Local" };
                cloneBtn.AddToClassList("effect-item__btn");
                cloneBtn.AddToClassList("effect-item__btn--action");
                _header.Add(cloneBtn);
            }

            // Move Up
            var upBtn = new Button(() => OnMoveUp()) { text = "▲" };
            upBtn.AddToClassList("effect-item__btn");
            upBtn.SetEnabled(_itemVm.Index > 0);
            _header.Add(upBtn);

            // Move Down
            var downBtn = new Button(() => OnMoveDown()) { text = "▼" };
            downBtn.AddToClassList("effect-item__btn");
            downBtn.SetEnabled(_itemVm.Index < _feedbackVm.EffectCount - 1);
            _header.Add(downBtn);

            // Delete
            var deleteBtn = new Button(() => OnDelete()) { text = "✕" };
            deleteBtn.AddToClassList("effect-item__btn");
            deleteBtn.AddToClassList("effect-item__btn--delete");
            _header.Add(deleteBtn);

            // Click header to toggle expand
            _header.RegisterCallback<ClickEvent>(OnHeaderClicked);

            Add(_header);

            // ═══════════════════════════════════════════════════════
            //  Body (expandable)
            // ═══════════════════════════════════════════════════════

            _body = new VisualElement();
            _body.AddToClassList("effect-item__body");
            if (!_itemVm.IsExpanded)
                _body.AddToClassList("effect-item__body--hidden");

            // Preset reference field (for shared effects)
            if (!_itemVm.IsSubAsset)
            {
                var presetField = new ObjectField("Preset Reference")
                {
                    objectType = typeof(JuiceEffectData),
                    value = _itemVm.Effect,
                    allowSceneObjects = false
                };
                presetField.AddToClassList("effect-item__preset-field");
                presetField.RegisterValueChangedCallback(OnPresetChanged);
                _body.Add(presetField);
            }

            // Serialized properties
            BuildPropertyFields();

            Add(_body);
        }

        // ═══════════════════════════════════════════════════════
        //  Property Rendering
        // ═══════════════════════════════════════════════════════

        private void BuildPropertyFields()
        {
            var cachedEditor = _editor.GetOrCreateEditor(_itemVm.Effect);
            var so = cachedEditor.serializedObject;
            so.Update();

            var iterator = so.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.name == "m_Script") continue;

                var propField = new PropertyField(iterator.Copy());
                propField.Bind(so);
                propField.RegisterCallback<SerializedPropertyChangeEvent>(_ =>
                {
                    EditorUtility.SetDirty(_itemVm.Effect);
                });
                _body.Add(propField);
            }
        }

        // ═══════════════════════════════════════════════════════
        //  Event Handlers
        // ═══════════════════════════════════════════════════════

        private void OnHeaderClicked(ClickEvent evt)
        {
            // Don't toggle if click was on a button
            if (evt.target is Button) return;

            _feedbackVm.ToggleExpanded(_itemVm.Index);
            bool expanded = _itemVm.IsExpanded;

            _arrow.text = expanded ? "▼" : "▶";
            _body.EnableInClassList("effect-item__body--hidden", !expanded);
            _header.EnableInClassList("effect-item__header--expanded", expanded);

            evt.StopPropagation();
        }

        private void OnMoveUp()
        {
            _feedbackVm.MoveUp(_itemVm.Index);
        }

        private void OnMoveDown()
        {
            _feedbackVm.MoveDown(_itemVm.Index);
        }

        private void OnDelete()
        {
            _feedbackVm.RemoveEffect(_itemVm.Index);
        }

        private void OnSavePreset()
        {
            _feedbackVm.SaveAsPreset(_itemVm.Index);
        }

        private void OnCloneToLocal()
        {
            _feedbackVm.CloneToLocal(_itemVm.Index);
        }

        private void OnPresetChanged(ChangeEvent<Object> evt)
        {
            if (evt.newValue is JuiceEffectData newEffect && newEffect != _itemVm.Effect)
            {
                _feedbackVm.ReplacePreset(_itemVm.Index, newEffect);
            }
        }
    }
}
