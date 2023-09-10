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

            AbstractSingleDecalEditor.DrawerDecalEditor(This_S_Object);

            EditorGUILayout.LabelField("ScaleSettings", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            var s_Scale = This_S_Object.FindProperty("Scale");
            var s_FixedAspect = This_S_Object.FindProperty("FixedAspect");
            AbstractSingleDecalEditor.DrawerScaleEditor(ThisObject, This_S_Object, s_Scale, s_FixedAspect);

            var s_MaxDistance = This_S_Object.FindProperty("MaxDistance");
            TextureTransformerEditor.DrawerProperty(s_MaxDistance, (float MaxDistanceValue) =>
            {
                Undo.RecordObject(ThisObject, "ApplyScale - MaxDistance");
                ThisObject.MaxDistance = MaxDistanceValue;
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

                var s_Padding = This_S_Object.FindProperty("Padding");
                EditorGUILayout.PropertyField(s_Padding, new GUIContent("Padding"));

                var s_IsSeparateMatAndTexture = This_S_Object.FindProperty("IsSeparateMatAndTexture");
                EditorGUILayout.PropertyField(s_IsSeparateMatAndTexture, new GUIContent("SeparateMaterialAndTexture"));

                EditorGUI.indentLevel -= 1;
            }


            EditorGUI.EndDisabledGroup();
            DrawerRealTimePreviewEditor(ThisObject);
            EditorGUI.BeginDisabledGroup(ThisObject.IsRealTimePreview);
            PreviewContext.instance.DrawApplyAndRevert(ThisObject);
            EditorGUI.EndDisabledGroup();

            This_S_Object.ApplyModifiedProperties();
        }

        private static void DrawerRealTimePreviewEditor(SimpleDecal Target)
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
