#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace Rs64.TexTransTool.Editor
{

    [CustomEditor(typeof(TextureTransformer))]
    public class TextureTransformerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var Target = target as TextureTransformer;
            TextureTransformerEditorDrow(Target);
        }
        public static void TextureTransformerEditorDrow(TextureTransformer Target)
        {
            EditorGUI.BeginDisabledGroup(!Target.IsPossibleAppry);
            {
                if (!Target.IsAppry)
                {
                    if (GUILayout.Button("Appry"))
                    {
                        Undo.RecordObject(Target, "TextureTransformer - Appry");
                        Target.Appry();
                    }
                }
                else
                {
                    if (GUILayout.Button("Revart"))
                    {
                        Undo.RecordObject(Target, "TextureTransformer - Revart");
                        Target.Revart();

                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!(Target.IsPossibleCompile && !Target.IsAppry));
            if (GUILayout.Button("Compile"))
            {
                Undo.RecordObject(Target, "TextureTransformer - Compile");
                Target.Compile();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
#endif