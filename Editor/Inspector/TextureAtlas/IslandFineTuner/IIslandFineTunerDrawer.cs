using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;
using System.Collections.Generic;
using System;
using net.rs64.TexTransTool.TextureAtlas.IslandFineTuner;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomPropertyDrawer(typeof(IIslandFineTuner))]
    internal class IIslandFineTunerDrawer : PropertyDrawer
    {
        static Func<IIslandFineTuner>[] s_fineTuners;
        static GUIContent[] s_names;
        static string[] s_typeFullName;

        [InitializeOnLoadMethod] static void RegisterLangSwitch() { TTTConfig.OnSwitchLanguage += _ => Init(); }
        public static void Init()
        {
            var dict = new Dictionary<Type, Func<IIslandFineTuner>>(){
                {typeof(SizeOffset),()=>new SizeOffset()},
                {typeof(SizePriority),()=>new SizePriority()},
            };

            var fineTuners = new List<Func<IIslandFineTuner>>();
            var names = new List<GUIContent>();
            var typeFullName = new List<string>();

            foreach (var kv in dict)
            {
                names.Add(("IIslandFineTuner:prop:" + kv.Key.Name).Glc());
                typeFullName.Add(kv.Key.Assembly.GetName().Name + ' ' + kv.Key.FullName.Replace("+", "/"));
                fineTuners.Add(kv.Value);
            }

            s_fineTuners = fineTuners.ToArray();
            s_names = names.ToArray();
            s_typeFullName = typeFullName.ToArray();
        }
        public static bool DrawTunerSelector(Rect position, SerializedProperty property, GUIContent label = null)
        {
            if (s_fineTuners is null) { Init(); }

            var index = Array.IndexOf(s_typeFullName, property.managedReferenceFullTypename);

            var pLabel = EditorGUI.BeginProperty(position, label ?? s_names[index], property);
            var beforeIndex = EditorGUI.Popup(position, pLabel, index, s_names);
            EditorGUI.EndProperty();
            if (index != beforeIndex) { property.managedReferenceValue = s_fineTuners[beforeIndex].Invoke(); return true; }
            return false;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawTunerSelector(position, property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }
}
