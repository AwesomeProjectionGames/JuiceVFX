#nullable enable

using UnityEngine.UIElements;

namespace JuiceVFX.Editor
{
    /// <summary>
    /// Reusable status indicator chip (e.g. "● ACTIVE", "FINISHED", "⚡ 3 Active").
    /// Swap between visual states via <see cref="SetState"/>.
    /// </summary>
    public sealed class StatusChip : Label
    {
        private string _currentStateClass = string.Empty;

        public StatusChip() : this(string.Empty, string.Empty) { }

        public StatusChip(string label, string stateClass)
        {
            AddToClassList("status-chip");
            SetState(label, stateClass);
        }

        /// <summary>
        /// Updates the chip text and swaps the USS modifier class.
        /// </summary>
        /// <param name="label">Display text.</param>
        /// <param name="stateClass">USS modifier (e.g. "status-chip--active").</param>
        public void SetState(string label, string stateClass)
        {
            text = label;

            if (!string.IsNullOrEmpty(_currentStateClass))
                RemoveFromClassList(_currentStateClass);

            _currentStateClass = stateClass;

            if (!string.IsNullOrEmpty(stateClass))
                AddToClassList(stateClass);
        }
    }
}
