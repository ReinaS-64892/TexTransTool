#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;

namespace net.rs64.TexTransTool.Editor.Decal
{

    [CustomEditor(typeof(SimpleDecal), true)]
    public class SimpleDecalEditor : UnityEditor.Editor
    {
        bool FoldoutOption;
        public override void OnInspectorGUI()
        {
            var This_S_Object = serializedObject;
            var ThisObject = target as SimpleDecal;


            EditorGUI.BeginDisabledGroup(ThisObject.IsApply);

            AbstructSingleDecalEditor.DrowDecalEditor(This_S_Object);


            EditorGUILayout.LabelField("ScaleSettings", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            var s_Scale = This_S_Object.FindProperty("Scale");
            var s_FixedAspect = This_S_Object.FindProperty("FixedAspect");
            AbstructSingleDecalEditor.DorwScaileEditor(ThisObject, This_S_Object, s_Scale, s_FixedAspect);

            var s_MaxDistans = This_S_Object.FindProperty("MaxDistans");
            TextureTransformerEditor.DrowProperty(s_MaxDistans, (float MaxDistansValue) =>
            {
                Undo.RecordObject(ThisObject, "ApplyScaile - MaxDistans");
                ThisObject.MaxDistans = MaxDistansValue;
                ThisObject.ScaleApply();
            });
            EditorGUI.indentLevel -= 1;


            EditorGUILayout.LabelField("CullingSettings", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;

            var s_PolygonCulling = This_S_Object.FindProperty("PolygonCulling");
            EditorGUILayout.PropertyField(s_PolygonCulling, new GUIContent("Polygon Culling"));

            var s_SideCulling = This_S_Object.FindProperty("SideCulling");
            EditorGUILayout.PropertyField(s_SideCulling, new GUIContent("Side Culling"));

            var s_IslandCulling = This_S_Object.FindProperty("IslandCulling");
            EditorGUILayout.PropertyField(s_IslandCulling);
            if (s_IslandCulling.boolValue)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.LabelField("IslandSelectorPos");
                EditorGUI.indentLevel += 1;
                var s_IslandSelectorPos = This_S_Object.FindProperty("IslandSelectorPos");
                var s_IslandSelectorPosX = s_IslandSelectorPos.FindPropertyRelative("x");
                var s_IslandSelectorPosY = s_IslandSelectorPos.FindPropertyRelative("y");
                EditorGUILayout.Slider(s_IslandSelectorPosX, 0, 1, new GUIContent("x"));
                EditorGUILayout.Slider(s_IslandSelectorPosY, 0, 1, new GUIContent("y"));
                EditorGUI.indentLevel -= 1;
                var s_IslandSelectorRange = This_S_Object.FindProperty("IslandSelectorRange");
                EditorGUILayout.Slider(s_IslandSelectorRange, 0, 1);
                EditorGUI.indentLevel -= 1;
            }
            EditorGUI.indentLevel -= 1;


            FoldoutOption = EditorGUILayout.Foldout(FoldoutOption, "Advanced Option");
            if (FoldoutOption)
            {
                EditorGUI.indentLevel += 1;

                var s_FastMode = This_S_Object.FindProperty("FastMode");
                EditorGUILayout.PropertyField(s_FastMode, new GUIContent("FastMode"));

                var s_Pading = This_S_Object.FindProperty("Pading");
                EditorGUILayout.PropertyField(s_Pading, new GUIContent("Pading"));

                var s_IsSeparateMatAndTexture = This_S_Object.FindProperty("IsSeparateMatAndTexture");
                EditorGUILayout.PropertyField(s_IsSeparateMatAndTexture, new GUIContent("SeparateMaterialAndTexture"));

                EditorGUI.indentLevel -= 1;
            }


            EditorGUI.EndDisabledGroup();
            DrowRealTimePreviewEditor(ThisObject);
            EditorGUI.BeginDisabledGroup(ThisObject.IsRealTimePreview);
            TextureTransformerEditor.DrowApplyAndRevart(ThisObject);
            EditorGUI.EndDisabledGroup();

            This_S_Object.ApplyModifiedProperties();
        }

        private static void DrowRealTimePreviewEditor(SimpleDecal Target)
        {
            if (Target == null) return;
            {
                if (!Target.IsRealTimePreview)
                {
                    EditorGUI.BeginDisabledGroup(!Target.IsPossibleApply || Target.IsApply);
                    if (GUILayout.Button("EnableRealTimePreview"))
                    {
                        Target.EnableRealTimePreview();
                        EditorUtility.SetDirty(Target);
                    }
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    if (GUILayout.Button("DisableRealTimePreview"))
                    {
                        Target.DisableRealTimePreview();
                        EditorUtility.SetDirty(Target);

                    }
                }
            }
        }
    }


}
#endif