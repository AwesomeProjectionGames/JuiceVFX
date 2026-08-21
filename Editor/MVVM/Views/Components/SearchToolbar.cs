#nullable enable

using System;
using UnityEngine.UIElements;

namespace JuiceVFX.Editor
{
    /// <summary>
    /// Reusable search field component with an integrated clear button.
    /// Fires <see cref="OnSearchChanged"/> whenever the text changes.
    /// </summary>
    public sealed class SearchToolbar : VisualElement
    {
        private readonly TextField _field;
        private readonly Button _clearBtn;

        /// <summary>Raised when the search text changes (including programmatic clears).</summary>
        public event Action<string>? OnSearchChanged;

        public string SearchText
        {
            get => _field.value;
            set => _field.value = value;
        }

        public SearchToolbar(string placeholder = "Search...")
        {
            AddToClassList("search-toolbar");

            var icon = new Label("🔍");
            icon.AddToClassList("search-toolbar__icon");
            Add(icon);

            _field = new TextField { value = string.Empty };
            _field.AddToClassList("search-toolbar__field");
            _field.RegisterValueChangedCallback(OnTextChanged);
            Add(_field);

            _clearBtn = new Button(OnClearClicked) { text = "✕" };
            _clearBtn.AddToClassList("search-toolbar__clear-btn");
            _clearBtn.style.display = DisplayStyle.None;
            Add(_clearBtn);
        }

        private void OnTextChanged(ChangeEvent<string> evt)
        {
            _clearBtn.style.display = string.IsNullOrEmpty(evt.newValue)
                ? DisplayStyle.None
                : DisplayStyle.Flex;

            OnSearchChanged?.Invoke(evt.newValue);
        }

        private void OnClearClicked()
        {
            _field.value = string.Empty;
            _field.Focus();
        }
    }
}
