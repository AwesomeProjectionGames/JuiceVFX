#nullable enable

using UnityEngine;
using UnityEngine.UIElements;

namespace JuiceVFX.Editor
{
    /// <summary>
    /// Reusable horizontal split-pane container with a draggable handle.
    /// The left pane has a fixed width; the right pane fills remaining space.
    /// </summary>
    public sealed class SplitPane : VisualElement
    {
        private readonly VisualElement _leftPane;
        private readonly VisualElement _handle;
        private readonly VisualElement _rightPane;
        private float _leftWidth;
        private bool _isDragging;

        /// <summary>Left pane container — add children here.</summary>
        public VisualElement LeftPane => _leftPane;

        /// <summary>Right pane container — add children here.</summary>
        public VisualElement RightPane => _rightPane;

        public SplitPane(float initialLeftWidth = 380f)
        {
            _leftWidth = initialLeftWidth;
            AddToClassList("split-pane");

            // Left pane
            _leftPane = new VisualElement();
            _leftPane.AddToClassList("split-pane__left");
            _leftPane.style.width = _leftWidth;
            Add(_leftPane);

            // Drag handle
            _handle = new VisualElement();
            _handle.AddToClassList("split-pane__handle");
            Add(_handle);

            // Right pane
            _rightPane = new VisualElement();
            _rightPane.AddToClassList("split-pane__right");
            Add(_rightPane);

            // Drag events
            _handle.RegisterCallback<MouseDownEvent>(OnHandleMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnHandleMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 0) return;
            _isDragging = true;
            _handle.AddToClassList("split-pane__handle--dragging");
            _handle.CaptureMouse();
            evt.StopPropagation();
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!_isDragging) return;

            float newWidth = evt.localMousePosition.x;
            float min = 240f;
            float max = resolvedStyle.width - 240f;
            _leftWidth = Mathf.Clamp(newWidth, min, max);
            _leftPane.style.width = _leftWidth;
            evt.StopPropagation();
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (!_isDragging) return;
            _isDragging = false;
            _handle.RemoveFromClassList("split-pane__handle--dragging");
            _handle.ReleaseMouse();
            evt.StopPropagation();
        }
    }
}
