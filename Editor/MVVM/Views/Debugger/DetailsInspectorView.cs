#nullable enable

using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace JuiceVFX.Editor
{
    /// <summary>
    /// Right-pane view: detail inspector for the currently selected debug entry.
    /// Renders multiple "cards" showing header, live runner, player/invoker,
    /// gamepads, transform, renderers, and effect parameters.
    /// </summary>
    public sealed class DetailsInspectorView : VisualElement
    {
        private readonly DebuggerViewModel _vm;

        private readonly VisualElement _emptyState;
        private readonly ScrollView _scrollView;

        // Cached editor for effect data inspector
        private UnityEditor.Editor? _cachedEffectEditor;
        private JuiceEffectData? _cachedEffectEditorTarget;

        public DetailsInspectorView(DebuggerViewModel vm)
        {
            _vm = vm;
            AddToClassList("details-pane");

            // ── Empty state ──
            _emptyState = new VisualElement();
            _emptyState.AddToClassList("details-pane__empty");

            var emptyTitle = new Label("👈 Select an Event from Timeline");
            emptyTitle.AddToClassList("details-pane__empty-title");
            _emptyState.Add(emptyTitle);

            var emptySub = new Label("Select any recorded Juice playback event on the left\nto inspect its full context.");
            emptySub.AddToClassList("details-pane__empty-subtitle");
            _emptyState.Add(emptySub);

            Add(_emptyState);

            // ── Scroll view for details ──
            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            _scrollView.AddToClassList("details-pane__scroll");
            _scrollView.style.display = DisplayStyle.None;
            Add(_scrollView);

            // ── Bind ──
            _vm.SelectionChanged += OnSelectionChanged;
        }

        ~DetailsInspectorView()
        {
            CleanupCachedEditor();
        }

        // ═══════════════════════════════════════════════════════
        //  Selection Changed → Rebuild Content
        // ═══════════════════════════════════════════════════════

        private void OnSelectionChanged(JuiceDebugEntry? entry)
        {
            CleanupCachedEditor();
            _scrollView.Clear();

            if (entry == null)
            {
                _emptyState.style.display = DisplayStyle.Flex;
                _scrollView.style.display = DisplayStyle.None;
                return;
            }

            _emptyState.style.display = DisplayStyle.None;
            _scrollView.style.display = DisplayStyle.Flex;

            BuildHeaderCard(entry);
            BuildLiveRunnerCard(entry);
            BuildPlayerInvokerCard(entry);
            BuildGamepadsCard(entry);
            BuildTransformCard(entry);
            BuildRenderersCard(entry);
            BuildEffectParametersCard(entry);
        }

        // ═══════════════════════════════════════════════════════
        //  Card Builders
        // ═══════════════════════════════════════════════════════

        private void BuildHeaderCard(JuiceDebugEntry entry)
        {
            var card = CreateCard();

            // Top row: pill + event info + copy button
            var topRow = CreateRow();
            var pill = new CategoryPill(entry.Category);
            topRow.Add(pill);
            AddSpacer(topRow, 5);

            var infoLabel = new Label($"Event #{entry.Id} at {TimeSpan.FromSeconds(entry.TimeStamp):mm\\:ss\\.ff} (Frame {entry.FrameCount})");
            infoLabel.AddToClassList("detail-card__value");
            topRow.Add(infoLabel);

            AddFlexSpacer(topRow);

            var copyBtn = new Button(() => _vm.CopyEntryMarkdown(entry)) { text = "📋 Copy" };
            copyBtn.AddToClassList("detail-card__action-btn");
            topRow.Add(copyBtn);

            card.Add(topRow);

            // Effect name (large)
            string effectName = ObjectNames.NicifyVariableName(entry.EffectName.Replace("EffectData", ""));
            var nameLabel = new Label(effectName);
            nameLabel.style.fontSize = 16;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.color = new Color(0.83f, 0.83f, 0.83f);
            nameLabel.style.marginTop = 6;
            nameLabel.style.marginBottom = 6;
            card.Add(nameLabel);

            // Action buttons
            var actions = new VisualElement();
            actions.AddToClassList("detail-card__actions");

            if (entry.EffectData != null)
            {
                var pingEffect = new Button(() => _vm.PingEffectAsset(entry)) { text = "🔍 Ping Effect Asset" };
                pingEffect.AddToClassList("detail-card__action-btn");
                actions.Add(pingEffect);
            }

            if (entry.Player != null)
            {
                var pingPlayer = new Button(() => _vm.PingPlayer(entry)) { text = "🎯 Ping Player GO" };
                pingPlayer.AddToClassList("detail-card__action-btn");
                actions.Add(pingPlayer);
            }

            if (Application.isPlaying && entry.Player != null && entry.EffectData != null)
            {
                var replayBtn = new Button(() => _vm.ReplayEntry(entry)) { text = "⚡ Replay" };
                replayBtn.AddToClassList("detail-card__action-btn");
                replayBtn.AddToClassList("detail-card__action-btn--replay");
                actions.Add(replayBtn);
            }

            card.Add(actions);
            _scrollView.Add(card);
        }

        private void BuildLiveRunnerCard(JuiceDebugEntry entry)
        {
            var card = CreateCard();
            var header = CreateCardHeader("⚡ Live Runner State");

            var runner = entry.GetRunner();
            bool isActive = runner != null && runner.IsPlaying && !runner.IsFinished;

            var chip = new StatusChip(
                isActive ? "● ACTIVE" : "FINISHED / IDLE",
                isActive ? "status-chip--active" : "status-chip--inactive");
            header.Add(chip);

            card.Add(header);

            if (isActive && runner != null)
            {
                // Progress bar
                var progressContainer = new VisualElement();
                progressContainer.AddToClassList("live-progress");

                var fill = new VisualElement();
                fill.AddToClassList("live-progress__fill");
                float pct = Mathf.Clamp01(runner.Progress);
                fill.style.width = new Length(pct * 100f, LengthUnit.Percent);
                progressContainer.Add(fill);

                var progressLabel = new Label($"{runner.ElapsedTime:0.00}s / {runner.Duration:0.00}s ({pct * 100f:0}%)");
                progressLabel.AddToClassList("live-progress__label");
                progressContainer.Add(progressLabel);

                card.Add(progressContainer);

                if (runner.DelayRemaining > 0f)
                    card.Add(CreateLabeledRow("Delay Remaining:", $"{runner.DelayRemaining:0.00}s"));

                var stopBtn = new Button(() => _vm.StopRunner(entry)) { text = "Stop Runner" };
                stopBtn.AddToClassList("detail-card__action-btn");
                stopBtn.AddToClassList("detail-card__action-btn--danger");
                stopBtn.style.alignSelf = Align.FlexEnd;
                stopBtn.style.marginTop = 4;
                card.Add(stopBtn);
            }
            else
            {
                var info = new Label("Runner lifecycle completed.");
                info.AddToClassList("detail-card__subtitle");
                card.Add(info);
            }

            _scrollView.Add(card);
        }

        private void BuildPlayerInvokerCard(JuiceDebugEntry entry)
        {
            var card = CreateCard();
            card.Add(CreateCardHeader("🕹️ Target Player & Invoker"));

            card.Add(CreateLabeledRow("Invoked By:",
                string.IsNullOrEmpty(entry.InvokerFullInfo) ? "Direct call" : entry.InvokerFullInfo, true));

            if (entry.Player != null)
            {
                var playerRow = CreateRow();
                var playerLabel = new Label("Player Component:");
                playerLabel.AddToClassList("detail-card__label");
                playerRow.Add(playerLabel);

                var objField = new UnityEditor.UIElements.ObjectField { objectType = typeof(AbstractJuicePlayer), value = entry.Player };
                objField.SetEnabled(false);
                objField.style.flexGrow = 1;
                playerRow.Add(objField);
                card.Add(playerRow);
            }
            else
            {
                card.Add(CreateLabeledRow("Player:", $"{entry.PlayerName} ({entry.PlayerTypeName}) [Destroyed/Inactive]"));
            }

            if (!string.IsNullOrEmpty(entry.HierarchyPath))
                card.Add(CreateLabeledRow("Hierarchy:", entry.HierarchyPath));

            _scrollView.Add(card);
        }

        private void BuildGamepadsCard(JuiceDebugEntry entry)
        {
            var card = CreateCard();
            card.Add(CreateCardHeader($"🎮 Input Devices & Gamepads ({entry.Gamepads.Count})"));

            if (entry.Gamepads.Count == 0)
            {
                var helpBox = CreateHelpBox("No Gamepads connected to this feedback context.\n(Fallback or non-haptic execution: Keyboard/Mouse, AI, or Global)");
                card.Add(helpBox);
            }
            else
            {
                for (int i = 0; i < entry.Gamepads.Count; i++)
                {
                    var gp = entry.Gamepads[i];
                    var subCard = new VisualElement();
                    subCard.AddToClassList("sub-card");

                    var titleRow = CreateRow();
                    var gpTitle = new Label($"#{i + 1} {gp.DisplayName}");
                    gpTitle.AddToClassList("sub-card__title");
                    titleRow.Add(gpTitle);
                    AddFlexSpacer(titleRow);
                    if (gp.IsCurrent)
                    {
                        var badge = new Label("[Gamepad.current]");
                        badge.AddToClassList("sub-card__badge");
                        titleRow.Add(badge);
                    }
                    subCard.Add(titleRow);

                    var detailLabel = new Label($"Device ID: {gp.DeviceId}   Layout: {gp.Layout}   Connected: {gp.IsAdded}");
                    detailLabel.AddToClassList("sub-card__detail");
                    subCard.Add(detailLabel);

                    card.Add(subCard);
                }
            }

            _scrollView.Add(card);
        }

        private void BuildTransformCard(JuiceDebugEntry entry)
        {
            var card = CreateCard();
            card.Add(CreateCardHeader("📍 Transform & Spatial Data"));

            // Root Transform
            if (entry.RootTransform != null)
            {
                var row = CreateRow();
                var lbl = new Label("Root Transform:");
                lbl.AddToClassList("detail-card__label");
                row.Add(lbl);

                var objField = new UnityEditor.UIElements.ObjectField { objectType = typeof(Transform), value = entry.RootTransform };
                objField.SetEnabled(false);
                objField.style.flexGrow = 1;
                row.Add(objField);
                card.Add(row);
            }
            else
            {
                card.Add(CreateLabeledRow("Root Transform:", "None (Null Root)"));
            }

            if (entry.RootPosition.HasValue)
            {
                var p = entry.RootPosition.Value;
                card.Add(CreateLabeledRow("Root Position:", $"({p.x:F2}, {p.y:F2}, {p.z:F2})"));
            }

            if (entry.RootRotation.HasValue)
            {
                var e = entry.RootRotation.Value.eulerAngles;
                card.Add(CreateLabeledRow("Root Rotation:", $"Euler ({e.x:F1}°, {e.y:F1}°, {e.z:F1}°)"));
            }

            if (entry.ContactPoint.HasValue)
            {
                var cp = entry.ContactPoint.Value;
                string distStr = entry.RootPosition.HasValue
                    ? $" (Dist: {Vector3.Distance(cp, entry.RootPosition.Value):F2}m)"
                    : "";
                card.Add(CreateLabeledRow("Contact Point:", $"({cp.x:F2}, {cp.y:F2}, {cp.z:F2}){distStr}", true));
            }
            else
            {
                card.Add(CreateLabeledRow("Contact Point:", "None (Defaults to Root Transform)"));
            }

            if (entry.ContactRotation.HasValue)
            {
                var ce = entry.ContactRotation.Value.eulerAngles;
                card.Add(CreateLabeledRow("Contact Rotation:", $"Euler ({ce.x:F1}°, {ce.y:F1}°, {ce.z:F1}°)"));
            }

            _scrollView.Add(card);
        }

        private void BuildRenderersCard(JuiceDebugEntry entry)
        {
            var card = CreateCard();
            card.Add(CreateCardHeader($"🎨 Connected Renderers ({entry.Renderers.Count})"));

            if (entry.Renderers.Count == 0)
            {
                card.Add(CreateHelpBox("No Renderers connected to this feedback context."));
            }
            else
            {
                foreach (var rend in entry.Renderers)
                {
                    var subCard = new VisualElement();
                    subCard.AddToClassList("sub-card");

                    var titleRow = CreateRow();
                    if (rend.Renderer != null)
                    {
                        var objField = new UnityEditor.UIElements.ObjectField
                        {
                            objectType = typeof(Renderer),
                            value = rend.Renderer
                        };
                        objField.SetEnabled(false);
                        objField.style.width = 180;
                        titleRow.Add(objField);
                    }
                    else
                    {
                        var nameLabel = new Label($"{rend.Name} (Destroyed)");
                        nameLabel.AddToClassList("sub-card__detail");
                        nameLabel.style.width = 180;
                        titleRow.Add(nameLabel);
                    }

                    var typeLabel = new Label(rend.TypeName);
                    typeLabel.AddToClassList("sub-card__detail");
                    titleRow.Add(typeLabel);

                    AddFlexSpacer(titleRow);

                    var enabledLabel = new Label(rend.Enabled ? "Enabled" : "Disabled");
                    enabledLabel.AddToClassList("sub-card__detail");
                    titleRow.Add(enabledLabel);

                    subCard.Add(titleRow);

                    if (rend.MaterialNames != null && rend.MaterialNames.Length > 0)
                    {
                        var matLabel = new Label($"Materials: {string.Join(", ", rend.MaterialNames)}");
                        matLabel.AddToClassList("sub-card__detail");
                        subCard.Add(matLabel);
                    }

                    card.Add(subCard);
                }
            }

            _scrollView.Add(card);
        }

        private void BuildEffectParametersCard(JuiceDebugEntry entry)
        {
            var card = CreateCard();
            card.Add(CreateCardHeader("⚙️ Effect Parameters & Configuration"));

            card.Add(CreateLabeledRow("Target Mode:", entry.Target.ToString(), true));
            card.Add(CreateLabeledRow("Multiplier:", $"x{entry.Multiplier:F2}", true));

            string durInfo = entry.HasDurationOverride
                ? $"{entry.Duration:F2}s (Overridden)"
                : $"{entry.Duration:F2}s (Default)";
            card.Add(CreateLabeledRow("Duration:", durInfo, true));

            if (entry.Delay > 0f)
                card.Add(CreateLabeledRow("Delay:", $"{entry.Delay:F2}s", true));

            // Embedded effect data inspector
            if (entry.EffectData != null)
            {
                var separator = new VisualElement();
                separator.style.height = 1;
                separator.style.backgroundColor = new Color(0.24f, 0.24f, 0.24f);
                separator.style.marginTop = 6;
                separator.style.marginBottom = 6;
                card.Add(separator);

                var assetTitle = new Label("Asset Inspector Properties:");
                assetTitle.AddToClassList("detail-card__title");
                assetTitle.style.marginBottom = 4;
                card.Add(assetTitle);

                BuildEffectDataInspector(card, entry.EffectData);
            }

            _scrollView.Add(card);
        }

        // ═══════════════════════════════════════════════════════
        //  Effect Data Inspector (embedded PropertyFields)
        // ═══════════════════════════════════════════════════════

        private void BuildEffectDataInspector(VisualElement container, JuiceEffectData effectData)
        {
            if (_cachedEffectEditorTarget != effectData)
            {
                CleanupCachedEditor();
                _cachedEffectEditor = UnityEditor.Editor.CreateEditor(effectData);
                _cachedEffectEditorTarget = effectData;
            }

            if (_cachedEffectEditor == null) return;

            var so = _cachedEffectEditor.serializedObject;
            so.Update();

            var iterator = so.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.name == "m_Script") continue;

                var propField = new PropertyField(iterator.Copy());
                propField.style.marginLeft = 8;
                propField.Bind(so);
                container.Add(propField);
            }
        }

        private void CleanupCachedEditor()
        {
            if (_cachedEffectEditor != null)
            {
                UnityEngine.Object.DestroyImmediate(_cachedEffectEditor);
                _cachedEffectEditor = null;
                _cachedEffectEditorTarget = null;
            }
        }

        // ═══════════════════════════════════════════════════════
        //  Card Factory Helpers
        // ═══════════════════════════════════════════════════════

        private static VisualElement CreateCard()
        {
            var card = new VisualElement();
            card.AddToClassList("detail-card");
            return card;
        }

        private static VisualElement CreateCardHeader(string title)
        {
            var header = new VisualElement();
            header.AddToClassList("detail-card__header");

            var lbl = new Label(title);
            lbl.AddToClassList("detail-card__title");
            header.Add(lbl);

            return header;
        }

        private static VisualElement CreateRow()
        {
            var row = new VisualElement();
            row.AddToClassList("detail-card__row");
            return row;
        }

        private static VisualElement CreateLabeledRow(string label, string value, bool bold = false)
        {
            var row = CreateRow();

            var lbl = new Label(label);
            lbl.AddToClassList("detail-card__label");
            row.Add(lbl);

            var val = new Label(value);
            val.AddToClassList("detail-card__value");
            if (bold) val.AddToClassList("detail-card__value--bold");
            row.Add(val);

            return row;
        }

        private static VisualElement CreateHelpBox(string text)
        {
            var box = new VisualElement();
            box.AddToClassList("juice-help-box");

            var lbl = new Label(text);
            lbl.AddToClassList("juice-help-box__text");
            box.Add(lbl);

            return box;
        }

        private static void AddFlexSpacer(VisualElement parent)
        {
            var spacer = new VisualElement { style = { flexGrow = 1 } };
            parent.Add(spacer);
        }

        private static void AddSpacer(VisualElement parent, float width)
        {
            var spacer = new VisualElement { style = { width = width } };
            parent.Add(spacer);
        }
    }
}
