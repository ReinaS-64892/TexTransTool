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

            var sPolygonCulling = thisSObject.FindProperty("PolygonOutOfCulling");
            EditorGUILayout.PropertyField(sPolygonCulling, "SimpleDecal:prop:PolygonCulling".Glc());

            var sSideCulling = thisSObject.FindProperty("BackCulling");
            EditorGUILayout.PropertyField(sSideCulling, "SimpleDecal:prop:BackCulling".Glc());


            EditorGUI.indentLevel -= 1;

            DecalEditorUtil.DrawerAdvancedOption(thisSObject);

            s_ExperimentalFutureOption = EditorGUILayout.Foldout(s_ExperimentalFutureOption, "Common:ExperimentalFuture".Glc());
            if (s_ExperimentalFutureOption)
            {
                var sIslandSelector = thisSObject.FindProperty("IslandSelector");
                EditorGUILayout.PropertyField(sIslandSelector, "SimpleDecal:prop:ExperimentalFuture:IslandSelector".Glc());

                var sOverrideDecalTextureWithMultiLayerImageCanvas = thisSObject.FindProperty("OverrideDecalTextureWithMultiLayerImageCanvas");
                if (behaveLayerUtil.IsLayerMode is false) EditorGUILayout.PropertyField(sOverrideDecalTextureWithMultiLayerImageCanvas);

                if (sIslandSelector.objectReferenceValue == null || sIslandSelector.objectReferenceValue is PinIslandSelector)
                {
                    var sIslandCulling = thisSObject.FindProperty("IslandCulling");
                    if (sIslandCulling.boolValue && GUILayout.Button("Migrate IslandCulling to  IslandSelector"))
                    {
#pragma warning disable CS0612
                        MigrateIslandCullingToIslandSelector(targets);
#pragma warning restore CS0612
                    }
                }

                var sUseDepth = thisSObject.FindProperty("UseDepth");
                var sDepthInvert = thisSObject.FindProperty("DepthInvert");
                EditorGUILayout.PropertyField(sUseDepth, "SimpleDecal:prop:ExperimentalFuture:UseDepth".Glc());
                if (sUseDepth.boolValue) { EditorGUILayout.PropertyField(sDepthInvert, "SimpleDecal:prop:ExperimentalFuture:DepthInvert".Glc()); }

            }

            if (behaveLayerUtil.IsDrawPreviewButton) PreviewButtonDrawUtil.Draw(target as TexTransMonoBase);
            behaveLayerUtil.DrawAddLayerButton(target as Component);

            thisSObject.ApplyModifiedProperties();
        }
        static bool s_ExperimentalFutureOption = false;

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


        [InitializeOnLoadMethod]
        internal static void RegisterSummary()
        {
            TexTransGroupEditor.s_summary[typeof(SimpleDecal)] = at =>
            {
                var ve = new VisualElement();
                var serializedObject = new SerializedObject(at);
                var sTargetRenderers = serializedObject.FindProperty("TargetRenderers");
                var sAtlasTextureSize = serializedObject.FindProperty("DecalTexture");

                var targetRoot = new PropertyField();
                targetRoot.label = "CommonDecal:prop:TargetRenderer".GetLocalize();
                targetRoot.BindProperty(sTargetRenderers.GetArrayElementAtIndex(0));
                ve.hierarchy.Add(targetRoot);

                var atlasTextureSize = new ObjectField();
                atlasTextureSize.label = "CommonDecal:prop:DecalTexture".GetLocalize();
                atlasTextureSize.BindProperty(sAtlasTextureSize);
                ve.hierarchy.Add(atlasTextureSize);

                return ve;
            };
        }

        [Obsolete]
        public void MigrateIslandCullingToIslandSelector(IEnumerable<UnityEngine.Object> simpleDecals)
        {
            foreach (var uo in simpleDecals)
            {
                if (uo is SimpleDecal simpleDecal)
                {
                    MigrateIslandCullingToIslandSelector(simpleDecal);
                }
            }
        }
        [Obsolete]
        public void MigrateIslandCullingToIslandSelector(SimpleDecal simpleDecal)
        {
            if (simpleDecal.IslandSelector != null)
            {
                if (simpleDecal.IslandSelector is not PinIslandSelector) { Debug.LogError("IslandSelector にすでに何かが割り当てられているため、マイグレーションを実行できません。"); return; }
                else { if (!EditorUtility.DisplayDialog("Migrate IslandCulling To IslandSelector", "IslandSelector に RayCastIslandSelector が既に割り当てられています。 \n 割り当てられている RayCastIslandSelector を編集する形でマイグレーションしますか？", "実行")) { return; } }
            }
            Undo.RecordObject(simpleDecal, "MigrateIslandCullingToIslandSelector");

            simpleDecal.IslandCulling = false;
            var islandSelector = Migration.V3.SimpleDecalV3.GenerateIslandSelector(simpleDecal);

            Undo.RecordObject(islandSelector, "MigrateIslandCullingToIslandSelector - islandSelectorEdit");

            Migration.V3.SimpleDecalV3.SetIslandSelectorTransform(simpleDecal, islandSelector);

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
