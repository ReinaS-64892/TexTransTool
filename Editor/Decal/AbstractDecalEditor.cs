#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Rs64.TexTransTool.Decal;

namespace Rs64.TexTransTool.Editor.Decal
{
    [CustomEditor(typeof(AbstractDecal), true)]
    public class AbstractDecalEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var This_S_Object = serializedObject;
            DrowDecalEditor(This_S_Object);

            This_S_Object.ApplyModifiedProperties();
        }

        public static void DrowDecalEditor(SerializedObject This_S_Object)
        {
            var S_TargetRenderers = This_S_Object.FindProperty("TargetRenderers");
            var S_MultiRendereMode = This_S_Object.FindProperty("MultiRendereMode");
            TextureTransformerEditor.DorwRendarar(S_TargetRenderers, S_MultiRendereMode.boolValue);
            EditorGUILayout.PropertyField(S_MultiRendereMode);

            var S_DecalTexture = This_S_Object.FindProperty("DecalTexture");
            TextureTransformerEditor.ObjectReferencePorpty<Texture2D>(S_DecalTexture);

            var S_BlendType = This_S_Object.FindProperty("BlendType");
            EditorGUILayout.PropertyField(S_BlendType);

            var S_TargetPropatyName = This_S_Object.FindProperty("TargetPropatyName");
            EditorGUILayout.PropertyField(S_TargetPropatyName);
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
        }


    }
}
#endif