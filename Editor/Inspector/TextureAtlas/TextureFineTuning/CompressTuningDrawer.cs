using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;
using System.Collections.Generic;
using System;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    [CustomPropertyDrawer(typeof(Compress))]
    internal class CompressTuningDrawer : PropertyDrawer
    {
        public static readonly Dictionary<BuildTarget, (GUIContent[] displayName, TextureFormat[] formats)> SimpleFormatChoices = new()
        {
            {BuildTarget.StandaloneWindows64,
                (new GUIContent[]{new("RGBA-BC7"),new("RGBA-DXT5|BC3"),new("RGB-DXT1|BC1"),new("RG-BC5"),new("R-BC4")},
                 new TextureFormat[]{TextureFormat.BC7,TextureFormat.DXT5,TextureFormat.DXT1,TextureFormat.BC5,TextureFormat.BC4})},
            {BuildTarget.Android,
                (new GUIContent[]{new("RGBA-ASTC_4x4"),new("RGBA-ASTC_5x5"),new("RGBA-ASTC_6x6"),new("RGBA-ASTC_8x8"),new("RGBA-ASTC_10x10"),new("RGBA-ASTC_12x12")},
                 new TextureFormat[]{TextureFormat.ASTC_4x4,TextureFormat.ASTC_5x5,TextureFormat.ASTC_6x6,TextureFormat.ASTC_8x8,TextureFormat.ASTC_10x10,TextureFormat.ASTC_12x12})}
        };
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 18f;
            position = DrawCompressEditor(position, property);

            var sCompressPropertyNames = property.FindPropertyRelative("PropertyNameList");
            var sCompressSelect = property.FindPropertyRelative("Select");


            position.height = EditorGUI.GetPropertyHeight(sCompressPropertyNames);
            EditorGUI.PropertyField(position, sCompressPropertyNames, "TextureFineTuning:prop:TargetPropertyName".Glc());
            position.y += position.height;
            position.height = 18f;


            EditorGUI.PropertyField(position, sCompressSelect, "TextureFineTuning:prop:Select".Glc());
            position.y += 18;
        }

        public static Rect DrawCompressEditor(Rect position, SerializedProperty property)
        {
            var sCompressFormatQuality = property.FindPropertyRelative("FormatQualityValue");
            var sUseOverride = property.FindPropertyRelative("UseOverride");
            var sOverrideTextureFormat = property.FindPropertyRelative("OverrideTextureFormat");
            var sCompressCompressionQuality = property.FindPropertyRelative("CompressionQuality");

            position.height = 18f;
            if (!sUseOverride.boolValue)
            {
                EditorGUI.PropertyField(position, sCompressFormatQuality, "TextureFineTuning:prop:FormatQuality".GlcV());
                position.y += 18;
            }
            else
            {
                var nowChoicer = SimpleFormatChoices[EditorUserBuildSettings.activeBuildTarget];
                var preIndex = Array.IndexOf(nowChoicer.formats, (TextureFormat)sOverrideTextureFormat.enumValueFlag);//なぜかenumValueIndexではなくenumValueFlagのほうを使うと正しい挙動をする。
                var postIndex = EditorGUI.Popup(position, "TextureFineTuning:prop:SimpleFormatChoices".GlcV(), preIndex, nowChoicer.displayName);
                if (preIndex != postIndex) { sOverrideTextureFormat.enumValueFlag = (int)nowChoicer.formats[postIndex]; }
                position.y += 18;

                EditorGUI.PropertyField(position, sOverrideTextureFormat, "TextureFineTuning:prop:OverrideTextureFormat".GlcV());
                position.y += 18;
                EditorGUI.PropertyField(position, sCompressCompressionQuality, "TextureFineTuning:prop:CompressionQuality".Glc());
                position.y += 18;
            }
            EditorGUI.PropertyField(position, sUseOverride, "TextureFineTuning:prop:UseOverrideTextureFormat".Glc());
            position.y += 18;

            return position;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetPropertyHeightInter(property) + 18f + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("PropertyNameList"));
        }

        public static float GetPropertyHeightInter(SerializedProperty property)
        {
            return 18f * (property.FindPropertyRelative("UseOverride").boolValue ? 4 : 2);
        }
    }
}
