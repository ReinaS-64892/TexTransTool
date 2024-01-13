using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;

namespace net.rs64.TexTransTool.Editor.Decal
{

    [CustomEditor(typeof(NailEditor), true)]
    internal class NailEditorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawerWarning("NailEditor");
            var thisSObject = serializedObject;
            var thisObject = target as NailEditor;

            var sTargetAvatar = thisSObject.FindProperty("TargetAvatar");
            EditorGUILayout.PropertyField(sTargetAvatar, sTargetAvatar.name.GetLC());

            var sTargetRenderers = thisSObject.FindProperty("TargetRenderers");
            var sMultiRendererMode = thisSObject.FindProperty("MultiRendererMode");
            TextureTransformerEditor.DrawerRenderer(sTargetRenderers, sMultiRendererMode.boolValue);
            EditorGUILayout.PropertyField(sMultiRendererMode, sMultiRendererMode.name.GetLC());

            var sBlendType = thisSObject.FindProperty("BlendType");
            EditorGUILayout.PropertyField(sBlendType, sBlendType.name.GetLC());

            var sTargetPropertyName = thisSObject.FindProperty("TargetPropertyName");
            PropertyNameEditor.DrawInspectorGUI(sTargetPropertyName);

            var sUseTextureAspect = thisSObject.FindProperty("UseTextureAspect");
            EditorGUILayout.PropertyField(sUseTextureAspect, sUseTextureAspect.name.GetLC());


            var sLeftHand = thisSObject.FindProperty("LeftHand");
            var sRightHand = thisSObject.FindProperty("RightHand");
            EditorGUILayout.LabelField("LeftHand".GetLocalize());
            DrawerNailSet(sLeftHand);
            EditorGUILayout.LabelField("RightHand".GetLocalize());
            DrawerNailSet(sRightHand);


            DrawerOffsetUtilEditor(thisObject);
            DrawOffsetSaveAndLoader(thisObject);

            AbstractDecalEditor.DrawerAdvancedOption(thisSObject);

            PreviewContext.instance.DrawApplyAndRevert(thisObject);

            thisSObject.ApplyModifiedProperties();
        }

        public static void DrawerNailSet(SerializedProperty serializedProperty)
        {
            var sFingerUpVector = serializedProperty.FindPropertyRelative("FingerUpVector");
            var sThumb = serializedProperty.FindPropertyRelative("Thumb");
            var sIndex = serializedProperty.FindPropertyRelative("Index");
            var sMiddle = serializedProperty.FindPropertyRelative("Middle");
            var sRing = serializedProperty.FindPropertyRelative("Ring");
            var sLittle = serializedProperty.FindPropertyRelative("Little");

            EditorGUI.indentLevel += 1;

            EditorGUILayout.PropertyField(sFingerUpVector, sFingerUpVector.name.GetLC());

            EditorGUILayout.LabelField("Thumb".GetLocalize());
            DrawerNailDescriptor(sThumb);
            EditorGUILayout.LabelField("Index".GetLocalize());
            DrawerNailDescriptor(sIndex);
            EditorGUILayout.LabelField("Middle".GetLocalize());
            DrawerNailDescriptor(sMiddle);
            EditorGUILayout.LabelField("Ring".GetLocalize());
            DrawerNailDescriptor(sRing);
            EditorGUILayout.LabelField("Little".GetLocalize());
            DrawerNailDescriptor(sLittle);

            EditorGUI.indentLevel -= 1;
        }
        public static void DrawerNailDescriptor(SerializedProperty serializedProperty)
        {
            var sDecalTexture = serializedProperty.FindPropertyRelative("DecalTexture");
            var sPositionOffset = serializedProperty.FindPropertyRelative("PositionOffset");
            var sScaleOffset = serializedProperty.FindPropertyRelative("ScaleOffset");
            var sRotationOffset = serializedProperty.FindPropertyRelative("RotationOffset");

            EditorGUI.indentLevel += 1;

            EditorGUILayout.PropertyField(sDecalTexture, sDecalTexture.name.GetLC());
            DrawerPositionOffset(sPositionOffset);
            EditorGUILayout.PropertyField(sScaleOffset, sScaleOffset.name.GetLC());
            EditorGUILayout.PropertyField(sRotationOffset, sRotationOffset.name.GetLC());

            EditorGUI.indentLevel -= 1;
        }
        public static void DrawerPositionOffset(SerializedProperty serializedProperty)
        {
            var drawValue = serializedProperty.vector3Value * 100;
            serializedProperty.vector3Value = EditorGUI.Vector3Field(EditorGUILayout.GetControlRect(), "PositionOffset".GetLocalize(), drawValue) * 0.01f;

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
            var sObj = new SerializedObject(target);
            var sTargetAvatar = sObj.FindProperty("TargetAvatar");
            EditorGUILayout.PropertyField(sTargetAvatar, sTargetAvatar.name.GetLC());
            var sTargetRenderers = sObj.FindProperty("TargetRenderers");
            TextureTransformerEditor.DrawerTargetRenderersSummary(sTargetRenderers);

            sObj.ApplyModifiedProperties();
        }

    }


}
