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
            TextureTransformerEditor.DrawerWarning("NailEditor");
            var This_S_Object = serializedObject;
            var ThisObject = target as NailEditor;

            var S_TargetAvatar = This_S_Object.FindProperty("TargetAvatar");
            EditorGUILayout.PropertyField(S_TargetAvatar, S_TargetAvatar.name.GetLC());

            var S_TargetRenderers = This_S_Object.FindProperty("TargetRenderers");
            var S_MultiRendererMode = This_S_Object.FindProperty("MultiRendererMode");
            TextureTransformerEditor.DrawerRenderer(S_TargetRenderers, S_MultiRendererMode.boolValue);
            EditorGUILayout.PropertyField(S_MultiRendererMode, S_MultiRendererMode.name.GetLC());

            var S_BlendType = This_S_Object.FindProperty("BlendType");
            EditorGUILayout.PropertyField(S_BlendType, S_BlendType.name.GetLC());

            var S_TargetPropertyName = This_S_Object.FindProperty("TargetPropertyName");
            PropertyNameEditor.DrawInspectorGUI(S_TargetPropertyName);

            var S_UseTextureAspect = This_S_Object.FindProperty("UseTextureAspect");
            EditorGUILayout.PropertyField(S_UseTextureAspect, S_UseTextureAspect.name.GetLC());


            var S_LeftHand = This_S_Object.FindProperty("LeftHand");
            var S_RightHand = This_S_Object.FindProperty("RightHand");
            EditorGUILayout.LabelField("LeftHand".GetLocalize());
            DrawerNailSet(S_LeftHand);
            EditorGUILayout.LabelField("RightHand".GetLocalize());
            DrawerNailSet(S_RightHand);


            DrawerOffsetUtilEditor(ThisObject);
            DrawOffsetSaveAndLoader(ThisObject);


            PreviewContext.instance.DrawApplyAndRevert(ThisObject);

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

            EditorGUILayout.PropertyField(S_FingerUpVector, S_FingerUpVector.name.GetLC());

            EditorGUILayout.LabelField("Thumb".GetLocalize());
            DrawerNailDescriptor(S_Thumb);
            EditorGUILayout.LabelField("Index".GetLocalize());
            DrawerNailDescriptor(S_Index);
            EditorGUILayout.LabelField("Middle".GetLocalize());
            DrawerNailDescriptor(S_Middle);
            EditorGUILayout.LabelField("Ring".GetLocalize());
            DrawerNailDescriptor(S_Ring);
            EditorGUILayout.LabelField("Little".GetLocalize());
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

            EditorGUILayout.PropertyField(S_DecalTexture, S_DecalTexture.name.GetLC());
            DrawerPositionOffset(S_PositionOffset);
            EditorGUILayout.PropertyField(S_ScaleOffset, S_ScaleOffset.name.GetLC());
            EditorGUILayout.PropertyField(S_RotationOffset, S_RotationOffset.name.GetLC());

            EditorGUI.indentLevel -= 1;
        }
        public static void DrawerPositionOffset(SerializedProperty serializedProperty)
        {
            var DrawValue = serializedProperty.vector3Value * 100;
            serializedProperty.vector3Value = EditorGUI.Vector3Field(EditorGUILayout.GetControlRect(), "PositionOffset".GetLocalize(), DrawValue) * 0.01f;

        }

        public static void DrawerOffsetUtilEditor(NailEditor nailEditor)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Copy".GetLocalize(), GUILayout.Width(100));

            if (GUILayout.Button("LeftHand".GetLocalize() + "<=" + "RightHand".GetLocalize()))
            {
                Undo.RecordObject(nailEditor, "NailEditor Offset Copy Left <= Right");
                var nailOffsets = new NailOffSets();
                nailOffsets.Copy(nailEditor.RightHand);
                nailOffsets.UpVector = nailEditor.LeftHand.FingerUpVector;
                nailEditor.LeftHand.Copy(nailOffsets);
            }

            if (GUILayout.Button("LeftHand".GetLocalize() + "=>" + "RightHand".GetLocalize()))
            {
                Undo.RecordObject(nailEditor, "NailEditor Offset Copy Left => Right");
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
            nailOffsetData = EditorGUILayout.ObjectField("SaveData".GetLocalize(), nailOffsetData, typeof(NailOffsetData), false) as NailOffsetData;
            if (GUILayout.Button("Load".GetLocalize()))
            {
                if (nailOffsetData != null)
                {
                    thisObject.LeftHand.Copy(nailOffsetData.LeftHand);
                    thisObject.RightHand.Copy(nailOffsetData.RightHand);
                    EditorUtility.SetDirty(thisObject);
                    EditorUtility.SetDirty(nailOffsetData);
                }
            }
            if (GUILayout.Button("Save".GetLocalize()))
            {
                if (nailOffsetData != null)
                {
                    nailOffsetData.LeftHand.Copy(thisObject.LeftHand);
                    nailOffsetData.RightHand.Copy(thisObject.RightHand);
                    EditorUtility.SetDirty(thisObject);
                    EditorUtility.SetDirty(nailOffsetData);
                }
            }
            EditorGUILayout.EndHorizontal();

        }

        public static void DrawerSummary(NailEditor target)
        {
            var s_obj = new SerializedObject(target);
            var s_TargetAvatar = s_obj.FindProperty("TargetAvatar");
            EditorGUILayout.PropertyField(s_TargetAvatar, s_TargetAvatar.name.GetLC());
            var s_TargetRenderers = s_obj.FindProperty("TargetRenderers");
            TextureTransformerEditor.DrawerTargetRenderersSummary(s_TargetRenderers);

            s_obj.ApplyModifiedProperties();
        }

    }


}
#endif
