#nullable enable

using System.Collections.Generic;
using UnityEngine.UIElements;

namespace JuiceVFX.Editor
{
    /// <summary>
    /// Filter bar below the toolbar: search, category, player, gamepad, and active-only filters.
    /// All filter values are two-way bound to <see cref="DebuggerViewModel"/>.
    /// </summary>
    public sealed class DebuggerFilterBarView : VisualElement
    {
        private readonly DebuggerViewModel _vm;

        private readonly SearchToolbar _search;
        private readonly PopupField<string> _categoryPopup;
        private readonly PopupField<string> _playerPopup;
        private readonly PopupField<string> _gamepadPopup;
        private readonly Toggle _activeOnlyToggle;

        public DebuggerFilterBarView(DebuggerViewModel vm)
        {
            _vm = vm;
            AddToClassList("debugger-filter-bar");

            // ── Search ──
            _search = new SearchToolbar("Search effects...");
            _search.OnSearchChanged += text => _vm.SearchText.Value = text;
            Add(_search);

            AddSeparator();

            // ── Category ──
            AddFilterLabel("Category:");
            _categoryPopup = new PopupField<string>(
                new List<string>(DebuggerViewModel.Categories),
                _vm.SelectedCategory.Value);
            _categoryPopup.AddToClassList("debugger-filter-bar__popup");
            _categoryPopup.RegisterValueChangedCallback(evt => _vm.SelectedCategory.Value = evt.newValue);
            Add(_categoryPopup);

            // ── Player ──
            AddFilterLabel("Player:");
            _playerPopup = new PopupField<string>(
                new List<string>(_vm.AvailablePlayers),
                _vm.SelectedPlayerFilter.Value);
            _playerPopup.AddToClassList("debugger-filter-bar__popup");
            _playerPopup.RegisterValueChangedCallback(evt => _vm.SelectedPlayerFilter.Value = evt.newValue);
            Add(_playerPopup);

            // ── Gamepad ──
            var gpChoices = new List<string>(DebuggerViewModel.GamepadOptions);
            _gamepadPopup = new PopupField<string>(
                gpChoices,
                gpChoices[_vm.GamepadFilterIndex.Value]);
            _gamepadPopup.AddToClassList("debugger-filter-bar__popup");
            _gamepadPopup.RegisterValueChangedCallback(evt =>
            {
                int idx = System.Array.IndexOf(DebuggerViewModel.GamepadOptions, evt.newValue);
                _vm.GamepadFilterIndex.Value = idx >= 0 ? idx : 0;
            });
            Add(_gamepadPopup);

            // ── Active Only ──
            _activeOnlyToggle = new Toggle("Active Only") { value = _vm.ActiveOnlyFilter.Value };
            _activeOnlyToggle.AddToClassList("debugger-filter-bar__toggle");
            _activeOnlyToggle.RegisterValueChangedCallback(evt => _vm.ActiveOnlyFilter.Value = evt.newValue);
            Add(_activeOnlyToggle);

            // ── Spacer ──
            var spacer = new VisualElement();
            spacer.AddToClassList("debugger-filter-bar__spacer");
            Add(spacer);

            // ── React to player list changes ──
            _vm.FilteredEntriesChanged += RefreshPlayerChoices;
        }

        private void RefreshPlayerChoices()
        {
            var current = _playerPopup.value;
            _playerPopup.choices = new List<string>(_vm.AvailablePlayers);

            // Keep current selection if still valid
            if (!_vm.AvailablePlayers.Contains(current))
            {
                _playerPopup.SetValueWithoutNotify("All");
                _vm.SelectedPlayerFilter.Value = "All";
            }
        }

        private void AddFilterLabel(string text)
        {
            var label = new Label(text);
            label.AddToClassList("debugger-filter-bar__label");
            Add(label);
        }

        private void AddSeparator()
        {
            var sep = new VisualElement();
            sep.style.width = 8;
            Add(sep);
        }
    }
}
