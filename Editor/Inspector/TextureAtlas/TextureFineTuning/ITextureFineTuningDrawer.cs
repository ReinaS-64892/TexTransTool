using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;
using System.Collections.Generic;
using System;
using ColorSpace = net.rs64.TexTransTool.TextureAtlas.FineTuning.ColorSpace;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomPropertyDrawer(typeof(ITextureFineTuning))]
    internal class ITextureFineTuningDrawer : PropertyDrawer
    {
        static Func<ITextureFineTuning>[] s_fineTunings;
        static GUIContent[] s_names;
        static string[] s_typeFullName;

        [InitializeOnLoadMethod] static void RegisterLangSwitch() { TTTConfig.OnSwitchLanguage += _ => Init(); }
        public static void Init()
        {
            var dict = new Dictionary<Type, Func<ITextureFineTuning>>(){
                {typeof(Resize),()=>Resize.Default},
                {typeof(Compress),()=>Compress.Default},
                {typeof(ReferenceCopy),()=>ReferenceCopy.Default},
                {typeof(Remove),()=>Remove.Default},
                {typeof(MipMapRemove),()=>MipMapRemove.Default},
                {typeof(ColorSpace),()=>ColorSpace.Default},
            };

            var fineTunings = new List<Func<ITextureFineTuning>>();
            var names = new List<GUIContent>();
            var typeFullName = new List<string>();

            foreach (var kv in dict)
            {
                names.Add(("TextureFineTuning:prop:" + kv.Key.Name).Glc());
                typeFullName.Add(kv.Key.Assembly.GetName().Name + ' ' + kv.Key.FullName.Replace("+", "/"));
                fineTunings.Add(kv.Value);
            }

            s_fineTunings = fineTunings.ToArray();
            s_names = names.ToArray();
            s_typeFullName = typeFullName.ToArray();
        }
        public static bool DrawTuningSelector(Rect position, SerializedProperty property, GUIContent label = null)
        {
            if (s_fineTunings is null) { Init(); }

            var index = Array.IndexOf(s_typeFullName, property.managedReferenceFullTypename);

            var pLabel = EditorGUI.BeginProperty(position, label ?? s_names[index], property);
            var beforeIndex = EditorGUI.Popup(position, pLabel, index, s_names);
            EditorGUI.EndProperty();
            if (index != beforeIndex) { property.managedReferenceValue = s_fineTunings[beforeIndex].Invoke(); return true; }
            return false;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawTuningSelector(position, property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }
}
