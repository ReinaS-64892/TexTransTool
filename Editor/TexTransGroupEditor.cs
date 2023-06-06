#if UNITY_EDITOR
using System.Net.Mime;
using UnityEngine;
using UnityEditor;

namespace Rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(AbstractTexTransGroup), true)]
    public class TexTransGroupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var thsitarget = target as AbstractTexTransGroup;
            EditorGUI.BeginDisabledGroup(thsitarget.IsApply);
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();
            DrowApplyAndRevart(thsitarget);
            TextureTransformerEditor.DrowCompile(thsitarget);
        }

        public static void DrowApplyAndRevart(AbstractTexTransGroup Target)
        {
            if (Target == null) return;
            EditorGUI.BeginDisabledGroup(!Target.IsPossibleApply);
            {
                if (!Target.IsApply)
                {
                    if (GUILayout.Button("Apply"))
                    {
                        Undo.RecordObject(Target, "TextureTransformer - Apply");
                        Target.SelfCallApply();
                    }
                }
                else
                {
                    EditorGUI.BeginDisabledGroup(!Target.IsSelfCallApply);
                    if (GUILayout.Button("Revart"))
                    {
                        Undo.RecordObject(Target, "TextureTransformer - Revart");
                        Target.SelfCallRevart();

                    }
                    EditorGUI.EndDisabledGroup();
                }
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
#endif