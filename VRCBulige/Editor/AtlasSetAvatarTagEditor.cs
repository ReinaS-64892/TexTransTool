#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Rs.TexturAtlasCompiler.VRCBulige;
namespace Rs.TexturAtlasCompiler.VRCBulige.Editor
{
    [CustomEditor(typeof(AtlasSetAvatarTag))]
    public class AtlasSetAvatarTagEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var serialaizeatlaset = serializedObject.FindProperty("AtlasSet");
            var ClientSelect = serializedObject.FindProperty("ClientSelect");
            var SkindMesh = serialaizeatlaset.FindPropertyRelative("AtlasTargetMeshs");
            var Staticmesh = serialaizeatlaset.FindPropertyRelative("AtlasTargetStaticMeshs");
            var TextureSize = serialaizeatlaset.FindPropertyRelative("AtlasTextureSize");
            var Pading = serialaizeatlaset.FindPropertyRelative("Pading");
            var PadingType = serialaizeatlaset.FindPropertyRelative("PadingType");
            var SortingType = serialaizeatlaset.FindPropertyRelative("SortingType");
            var Contenar = serialaizeatlaset.FindPropertyRelative("Contenar");

            var AtlasSetAvatarTag = target as AtlasSetAvatarTag;
            var IsAppry = AtlasSetAvatarTag.AtlasSet.IsAppry;

            EditorGUI.BeginDisabledGroup(IsAppry);
            EditorGUILayout.PropertyField(SkindMesh);
            EditorGUILayout.PropertyField(Staticmesh);
            EditorGUILayout.PropertyField(TextureSize);
            EditorGUILayout.PropertyField(Pading);
            EditorGUILayout.PropertyField(PadingType);
            EditorGUILayout.PropertyField(SortingType);
            EditorGUILayout.PropertyField(ClientSelect);
            EditorGUILayout.PropertyField(Contenar);
            EditorGUI.EndDisabledGroup();


            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(IsAppry);
            if (GUILayout.Button("Appry"))
            {
                Undo.RecordObject(AtlasSetAvatarTag, "AtlasAppry");
                AtlasSetAvatarTag.AtlasSet.Appry();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!IsAppry);
            if (GUILayout.Button("Revart"))
            {
                Undo.RecordObject(AtlasSetAvatarTag, "AtlasRevart");
                AtlasSetAvatarTag.AtlasSet.Revart();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(IsAppry);
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("TexturAtlasCompileFored!"))
            {
                if (!AtlasSetAvatarTag.AtlasSet.IsAppry)
                {
                    Undo.RecordObject(AtlasSetAvatarTag, "AtlasCompile");
                    Compiler.AtlasSetCompile(AtlasSetAvatarTag.AtlasSet, AtlasSetAvatarTag.ClientSelect, true);
                }
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();

        }
    }
}
#endif