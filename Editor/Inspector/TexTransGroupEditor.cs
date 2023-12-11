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

            foreach (var ctf in rootTransform.GetChildren())
            {
                var tf = ctf.GetComponent<TextureTransformer>();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                if (tf == null)
                {
                    var ctfObj = new SerializedObject(ctf.gameObject);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Enabled".GetLocalize(), GUILayout.Width(50));
                    var s_ctfActive = ctfObj.FindProperty("m_IsActive");
                    EditorGUILayout.PropertyField(s_ctfActive, GUIContent.none, GUILayout.Width(EditorGUIUtility.singleLineHeight));
                    EditorGUILayout.LabelField(ctf.name);
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.BeginDisabledGroup(!s_ctfActive.boolValue);
                    DrawerSummaryList(ctf);
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.EndVertical();

                    ctfObj.ApplyModifiedProperties();
                    continue;
                }

                EditorGUILayout.BeginHorizontal();

                var sObj = new SerializedObject(tf.gameObject);
                EditorGUILayout.LabelField("Enabled".GetLocalize(), GUILayout.Width(50));
                var s_active = sObj.FindProperty("m_IsActive");
                EditorGUILayout.PropertyField(s_active, GUIContent.none, GUILayout.Width(EditorGUIUtility.singleLineHeight));
                EditorGUILayout.ObjectField(tf, typeof(TextureTransformer), true);

                sObj.ApplyModifiedProperties();
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.BeginDisabledGroup(!s_active.boolValue);

                switch (tf)
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
                    case TexTransGroup texTransGroup:
                        {
                            EditorGUILayout.LabelField("GroupChildren".GetLocalize());
                            DrawerSummaryList(ctf);
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
                if (tf is not ITTTChildExclusion)
                {
                    DrawerSummaryList(ctf);
                }

                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();
            }

        }
    }
}
#endif
