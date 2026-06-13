using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using JuiceVFX;

namespace JuiceVFX.Editor
{
    [CustomEditor(typeof(JuiceFeedback))]
    public class JuiceFeedbackEditor : UnityEditor.Editor
    {
        private JuiceFeedback _target;
        private Dictionary<JuiceEffectData, UnityEditor.Editor> _editors = new Dictionary<JuiceEffectData, UnityEditor.Editor>();
        private Dictionary<JuiceEffectData, bool> _expandedStates = new Dictionary<JuiceEffectData, bool>();
        private List<JuiceEffectData> _effectsToRemove = new List<JuiceEffectData>();

        private void OnEnable()
        {
            _target = (JuiceFeedback)target;
            CleanUpNullEffects();
        }

        private void OnDisable()
        {
            foreach (var editor in _editors.Values)
            {
                if (editor != null) DestroyImmediate(editor);
            }
            _editors.Clear();
        }

        private void CleanUpNullEffects()
        {
            bool modified = false;
            for (int i = _target.Effects.Count - 1; i >= 0; i--)
            {
                if (_target.Effects[i] == null)
                {
                    _target.Effects.RemoveAt(i);
                    modified = true;
                }
            }
            if (modified)
            {
                EditorUtility.SetDirty(_target);
                AssetDatabase.SaveAssets();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(10);
            
            // Header
            GUILayout.BeginHorizontal();
            GUILayout.Label("Juice Effects", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{_target.Effects.Count} Effect(s)", EditorStyles.miniLabel);
            GUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);

            for (int i = 0; i < _target.Effects.Count; i++)
            {
                var effect = _target.Effects[i];
                if (effect == null) continue;

                if (!_expandedStates.ContainsKey(effect))
                {
                    _expandedStates[effect] = true;
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Header
                EditorGUILayout.BeginHorizontal();
                
                // We use a foldout for the title
                string effectName = ObjectNames.NicifyVariableName(effect.GetType().Name.Replace("EffectData", ""));
                _expandedStates[effect] = EditorGUILayout.Foldout(_expandedStates[effect], effectName, true, EditorStyles.foldoutHeader);

                GUILayout.FlexibleSpace();

                bool isSubAsset = AssetDatabase.GetAssetPath(effect) == AssetDatabase.GetAssetPath(_target);

                // Reorder and Delete buttons
                if (isSubAsset)
                {
                    if (GUILayout.Button("Save Preset", EditorStyles.miniButton, GUILayout.Width(75)))
                    {
                        SaveAsPreset(effect, i);
                        GUIUtility.ExitGUI(); // Important when destroying/recreating assets in GUI
                    }
                }
                else
                {
                    if (GUILayout.Button("Clone to Local", EditorStyles.miniButton, GUILayout.Width(95)))
                    {
                        CloneToLocal(effect, i);
                        GUIUtility.ExitGUI();
                    }
                    GUILayout.Label("(Shared)", EditorStyles.miniLabel);
                }

                if (GUILayout.Button("▲", EditorStyles.miniButtonLeft, GUILayout.Width(25)) && i > 0)
                {
                    _target.Effects[i] = _target.Effects[i - 1];
                    _target.Effects[i - 1] = effect;
                    GUI.changed = true;
                }
                if (GUILayout.Button("▼", EditorStyles.miniButtonMid, GUILayout.Width(25)) && i < _target.Effects.Count - 1)
                {
                    _target.Effects[i] = _target.Effects[i + 1];
                    _target.Effects[i + 1] = effect;
                    GUI.changed = true;
                }
                
                var oldColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(25)))
                {
                    _effectsToRemove.Add(effect);
                }
                GUI.backgroundColor = oldColor;

                EditorGUILayout.EndHorizontal();

                // Body
                if (_expandedStates[effect])
                {
                    EditorGUI.indentLevel++;
                    
                    if (!isSubAsset)
                    {
                        EditorGUI.BeginChangeCheck();
                        var newEffect = (JuiceEffectData)EditorGUILayout.ObjectField("Preset Reference", effect, typeof(JuiceEffectData), false);
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (newEffect != null && newEffect != effect)
                            {
                                _target.Effects[i] = newEffect;
                                EditorUtility.SetDirty(_target);
                                AssetDatabase.SaveAssets();
                                GUIUtility.ExitGUI();
                            }
                        }
                        EditorGUILayout.Space(5);
                    }

                    if (!_editors.TryGetValue(effect, out var editor) || editor == null)
                    {
                        UnityEditor.Editor.CreateCachedEditor(effect, null, ref editor);
                        _editors[effect] = editor;
                    }
                    
                    EditorGUI.BeginChangeCheck();
                    // Optional: skip drawing the script field
                    editor.serializedObject.Update();
                    SerializedProperty iterator = editor.serializedObject.GetIterator();
                    bool enterChildren = true;
                    while (iterator.NextVisible(enterChildren))
                    {
                        enterChildren = false;
                        if (iterator.name == "m_Script") continue; // Hide the script reference
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                    editor.serializedObject.ApplyModifiedProperties();
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(effect);
                    }
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            if (_effectsToRemove.Count > 0)
            {
                foreach (var effect in _effectsToRemove)
                {
                    _target.Effects.Remove(effect);
                    if (AssetDatabase.GetAssetPath(effect) == AssetDatabase.GetAssetPath(_target))
                    {
                        AssetDatabase.RemoveObjectFromAsset(effect);
                        DestroyImmediate(effect, true);
                    }
                }
                _effectsToRemove.Clear();
                EditorUtility.SetDirty(_target);
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.Space(10);

            // Add button
            var oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            Rect btnRect = GUILayoutUtility.GetRect(new GUIContent("Add Juice Effect..."), EditorStyles.toolbarButton, GUILayout.Height(30));
            if (GUI.Button(btnRect, "Add Juice Effect...", EditorStyles.toolbarButton))
            {
                var dropdown = new EffectTypeDropdown(new AdvancedDropdownState());
                dropdown.OnItemSelected += OnAddEffect;
                dropdown.Show(btnRect);
            }
            GUI.backgroundColor = oldBg;

            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Or drop an existing preset:", EditorStyles.miniLabel);
            var droppedAsset = (JuiceEffectData)EditorGUILayout.ObjectField(null, typeof(JuiceEffectData), false, GUILayout.Width(200));
            if (droppedAsset != null)
            {
                _target.Effects.Add(droppedAsset);
                _expandedStates[droppedAsset] = true;
                EditorUtility.SetDirty(_target);
                AssetDatabase.SaveAssets();
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void CloneToLocal(JuiceEffectData effect, int index)
        {
            var clone = Instantiate(effect);
            clone.name = effect.name + " (Local)";
            AssetDatabase.AddObjectToAsset(clone, _target);
            _target.Effects[index] = clone;
            _expandedStates[clone] = true;
            EditorUtility.SetDirty(_target);
            AssetDatabase.SaveAssets();
        }

        private void SaveAsPreset(JuiceEffectData effect, int index)
        {
            string defaultFolder = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(_target));
            if (!string.IsNullOrEmpty(defaultFolder))
            {
                defaultFolder = defaultFolder.Replace("\\", "/");
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Effect as Preset",
                effect.GetType().Name + "_Preset",
                "asset",
                "Please enter a file name to save the effect preset.",
                defaultFolder
            );

            if (string.IsNullOrEmpty(path)) return;

            // Create a copy of the effect
            var copy = Instantiate(effect);
            copy.name = System.IO.Path.GetFileNameWithoutExtension(path);
            
            // Remove the old effect from sub-assets
            AssetDatabase.RemoveObjectFromAsset(effect);
            DestroyImmediate(effect, true);

            // Save the new preset
            AssetDatabase.CreateAsset(copy, path);
            
            // Assign the new preset to the list
            _target.Effects[index] = copy;

            EditorUtility.SetDirty(_target);
            AssetDatabase.SaveAssets();
        }

        private void OnAddEffect(Type effectType)
        {
            var effect = (JuiceEffectData)ScriptableObject.CreateInstance(effectType);
            effect.name = effectType.Name;
            
            AssetDatabase.AddObjectToAsset(effect, _target);
            _target.Effects.Add(effect);
            
            _expandedStates[effect] = true;
            
            EditorUtility.SetDirty(_target);
            AssetDatabase.SaveAssets();
        }
    }

    public class EffectTypeDropdown : AdvancedDropdown
    {
        public Action<Type> OnItemSelected;

        public EffectTypeDropdown(AdvancedDropdownState state) : base(state)
        {
            minimumSize = new Vector2(250, 350);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Add Juice Effect");

            var types = TypeCache.GetTypesDerivedFrom<JuiceEffectData>()
                .Where(t => !t.IsAbstract && !t.IsGenericType)
                .OrderBy(t => t.Name);

            foreach (var type in types)
            {
                string menuPath = type.Name.Replace("EffectData", "");
                var attr = type.GetCustomAttributes(typeof(CreateAssetMenuAttribute), false).FirstOrDefault() as CreateAssetMenuAttribute;
                if (attr != null && !string.IsNullOrEmpty(attr.menuName))
                {
                    menuPath = attr.menuName.Replace("AwesomeProjection/JuiceVFX/Effects/", "");
                }

                var pathParts = menuPath.Split('/');
                AdvancedDropdownItem parent = root;
                
                for (int i = 0; i < pathParts.Length; i++)
                {
                    var part = pathParts[i];
                    if (i == pathParts.Length - 1)
                    {
                        var item = new EffectDropdownItem(part, type);
                        parent.AddChild(item);
                    }
                    else
                    {
                        var existing = parent.children.FirstOrDefault(c => c.name == part);
                        if (existing == null)
                        {
                            existing = new AdvancedDropdownItem(part);
                            parent.AddChild(existing);
                        }
                        parent = existing;
                    }
                }
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is EffectDropdownItem effectItem)
            {
                OnItemSelected?.Invoke(effectItem.EffectType);
            }
        }

        private class EffectDropdownItem : AdvancedDropdownItem
        {
            public Type EffectType { get; }

            public EffectDropdownItem(string name, Type type) : base(name)
            {
                EffectType = type;
            }
        }
    }
}
