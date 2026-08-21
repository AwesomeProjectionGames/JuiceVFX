#nullable enable

using UnityEngine.UIElements;

namespace JuiceVFX.Editor
{
    /// <summary>
    /// Reusable color-coded category badge.
    /// Toggle the category via <see cref="SetCategory"/> to swap the USS modifier class.
    /// </summary>
    public sealed class CategoryPill : Label
    {
        private string _currentClass = string.Empty;

        public CategoryPill() : this("General") { }

        public CategoryPill(string category)
        {
            AddToClassList("category-pill");
            SetCategory(category);
        }

        /// <summary>
        /// Updates the displayed category text and applies the matching USS color class.
        /// </summary>
        public void SetCategory(string category)
        {
            text = category.ToUpperInvariant();

            if (!string.IsNullOrEmpty(_currentClass))
                RemoveFromClassList(_currentClass);

            _currentClass = $"category-pill--{category.ToLowerInvariant()}";
            AddToClassList(_currentClass);
        }
    }
}
