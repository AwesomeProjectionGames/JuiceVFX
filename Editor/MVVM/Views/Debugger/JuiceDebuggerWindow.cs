#nullable enable

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace JuiceVFX.Editor
{
    /// <summary>
    /// EditorWindow host for the JuiceVFX Debugger.
    /// Assembles the MVVM view hierarchy: Toolbar → FilterBar → SplitPane(Timeline | Details) → StatusBar.
    /// Handles Play-mode repaint scheduling and ViewModel lifecycle.
    /// </summary>
    public sealed class JuiceDebuggerWindow : EditorWindow
    {
        private DebuggerViewModel? _vm;
        private Label? _statusLabel;
        private Label? _statusMode;

        [MenuItem("Tools/JuiceVFX/Juice Debugger", false, 10)]
        [MenuItem("Window/Analysis/JuiceVFX Debugger", false, 100)]
        public static void OpenWindow()
        {
            var window = GetWindow<JuiceDebuggerWindow>("Juice Debugger");
            window.minSize = new Vector2(700, 420);
            window.Show();
        }

        private void OnEnable()
        {
            EditorApplication.update += RepaintOnPlayMode;
        }

        private void OnDisable()
        {
            EditorApplication.update -= RepaintOnPlayMode;
            _vm?.Dispose();
            _vm = null;
        }

        private void CreateGUI()
        {
            _vm = new DebuggerViewModel();

            // ── Load shared stylesheet ──
            var styleSheet = LoadStyleSheet();
            if (styleSheet != null)
                rootVisualElement.styleSheets.Add(styleSheet);

            // ── Root container ──
            var root = new VisualElement();
            root.AddToClassList("juice-debugger-root");
            root.style.flexGrow = 1;

            // ── Toolbar ──
            root.Add(new DebuggerToolbarView(_vm));

            // ── Filter Bar ──
            root.Add(new DebuggerFilterBarView(_vm));

            // ── Split Pane ──
            var splitPane = new SplitPane(380f);
            splitPane.LeftPane.Add(new TimelineListView(_vm));
            splitPane.RightPane.Add(new DetailsInspectorView(_vm));
            root.Add(splitPane);

            // ── Status Bar ──
            var statusBar = new VisualElement();
            statusBar.AddToClassList("status-bar");

            _statusLabel = new Label("Ready. No event selected.");
            _statusLabel.AddToClassList("status-bar__label");
            statusBar.Add(_statusLabel);

            var statusSpacer = new VisualElement { style = { flexGrow = 1 } };
            statusBar.Add(statusSpacer);

            _statusMode = new Label();
            _statusMode.AddToClassList("status-bar__mode");
            statusBar.Add(_statusMode);

            root.Add(statusBar);

            rootVisualElement.Add(root);

            // ── Bind status bar updates ──
            _vm.SelectionChanged += OnSelectionChanged;

            UpdateStatusMode();
        }

        // ═══════════════════════════════════════════════════════
        //  Status Bar Updates
        // ═══════════════════════════════════════════════════════

        private void OnSelectionChanged(JuiceDebugEntry? entry)
        {
            if (_statusLabel == null) return;

            _statusLabel.text = entry != null
                ? $"Selected: #{entry.Id} {entry.EffectName} ({entry.PlayerName})"
                : "Ready. No event selected.";
        }

        private void UpdateStatusMode()
        {
            if (_statusMode == null) return;

            if (Application.isPlaying)
            {
                _statusMode.text = "▶ PLAY MODE";
                _statusMode.RemoveFromClassList("status-bar__mode--edit");
                _statusMode.AddToClassList("status-bar__mode--play");
            }
            else
            {
                _statusMode.text = "■ EDIT MODE";
                _statusMode.RemoveFromClassList("status-bar__mode--play");
                _statusMode.AddToClassList("status-bar__mode--edit");
            }
        }

        // ═══════════════════════════════════════════════════════
        //  Play-Mode Repaint
        // ═══════════════════════════════════════════════════════

        private void RepaintOnPlayMode()
        {
            if (Application.isPlaying)
            {
                Repaint();
            }

            UpdateStatusMode();
        }

        // ═══════════════════════════════════════════════════════
        //  Stylesheet Loading
        // ═══════════════════════════════════════════════════════

        private static StyleSheet? LoadStyleSheet()
        {
            // Search for the USS file by GUID or path
            string[] guids = AssetDatabase.FindAssets("JuiceVFXEditor t:StyleSheet");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("JuiceVFXEditor.uss"))
                {
                    return AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                }
            }

            // Fallback: try direct path
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/SandboxGameFramework/_Dependencies/JuiceVFX/Editor/Styles/JuiceVFXEditor.uss");
        }
    }
}
