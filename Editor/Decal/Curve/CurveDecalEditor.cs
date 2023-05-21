#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using Rs64.TexTransTool.Decal.Curve.Cylindrical;

namespace Rs64.TexTransTool.Editor.Decal.Curve
{


    [CustomEditor(typeof(CurveDecalEditor))]
    public class CurveDecalEditor : AbstractDecalEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var This_S_Object = serializedObject;
            var S_Segments = This_S_Object.FindProperty("Segments");
            DrowSegmentFiled(S_Segments);

            var S_CylindricalCoordinatesSystem = This_S_Object.FindProperty("CylindricalCoordinatesSystem");
            EditorGUILayout.PropertyField(S_CylindricalCoordinatesSystem);

            var S_Size = This_S_Object.FindProperty("Size");
            EditorGUILayout.PropertyField(S_Size);

            var S_LoopCount = This_S_Object.FindProperty("LoopCount");
            EditorGUILayout.PropertyField(S_LoopCount);

            var S_DorwGizmoAwiys = This_S_Object.FindProperty("DorwGizmoAwiys");
            EditorGUILayout.PropertyField(S_DorwGizmoAwiys);



            This_S_Object.ApplyModifiedProperties();
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

            DecalEditorUtili.DrowArryResizeButton(Segment);

        }
    }

}
#endif