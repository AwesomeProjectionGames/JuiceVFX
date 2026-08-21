#nullable enable

using System;
using UnityEngine.UIElements;

namespace JuiceVFX.Editor
{
    /// <summary>
    /// Top toolbar for the Juice Debugger window.
    /// Displays record/pause toggle, clear button, auto-scroll toggle,
    /// max entries popup, and live statistics chips.
    /// </summary>
    public sealed class DebuggerToolbarView : VisualElement
    {
        private readonly DebuggerViewModel _vm;

        private readonly Button _recordBtn;
        private readonly Toggle _autoScrollToggle;
        private readonly PopupField<int> _maxEntriesPopup;
        private readonly StatusChip _activeChip;
        private readonly Label _totalLabel;

        public DebuggerToolbarView(DebuggerViewModel vm)
        {
            _vm = vm;
            AddToClassList("debugger-toolbar");

            // ── Title ──
            var title = new Label("🍹 JuiceVFX Monitor");
            title.AddToClassList("debugger-toolbar__title");
            Add(title);

            // ── Record / Pause ──
            _recordBtn = new Button(OnRecordClicked);
            _recordBtn.AddToClassList("debugger-toolbar__btn");
            _recordBtn.AddToClassList("debugger-toolbar__btn--record");
            Add(_recordBtn);

            // ── Clear ──
            var clearBtn = new Button(() => _vm.ClearHistory()) { text = "Clear History" };
            clearBtn.AddToClassList("debugger-toolbar__btn");
            Add(clearBtn);

            // ── Auto-scroll ──
            _autoScrollToggle = new Toggle("Auto-scroll") { value = _vm.AutoScrollToNewest.Value };
            _autoScrollToggle.AddToClassList("debugger-toolbar__toggle");
            _autoScrollToggle.RegisterValueChangedCallback(evt => _vm.AutoScrollToNewest.Value = evt.newValue);
            Add(_autoScrollToggle);

            // ── Max entries ──
            var maxLabel = new Label("Max:");
            maxLabel.AddToClassList("debugger-filter-bar__label");
            maxLabel.style.marginLeft = 8;
            Add(maxLabel);

            var choices = new System.Collections.Generic.List<int>(DebuggerViewModel.MaxEntriesOptions);
            _maxEntriesPopup = new PopupField<int>(
                choices,
                _vm.MaxEntries.Value,
                formatSelectedValueCallback: v => $"{v}",
                formatListItemCallback: v => $"{v} entries");
            _maxEntriesPopup.AddToClassList("debugger-toolbar__popup");
            _maxEntriesPopup.RegisterValueChangedCallback(evt => _vm.MaxEntries.Value = evt.newValue);
            Add(_maxEntriesPopup);

            // ── Spacer ──
            var spacer = new VisualElement();
            spacer.AddToClassList("debugger-toolbar__spacer");
            Add(spacer);

            // ── Active chip ──
            _activeChip = new StatusChip();
            Add(_activeChip);

            // ── Total label ──
            _totalLabel = new Label();
            _totalLabel.AddToClassList("debugger-toolbar__stats");
            Add(_totalLabel);

            // ── Bind ──
            _vm.IsRecording.ValueChanged += _ => UpdateRecordButton();
            _vm.FilteredEntriesChanged += UpdateStats;

            UpdateRecordButton();
            UpdateStats();
        }

        private void OnRecordClicked()
        {
            _vm.ToggleRecording();
        }

        private void UpdateRecordButton()
        {
            bool recording = _vm.IsRecording.Value;
            _recordBtn.text = recording ? "● Recording" : "❚❚ Paused";
            _recordBtn.tooltip = recording ? "Capture active — click to pause" : "Capture paused — click to resume";
        }

        private void UpdateStats()
        {
            int active = _vm.ActiveCount;
            if (active > 0)
            {
                _activeChip.SetState($"⚡ {active} Active", "status-chip--active");
                _activeChip.style.display = DisplayStyle.Flex;
            }
            else
            {
                _activeChip.style.display = DisplayStyle.None;
            }

            _totalLabel.text = $"Total: {_vm.TotalCount}";
        }
    }
}
