#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace JuiceVFX.Editor
{
    /// <summary>
    /// Central ViewModel for the Juice Debugger EditorWindow.
    /// Owns all filter/selection state, drives the timeline list and detail pane,
    /// and synchronises with the runtime <see cref="JuiceDebugger"/>.
    /// </summary>
    public sealed class DebuggerViewModel : IDisposable
    {
        // ═══════════════════════════════════════════════════════
        //  Observable State
        // ═══════════════════════════════════════════════════════

        public Observable<string> SearchText { get; } = new(string.Empty);
        public Observable<string> SelectedCategory { get; } = new("All");
        public Observable<string> SelectedPlayerFilter { get; } = new("All");
        public Observable<int> GamepadFilterIndex { get; } = new(0);
        public Observable<bool> ActiveOnlyFilter { get; } = new(false);
        public Observable<bool> AutoScrollToNewest { get; } = new(true);
        public Observable<bool> IsRecording { get; } = new(true);
        public Observable<int> MaxEntries { get; } = new(200);
        public Observable<JuiceDebugEntry?> SelectedEntry { get; } = new(null);

        // ═══════════════════════════════════════════════════════
        //  Computed / Derived
        // ═══════════════════════════════════════════════════════

        public List<DebugEntryViewModel> FilteredEntries { get; } = new();
        public List<string> AvailablePlayers { get; } = new() { "All" };
        public int ActiveCount { get; private set; }
        public int TotalCount { get; private set; }

        // ═══════════════════════════════════════════════════════
        //  Static Data
        // ═══════════════════════════════════════════════════════

        public static readonly string[] Categories =
            { "All", "Camera", "Haptics", "Transform", "Material", "Audio", "Light", "Time", "GameObject", "PostProcess" };

        public static readonly string[] GamepadOptions = { "Any Device", "With Gamepad", "No Gamepad" };
        public static readonly int[] MaxEntriesOptions = { 50, 100, 200, 500 };

        // ═══════════════════════════════════════════════════════
        //  Events (View subscribes to these)
        // ═══════════════════════════════════════════════════════

        /// <summary>Raised after the filtered entries list is rebuilt.</summary>
        public event Action? FilteredEntriesChanged;

        /// <summary>Raised when a new entry is added (for auto-scroll).</summary>
        public event Action? EntryAdded;

        /// <summary>Raised when the selected entry changes.</summary>
        public event Action<JuiceDebugEntry?>? SelectionChanged;

        // ═══════════════════════════════════════════════════════
        //  Lifecycle
        // ═══════════════════════════════════════════════════════

        public DebuggerViewModel()
        {
            // Sync initial state from runtime model
            IsRecording.Value = JuiceDebugger.IsRecording;
            MaxEntries.Value = JuiceDebugger.MaxEntries;

            // Subscribe to model events
            JuiceDebugger.OnEntryAdded += HandleEntryAdded;
            JuiceDebugger.OnHistoryCleared += HandleHistoryCleared;

            // Wire filter changes → refresh
            SearchText.ValueChanged += _ => RefreshFilteredEntries();
            SelectedCategory.ValueChanged += _ => RefreshFilteredEntries();
            SelectedPlayerFilter.ValueChanged += _ => RefreshFilteredEntries();
            GamepadFilterIndex.ValueChanged += _ => RefreshFilteredEntries();
            ActiveOnlyFilter.ValueChanged += _ => RefreshFilteredEntries();

            // Wire commands → runtime model
            IsRecording.ValueChanged += v => JuiceDebugger.IsRecording = v;
            MaxEntries.ValueChanged += v => JuiceDebugger.MaxEntries = v;

            RefreshFilteredEntries();
        }

        public void Dispose()
        {
            JuiceDebugger.OnEntryAdded -= HandleEntryAdded;
            JuiceDebugger.OnHistoryCleared -= HandleHistoryCleared;
        }

        // ═══════════════════════════════════════════════════════
        //  Commands
        // ═══════════════════════════════════════════════════════

        public void ToggleRecording()
        {
            IsRecording.Value = !IsRecording.Value;
        }

        public void ClearHistory()
        {
            JuiceDebugger.ClearHistory();
        }

        public void SelectEntry(JuiceDebugEntry? entry)
        {
            SelectedEntry.Value = entry;
            SelectionChanged?.Invoke(entry);
        }

        public void ReplayEntry(JuiceDebugEntry entry)
        {
            if (!Application.isPlaying || entry.Player == null || entry.EffectData == null) return;

            entry.Player.Play(
                new[] { entry.EffectData },
                entry.Target == JuiceEffectTarget.Camera,
                entry.ContactPoint,
                entry.ContactRotation,
                entry.Multiplier,
                entry.Duration);
        }

        public void PingEffectAsset(JuiceDebugEntry entry)
        {
            if (entry.EffectData == null) return;
            EditorGUIUtility.PingObject(entry.EffectData);
            Selection.activeObject = entry.EffectData;
        }

        public void PingPlayer(JuiceDebugEntry entry)
        {
            if (entry.Player == null) return;
            EditorGUIUtility.PingObject(entry.Player.gameObject);
            Selection.activeGameObject = entry.Player.gameObject;
        }

        public void StopRunner(JuiceDebugEntry entry)
        {
            entry.GetRunner()?.Stop();
        }

        public void CopyEntryMarkdown(JuiceDebugEntry entry)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"### 🍹 JuiceVFX Event #{entry.Id} - {entry.EffectName}");
            sb.AppendLine($"- **Timestamp:** {entry.TimeStamp:F2}s (Frame {entry.FrameCount})");
            sb.AppendLine($"- **Category:** {entry.Category}");
            sb.AppendLine($"- **Invoker:** `{entry.InvokerFullInfo}`");
            sb.AppendLine($"- **Player Component:** `{entry.PlayerTypeName}` on `{entry.PlayerName}`");
            sb.AppendLine($"- **Hierarchy:** `{entry.HierarchyPath}`");
            sb.AppendLine($"- **Duration:** {entry.Duration:F2}s | **Multiplier:** x{entry.Multiplier:F2} | **Delay:** {entry.Delay:F2}s");

            if (entry.Gamepads.Count > 0)
            {
                sb.AppendLine($"- **Gamepads ({entry.Gamepads.Count}):**");
                foreach (var gp in entry.Gamepads)
                    sb.AppendLine($"  - {gp.DisplayName} (ID: {gp.DeviceId}, Layout: {gp.Layout})");
            }
            else
            {
                sb.AppendLine("- **Gamepads:** None");
            }

            if (entry.Renderers.Count > 0)
            {
                sb.AppendLine($"- **Renderers ({entry.Renderers.Count}):**");
                foreach (var r in entry.Renderers)
                    sb.AppendLine($"  - {r.Name} ({r.TypeName}, Enabled: {r.Enabled}, Mats: {string.Join(", ", r.MaterialNames)})");
            }
            else
            {
                sb.AppendLine("- **Renderers:** None");
            }

            if (entry.RootPosition.HasValue)
                sb.AppendLine($"- **Root Position:** `{entry.RootPosition.Value}`");
            if (entry.ContactPoint.HasValue)
                sb.AppendLine($"- **Contact Point:** `{entry.ContactPoint.Value}`");

            EditorGUIUtility.systemCopyBuffer = sb.ToString();
            Debug.Log($"[JuiceDebugger] Event #{entry.Id} copied to clipboard!");
        }

        // ═══════════════════════════════════════════════════════
        //  Private — Event Handlers
        // ═══════════════════════════════════════════════════════

        private void HandleEntryAdded(JuiceDebugEntry entry)
        {
            RefreshFilteredEntries();
            EntryAdded?.Invoke();
        }

        private void HandleHistoryCleared()
        {
            SelectedEntry.Value = null;
            SelectionChanged?.Invoke(null);
            RefreshFilteredEntries();
        }

        // ═══════════════════════════════════════════════════════
        //  Private — Filtering
        // ═══════════════════════════════════════════════════════

        private void RefreshFilteredEntries()
        {
            FilteredEntries.Clear();

            IEnumerable<JuiceDebugEntry> list = JuiceDebugger.History;

            // Text search
            string search = SearchText.Value;
            if (!string.IsNullOrEmpty(search))
            {
                string query = search.ToLowerInvariant();
                list = list.Where(e =>
                    (e.EffectName != null && e.EffectName.ToLowerInvariant().Contains(query)) ||
                    (e.PlayerName != null && e.PlayerName.ToLowerInvariant().Contains(query)) ||
                    (e.InvokerFullInfo != null && e.InvokerFullInfo.ToLowerInvariant().Contains(query)) ||
                    e.Gamepads.Any(g => g.DisplayName.ToLowerInvariant().Contains(query)));
            }

            // Category filter
            string category = SelectedCategory.Value;
            if (category != "All")
                list = list.Where(e => e.Category == category);

            // Player filter
            string player = SelectedPlayerFilter.Value;
            if (player != "All")
                list = list.Where(e => e.PlayerName == player);

            // Gamepad filter
            int gpFilter = GamepadFilterIndex.Value;
            if (gpFilter == 1) list = list.Where(e => e.Gamepads.Count > 0);
            else if (gpFilter == 2) list = list.Where(e => e.Gamepads.Count == 0);

            // Active only
            if (ActiveOnlyFilter.Value)
                list = list.Where(e => e.IsRunnerActive);

            foreach (var entry in list)
                FilteredEntries.Add(new DebugEntryViewModel(entry));

            // Rebuild player filter options
            AvailablePlayers.Clear();
            AvailablePlayers.Add("All");
            AvailablePlayers.AddRange(
                JuiceDebugger.History
                    .Select(e => e.PlayerName)
                    .Distinct()
                    .Where(n => !string.IsNullOrEmpty(n)));

            // Update counters
            ActiveCount = JuiceDebugger.History.Count(e => e.IsRunnerActive);
            TotalCount = JuiceDebugger.History.Count;

            FilteredEntriesChanged?.Invoke();
        }
    }
}
