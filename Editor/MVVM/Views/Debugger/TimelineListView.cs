#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace JuiceVFX.Editor
{
    /// <summary>
    /// Left-pane view: virtualized timeline list of debug entries.
    /// Uses UIToolkit <see cref="ListView"/> for efficient rendering of large histories.
    /// </summary>
    public sealed class TimelineListView : VisualElement
    {
        private readonly DebuggerViewModel _vm;
        private readonly ListView _listView;
        private readonly Label _headerTitle;
        private readonly VisualElement _emptyState;
        private readonly Label _emptyTitle;
        private readonly Label _emptySubtitle;

        public TimelineListView(DebuggerViewModel vm)
        {
            _vm = vm;
            AddToClassList("timeline-pane");

            // ── Header ──
            var header = new VisualElement();
            header.AddToClassList("timeline-pane__header");

            _headerTitle = new Label();
            _headerTitle.AddToClassList("timeline-pane__header-title");
            header.Add(_headerTitle);

            Add(header);

            // ── Empty State ──
            _emptyState = new VisualElement();
            _emptyState.AddToClassList("timeline-pane__empty");

            _emptyTitle = new Label();
            _emptyTitle.AddToClassList("timeline-pane__empty-title");
            _emptyState.Add(_emptyTitle);

            _emptySubtitle = new Label();
            _emptySubtitle.AddToClassList("timeline-pane__empty-subtitle");
            _emptyState.Add(_emptySubtitle);

            Add(_emptyState);

            // ── ListView ──
            _listView = new ListView
            {
                makeItem = MakeItem,
                bindItem = BindItem,
                fixedItemHeight = 72,
                selectionType = SelectionType.Single,
                showAlternatingRowBackgrounds = AlternatingRowBackground.None,
                virtualizationMethod = CollectionVirtualizationMethod.FixedHeight
            };
            _listView.style.flexGrow = 1;
            _listView.selectedIndicesChanged += OnSelectionChanged;
            Add(_listView);

            // ── Bind ──
            _vm.FilteredEntriesChanged += RefreshList;
            _vm.EntryAdded += OnEntryAdded;

            RefreshList();
        }

        // ═══════════════════════════════════════════════════════
        //  ListView Callbacks
        // ═══════════════════════════════════════════════════════

        private VisualElement MakeItem()
        {
            return new TimelineEntryElement();
        }

        private void BindItem(VisualElement element, int index)
        {
            if (element is TimelineEntryElement entry && index < _vm.FilteredEntries.Count)
            {
                entry.Bind(_vm.FilteredEntries[index]);
            }
        }

        private void OnSelectionChanged(IEnumerable<int> indices)
        {
            foreach (int idx in indices)
            {
                if (idx >= 0 && idx < _vm.FilteredEntries.Count)
                {
                    _vm.SelectEntry(_vm.FilteredEntries[idx].Entry);
                    return;
                }
            }
            _vm.SelectEntry(null);
        }

        private void OnEntryAdded()
        {
            if (_vm.AutoScrollToNewest.Value && _vm.FilteredEntries.Count > 0)
            {
                schedule.Execute(() => _listView.ScrollToItem(_vm.FilteredEntries.Count - 1));
            }
        }

        // ═══════════════════════════════════════════════════════
        //  Refresh
        // ═══════════════════════════════════════════════════════

        private void RefreshList()
        {
            int count = _vm.FilteredEntries.Count;
            int total = _vm.TotalCount;

            _headerTitle.text = $"Timeline ({count} / {total})";

            bool isEmpty = count == 0;
            _emptyState.style.display = isEmpty ? DisplayStyle.Flex : DisplayStyle.None;
            _listView.style.display = isEmpty ? DisplayStyle.None : DisplayStyle.Flex;

            if (isEmpty)
            {
                if (total == 0)
                {
                    _emptyTitle.text = "No Juice effects played yet.";
                    _emptySubtitle.text = "Trigger any feedback in Play Mode to see real-time events.";
                }
                else
                {
                    _emptyTitle.text = "No events match filters.";
                    _emptySubtitle.text = "Try adjusting the active filters above.";
                }
            }

            _listView.itemsSource = _vm.FilteredEntries;
            _listView.Rebuild();
        }

        // ═══════════════════════════════════════════════════════
        //  Timeline Entry Element (item template)
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Reusable visual element for a single timeline entry row.
        /// Created by <see cref="MakeItem"/> and populated by <see cref="BindItem"/>.
        /// </summary>
        private sealed class TimelineEntryElement : VisualElement
        {
            private readonly Label _time;
            private readonly CategoryPill _pill;
            private readonly Label _statusOrId;
            private readonly Label _effectName;
            private readonly Label _playerChip;
            private readonly Label _gamepadChip;
            private readonly Label _rendererChip;
            private readonly Label _durationChip;
            private readonly Label _multiplierChip;
            private readonly VisualElement _progressBar;
            private readonly VisualElement _progressFill;

            public TimelineEntryElement()
            {
                AddToClassList("timeline-entry");

                // Row 1: Time | Category | Status/ID
                var row1 = new VisualElement();
                row1.AddToClassList("timeline-entry__row");

                _time = new Label();
                _time.AddToClassList("timeline-entry__time");
                row1.Add(_time);

                _pill = new CategoryPill();
                row1.Add(_pill);

                var spacer1 = new VisualElement { style = { flexGrow = 1 } };
                row1.Add(spacer1);

                _statusOrId = new Label();
                row1.Add(_statusOrId);

                Add(row1);

                // Row 2: Effect Name
                var row2 = new VisualElement();
                row2.AddToClassList("timeline-entry__row");

                _effectName = new Label();
                _effectName.AddToClassList("timeline-entry__name");
                row2.Add(_effectName);

                Add(row2);

                // Row 3: Chips
                var row3 = new VisualElement();
                row3.AddToClassList("timeline-entry__row");

                _playerChip = new Label();
                _playerChip.AddToClassList("timeline-entry__chip");
                _playerChip.AddToClassList("timeline-entry__chip--player");
                row3.Add(_playerChip);

                _gamepadChip = new Label();
                _gamepadChip.AddToClassList("timeline-entry__chip");
                _gamepadChip.AddToClassList("timeline-entry__chip--gamepad");
                row3.Add(_gamepadChip);

                _rendererChip = new Label();
                _rendererChip.AddToClassList("timeline-entry__chip");
                _rendererChip.AddToClassList("timeline-entry__chip--renderer");
                row3.Add(_rendererChip);

                var spacer3 = new VisualElement { style = { flexGrow = 1 } };
                row3.Add(spacer3);

                _durationChip = new Label();
                _durationChip.AddToClassList("timeline-entry__chip");
                _durationChip.AddToClassList("timeline-entry__chip--duration");
                row3.Add(_durationChip);

                _multiplierChip = new Label();
                _multiplierChip.AddToClassList("timeline-entry__chip");
                _multiplierChip.AddToClassList("timeline-entry__chip--multiplier");
                row3.Add(_multiplierChip);

                Add(row3);

                // Progress bar (shown when active)
                _progressBar = new VisualElement();
                _progressBar.AddToClassList("timeline-entry__progress");

                _progressFill = new VisualElement();
                _progressFill.AddToClassList("timeline-entry__progress-fill");
                _progressBar.Add(_progressFill);

                Add(_progressBar);
            }

            public void Bind(DebugEntryViewModel vm)
            {
                _time.text = vm.FormattedTime;
                _pill.SetCategory(vm.Category);
                _effectName.text = vm.DisplayName;

                _playerChip.text = $"🎯 {vm.PlayerLabel}";
                _gamepadChip.text = $"🎮 {vm.GamepadLabel}";
                _durationChip.text = $"⏱ {vm.DurationLabel}";

                if (vm.RendererCount > 0)
                {
                    _rendererChip.text = $"🎨 {vm.RendererCount}";
                    _rendererChip.style.display = DisplayStyle.Flex;
                }
                else
                {
                    _rendererChip.style.display = DisplayStyle.None;
                }

                if (vm.HasMultiplier)
                {
                    _multiplierChip.text = vm.MultiplierLabel;
                    _multiplierChip.style.display = DisplayStyle.Flex;
                }
                else
                {
                    _multiplierChip.style.display = DisplayStyle.None;
                }

                bool isActive = vm.IsActive;

                // Status / ID label
                if (isActive)
                {
                    _statusOrId.text = "● RUNNING";
                    _statusOrId.RemoveFromClassList("timeline-entry__id");
                    _statusOrId.AddToClassList("timeline-entry__running-label");
                }
                else
                {
                    _statusOrId.text = $"#{vm.Id}";
                    _statusOrId.RemoveFromClassList("timeline-entry__running-label");
                    _statusOrId.AddToClassList("timeline-entry__id");
                }

                // Active state class
                EnableInClassList("timeline-entry--active", isActive);

                // Progress bar
                if (isActive)
                {
                    _progressBar.style.display = DisplayStyle.Flex;
                    float pct = Mathf.Clamp01(vm.Progress);
                    _progressFill.style.width = new Length(pct * 100f, LengthUnit.Percent);
                }
                else
                {
                    _progressBar.style.display = DisplayStyle.None;
                }
            }
        }
    }
}
