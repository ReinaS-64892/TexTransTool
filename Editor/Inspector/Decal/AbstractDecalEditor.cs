using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;

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
            EditorGUILayout.LabelField("RenderersSettings".GetLocalize(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;

            var sTargetRenderers = thisSObject.FindProperty("TargetRenderers");
            var sMultiRendererMode = thisSObject.FindProperty("MultiRendererMode");
            TextureTransformerEditor.DrawerRenderer(sTargetRenderers, sMultiRendererMode.boolValue);
            EditorGUILayout.PropertyField(sMultiRendererMode, new GUIContent("MultiRendererMode".GetLocalize()));


            EditorGUI.indentLevel -= 1;
            EditorGUILayout.LabelField("TextureSettings".GetLocalize(), EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;


            var sDecalTexture = thisSObject.FindProperty("DecalTexture");
            TextureTransformerEditor.DrawerTexture2D(sDecalTexture, "DecalTexture".GetLC());

            var sColor = thisSObject.FindProperty("Color");
            EditorGUILayout.PropertyField(sColor, new GUIContent("Color".GetLocalize()));

            var sBlendType = thisSObject.FindProperty("BlendTypeKey");
            EditorGUILayout.PropertyField(sBlendType, "BlendTypeKey".GetLC());

            var sTargetPropertyName = thisSObject.FindProperty("TargetPropertyName");
            EditorGUILayout.PropertyField(sTargetPropertyName, "TargetPropertyName".GetLC());
            EditorGUI.indentLevel -= 1;
        }
        public static void DrawerRealTimePreviewEditor(AbstractDecal target)
        {
            if (target == null) { return; }


            if (!RealTimePreviewManager.Contains(target))
            {
                bool IsPossibleRealTimePreview;
                if (RealTimePreviewManager.IsContainsRealTimePreviewDecal)
                {
                    IsPossibleRealTimePreview = target.IsPossibleApply;
                }
                else
                {
                    IsPossibleRealTimePreview = !PreviewContext.IsPreviewContains;
                    IsPossibleRealTimePreview &= !AnimationMode.InAnimationMode();
                }
                EditorGUI.BeginDisabledGroup(!IsPossibleRealTimePreview);
                if (GUILayout.Button(IsPossibleRealTimePreview ? "RealTimePreview".GetLocalize() : "(Other Previewing Or Previewing Animation)".GetLocalize()))
                {
                    RealTimePreviewManager.instance.RegtAbstractDecal(target);
                }
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(RealTimePreviewManager.instance.LastDecalUpdateTime + "ms", GUILayout.Width(40));

                if (GUILayout.Button("ExitRealTimePreview".GetLocalize()))
                {
                    RealTimePreviewManager.instance.UnRegtAbstractDecal(target);
                }
                if (RealTimePreviewManager.ContainsPreviewCount > 1 && GUILayout.Button("AllExit".GetLocalize(), GUILayout.Width(60)))
                {
                    RealTimePreviewManager.instance.ExitPreview();
                }
                EditorGUILayout.EndHorizontal();
            }

        }


        static bool FoldoutAdvancedOption;
        public static void DrawerAdvancedOption(SerializedObject sObject)
        {
            FoldoutAdvancedOption = EditorGUILayout.Foldout(FoldoutAdvancedOption, "AdvancedOption".GetLocalize());
            if (FoldoutAdvancedOption)
            {
                EditorGUI.indentLevel += 1;

                var sHighQualityPadding = sObject.FindProperty("HighQualityPadding");
                EditorGUILayout.PropertyField(sHighQualityPadding, new GUIContent("HighQualityPadding".GetLocalize()));

                var sPadding = sObject.FindProperty("Padding");
                EditorGUILayout.PropertyField(sPadding, "Padding".GetLC());

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
