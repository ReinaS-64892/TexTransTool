using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using net.rs64.TexTransTool.IslandSelector;
using System;
using System.Collections.Generic;

namespace net.rs64.TexTransTool.Editor.Decal
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(SimpleDecal))]
    internal class SimpleDecalEditor : UnityEditor.Editor
    {
        CanBehaveAsLayerEditorUtil behaveLayerUtil;
        void BehaveUtilInit() { behaveLayerUtil = new(target as Component); }
        void OnEnable() { BehaveUtilInit(); EditorApplication.hierarchyChanged += BehaveUtilInit; }
        void OnDisable() { EditorApplication.hierarchyChanged -= BehaveUtilInit; }
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawOldSaveDataVersionWarning(target as TexTransMonoBase);
            var thisSObject = serializedObject;
            var thisObject = target as SimpleDecal;
            var isMultiEdit = targets.Length != 1;


            if (behaveLayerUtil.IsLayerMode is false)
            {
                EditorGUILayout.LabelField("CommonDecal:label:RenderersSettings".Glc(), EditorStyles.boldLabel);
                EditorGUI.indentLevel += 1;

                var sRendererSelector = thisSObject.FindProperty("RendererSelector");
                EditorGUILayout.PropertyField(sRendererSelector, "Common:RendererSelectMode".Glc());

                EditorGUI.indentLevel -= 1;
            }

            EditorGUILayout.LabelField("CommonDecal:label:TextureSettings".Glc(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;

            if (thisSObject.FindProperty("OverrideDecalTextureWithMultiLayerImageCanvas").objectReferenceValue == null || behaveLayerUtil.IsLayerMode)
            {
                var sDecalTexture = thisSObject.FindProperty("DecalTexture");
                EditorGUILayout.PropertyField(sDecalTexture, "CommonDecal:prop:DecalTexture".Glc());

                var sColor = thisSObject.FindProperty("Color");
                EditorGUILayout.PropertyField(sColor, "CommonDecal:prop:Color".Glc());
            }

            var sBlendType = thisSObject.FindProperty("BlendTypeKey");
            EditorGUILayout.PropertyField(sBlendType, "CommonDecal:prop:BlendTypeKey".Glc());

            var sTargetPropertyName = thisSObject.FindProperty("TargetPropertyName");
            if (behaveLayerUtil.IsLayerMode is false) EditorGUILayout.PropertyField(sTargetPropertyName, "CommonDecal:prop:TargetPropertyName".Glc());
            EditorGUI.indentLevel -= 1;


            if (!isMultiEdit)
            {
                var tf_sObg = new SerializedObject(thisObject.transform);
                var decalTexture = thisObject.DecalTexture;
                DrawerScale(thisSObject, tf_sObg, decalTexture);
                tf_sObg.ApplyModifiedProperties();
            }

            EditorGUILayout.LabelField("SimpleDecal:label:CullingSettings".Glc(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;

            var sSideCulling = thisSObject.FindProperty("BackCulling");
            EditorGUILayout.PropertyField(sSideCulling, "SimpleDecal:prop:BackCulling".Glc());

            var sIslandSelector = thisSObject.FindProperty("IslandSelector");
            EditorGUILayout.PropertyField(sIslandSelector, "SimpleDecal:prop:IslandSelector".Glc());

            EditorGUI.indentLevel -= 1;

            DecalEditorUtil.DrawerAdvancedOption(thisSObject);

            if (behaveLayerUtil.IsDrawPreviewButton) PreviewButtonDrawUtil.Draw(target as TexTransMonoBase);
            behaveLayerUtil.DrawAddLayerButton(target as Component);

            thisSObject.ApplyModifiedProperties();
        }

        public static void DrawerScale(SerializedObject thisSObject, SerializedObject tf_sObg, Texture2D decalTexture)
        {
            EditorGUILayout.LabelField("SimpleDecal:label:ScaleSettings".Glc(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;

            var sLocalScale = tf_sObg.FindProperty("m_LocalScale");
            var sFixedAspect = thisSObject.FindProperty("FixedAspect");

            var localScaleValue = sLocalScale.vector3Value;
            if (localScaleValue.x < 0 || localScaleValue.y < 0 || localScaleValue.z < 0) { EditorGUILayout.HelpBox("SimpleDecal:info:ScaleInvert".GetLocalize(), MessageType.Info); }

            TextureTransformerEditor.Filter<float> editCollBack = (value) =>
            {
                var aspectValue = 1f;
                if (decalTexture != null) { aspectValue = ((float)decalTexture.height / (float)decalTexture.width); }
                sLocalScale.FindPropertyRelative("y").floatValue = value * aspectValue;
                return value;
            };

            if (sFixedAspect.boolValue)
            {
                TextureTransformerEditor.DrawerPropertyFloat(
                    sLocalScale.FindPropertyRelative("x"),
                    "SimpleDecal:prop:Scale".Glc(),
                    editCollBack
                );
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("SimpleDecal:prop:Scale".Glc(), GUILayout.Width(60));
                EditorGUILayout.LabelField("x", GUILayout.Width(30));
                EditorGUILayout.PropertyField(sLocalScale.FindPropertyRelative("x"), GUIContent.none);
                EditorGUILayout.LabelField("y", GUILayout.Width(30));
                EditorGUILayout.PropertyField(sLocalScale.FindPropertyRelative("y"), GUIContent.none);
                EditorGUILayout.EndHorizontal();
            }

            TextureTransformerEditor.DrawerPropertyBool(sFixedAspect, "SimpleDecal:prop:FixedAspect".Glc(), (Value) => { if (Value) { editCollBack.Invoke(sLocalScale.FindPropertyRelative("x").floatValue); } return Value; });

            EditorGUILayout.PropertyField(sLocalScale.FindPropertyRelative("z"), "SimpleDecal:prop:MaxDistance".Glc());

            EditorGUI.indentLevel -= 1;
        }
    }


    internal static class DecalEditorUtil
    {
        static bool FoldoutAdvancedOption;
        public static void DrawerAdvancedOption(SerializedObject sObject)
        {
            FoldoutAdvancedOption = EditorGUILayout.Foldout(FoldoutAdvancedOption, "CommonDecal:label:AdvancedOption".Glc());
            if (FoldoutAdvancedOption)
            {
                EditorGUI.indentLevel += 1;

                var sHighQualityPadding = sObject.FindProperty("HighQualityPadding");
                EditorGUILayout.PropertyField(sHighQualityPadding, "CommonDecal:prop:HighQualityPadding".Glc());

                var sPadding = sObject.FindProperty("Padding");
                EditorGUILayout.PropertyField(sPadding, "CommonDecal:prop:Padding".Glc());

                EditorGUI.indentLevel -= 1;
            }

        }

    }
}
