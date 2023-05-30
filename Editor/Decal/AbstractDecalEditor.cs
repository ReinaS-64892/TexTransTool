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
            DecalEditorUtili.DorwRendarar(S_TargetRenderers, S_MultiRendereMode.boolValue);

            var S_DecalTexture = This_S_Object.FindProperty("DecalTexture");
            TextureTransformerEditor.objectReferencePorpty<Texture2D>(S_DecalTexture);

            var S_BlendType = This_S_Object.FindProperty("BlendType");

            var S_TargetPropatyName = This_S_Object.FindProperty("TargetPropatyName");
            EditorGUILayout.PropertyField(S_TargetPropatyName);
        }
    }
}
#endif