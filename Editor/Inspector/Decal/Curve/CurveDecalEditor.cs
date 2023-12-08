#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal.Curve;
using net.rs64.TexTransTool.Decal;

namespace net.rs64.TexTransTool.Editor.Decal.Curve
{


    [CustomEditor(typeof(CurveDecal))]
    internal class CurveDecalEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var This_S_Object = serializedObject;

            DrawerCurveDecalEditor(This_S_Object);

            This_S_Object.ApplyModifiedProperties();
        }

        public static void DrawerCurveDecalEditor(SerializedObject This_S_Object)
        {
            var S_TargetRenderers = This_S_Object.FindProperty("TargetRenderers");
            var S_MultiRendererMode = This_S_Object.FindProperty("MultiRendererMode");
            TextureTransformerEditor.DrawerRenderer(S_TargetRenderers, S_MultiRendererMode.boolValue);

            var S_isUseStartAndEnd = This_S_Object.FindProperty("UseFirstAndEnd");
            EditorGUILayout.PropertyField(S_isUseStartAndEnd);

            var isUseStartAndEnd = S_isUseStartAndEnd.boolValue;
            if (isUseStartAndEnd)
            {
                var S_End = This_S_Object.FindProperty("EndTexture");
                TextureTransformerEditor.DrawerObjectReference<Texture2D>(S_End);
            }

            var S_DecalTexture = This_S_Object.FindProperty("DecalTexture");
            TextureTransformerEditor.DrawerObjectReference<Texture2D>(S_DecalTexture);

            if (isUseStartAndEnd)
            {
                var S_Start = This_S_Object.FindProperty("FirstTexture");
                TextureTransformerEditor.DrawerObjectReference<Texture2D>(S_Start);
            }
            var S_BlendType = This_S_Object.FindProperty("BlendType");

            var S_TargetPropertyName = This_S_Object.FindProperty("TargetPropertyName");
            EditorGUILayout.PropertyField(S_TargetPropertyName);


            var S_Segments = This_S_Object.FindProperty("Segments");
            DrawerSegmentFiled(S_Segments);

            var S_CylindricalCoordinatesSystem = This_S_Object.FindProperty("CylindricalCoordinatesSystem");
            EditorGUILayout.PropertyField(S_CylindricalCoordinatesSystem);

            var S_Size = This_S_Object.FindProperty("Size");
            EditorGUILayout.PropertyField(S_Size);

            var S_LoopCount = This_S_Object.FindProperty("LoopCount");
            EditorGUILayout.PropertyField(S_LoopCount);

            var S_CurveStartOffset = This_S_Object.FindProperty("CurveStartOffset");
            EditorGUILayout.PropertyField(S_CurveStartOffset);

            var S_RollMode = This_S_Object.FindProperty("RollMode");
            EditorGUILayout.PropertyField(S_RollMode);

            var S_DrawGizmoAlways = This_S_Object.FindProperty("DrawGizmoAlways");
            EditorGUILayout.PropertyField(S_DrawGizmoAlways);
        }

        public static void DrawerSegmentFiled(SerializedProperty Segment)
        {
            EditorGUILayout.LabelField("Segments");
            var arraySize = Segment.arraySize;
            int count = 0;
            while (arraySize > count)
            {
                var S_Segment = Segment.GetArrayElementAtIndex(count);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(S_Segment);
                /*
                var S_SegmentRoll = S_Segment.FindPropertyRelative("Roll");
                if (S_SegmentRoll != null) S_SegmentRoll.floatValue = EditorGUI.Slider(EditorGUILayout.GetControlRect(), S_SegmentRoll.floatValue, -360, 360);
                */
                EditorGUILayout.EndHorizontal();

                count += 1;
            }

            TextureTransformerEditor.DrawerArrayResizeButton(Segment);

        }
    }

}
#endif
