#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using Rs64.TexTransTool.Decal;
using Rs64.TexTransTool.Editor;

namespace Rs64.TexTransTool.Editor.Decal
{

    [CustomEditor(typeof(SimpleDecal), true)]
    public class SimpleDecalEditor : UnityEditor.Editor
    {
        bool FordiantAdvansd;
        public override void OnInspectorGUI()
        {
            var This_S_Object = serializedObject;
            var ThisObject = target as SimpleDecal;


            EditorGUI.BeginDisabledGroup(ThisObject.IsApply);

            AbstractDecalEditor.DrowDecalEditor(This_S_Object);

            var S_Scale = This_S_Object.FindProperty("Scale");
            var S_FixedAspect = This_S_Object.FindProperty("FixedAspect");
            AbstractDecalEditor.DorwScaileEditor(ThisObject, This_S_Object, S_Scale, S_FixedAspect);
            TextureTransformerEditor.DrowProperty(S_FixedAspect, (bool FixdAspectValue) =>
            {
                Undo.RecordObject(ThisObject, "ApplyScaile - SideChek");
                ThisObject.FixedAspect = FixdAspectValue;
                ThisObject.ScaleApply();
            });

            var S_MaxDistans = This_S_Object.FindProperty("MaxDistans");
            TextureTransformerEditor.DrowProperty(S_MaxDistans, (float MaxDistansValue) =>
            {
                Undo.RecordObject(ThisObject, "ApplyScaile - MaxDistans");
                ThisObject.MaxDistans = MaxDistansValue;
                ThisObject.ScaleApply();
            });

            var S_PolygonCaling = This_S_Object.FindProperty("PolygonCaling");
            EditorGUILayout.PropertyField(S_PolygonCaling);

            var S_SideChek = This_S_Object.FindProperty("SideChek");
            EditorGUILayout.PropertyField(S_SideChek);



            EditorGUI.EndDisabledGroup();
            DrowRealTimePreviewEditor(ThisObject);
            EditorGUI.BeginDisabledGroup(ThisObject.IsRealTimePreview);
            TextureTransformerEditor.TextureTransformerEditorDrow(ThisObject);
            EditorGUI.EndDisabledGroup();

            This_S_Object.ApplyModifiedProperties();
        }

        private static void DrowRealTimePreviewEditor(SimpleDecal Target)
        {
            if (Target == null) return;
            {
                if (!Target.IsRealTimePreview)
                {
                    EditorGUI.BeginDisabledGroup(!Target.IsPossibleCompile || Target.IsApply);
                    if (GUILayout.Button("EnableRealTimePreview"))
                    {
                        Undo.RecordObject(Target, "SimpleDecal - EnableRealTimePreview");
                        Target.EnableRealTimePreview();
                    }
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    if (GUILayout.Button("DisableRealTimePreview"))
                    {
                        Undo.RecordObject(Target, "SimpleDecal - DisableRealTimePreview");
                        Target.DisableRealTimePreview();

                    }
                }
            }
        }
    }


}
#endif