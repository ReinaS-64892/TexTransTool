#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransCore.Decal;

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
            EditorGUILayout.LabelField("RenderersSettings", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            var s_TargetRenderers = This_S_Object.FindProperty("TargetRenderers");
            var s_MultiRendererMode = This_S_Object.FindProperty("MultiRendererMode");
            TextureTransformerEditor.DrawerRenderer(s_TargetRenderers, s_MultiRendererMode.boolValue);
            EditorGUILayout.PropertyField(s_MultiRendererMode);

            EditorGUI.indentLevel -= 1;
            EditorGUILayout.LabelField("TextureSettings", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;


            var s_DecalTexture = This_S_Object.FindProperty("DecalTexture");
            TextureTransformerEditor.DrawerObjectReference<Texture2D>(s_DecalTexture);

            var s_Color = This_S_Object.FindProperty("Color");
            EditorGUILayout.PropertyField(s_Color);

            var s_BlendType = This_S_Object.FindProperty("BlendType");
            EditorGUILayout.PropertyField(s_BlendType);

            var s_TargetPropertyName = This_S_Object.FindProperty("TargetPropertyName");
            PropertyNameEditor.DrawInspectorGUI(s_TargetPropertyName);
            EditorGUI.indentLevel -= 1;
        }
        public static void DrawerRealTimePreviewEditor(AbstractDecal Target)
        {
            if (Target == null) return;
            {
                if (!RealTimePreviewManager.instance.RealTimePreviews.ContainsKey(Target))
                {
                    EditorGUI.BeginDisabledGroup(!Target.IsPossibleApply || AnimationMode.InAnimationMode() || PreviewContext.IsPreviewing(Target));
                    var IsOtherPreview = AnimationMode.InAnimationMode() || PreviewContext.IsPreviewing(Target);
                    if (GUILayout.Button(!IsOtherPreview ? "EnableRealTimePreview" : "(Other Previewing Or Previewing Animation)"))
                    {
                        RealTimePreviewManager.instance.RegtAbstractDecal(Target);
                    }
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    if (GUILayout.Button("DisableRealTimePreview"))
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
        public static void DrawerScaleEditor<T>(AbstractSingleDecal<T> ThisObject, SerializedObject This_S_Object, SerializedProperty S_Scale, SerializedProperty S_FixedAspect) where T : DecalUtility.IConvertSpace
        {
            if (S_FixedAspect.boolValue)
            {
                TextureTransformerEditor.DrawerProperty(S_Scale.displayName, S_Scale.vector2Value.x, EditValue =>
                {
                    S_Scale.vector2Value = new Vector2(EditValue, EditValue);
                    This_S_Object.ApplyModifiedProperties();
                    Undo.RecordObject(ThisObject, "ScaleApply - ScaleEdit");
                    ThisObject.ScaleApply();
                }
                );
            }
            else
            {
                TextureTransformerEditor.DrawerProperty(S_Scale, (Vector2 EditValue) =>
                {
                    S_Scale.vector2Value = EditValue;
                    This_S_Object.ApplyModifiedProperties();
                    Undo.RecordObject(ThisObject, "ScaleApply - ScaleEdit");
                    ThisObject.ScaleApply();
                });
            }
            TextureTransformerEditor.DrawerProperty(S_FixedAspect, (bool FixedAspectValue) =>
            {
                S_FixedAspect.boolValue = FixedAspectValue;
                This_S_Object.ApplyModifiedProperties();
                Undo.RecordObject(ThisObject, "ApplyScale - Size");
                ThisObject.ScaleApply();
            });
        }


    }
}
#endif
