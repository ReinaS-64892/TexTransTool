using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Pool;
using net.rs64.TexTransTool.Preview.RealTime;
using net.rs64.TexTransTool.Preview;

namespace net.rs64.TexTransTool.Editor.Decal
{
    internal class AbstractDecalEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var thisSObject = serializedObject;
            DrawerDecalEditor(thisSObject);

            thisSObject.ApplyModifiedProperties();
        }

        public static void DrawerDecalEditor(SerializedObject thisSObject)
        {
            EditorGUILayout.LabelField("CommonDecal:label:RenderersSettings".Glc(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;

            var sTargetRenderers = thisSObject.FindProperty("TargetRenderers");
            var sMultiRendererMode = thisSObject.FindProperty("MultiRendererMode");
            TextureTransformerEditor.DrawerRenderer(sTargetRenderers, "CommonDecal:prop:TargetRenderer".Glc(), sMultiRendererMode.boolValue);
            EditorGUILayout.PropertyField(sMultiRendererMode, "CommonDecal:prop:MultiRendererMode".Glc());


            EditorGUI.indentLevel -= 1;
            EditorGUILayout.LabelField("CommonDecal:label:TextureSettings".Glc(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;


            var sDecalTexture = thisSObject.FindProperty("DecalTexture");
            EditorGUILayout.PropertyField(sDecalTexture, "CommonDecal:prop:DecalTexture".Glc());

            var sColor = thisSObject.FindProperty("Color");
            EditorGUILayout.PropertyField(sColor, "CommonDecal:prop:Color".Glc());

            var sBlendType = thisSObject.FindProperty("BlendTypeKey");
            EditorGUILayout.PropertyField(sBlendType, "CommonDecal:prop:BlendTypeKey".Glc());

            var sTargetPropertyName = thisSObject.FindProperty("TargetPropertyName");
            EditorGUILayout.PropertyField(sTargetPropertyName, "CommonDecal:prop:TargetPropertyName".Glc());
            EditorGUI.indentLevel -= 1;
        }
        public static void DrawerRealTimePreviewEditor(object[] target)
        {
            var list = ListPool<AbstractDecal>.Get(); list.Capacity = target.Length;
            foreach (var decal in target)
            { if (decal is AbstractDecal abstractDecal) { list.Add(abstractDecal); } }

            DrawerRealTimePreviewEditor(list);

            ListPool<AbstractDecal>.Release(list);
        }
        public static void DrawerRealTimePreviewEditor(IEnumerable<AbstractDecal> target)
        {
            if (!target.Any()) { return; }

            var rpm = RealTimePreviewContext.instance;
            if (!rpm.IsPreview())
            {
                bool IsPossibleRealTimePreview = !OneTimePreviewContext.IsPreviewContains;
                IsPossibleRealTimePreview &= !AnimationMode.InAnimationMode();
                IsPossibleRealTimePreview |= rpm.IsPreview();

                EditorGUI.BeginDisabledGroup(!IsPossibleRealTimePreview);
                if (GUILayout.Button(IsPossibleRealTimePreview ? "SimpleDecal:button:RealTimePreview".Glc() : "Common:PreviewNotAvailable".Glc()))
                {
                    OneTimePreviewContext.LastPreviewClear();

                    var domainRoot = DomainMarkerFinder.FindMarker(target.First().gameObject);
                    if (domainRoot != null)
                    {
                        rpm.EnterRealtimePreview(domainRoot);
                    }
                    else
                    {
                        Debug.Log("Domain not found");
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            else
            {

                EditorGUILayout.BeginHorizontal();
                // EditorGUILayout.LabelField(RealTimePreviewManager.instance.LastDecalUpdateTime + "ms", GUILayout.Width(40));

                if (GUILayout.Button("SimpleDecal:button:ExitRealTimePreview".Glc()))
                {
                    rpm.ExitRealTimePreview();
                }
                EditorGUILayout.EndHorizontal();
            }
        }


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
