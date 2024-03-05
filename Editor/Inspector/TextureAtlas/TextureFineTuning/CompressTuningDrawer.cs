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
        public static readonly Dictionary<BuildTarget, (string[] displayName, TextureFormat[] formats)> SimpleFormatChoices = new()
        {
            {BuildTarget.StandaloneWindows64,
                (new string[]{"RGBA-BC7","RGBA-DXT5|BC3","RGB-DXT1|BC1","RG-BC5","R-BC4"},
                 new TextureFormat[]{TextureFormat.BC7,TextureFormat.DXT5,TextureFormat.DXT1,TextureFormat.BC5,TextureFormat.BC4})},
            {BuildTarget.Android,
                (new string[]{"RGBA-ASTC_4x4","RGBA-ASTC_5x5","RGBA-ASTC_6x6","RGBA-ASTC_8x8","RGBA-ASTC_10x10","RGBA-ASTC_12x12"},
                 new TextureFormat[]{TextureFormat.ASTC_4x4,TextureFormat.ASTC_5x5,TextureFormat.ASTC_6x6,TextureFormat.ASTC_8x8,TextureFormat.ASTC_10x10,TextureFormat.ASTC_12x12})}
        };
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 18f;
            if (ITextureFineTuningDrawer.DrawTuningSelector(position, property)) { return; }
            position.y += 18;

            var sCompressFormatQuality = property.FindPropertyRelative("FormatQualityValue");
            var sUseOverride = property.FindPropertyRelative("UseOverride");
            var sOverrideTextureFormat = property.FindPropertyRelative("OverrideTextureFormat");
            var sCompressCompressionQuality = property.FindPropertyRelative("CompressionQuality");
            var sCompressPropertyNames = property.FindPropertyRelative("PropertyNames");
            var sCompressSelect = property.FindPropertyRelative("Select");
            if (!sUseOverride.boolValue)
            {
                EditorGUI.PropertyField(position, sCompressFormatQuality, "TextureFineTuning:prop:FormatQuality".Glc());
                position.y += 18;
            }
            else
            {
                var nowChoicer = SimpleFormatChoices[EditorUserBuildSettings.activeBuildTarget];
                var preIndex = Array.IndexOf(nowChoicer.formats, (TextureFormat)sOverrideTextureFormat.enumValueFlag);//なぜかenumValueIndexではなくenumValueFlagのほうを使うと正しい挙動をする。
                var postIndex = EditorGUI.Popup(position, "TextureFineTuning:prop:SimpleFormatChoices".GetLocalize(), preIndex, nowChoicer.displayName);
                if (preIndex != postIndex) { sOverrideTextureFormat.enumValueFlag = (int)nowChoicer.formats[postIndex]; }
                position.y += 18;

                EditorGUI.PropertyField(position, sOverrideTextureFormat, "TextureFineTuning:prop:OverrideTextureFormat".Glc());
                position.y += 18;
                EditorGUI.PropertyField(position, sCompressCompressionQuality, "TextureFineTuning:prop:CompressionQuality".Glc());
                position.y += 18;
            }
            EditorGUI.PropertyField(position, sUseOverride, "TextureFineTuning:prop:UseOverrideTextureFormat".Glc());
            position.y += 18;
            EditorGUI.PropertyField(position, sCompressPropertyNames, "TextureFineTuning:prop:TargetPropertyName".Glc());
            position.y += 18;
            EditorGUI.PropertyField(position, sCompressSelect, "TextureFineTuning:prop:Select".Glc());
            position.y += 18;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * (property.FindPropertyRelative("UseOverride").boolValue ? 7 : 5);
        }
    }
}
