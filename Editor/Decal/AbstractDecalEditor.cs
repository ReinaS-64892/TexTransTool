#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;

namespace net.rs64.TexTransTool.Editor.Decal
{
    public class AbstructSingleDecalEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var This_S_Object = serializedObject;
            DrowDecalEditor(This_S_Object);

            This_S_Object.ApplyModifiedProperties();
        }

        public static void DrowDecalEditor(SerializedObject This_S_Object)
        {
            EditorGUILayout.LabelField("RenderesSettings", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            var s_TargetRenderers = This_S_Object.FindProperty("TargetRenderers");
            var s_MultiRendereMode = This_S_Object.FindProperty("MultiRendereMode");
            TextureTransformerEditor.DorwRendarar(s_TargetRenderers, s_MultiRendereMode.boolValue);
            EditorGUILayout.PropertyField(s_MultiRendereMode);

            EditorGUI.indentLevel -= 1;
            EditorGUILayout.LabelField("TextureSettings", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;


            var s_DecalTexture = This_S_Object.FindProperty("DecalTexture");
            TextureTransformerEditor.ObjectReferencePorpty<Texture2D>(s_DecalTexture);

            var s_Color = This_S_Object.FindProperty("Color");
            EditorGUILayout.PropertyField(s_Color);

            var s_BlendType = This_S_Object.FindProperty("BlendType");
            EditorGUILayout.PropertyField(s_BlendType);

            var s_TargetPropatyName = This_S_Object.FindProperty("TargetPropatyName");
            PropatyNameEditor.DrawInspectorGUI(s_TargetPropatyName);
            EditorGUI.indentLevel -= 1;
        }

        public static void DorwScaileEditor<T>(AbstructSingleDecal<T> ThisObject, SerializedObject This_S_Object, SerializedProperty S_Scale, SerializedProperty S_FixedAspect) where T : DecalUtil.IConvertSpace
        {
            if (S_FixedAspect.boolValue)
            {
                TextureTransformerEditor.DrowProperty(S_Scale.displayName, S_Scale.vector2Value.x, EditVulue =>
                {
                    S_Scale.vector2Value = new Vector2(EditVulue, EditVulue);
                    This_S_Object.ApplyModifiedProperties();
                    Undo.RecordObject(ThisObject, "ScaleApply - ScaleEdit");
                    ThisObject.ScaleApply();
                }
                );
            }
            else
            {
                TextureTransformerEditor.DrowProperty(S_Scale, (Vector2 EditVulue) =>
                {
                    S_Scale.vector2Value = EditVulue;
                    This_S_Object.ApplyModifiedProperties();
                    Undo.RecordObject(ThisObject, "ScaleApply - ScaleEdit");
                    ThisObject.ScaleApply();
                });
            }
            TextureTransformerEditor.DrowProperty(S_FixedAspect, (bool FixdAspectValue) =>
            {
                S_FixedAspect.boolValue = FixdAspectValue;
                This_S_Object.ApplyModifiedProperties();
                Undo.RecordObject(ThisObject, "ApplyScaile - Size");
                ThisObject.ScaleApply();
            });
        }


    }
}
#endif