#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransCore.Decal;
using System;

namespace net.rs64.TexTransTool.Editor.Decal
{
    public class AbstractDecalEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var This_S_Object = serializedObject;
            DrawerDecalEditor(This_S_Object);

            This_S_Object.ApplyModifiedProperties();
        }

        public static void DrawerDecalEditor(SerializedObject This_S_Object)
        {
            EditorGUILayout.LabelField("RenderersSettings".GetLocalize(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;

            var s_TargetRenderers = This_S_Object.FindProperty("TargetRenderers");
            var s_MultiRendererMode = This_S_Object.FindProperty("MultiRendererMode");
            TextureTransformerEditor.DrawerRenderer(s_TargetRenderers, s_MultiRendererMode.boolValue);
            EditorGUILayout.PropertyField(s_MultiRendererMode, new GUIContent("MultiRendererMode".GetLocalize()));


            EditorGUI.indentLevel -= 1;
            EditorGUILayout.LabelField("TextureSettings".GetLocalize(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;


            var s_DecalTexture = This_S_Object.FindProperty("DecalTexture");
            TextureTransformerEditor.DrawerTexture2D(s_DecalTexture, "DecalTexture".GetLC());

            var s_Color = This_S_Object.FindProperty("Color");
            EditorGUILayout.PropertyField(s_Color, new GUIContent("Color".GetLocalize()));

            var s_BlendType = This_S_Object.FindProperty("BlendType");
            EditorGUILayout.PropertyField(s_BlendType, new GUIContent("BlendType".GetLocalize()));

            var s_TargetPropertyName = This_S_Object.FindProperty("TargetPropertyName");
            PropertyNameEditor.DrawInspectorGUI(s_TargetPropertyName, "TargetPropertyName".GetLocalize());
            EditorGUI.indentLevel -= 1;
        }
        public static void DrawerRealTimePreviewEditor(AbstractDecal Target)
        {
            if (Target == null) return;
            {
                if (!RealTimePreviewManager.instance.RealTimePreviews.ContainsKey(Target))
                {
                    var IsPossibleRealTimePreview = false;
                    if (RealTimePreviewManager.IsContainsRealTimePreview)
                    {
                        IsPossibleRealTimePreview = Target.IsPossibleApply;
                    }
                    else
                    {
                        IsPossibleRealTimePreview = !PreviewContext.IsPreviewContains;
                        IsPossibleRealTimePreview &= !AnimationMode.InAnimationMode();
                    }
                    EditorGUI.BeginDisabledGroup(!IsPossibleRealTimePreview);
                    if (GUILayout.Button(IsPossibleRealTimePreview ? "RealTimePreview".GetLocalize() : "(Other Previewing Or Previewing Animation)".GetLocalize()))
                    {
                        RealTimePreviewManager.instance.RegtAbstractDecal(Target);
                    }
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    if (GUILayout.Button("ExitRealTimePreview".GetLocalize()))
                    {
                        RealTimePreviewManager.instance.UnRegtAbstractDecal(Target);
                    }
                    else
                    {
                        Target.ThisIsForces = true;
                    }
                }
            }
        }


        static bool FoldoutAdvancedOption;
        public static void DrawerAdvancedOption(SerializedObject S_Object)
        {
            FoldoutAdvancedOption = EditorGUILayout.Foldout(FoldoutAdvancedOption, "AdvancedOption".GetLocalize());
            if (FoldoutAdvancedOption)
            {
                EditorGUI.indentLevel += 1;

                var s_HighQualityPadding = S_Object.FindProperty("HighQualityPadding");
                EditorGUILayout.PropertyField(s_HighQualityPadding, new GUIContent("HighQualityPadding".GetLocalize()));

                var s_Padding = S_Object.FindProperty("Padding");
                EditorGUILayout.PropertyField(s_Padding, "Padding".GetLC());

                EditorGUI.indentLevel -= 1;
            }

        }

        public static void DrawerDecalGrabEditor(AbstractDecal thisObject)
        {
            if (thisObject == null) return;
            {
                if (DecalGrabManager.instance.NowGrabDecal == null)
                {
                    if (GUILayout.Button("GrabDecal".GetLocalize()))
                    {
                        DecalGrabManager.instance.Grab(thisObject);
                    }

                }
                else if (DecalGrabManager.instance.NowGrabDecal == thisObject)
                {
                    if (GUILayout.Button("DropDecal".GetLocalize()))
                    {
                        DecalGrabManager.instance.Drop(thisObject);
                    }
                }
            }
        }
    }
}
#endif
