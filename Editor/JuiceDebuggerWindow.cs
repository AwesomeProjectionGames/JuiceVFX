#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using JuiceVFX;

namespace JuiceVFX.Editor
{
    /// <summary>
    /// Editor Window for debugging and inspecting JuiceVFX playback history in real-time.
    /// Displays comprehensive details: invoker, player, input devices, transforms, renderers, and effect parameters.
    /// </summary>
    public class JuiceDebuggerWindow : EditorWindow
    {
        private Vector2 _historyScrollPos;
        private Vector2 _detailsScrollPos;
        private JuiceDebugEntry? _selectedEntry;
        private int _selectedEntryId = -1;

        // Filters
        private string _searchText = string.Empty;
        private string _selectedCategory = "All";
        private string _selectedPlayerFilter = "All";
        private int _gamepadFilterIndex = 0; // 0: All, 1: With Gamepad, 2: Without Gamepad
        private bool _activeOnlyFilter = false;
        private bool _autoScrollToNewest = true;

        private UnityEditor.Editor? _cachedEffectEditor;
        private JuiceEffectData? _cachedEffectEditorTarget;

        private float _leftPaneWidth = 380f;
        private bool _isResizing = false;

        // Color palette for categories
        private static readonly Color ColorCamera = new Color(0.12f, 0.53f, 0.90f);
        private static readonly Color ColorHaptics = new Color(0.98f, 0.55f, 0.00f);
        private static readonly Color ColorTransform = new Color(0.60f, 0.25f, 0.85f);
        private static readonly Color ColorMaterial = new Color(0.20f, 0.70f, 0.35f);
        private static readonly Color ColorAudio = new Color(0.95f, 0.75f, 0.10f);
        private static readonly Color ColorLight = new Color(0.00f, 0.75f, 0.90f);
        private static readonly Color ColorTime = new Color(0.90f, 0.22f, 0.21f);
        private static readonly Color ColorGameObject = new Color(0.00f, 0.60f, 0.55f);
        private static readonly Color ColorPostProcess = new Color(0.70f, 0.30f, 0.60f);
        private static readonly Color ColorDefault = new Color(0.50f, 0.50f, 0.50f);

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
            JuiceDebugger.OnEntryAdded += HandleEntryAdded;
            JuiceDebugger.OnHistoryCleared += HandleHistoryCleared;
            EditorApplication.update += RepaintOnPlayMode;
        }

        private void OnDisable()
        {
            JuiceDebugger.OnEntryAdded -= HandleEntryAdded;
            JuiceDebugger.OnHistoryCleared -= HandleHistoryCleared;
            EditorApplication.update -= RepaintOnPlayMode;

            if (_cachedEffectEditor != null)
            {
                DestroyImmediate(_cachedEffectEditor);
                _cachedEffectEditor = null;
            }
        }

        private void RepaintOnPlayMode()
        {
            if (Application.isPlaying)
            {
                Repaint();
            }
        }

        private void HandleEntryAdded(JuiceDebugEntry entry)
        {
            if (_autoScrollToNewest)
            {
                _historyScrollPos.y = float.MaxValue;
            }
            Repaint();
        }

        private void HandleHistoryCleared()
        {
            _selectedEntry = null;
            _selectedEntryId = -1;
            Repaint();
        }

        private void OnGUI()
        {
            DrawMainToolbar();
            DrawFilterBar();

            EditorGUILayout.Space(2);

            var totalRect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            DrawSplitViews(totalRect);

            DrawStatusBar();
        }

        #region Toolbar & Filters

        private void DrawMainToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // Title & Status
            GUILayout.Label("🍹 JuiceVFX Monitor", EditorStyles.boldLabel, GUILayout.Width(135));

            GUILayout.Space(5);

            // Play / Pause Toggle
            var recordContent = JuiceDebugger.IsRecording
                ? new GUIContent("● Recording", "Capture active")
                : new GUIContent("❚❚ Paused", "Capture paused");

            var prevColor = GUI.color;
            if (JuiceDebugger.IsRecording) GUI.color = new Color(0.4f, 1f, 0.4f);
            if (GUILayout.Button(recordContent, EditorStyles.toolbarButton, GUILayout.Width(90)))
            {
                JuiceDebugger.IsRecording = !JuiceDebugger.IsRecording;
            }
            GUI.color = prevColor;

            // Clear Button
            if (GUILayout.Button(new GUIContent("Clear History", "Clear all recorded entries"), EditorStyles.toolbarButton, GUILayout.Width(85)))
            {
                JuiceDebugger.ClearHistory();
            }

            // Auto-scroll toggle
            _autoScrollToNewest = GUILayout.Toggle(_autoScrollToNewest, new GUIContent("Auto-scroll", "Automatically scroll to latest event"), EditorStyles.toolbarButton, GUILayout.Width(80));

            GUILayout.Space(10);

            // Max entries
            GUILayout.Label("Max:", EditorStyles.miniLabel, GUILayout.Width(28));
            int newMax = EditorGUILayout.IntPopup(JuiceDebugger.MaxEntries,
                new[] { "50 entries", "100 entries", "200 entries", "500 entries" },
                new[] { 50, 100, 200, 500 },
                EditorStyles.toolbarPopup, GUILayout.Width(90));
            if (newMax != JuiceDebugger.MaxEntries)
            {
                JuiceDebugger.MaxEntries = newMax;
            }

            GUILayout.FlexibleSpace();

            // Stats Chip
            int activeCount = JuiceDebugger.History.Count(e => e.IsRunnerActive);
            if (activeCount > 0)
            {
                var chipColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.2f, 0.8f, 0.3f);
                GUILayout.Label($"⚡ {activeCount} Active", EditorStyles.helpBox, GUILayout.Height(18));
                GUI.backgroundColor = chipColor;
            }

            GUILayout.Label($"Total: {JuiceDebugger.History.Count}", EditorStyles.miniLabel);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawFilterBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // Search text
            GUILayout.Label("🔍", GUILayout.Width(18));
            _searchText = EditorGUILayout.TextField(_searchText, EditorStyles.toolbarSearchField, GUILayout.Width(160));
            if (!string.IsNullOrEmpty(_searchText))
            {
                if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton") ?? EditorStyles.toolbarButton))
                {
                    _searchText = string.Empty;
                    GUI.FocusControl(null);
                }
            }

            GUILayout.Space(8);

            // Category Filter
            GUILayout.Label("Category:", EditorStyles.miniLabel, GUILayout.Width(55));
            string[] categories = new[] { "All", "Camera", "Haptics", "Transform", "Material", "Audio", "Light", "Time", "GameObject", "PostProcess" };
            int catIdx = Mathf.Max(0, Array.IndexOf(categories, _selectedCategory));
            int newCatIdx = EditorGUILayout.Popup(catIdx, categories, EditorStyles.toolbarPopup, GUILayout.Width(95));
            _selectedCategory = categories[newCatIdx];

            GUILayout.Space(5);

            // Player Filter
            var players = new List<string> { "All" };
            players.AddRange(JuiceDebugger.History.Select(e => e.PlayerName).Distinct().Where(n => !string.IsNullOrEmpty(n)));
            int playerIdx = Mathf.Max(0, players.IndexOf(_selectedPlayerFilter));
            GUILayout.Label("Player:", EditorStyles.miniLabel, GUILayout.Width(40));
            int newPlayerIdx = EditorGUILayout.Popup(playerIdx, players.ToArray(), EditorStyles.toolbarPopup, GUILayout.Width(110));
            _selectedPlayerFilter = players[Mathf.Clamp(newPlayerIdx, 0, players.Count - 1)];

            GUILayout.Space(5);

            // Gamepad filter
            string[] gpOptions = new[] { "Any Device", "With Gamepad", "No Gamepad" };
            _gamepadFilterIndex = EditorGUILayout.Popup(_gamepadFilterIndex, gpOptions, EditorStyles.toolbarPopup, GUILayout.Width(95));

            GUILayout.Space(5);

            // Active only
            _activeOnlyFilter = GUILayout.Toggle(_activeOnlyFilter, "Active Only", EditorStyles.toolbarButton, GUILayout.Width(80));

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Split Views

        private void DrawSplitViews(Rect totalRect)
        {
            if (totalRect.width < 10) return;

            float splitterWidth = 5f;
            float leftWidth = Mathf.Clamp(_leftPaneWidth, 240f, totalRect.width - 240f);
            float rightWidth = totalRect.width - leftWidth - splitterWidth;

            Rect leftRect = new Rect(totalRect.x, totalRect.y, leftWidth, totalRect.height);
            Rect splitterRect = new Rect(totalRect.x + leftWidth, totalRect.y, splitterWidth, totalRect.height);
            Rect rightRect = new Rect(totalRect.x + leftWidth + splitterWidth, totalRect.y, rightWidth, totalRect.height);

            // Draw Left Pane (History List)
            GUILayout.BeginArea(leftRect);
            DrawHistoryPane(leftRect.size);
            GUILayout.EndArea();

            // Draw Splitter
            EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);
            HandleSplitterResize(splitterRect);
            EditorGUI.DrawRect(splitterRect, new Color(0.15f, 0.15f, 0.15f, 0.8f));

            // Draw Right Pane (Details Inspector)
            GUILayout.BeginArea(rightRect);
            DrawDetailsPane(rightRect.size);
            GUILayout.EndArea();
        }

        private void HandleSplitterResize(Rect splitterRect)
        {
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.MouseDown:
                    if (splitterRect.Contains(evt.mousePosition))
                    {
                        _isResizing = true;
                        evt.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (_isResizing)
                    {
                        _leftPaneWidth = evt.mousePosition.x;
                        Repaint();
                        evt.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (_isResizing)
                    {
                        _isResizing = false;
                        evt.Use();
                    }
                    break;
            }
        }

        #endregion

        #region History Pane

        private IEnumerable<JuiceDebugEntry> GetFilteredHistory()
        {
            var list = JuiceDebugger.History.AsEnumerable();

            if (!string.IsNullOrEmpty(_searchText))
            {
                string query = _searchText.ToLowerInvariant();
                list = list.Where(e =>
                    (e.EffectName != null && e.EffectName.ToLowerInvariant().Contains(query)) ||
                    (e.PlayerName != null && e.PlayerName.ToLowerInvariant().Contains(query)) ||
                    (e.InvokerFullInfo != null && e.InvokerFullInfo.ToLowerInvariant().Contains(query)) ||
                    e.Gamepads.Any(g => g.DisplayName.ToLowerInvariant().Contains(query))
                );
            }

            if (_selectedCategory != "All")
            {
                list = list.Where(e => e.Category == _selectedCategory);
            }

            if (_selectedPlayerFilter != "All")
            {
                list = list.Where(e => e.PlayerName == _selectedPlayerFilter);
            }

            if (_gamepadFilterIndex == 1) // With Gamepad
            {
                list = list.Where(e => e.Gamepads.Count > 0);
            }
            else if (_gamepadFilterIndex == 2) // Without Gamepad
            {
                list = list.Where(e => e.Gamepads.Count == 0);
            }

            if (_activeOnlyFilter)
            {
                list = list.Where(e => e.IsRunnerActive);
            }

            return list;
        }

        private void DrawHistoryPane(Vector2 size)
        {
            var filtered = GetFilteredHistory().ToList();

            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true), GUILayout.Width(size.x));

            // Pane Header
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Timeline ({filtered.Count} / {JuiceDebugger.History.Count})", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);

            _historyScrollPos = EditorGUILayout.BeginScrollView(_historyScrollPos, GUILayout.ExpandHeight(true));

            if (filtered.Count == 0)
            {
                EditorGUILayout.Space(20);
                EditorGUILayout.HelpBox(
                    JuiceDebugger.History.Count == 0
                        ? "No Juice effects played yet.\nTrigger any feedback in Play Mode or through scripts to see real-time events."
                        : "No events match the active filters.",
                    MessageType.Info);
            }
            else
            {
                for (int i = 0; i < filtered.Count; i++)
                {
                    DrawHistoryItem(filtered[i]);
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawHistoryItem(JuiceDebugEntry entry)
        {
            bool isSelected = _selectedEntryId == entry.Id;
            Color catColor = GetCategoryColor(entry.Category);

            var itemStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(6, 6, 4, 4),
                margin = new RectOffset(2, 2, 2, 2)
            };

            var originalBg = GUI.backgroundColor;
            if (isSelected)
            {
                GUI.backgroundColor = new Color(0.24f, 0.48f, 0.85f, 1f);
            }
            else if (entry.IsRunnerActive)
            {
                GUI.backgroundColor = new Color(0.2f, 0.4f, 0.25f, 0.9f);
            }

            EditorGUILayout.BeginVertical(itemStyle);
            GUI.backgroundColor = originalBg;

            // Row 1: Time, Category Badge, Frame
            EditorGUILayout.BeginHorizontal();

            // Time string
            string timeStr = TimeSpan.FromSeconds(entry.TimeStamp).ToString(@"mm\:ss\.ff");
            GUILayout.Label(timeStr, EditorStyles.miniLabel, GUILayout.Width(55));

            // Category Pill
            DrawColorPill(entry.Category, catColor);

            GUILayout.FlexibleSpace();

            if (entry.IsRunnerActive)
            {
                var prevC = GUI.color;
                GUI.color = Color.green;
                GUILayout.Label("● RUNNING", EditorStyles.miniBoldLabel, GUILayout.Width(70));
                GUI.color = prevC;
            }
            else
            {
                GUILayout.Label($"#{entry.Id}", EditorStyles.miniLabel, GUILayout.Width(35));
            }

            EditorGUILayout.EndHorizontal();

            // Row 2: Effect Name
            EditorGUILayout.BeginHorizontal();
            string effectDisplayName = ObjectNames.NicifyVariableName(entry.EffectName.Replace("EffectData", ""));
            GUILayout.Label(effectDisplayName, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Row 3: Player / Device / Renderers summary chips
            EditorGUILayout.BeginHorizontal();

            // Player chip
            GUILayout.Label($"🎯 {entry.PlayerName}", EditorStyles.miniLabel, GUILayout.MaxWidth(130));

            // Gamepad chip
            if (entry.Gamepads.Count > 0)
            {
                string gpName = entry.Gamepads[0].DisplayName;
                if (gpName.Length > 12) gpName = gpName.Substring(0, 10) + "..";
                GUILayout.Label($"🎮 {gpName}", EditorStyles.miniLabel, GUILayout.MaxWidth(100));
            }
            else
            {
                GUILayout.Label("🎮 None", EditorStyles.miniLabel, GUILayout.Width(50));
            }

            // Renderers count
            if (entry.Renderers.Count > 0)
            {
                GUILayout.Label($"🎨 {entry.Renderers.Count}", EditorStyles.miniLabel, GUILayout.Width(35));
            }

            GUILayout.FlexibleSpace();

            // Duration / Multiplier
            GUILayout.Label($"⏱ {entry.Duration:0.##}s", EditorStyles.miniLabel);
            if (Math.Abs(entry.Multiplier - 1f) > 0.01f)
            {
                GUILayout.Label($"x{entry.Multiplier:0.#}", EditorStyles.miniBoldLabel);
            }

            EditorGUILayout.EndHorizontal();

            // Active progress bar if running
            if (entry.IsRunnerActive)
            {
                float progress = entry.Progress;
                Rect barRect = EditorGUILayout.GetControlRect(false, 3);
                EditorGUI.ProgressBar(barRect, progress, "");
            }

            EditorGUILayout.EndVertical();

            // Handle Selection Click
            Rect clickRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseDown && clickRect.Contains(Event.current.mousePosition))
            {
                SelectEntry(entry);
                Event.current.Use();
            }
        }

        private void SelectEntry(JuiceDebugEntry entry)
        {
            _selectedEntry = entry;
            _selectedEntryId = entry.Id;

            if (_cachedEffectEditor != null)
            {
                DestroyImmediate(_cachedEffectEditor);
                _cachedEffectEditor = null;
                _cachedEffectEditorTarget = null;
            }

            GUI.FocusControl(null);
            Repaint();
        }

        #endregion

        #region Details Inspector Pane

        private void DrawDetailsPane(Vector2 size)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true), GUILayout.Width(size.x));

            if (_selectedEntry == null)
            {
                DrawEmptyDetailsView();
                EditorGUILayout.EndVertical();
                return;
            }

            var entry = _selectedEntry;

            _detailsScrollPos = EditorGUILayout.BeginScrollView(_detailsScrollPos, GUILayout.ExpandHeight(true));

            // 1. Header Card (Title, Category, Status, Quick Actions)
            DrawDetailsHeaderCard(entry);

            EditorGUILayout.Space(6);

            // 2. Realtime Runner Status (if in Play Mode)
            DrawLiveRunnerStatusCard(entry);

            EditorGUILayout.Space(6);

            // 3. Invoker & Target Player Card
            DrawPlayerAndInvokerCard(entry);

            EditorGUILayout.Space(6);

            // 4. Input Devices & Gamepads Card
            DrawGamepadsCard(entry);

            EditorGUILayout.Space(6);

            // 5. Transform & Spatial Data Card
            DrawTransformCard(entry);

            EditorGUILayout.Space(6);

            // 6. Connected Renderers Card
            DrawRenderersCard(entry);

            EditorGUILayout.Space(6);

            // 7. Effect Data & Parameters Card
            DrawEffectParametersCard(entry);

            EditorGUILayout.Space(10);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawEmptyDetailsView()
        {
            EditorGUILayout.Space(30);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();

            GUILayout.Label("👈 Select an Event from Timeline", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            GUILayout.Label("Select any recorded Juice playback event on the left\nto inspect its full context (invoker, player, devices, transforms, renderers).", EditorStyles.wordWrappedMiniLabel, GUILayout.Width(280));

            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDetailsHeaderCard(JuiceDebugEntry entry)
        {
            Color catColor = GetCategoryColor(entry.Category);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Top row: Category Pill + ID + Time + Copy button
            EditorGUILayout.BeginHorizontal();
            DrawColorPill(entry.Category, catColor);
            GUILayout.Space(5);
            GUILayout.Label($"Event #{entry.Id} at {TimeSpan.FromSeconds(entry.TimeStamp):mm\\:ss\\.ff} (Frame {entry.FrameCount})", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent("📋 Copy Markdown", "Copy full event debug details to clipboard"), EditorStyles.miniButton, GUILayout.Width(110)))
            {
                CopyEventMarkdownToClipboard(entry);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            // Main Title
            string effectName = ObjectNames.NicifyVariableName(entry.EffectName.Replace("EffectData", ""));
            EditorGUILayout.LabelField(effectName, EditorStyles.largeLabel);

            EditorGUILayout.Space(4);

            // Action Buttons Row
            EditorGUILayout.BeginHorizontal();

            if (entry.EffectData != null)
            {
                if (GUILayout.Button("🔍 Ping Effect Asset", EditorStyles.miniButtonLeft))
                {
                    EditorGUIUtility.PingObject(entry.EffectData);
                    Selection.activeObject = entry.EffectData;
                }
            }

            if (entry.Player != null)
            {
                if (GUILayout.Button("🎯 Ping Player GO", EditorStyles.miniButtonMid))
                {
                    EditorGUIUtility.PingObject(entry.Player.gameObject);
                    Selection.activeGameObject = entry.Player.gameObject;
                }
            }

            // Replay Button (Play Mode)
            if (Application.isPlaying && entry.Player != null && entry.EffectData != null)
            {
                var prevC = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.3f, 0.9f, 0.4f);
                if (GUILayout.Button("⚡ Replay", EditorStyles.miniButtonRight, GUILayout.Width(65)))
                {
                    entry.Player.Play(new[] { entry.EffectData }, entry.Target == JuiceEffectTarget.Camera, entry.ContactPoint, entry.ContactRotation, entry.Multiplier, entry.Duration);
                }
                GUI.backgroundColor = prevC;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawLiveRunnerStatusCard(JuiceDebugEntry entry)
        {
            var runner = entry.GetRunner();
            bool isActive = runner != null && runner.IsPlaying && !runner.IsFinished;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("⚡ Live Runner State", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (isActive)
            {
                var prevC = GUI.color;
                GUI.color = Color.green;
                GUILayout.Label("● ACTIVE", EditorStyles.boldLabel);
                GUI.color = prevC;
            }
            else
            {
                GUILayout.Label("FINISHED / IDLE", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();

            if (isActive && runner != null)
            {
                EditorGUILayout.Space(2);
                float progress = runner.Progress;
                Rect barRect = EditorGUILayout.GetControlRect(false, 18);
                EditorGUI.ProgressBar(barRect, progress, $"{runner.ElapsedTime:0.00}s / {runner.Duration:0.00}s ({progress * 100f:0}%)");

                if (runner.DelayRemaining > 0f)
                {
                    EditorGUILayout.LabelField("Delay Remaining:", $"{runner.DelayRemaining:0.00}s", EditorStyles.miniLabel);
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Stop Runner", EditorStyles.miniButton, GUILayout.Width(90)))
                {
                    runner.Stop();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("Runner lifecycle completed.", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPlayerAndInvokerCard(JuiceDebugEntry entry)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("🕹️ Target Player & Invoker", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            // Invoker
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Invoked By:");
            GUILayout.Label(string.IsNullOrEmpty(entry.InvokerFullInfo) ? "Direct call" : entry.InvokerFullInfo, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            // Player Component
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Player Component:");
            if (entry.Player != null)
            {
                EditorGUILayout.ObjectField(entry.Player, typeof(AbstractJuicePlayer), true);
            }
            else
            {
                GUILayout.Label($"{entry.PlayerName} ({entry.PlayerTypeName}) [Destroyed/Inactive]", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();

            // Hierarchy Path
            if (!string.IsNullOrEmpty(entry.HierarchyPath))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Hierarchy Path:");
                EditorGUILayout.SelectableLabel(entry.HierarchyPath, EditorStyles.miniLabel, GUILayout.Height(18));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawGamepadsCard(JuiceDebugEntry entry)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"🎮 Input Devices & Gamepads ({entry.Gamepads.Count})", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);

            if (entry.Gamepads.Count == 0)
            {
                EditorGUILayout.HelpBox("No Gamepads connected to this feedback context.\n(Fallback or non-haptic execution: Keyboard/Mouse, AI, or Global)", MessageType.None);
            }
            else
            {
                for (int i = 0; i < entry.Gamepads.Count; i++)
                {
                    var gp = entry.Gamepads[i];
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    EditorGUILayout.BeginHorizontal();

                    GUILayout.Label($"#{i + 1} {gp.DisplayName}", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    if (gp.IsCurrent)
                    {
                        GUILayout.Label("[Gamepad.current]", EditorStyles.miniBoldLabel);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"Device ID: {gp.DeviceId}", EditorStyles.miniLabel);
                    GUILayout.Space(10);
                    GUILayout.Label($"Layout: {gp.Layout}", EditorStyles.miniLabel);
                    GUILayout.Space(10);
                    GUILayout.Label($"Connected: {gp.IsAdded}", EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawTransformCard(JuiceDebugEntry entry)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("📍 Transform & Spatial Data", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            // Root Transform
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Root Transform:");
            if (entry.RootTransform != null)
            {
                EditorGUILayout.ObjectField(entry.RootTransform, typeof(Transform), true);
            }
            else
            {
                GUILayout.Label("None (Null Root)", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();

            // Root Position / Rotation
            if (entry.RootPosition.HasValue)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Root Position:");
                GUILayout.Label($"({entry.RootPosition.Value.x:F2}, {entry.RootPosition.Value.y:F2}, {entry.RootPosition.Value.z:F2})", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }

            if (entry.RootRotation.HasValue)
            {
                Vector3 euler = entry.RootRotation.Value.eulerAngles;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Root Rotation:");
                GUILayout.Label($"Euler ({euler.x:F1}°, {euler.y:F1}°, {euler.z:F1}°)", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }

            // Contact Point
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Contact Point:");
            if (entry.ContactPoint.HasValue)
            {
                Vector3 cp = entry.ContactPoint.Value;
                string distStr = entry.RootPosition.HasValue ? $" (Dist: {Vector3.Distance(cp, entry.RootPosition.Value):F2}m)" : "";
                GUILayout.Label($"({cp.x:F2}, {cp.y:F2}, {cp.z:F2}){distStr}", EditorStyles.miniBoldLabel);
            }
            else
            {
                GUILayout.Label("None (Defaults to Root Transform)", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();

            // Contact Rotation
            if (entry.ContactRotation.HasValue)
            {
                Vector3 cpEuler = entry.ContactRotation.Value.eulerAngles;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Contact Rotation:");
                GUILayout.Label($"Euler ({cpEuler.x:F1}°, {cpEuler.y:F1}°, {cpEuler.z:F1}°)", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawRenderersCard(JuiceDebugEntry entry)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"🎨 Connected Renderers ({entry.Renderers.Count})", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);

            if (entry.Renderers.Count == 0)
            {
                EditorGUILayout.HelpBox("No Renderers connected to this feedback context.", MessageType.None);
            }
            else
            {
                for (int i = 0; i < entry.Renderers.Count; i++)
                {
                    var rendInfo = entry.Renderers[i];
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    EditorGUILayout.BeginHorizontal();

                    if (rendInfo.Renderer != null)
                    {
                        EditorGUILayout.ObjectField(rendInfo.Renderer, typeof(Renderer), true, GUILayout.Width(180));
                    }
                    else
                    {
                        GUILayout.Label(rendInfo.Name + " (Destroyed)", EditorStyles.miniLabel, GUILayout.Width(180));
                    }

                    GUILayout.Label(rendInfo.TypeName, EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();

                    string visStr = rendInfo.Enabled ? "Enabled" : "Disabled";
                    GUILayout.Label(visStr, EditorStyles.miniLabel);

                    EditorGUILayout.EndHorizontal();

                    if (rendInfo.MaterialNames != null && rendInfo.MaterialNames.Length > 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Materials:", EditorStyles.miniLabel, GUILayout.Width(60));
                        GUILayout.Label(string.Join(", ", rendInfo.MaterialNames), EditorStyles.miniLabel);
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawEffectParametersCard(JuiceDebugEntry entry)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("⚙️ Effect Parameters & Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Target Mode:");
            GUILayout.Label(entry.Target.ToString(), EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Multiplier:");
            GUILayout.Label($"x{entry.Multiplier:F2}", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Duration:");
            string durInfo = entry.HasDurationOverride ? $"{entry.Duration:F2}s (Overridden)" : $"{entry.Duration:F2}s (Default)";
            GUILayout.Label(durInfo, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            if (entry.Delay > 0f)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Delay:");
                GUILayout.Label($"{entry.Delay:F2}s", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
            }

            // Embedded ScriptableObject inspector for EffectData
            if (entry.EffectData != null)
            {
                EditorGUILayout.Space(6);
                GUILayout.Label("Asset Inspector Properties:", EditorStyles.boldLabel);

                if (_cachedEffectEditor == null || _cachedEffectEditorTarget != entry.EffectData)
                {
                    if (_cachedEffectEditor != null) DestroyImmediate(_cachedEffectEditor);
                    _cachedEffectEditor = UnityEditor.Editor.CreateEditor(entry.EffectData);
                    _cachedEffectEditorTarget = entry.EffectData;
                }

                if (_cachedEffectEditor != null)
                {
                    EditorGUI.indentLevel++;
                    _cachedEffectEditor.serializedObject.Update();
                    SerializedProperty iterator = _cachedEffectEditor.serializedObject.GetIterator();
                    bool enterChildren = true;
                    while (iterator.NextVisible(enterChildren))
                    {
                        enterChildren = false;
                        if (iterator.name == "m_Script") continue;
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                    _cachedEffectEditor.serializedObject.ApplyModifiedProperties();
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Status Bar & Helpers

        private void DrawStatusBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (_selectedEntry != null)
            {
                GUILayout.Label($"Selected: #{_selectedEntry.Id} {_selectedEntry.EffectName} ({_selectedEntry.PlayerName})", EditorStyles.miniLabel);
            }
            else
            {
                GUILayout.Label("Ready. No event selected.", EditorStyles.miniLabel);
            }

            GUILayout.FlexibleSpace();

            if (Application.isPlaying)
            {
                var prevC = GUI.color;
                GUI.color = Color.green;
                GUILayout.Label("▶ PLAY MODE", EditorStyles.miniBoldLabel);
                GUI.color = prevC;
            }
            else
            {
                GUILayout.Label("■ EDIT MODE", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawColorPill(string text, Color color)
        {
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            var pillStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fontSize = 9,
                fontStyle = FontStyle.Bold,
                fixedHeight = 16,
                padding = new RectOffset(5, 5, 1, 1),
                normal = { textColor = Color.white }
            };
            GUILayout.Label(text.ToUpperInvariant(), pillStyle);
            GUI.backgroundColor = oldColor;
        }

        private static Color GetCategoryColor(string category)
        {
            switch (category)
            {
                case "Camera": return ColorCamera;
                case "Haptics": return ColorHaptics;
                case "Transform": return ColorTransform;
                case "Material": return ColorMaterial;
                case "Audio": return ColorAudio;
                case "Light": return ColorLight;
                case "Time": return ColorTime;
                case "GameObject": return ColorGameObject;
                case "PostProcess": return ColorPostProcess;
                default: return ColorDefault;
            }
        }

        private static void CopyEventMarkdownToClipboard(JuiceDebugEntry entry)
        {
            var sb = new System.Text.StringBuilder();
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
                {
                    sb.AppendLine($"  - {gp.DisplayName} (ID: {gp.DeviceId}, Layout: {gp.Layout})");
                }
            }
            else
            {
                sb.AppendLine($"- **Gamepads:** None");
            }

            if (entry.Renderers.Count > 0)
            {
                sb.AppendLine($"- **Renderers ({entry.Renderers.Count}):**");
                foreach (var r in entry.Renderers)
                {
                    sb.AppendLine($"  - {r.Name} ({r.TypeName}, Enabled: {r.Enabled}, Mats: {string.Join(", ", r.MaterialNames)})");
                }
            }
            else
            {
                sb.AppendLine($"- **Renderers:** None");
            }

            if (entry.RootPosition.HasValue)
            {
                sb.AppendLine($"- **Root Position:** `{entry.RootPosition.Value}`");
            }
            if (entry.ContactPoint.HasValue)
            {
                sb.AppendLine($"- **Contact Point:** `{entry.ContactPoint.Value}`");
            }

            EditorGUIUtility.systemCopyBuffer = sb.ToString();
            Debug.Log($"[JuiceDebugger] Event #{entry.Id} copied to clipboard!");
        }

        #endregion
    }
}
