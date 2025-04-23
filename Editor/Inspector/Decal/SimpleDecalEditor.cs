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
    internal class SimpleDecalEditor : TTCanBehaveAsLayerEditor
    {
        protected override void OnTexTransComponentInspectorGUI()
        {
            var thisSObject = serializedObject;
            var thisObject = target as SimpleDecal;
            var isMultiEdit = targets.Length != 1;


            if (IsLayerMode is false)
            {
                EditorGUILayout.LabelField("CommonDecal:label:RenderersSettings".Glc(), EditorStyles.boldLabel);

                using var indexScope = new EditorGUI.IndentLevelScope(1);
                var sRendererSelector = thisSObject.FindProperty(nameof(SimpleDecal.RendererSelector));
                EditorGUILayout.PropertyField(sRendererSelector, "Common:RendererSelectMode".Glc());
            }

            EditorGUILayout.LabelField("CommonDecal:label:TextureSettings".Glc(), EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope(1))
            {
                var sDecalTexture = thisSObject.FindProperty(nameof(SimpleDecal.DecalTexture));
                EditorGUILayout.PropertyField(sDecalTexture, "CommonDecal:prop:DecalTexture".Glc());

                var sColor = thisSObject.FindProperty(nameof(SimpleDecal.Color));
                EditorGUILayout.PropertyField(sColor, "CommonDecal:prop:Color".Glc());

                var sBlendType = thisSObject.FindProperty(nameof(SimpleDecal.BlendTypeKey));
                EditorGUILayout.PropertyField(sBlendType, "CommonDecal:prop:BlendTypeKey".Glc());

                var sTargetPropertyName = thisSObject.FindProperty(nameof(SimpleDecal.TargetPropertyName));
                if (IsLayerMode is false) EditorGUILayout.PropertyField(sTargetPropertyName, "CommonDecal:prop:TargetPropertyName".Glc());
            }


            if (!isMultiEdit)
            {
                var tf_sObg = new SerializedObject(thisObject.transform);
                var decalTexture = thisObject.DecalTexture;
                DrawerScale(thisSObject, tf_sObg, decalTexture);
                tf_sObg.ApplyModifiedProperties();
            }

            EditorGUILayout.LabelField("SimpleDecal:label:CullingSettings".Glc(), EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope(1))
            {
                var sSideCulling = thisSObject.FindProperty(nameof(SimpleDecal.BackCulling));
                EditorGUILayout.PropertyField(sSideCulling, "SimpleDecal:prop:BackCulling".Glc());

                var sIslandSelector = thisSObject.FindProperty(nameof(SimpleDecal.IslandSelector));
                EditorGUILayout.PropertyField(sIslandSelector, "SimpleDecal:prop:IslandSelector".Glc());
            }

            DecalEditorUtil.DrawerAdvancedOption(thisSObject);
        }

        public static void DrawerScale(SerializedObject thisSObject, SerializedObject tf_sObg, Texture2D decalTexture)
        {
            EditorGUILayout.LabelField("SimpleDecal:label:ScaleSettings".Glc(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;

            var sLocalScale = tf_sObg.FindProperty("m_LocalScale");
            var sFixedAspect = thisSObject.FindProperty(nameof(SimpleDecal.FixedAspect));

            var localScaleValue = sLocalScale.vector3Value;
            if (localScaleValue.x < 0 || localScaleValue.y < 0 || localScaleValue.z < 0) { EditorGUILayout.HelpBox("SimpleDecal:info:ScaleInvert".GetLocalize(), MessageType.Info); }

            DecalEditorUtil.Filter<float> editCollBack = (value) =>
            {
                var aspectValue = 1f;
                if (decalTexture != null) { aspectValue = ((float)decalTexture.height / (float)decalTexture.width); }
                sLocalScale.FindPropertyRelative("y").floatValue = value * aspectValue;
                return value;
            };

            if (sFixedAspect.boolValue)
            {
                DecalEditorUtil.DrawerPropertyFloat(
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

            DecalEditorUtil.DrawerPropertyBool(sFixedAspect, "SimpleDecal:prop:FixedAspect".Glc(), (Value) => { if (Value) { editCollBack.Invoke(sLocalScale.FindPropertyRelative("x").floatValue); } return Value; });

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
            using (new EditorGUI.IndentLevelScope(1))
                if (FoldoutAdvancedOption)
                {
                    var sPadding = sObject.FindProperty("Padding");
                    EditorGUILayout.PropertyField(sPadding, "CommonDecal:prop:Padding".Glc());
                }

        }

        #region DrawerProperty

        public delegate T Filter<T>(T Target);
        public static void DrawerPropertyBool(SerializedProperty prop, GUIContent gUIContent = null, Filter<bool> editAndFilterCollBack = null)
        {
            var preValue = prop.boolValue;
            EditorGUILayout.PropertyField(prop, gUIContent != null ? gUIContent : new GUIContent(prop.displayName));
            var postValue = prop.boolValue;
            if (editAndFilterCollBack != null && preValue != postValue) { prop.boolValue = editAndFilterCollBack.Invoke(postValue); }
        }
        public static void DrawerPropertyFloat(SerializedProperty prop, GUIContent gUIContent = null, Filter<float> editAndFilterCollBack = null)
        {
            var preValue = prop.floatValue;
            EditorGUILayout.PropertyField(prop, gUIContent != null ? gUIContent : new GUIContent(prop.displayName));
            var postValue = prop.floatValue;
            if (editAndFilterCollBack != null && !Mathf.Approximately(preValue, postValue)) { prop.floatValue = editAndFilterCollBack.Invoke(postValue); }
        }
        #endregion
    }
}
