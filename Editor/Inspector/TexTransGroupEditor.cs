#if UNITY_EDITOR
using System.Net.Mime;
using UnityEngine;
using UnityEditor;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.Editor.Decal;
using net.rs64.TexTransTool.MatAndTexUtils;
using net.rs64.TexTransTool.Editor.MatAndTexUtils;
using net.rs64.TexTransTool.TextureAtlas;
using net.rs64.TexTransTool.TextureAtlas.Editor;
using net.rs64.TexTransCore.Decal;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Editor
{
    [CustomEditor(typeof(TexTransGroup), true)]
    internal class TexTransGroupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var thisTarget = target as TexTransGroup;

            PreviewContext.instance.DrawApplyAndRevert(thisTarget);

            if (!PreviewContext.IsPreviewContains)
            {
                DrawerSummaryList(thisTarget.transform);
            }
            else
            {
                EditorGUILayout.LabelField("Summary display during preview is not supported.".GetLocalize());
            }
        }

        private static void DrawerSummaryList(Transform rootTransform)
        {

            foreach (var childeTransform in rootTransform.GetChildren())
            {
                var textureTransformer = childeTransform.GetComponent<TexTransBehavior>();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                if (textureTransformer == null)
                {
                    var sChildeGameObject = new SerializedObject(childeTransform.gameObject);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Enabled".GetLocalize(), GUILayout.Width(50));
                    var sChildeIsActive = sChildeGameObject.FindProperty("m_IsActive");
                    EditorGUILayout.PropertyField(sChildeIsActive, GUIContent.none, GUILayout.Width(EditorGUIUtility.singleLineHeight));
                    EditorGUILayout.LabelField(childeTransform.name);
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.BeginDisabledGroup(!sChildeIsActive.boolValue);
                    DrawerSummaryList(childeTransform);
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.EndVertical();

                    sChildeGameObject.ApplyModifiedProperties();
                    continue;
                }

                EditorGUILayout.BeginHorizontal();

                var sObj = new SerializedObject(textureTransformer.gameObject);
                EditorGUILayout.LabelField("Enabled".GetLocalize(), GUILayout.Width(50));
                var sActive = sObj.FindProperty("m_IsActive");
                EditorGUILayout.PropertyField(sActive, GUIContent.none, GUILayout.Width(EditorGUIUtility.singleLineHeight));
                EditorGUILayout.ObjectField(textureTransformer, typeof(TexTransBehavior), true);

                sObj.ApplyModifiedProperties();
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.BeginDisabledGroup(!sActive.boolValue);

                switch (textureTransformer)
                {
                    case SimpleDecal simpleDecal:
                        {
                            SimpleDecalEditor.DrawerSummary(simpleDecal);
                            break;
                        }
                    case AtlasTexture atlasTexture:
                        {
                            AtlasTextureEditor.DrawerSummary(atlasTexture);
                            break;
                        }
                    case TextureBlender textureBlender:
                        {
                            TextureBlenderEditor.DrawerSummary(textureBlender);
                            break;
                        }
                    case NailEditor nailEditor:
                        {
                            NailEditorEditor.DrawerSummary(nailEditor);
                            break;
                        }
                    case TexTransGroup:
                        {
                            EditorGUILayout.LabelField("GroupChildren".GetLocalize());
                            break;
                        }
                    case MatAndTexAbsoluteSeparator matAndTexAbsoluteSeparator:
                        {
                            MatAndTexAbsoluteSeparatorEditor.DrawerSummary(matAndTexAbsoluteSeparator);
                            break;
                        }
                    case MatAndTexRelativeSeparator matAndTexRelativeSeparator:
                        {
                            MatAndTexRelativeSeparatorEditor.DrawerSummary(matAndTexRelativeSeparator);
                            break;
                        }
                    default:
                        {
                            EditorGUILayout.LabelField("SummaryNone".GetLocalize());
                            break;
                        }
                }
                if (textureTransformer is not ITTTChildExclusion)
                {
                    DrawerSummaryList(childeTransform);
                }

                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();
            }

        }
    }
}
#endif
