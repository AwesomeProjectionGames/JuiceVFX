#nullable enable

using System;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace JuiceVFX.Editor
{
    /// <summary>
    /// Hierarchical dropdown for selecting a <see cref="JuiceEffectData"/> subtype.
    /// Groups types by their <see cref="CreateAssetMenuAttribute.menuName"/> path.
    /// Uses Unity's <see cref="AdvancedDropdown"/> for polished selection UX.
    /// </summary>
    public sealed class EffectTypeDropdown : AdvancedDropdown
    {
        /// <summary>Invoked when the user selects an effect type.</summary>
        public event Action<Type>? OnItemSelected;

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

                var attr = type.GetCustomAttributes(typeof(CreateAssetMenuAttribute), false)
                    .FirstOrDefault() as CreateAssetMenuAttribute;

                if (attr != null && !string.IsNullOrEmpty(attr.menuName))
                    menuPath = attr.menuName.Replace("AwesomeProjection/JuiceVFX/Effects/", "");

                var pathParts = menuPath.Split('/');
                var parent = root;

                for (int i = 0; i < pathParts.Length; i++)
                {
                    var part = pathParts[i];

                    if (i == pathParts.Length - 1)
                    {
                        // Leaf: actual type
                        parent.AddChild(new EffectDropdownItem(part, type));
                    }
                    else
                    {
                        // Branch: find or create sub-group
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
                OnItemSelected?.Invoke(effectItem.EffectType);
        }

        /// <summary>
        /// Internal dropdown item that carries the concrete <see cref="Type"/> reference.
        /// </summary>
        private sealed class EffectDropdownItem : AdvancedDropdownItem
        {
            public Type EffectType { get; }

            public EffectDropdownItem(string name, Type type) : base(name)
            {
                EffectType = type;
            }
        }
    }
}
