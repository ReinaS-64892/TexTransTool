using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Pool;
using net.rs64.TexTransTool.Preview.RealTime;
using net.rs64.TexTransTool.Preview;

namespace net.rs64.TexTransTool.Editor.Decal
{
    internal static class DecalEditorUtil
    {

        static bool FoldoutAdvancedOption;
        public static void DrawerAdvancedOption(SerializedObject sObject)
        {
            FoldoutAdvancedOption = EditorGUILayout.Foldout(FoldoutAdvancedOption, "CommonDecal:label:AdvancedOption".Glc());
            if (FoldoutAdvancedOption)
            {
                EditorGUI.indentLevel += 1;

                var sHighQualityPadding = sObject.FindProperty("HighQualityPadding");
                EditorGUILayout.PropertyField(sHighQualityPadding, "CommonDecal:prop:HighQualityPadding".Glc());

                var sPadding = sObject.FindProperty("Padding");
                EditorGUILayout.PropertyField(sPadding, "CommonDecal:prop:Padding".Glc());

                EditorGUI.indentLevel -= 1;
            }

        }

    }
}
