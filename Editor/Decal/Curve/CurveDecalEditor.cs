#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using Rs64.TexTransTool.Decal.Curve;

namespace Rs64.TexTransTool.Editor.Decal.Curve
{


    [CustomEditor(typeof(CurveDecal))]
    public class CurveDecalEditor : AbstractDecalEditor
    {
        public override void OnInspectorGUI()
        {
            var This_S_Object = serializedObject;

            DrowCurveDecalEditor(This_S_Object);

            This_S_Object.ApplyModifiedProperties();
        }

        public static void DrowCurveDecalEditor(SerializedObject This_S_Object)
        {
            var S_TargetRenderers = This_S_Object.FindProperty("TargetRenderers");
            var S_MultiRendereMode = This_S_Object.FindProperty("MultiRendereMode");
            TextureTransformerEditor.DorwRendarar(S_TargetRenderers, S_MultiRendereMode.boolValue);

            var S_isUseStartAndEnd = This_S_Object.FindProperty("UseFirstAndEnd");
            EditorGUILayout.PropertyField(S_isUseStartAndEnd);

            var isUseStartAndEnd = S_isUseStartAndEnd.boolValue;
            if (isUseStartAndEnd)
            {
                var S_End = This_S_Object.FindProperty("EndTexture");
                TextureTransformerEditor.ObjectReferencePorpty<Texture2D>(S_End);
            }

            var S_DecalTexture = This_S_Object.FindProperty("DecalTexture");
            TextureTransformerEditor.ObjectReferencePorpty<Texture2D>(S_DecalTexture);

            if (isUseStartAndEnd)
            {
                var S_Start = This_S_Object.FindProperty("FirstTexture");
                TextureTransformerEditor.ObjectReferencePorpty<Texture2D>(S_Start);
            }
            var S_BlendType = This_S_Object.FindProperty("BlendType");

            var S_TargetPropatyName = This_S_Object.FindProperty("TargetPropatyName");
            EditorGUILayout.PropertyField(S_TargetPropatyName);


            var S_Segments = This_S_Object.FindProperty("Segments");
            DrowSegmentFiled(S_Segments);

            var S_CylindricalCoordinatesSystem = This_S_Object.FindProperty("CylindricalCoordinatesSystem");
            EditorGUILayout.PropertyField(S_CylindricalCoordinatesSystem);

            var S_Size = This_S_Object.FindProperty("Size");
            EditorGUILayout.PropertyField(S_Size);

            var S_LoopCount = This_S_Object.FindProperty("LoopCount");
            EditorGUILayout.PropertyField(S_LoopCount);

            var S_CurveStartOffset = This_S_Object.FindProperty("CurveStartOffset");
            EditorGUILayout.PropertyField(S_CurveStartOffset);

            var S_RoolMode = This_S_Object.FindProperty("RoolMode");
            EditorGUILayout.PropertyField(S_RoolMode);

            var S_DorwGizmoAwiys = This_S_Object.FindProperty("DorwGizmoAwiys");
            EditorGUILayout.PropertyField(S_DorwGizmoAwiys);
        }

        public static void DrowSegmentFiled(SerializedProperty Segment)
        {
            EditorGUILayout.LabelField("Segments");
            var arrysize = Segment.arraySize;
            int Count = 0;
            while (arrysize > Count)
            {
                var S_Segment = Segment.GetArrayElementAtIndex(Count);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(S_Segment);
                /*
                var S_SegmentRool = S_Segment.FindPropertyRelative("Rool");
                if (S_SegmentRool != null) S_SegmentRool.floatValue = EditorGUI.Slider(EditorGUILayout.GetControlRect(), S_SegmentRool.floatValue, -360, 360);
                */
                EditorGUILayout.EndHorizontal();

                Count += 1;
            }

            TextureTransformerEditor.DrowArryResizeButton(Segment);

        }
    }

}
#endif