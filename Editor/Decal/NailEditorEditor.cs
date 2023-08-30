#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.Editor;
using System;

namespace net.rs64.TexTransTool.Editor.Decal
{

    [CustomEditor(typeof(NailEditor), true)]
    public class NailEditorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var This_S_Object = serializedObject;
            var ThisObject = target as NailEditor;

            var S_TargetRenderers = This_S_Object.FindProperty("TargetRenderers");
            var S_MultiRendererMode = This_S_Object.FindProperty("MultiRendererMode");
            TextureTransformerEditor.DrawerRenderer(S_TargetRenderers, S_MultiRendererMode.boolValue);
            EditorGUILayout.PropertyField(S_MultiRendererMode);

            var S_BlendType = This_S_Object.FindProperty("BlendType");
            EditorGUILayout.PropertyField(S_BlendType);

            var S_TargetPropertyName = This_S_Object.FindProperty("TargetPropertyName");
            PropertyNameEditor.DrawInspectorGUI(S_TargetPropertyName);

            var S_UseTextureAspect = This_S_Object.FindProperty("UseTextureAspect");
            EditorGUILayout.PropertyField(S_UseTextureAspect);


            var S_LeftHand = This_S_Object.FindProperty("LeftHand");
            var S_RightHand = This_S_Object.FindProperty("RightHand");
            EditorGUILayout.LabelField("LeftHand");
            DrawerNailSet(S_LeftHand);
            EditorGUILayout.LabelField("RightHand");
            DrawerNailSet(S_RightHand);


            DrawerOffsetUtilEditor(ThisObject);
            DrawOffsetSaveAndLoader(ThisObject);


            TextureTransformerEditor.DrawerApplyAndRevert(ThisObject);

            This_S_Object.ApplyModifiedProperties();
        }

        public static void DrawerNailSet(SerializedProperty serializedProperty)
        {
            var S_FingerUpVector = serializedProperty.FindPropertyRelative("FingerUpVector");
            var S_Thumb = serializedProperty.FindPropertyRelative("Thumb");
            var S_Index = serializedProperty.FindPropertyRelative("Index");
            var S_Middle = serializedProperty.FindPropertyRelative("Middle");
            var S_Ring = serializedProperty.FindPropertyRelative("Ring");
            var S_Little = serializedProperty.FindPropertyRelative("Little");

            EditorGUI.indentLevel += 1;

            EditorGUILayout.PropertyField(S_FingerUpVector);

            EditorGUILayout.LabelField("Thumb");
            DrawerNailDescriptor(S_Thumb);
            EditorGUILayout.LabelField("Index");
            DrawerNailDescriptor(S_Index);
            EditorGUILayout.LabelField("Middle");
            DrawerNailDescriptor(S_Middle);
            EditorGUILayout.LabelField("Ring");
            DrawerNailDescriptor(S_Ring);
            EditorGUILayout.LabelField("Little");
            DrawerNailDescriptor(S_Little);

            EditorGUI.indentLevel -= 1;
        }
        public static void DrawerNailDescriptor(SerializedProperty serializedProperty)
        {
            var S_DecalTexture = serializedProperty.FindPropertyRelative("DecalTexture");
            var S_PositionOffset = serializedProperty.FindPropertyRelative("PositionOffset");
            var S_ScaleOffset = serializedProperty.FindPropertyRelative("ScaleOffset");
            var S_RotationOffset = serializedProperty.FindPropertyRelative("RotationOffset");

            EditorGUI.indentLevel += 1;

            EditorGUILayout.PropertyField(S_DecalTexture);
            DrawerPositionOffset(S_PositionOffset);
            EditorGUILayout.PropertyField(S_ScaleOffset);
            EditorGUILayout.PropertyField(S_RotationOffset);

            EditorGUI.indentLevel -= 1;
        }
        public static void DrawerPositionOffset(SerializedProperty serializedProperty)
        {
            var DrawValue = serializedProperty.vector3Value * 100;
            serializedProperty.vector3Value = EditorGUI.Vector3Field(EditorGUILayout.GetControlRect(), "PositionOffset", DrawValue) * 0.01f;

        }

        public static void DrawerOffsetUtilEditor(NailEditor nailEditor)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Copy", GUILayout.Width(100));

            if (GUILayout.Button("Left <= Right "))
            {
                Undo.RecordObject(nailEditor,"NailEditor Offset Copy Left <= Right");
                var nailOffsets = new NailOffSets();
                nailOffsets.Copy(nailEditor.RightHand);
                nailOffsets.UpVector = nailEditor.LeftHand.FingerUpVector;
                nailEditor.LeftHand.Copy(nailOffsets);
            }

            if (GUILayout.Button("Left => Right"))
            {
                Undo.RecordObject(nailEditor,"NailEditor Offset Copy Left => Right");
                var nailOffsets = new NailOffSets();
                nailOffsets.Copy(nailEditor.LeftHand);
                nailOffsets.UpVector = nailEditor.RightHand.FingerUpVector;
                nailEditor.RightHand.Copy(nailOffsets);
            }

            EditorGUILayout.EndHorizontal();

        }

        public NailOffsetData nailOffsetData;
        public void DrawOffsetSaveAndLoader(NailEditor thisObject)
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
