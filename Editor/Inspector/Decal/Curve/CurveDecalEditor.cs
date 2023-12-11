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
            var thisSObj = serializedObject;

            DrawerCurveDecalEditor(thisSObj);

            thisSObj.ApplyModifiedProperties();
        }

        public static void DrawerCurveDecalEditor(SerializedObject thisSObject)
        {
            var sTargetRenderers = thisSObject.FindProperty("TargetRenderers");
            var sMultiRendererMode = thisSObject.FindProperty("MultiRendererMode");
            TextureTransformerEditor.DrawerRenderer(sTargetRenderers, sMultiRendererMode.boolValue);

            var sIsUseStartAndEnd = thisSObject.FindProperty("UseFirstAndEnd");
            EditorGUILayout.PropertyField(sIsUseStartAndEnd);

            var isUseStartAndEnd = sIsUseStartAndEnd.boolValue;
            if (isUseStartAndEnd)
            {
                var sEnd = thisSObject.FindProperty("EndTexture");
                TextureTransformerEditor.DrawerObjectReference<Texture2D>(sEnd);
            }

            var sDecalTexture = thisSObject.FindProperty("DecalTexture");
            TextureTransformerEditor.DrawerObjectReference<Texture2D>(sDecalTexture);

            if (isUseStartAndEnd)
            {
                var sStart = thisSObject.FindProperty("FirstTexture");
                TextureTransformerEditor.DrawerObjectReference<Texture2D>(sStart);
            }
            // var sBlendType = thisSObject.FindProperty("BlendType");

            var sTargetPropertyName = thisSObject.FindProperty("TargetPropertyName");
            EditorGUILayout.PropertyField(sTargetPropertyName);


            var sSegments = thisSObject.FindProperty("Segments");
            DrawerSegmentFiled(sSegments);

            var sCylindricalCoordinatesSystem = thisSObject.FindProperty("CylindricalCoordinatesSystem");
            EditorGUILayout.PropertyField(sCylindricalCoordinatesSystem);

            var sSize = thisSObject.FindProperty("Size");
            EditorGUILayout.PropertyField(sSize);

            var sLoopCount = thisSObject.FindProperty("LoopCount");
            EditorGUILayout.PropertyField(sLoopCount);

            var sCurveStartOffset = thisSObject.FindProperty("CurveStartOffset");
            EditorGUILayout.PropertyField(sCurveStartOffset);

            var sRollMode = thisSObject.FindProperty("RollMode");
            EditorGUILayout.PropertyField(sRollMode);

            var sDrawGizmoAlways = thisSObject.FindProperty("DrawGizmoAlways");
            EditorGUILayout.PropertyField(sDrawGizmoAlways);
        }

        public static void DrawerSegmentFiled(SerializedProperty segment)
        {
            EditorGUILayout.LabelField("Segments");
            var arraySize = segment.arraySize;
            int count = 0;
            while (arraySize > count)
            {
                var sSegment = segment.GetArrayElementAtIndex(count);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(sSegment);
                /*
                var sSegmentRoll = sSegment.FindPropertyRelative("Roll");
                if (sSegmentRoll != null) sSegmentRoll.floatValue = EditorGUI.Slider(EditorGUILayout.GetControlRect(), sSegmentRoll.floatValue, -360, 360);
                */
                EditorGUILayout.EndHorizontal();

                count += 1;
            }

            TextureTransformerEditor.DrawerArrayResizeButton(segment);

        }
    }

}
#endif
