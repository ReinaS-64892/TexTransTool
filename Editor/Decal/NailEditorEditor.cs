#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using Rs64.TexTransTool.Decal;
using Rs64.TexTransTool.Editor;
using System;

namespace Rs64.TexTransTool.Editor.Decal
{

    [CustomEditor(typeof(NailEditor), true)]
    public class NailEditorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var This_S_Object = serializedObject;
            var ThisObject = target as NailEditor;

            var S_TargetRenderers = This_S_Object.FindProperty("TargetRenderers");
            var S_MultiRendereMode = This_S_Object.FindProperty("MultiRendereMode");
            TextureTransformerEditor.DorwRendarar(S_TargetRenderers, S_MultiRendereMode.boolValue);
            EditorGUILayout.PropertyField(S_MultiRendereMode);

            var S_BlendType = This_S_Object.FindProperty("BlendType");
            EditorGUILayout.PropertyField(S_BlendType);

            var S_TargetPropatyName = This_S_Object.FindProperty("TargetPropatyName");
            EditorGUILayout.PropertyField(S_TargetPropatyName);

            var S_UseTextureAspect = This_S_Object.FindProperty("UseTextureAspect");
            EditorGUILayout.PropertyField(S_UseTextureAspect);


            var S_LeftHand = This_S_Object.FindProperty("LeftHand");
            var S_RightHand = This_S_Object.FindProperty("RightHand");
            EditorGUILayout.LabelField("LeftHand");
            DrawNailSet(S_LeftHand);
            EditorGUILayout.LabelField("RightHand");
            DrawNailSet(S_RightHand);


            DrawOffsetUtiliEditor(ThisObject);
            DrawOffsetSaveAndLoadser(ThisObject);


            TextureTransformerEditor.DrowApplyAndRevart(ThisObject);

            This_S_Object.ApplyModifiedProperties();
        }

        public static void DrawNailSet(SerializedProperty serializedProperty)
        {
            var S_FingerUpvector = serializedProperty.FindPropertyRelative("FingerUpvector");
            var S_Thumb = serializedProperty.FindPropertyRelative("Thumb");
            var S_Index = serializedProperty.FindPropertyRelative("Index");
            var S_Middle = serializedProperty.FindPropertyRelative("Middle");
            var S_Ring = serializedProperty.FindPropertyRelative("Ring");
            var S_Little = serializedProperty.FindPropertyRelative("Little");

            EditorGUI.indentLevel += 1;

            EditorGUILayout.PropertyField(S_FingerUpvector);

            EditorGUILayout.LabelField("Thumb");
            DrawNailDiscriptor(S_Thumb);
            EditorGUILayout.LabelField("Index");
            DrawNailDiscriptor(S_Index);
            EditorGUILayout.LabelField("Middle");
            DrawNailDiscriptor(S_Middle);
            EditorGUILayout.LabelField("Ring");
            DrawNailDiscriptor(S_Ring);
            EditorGUILayout.LabelField("Little");
            DrawNailDiscriptor(S_Little);

            EditorGUI.indentLevel -= 1;
        }
        public static void DrawNailDiscriptor(SerializedProperty serializedProperty)
        {
            var S_DecalTexture = serializedProperty.FindPropertyRelative("DecalTexture");
            var S_PositionOffset = serializedProperty.FindPropertyRelative("PositionOffset");
            var S_ScaileOffset = serializedProperty.FindPropertyRelative("ScaileOffset");
            var S_RotationOffset = serializedProperty.FindPropertyRelative("RotationOffset");

            EditorGUI.indentLevel += 1;

            EditorGUILayout.PropertyField(S_DecalTexture);
            DrawPositionOffset(S_PositionOffset);
            EditorGUILayout.PropertyField(S_ScaileOffset);
            EditorGUILayout.PropertyField(S_RotationOffset);

            EditorGUI.indentLevel -= 1;
        }
        public static void DrawPositionOffset(SerializedProperty serializedProperty)
        {
            var DrawValue = serializedProperty.vector3Value * 100;
            serializedProperty.vector3Value = EditorGUI.Vector3Field(EditorGUILayout.GetControlRect(), "PositionOffset", DrawValue) * 0.01f;

        }

        public static void DrawOffsetUtiliEditor(NailEditor nailEditor)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Copy", GUILayout.Width(100));

            if (GUILayout.Button("Left <= Right "))
            {
                EditorUtility.SetDirty(nailEditor);
                var nailOffsets = new NailOffSets();
                nailOffsets.Copy(nailEditor.RightHand);
                nailEditor.LeftHand.Copy(nailOffsets);
            }

            if (GUILayout.Button("Left => Right"))
            {
                EditorUtility.SetDirty(nailEditor);
                var nailOffsets = new NailOffSets();
                nailOffsets.Copy(nailEditor.LeftHand);
                nailEditor.RightHand.Copy(nailOffsets);
            }

            EditorGUILayout.EndHorizontal();

        }

        public NailOffsetData nailOffsetData;
        public void DrawOffsetSaveAndLoadser(NailEditor thisObject)
        {
            EditorGUILayout.BeginHorizontal();
            nailOffsetData = EditorGUILayout.ObjectField("SaveData", nailOffsetData, typeof(NailOffsetData), false) as NailOffsetData;
            if (GUILayout.Button("Load"))
            {
                if (nailOffsetData != null)
                {
                    EditorUtility.SetDirty(nailOffsetData);
                    thisObject.LeftHand.Copy(nailOffsetData.LeftHand);
                    thisObject.RightHand.Copy(nailOffsetData.RightHand);
                }
            }
            if (GUILayout.Button("Save"))
            {
                if (nailOffsetData != null)
                {
                    EditorUtility.SetDirty(nailOffsetData);
                    nailOffsetData.LeftHand.Copy(thisObject.LeftHand);
                    nailOffsetData.RightHand.Copy(thisObject.RightHand);
                }
            }
            EditorGUILayout.EndHorizontal();

        }

    }


}
#endif