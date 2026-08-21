#nullable enable

using System;
using UnityEditor;

namespace JuiceVFX.Editor
{
    /// <summary>
    /// View-model projection of a single <see cref="JuiceDebugEntry"/> for display in the timeline.
    /// Pre-computes formatted strings to avoid per-frame allocations in the View layer.
    /// </summary>
    public sealed class DebugEntryViewModel
    {
        public JuiceDebugEntry Entry { get; }

        // ── Pre-computed display values ──

        public int Id => Entry.Id;
        public string FormattedTime { get; }
        public string DisplayName { get; }
        public string Category => Entry.Category;
        public string CategoryClass { get; }
        public string PlayerLabel { get; }
        public string GamepadLabel { get; }
        public int RendererCount => Entry.Renderers.Count;
        public string DurationLabel { get; }
        public bool HasMultiplier { get; }
        public string MultiplierLabel { get; }

        // ── Live (dynamic) properties ──

        public bool IsActive => Entry.IsRunnerActive;
        public float Progress => Entry.Progress;

        public DebugEntryViewModel(JuiceDebugEntry entry)
        {
            Entry = entry;

            FormattedTime = TimeSpan.FromSeconds(entry.TimeStamp).ToString(@"mm\:ss\.ff");
            DisplayName = ObjectNames.NicifyVariableName(entry.EffectName.Replace("EffectData", ""));
            CategoryClass = $"category-pill--{entry.Category.ToLowerInvariant()}";
            PlayerLabel = entry.PlayerName;
            DurationLabel = $"{entry.Duration:0.##}s";
            HasMultiplier = Math.Abs(entry.Multiplier - 1f) > 0.01f;
            MultiplierLabel = $"x{entry.Multiplier:0.#}";

            if (entry.Gamepads.Count > 0)
            {
                var name = entry.Gamepads[0].DisplayName;
                GamepadLabel = name.Length > 12 ? name.Substring(0, 10) + ".." : name;
            }
            else
            {
                GamepadLabel = "None";
            }
        }
    }
}
