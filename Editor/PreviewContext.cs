using net.rs64.TexTransTool.Build;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    public class PreviewContext : ScriptableSingleton<PreviewContext>
    {
        [SerializeField]
        private Object previweing = null;
        [SerializeField]
        private PreviewDomain previewDomain;
        [SerializeField]
        private PreviewAvatarDomain previewAvatarDomain;

        protected PreviewContext()
        {
        }

        public static bool IsPreviewing(TextureTransformer transformer) => transformer == instance.previweing;
        public static bool IsPreviewing(AvatarDomainDefinition domainDefinition) => domainDefinition == instance.previweing;

        public void DrawApplyAndRevert(TextureTransformer target)
        {
            if (target == null) return;
            if (previweing == null)
            {
                if (GUILayout.Button("Preview"))
                {
                    previweing = target;
                    previewDomain = new PreviewDomain(target.GetRenderers);
                    try
                    {
                        try
                        {
                            target.Apply(previewDomain);
                        }
                        finally
                        {
                            previewDomain.EditFinish();
                        }
                    }
                    catch
                    {
                        previewDomain.Dispose();
                        throw;
                    }
                }
            }
            else if (previweing == target)
            {
                if (GUILayout.Button("Revert"))
                {
                    previweing = null;
                    previewDomain.Dispose();
                    previewDomain = null;
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Preview (Other Previewing)");
                EditorGUI.EndDisabledGroup();
            }
        }
        
        public void DrawApplyAndRevert(AvatarDomainDefinition target)
        {
            if (target == null) return;
            if (previweing == null)
            {
                if (GUILayout.Button("Preview - AvatarDomain-Apply"))
                {
                    previweing = target;
                    previewAvatarDomain = new PreviewAvatarDomain(target.Avatar);
                    try
                    {
                        try
                        {
                            target.Apply(previewAvatarDomain);
                        }
                        finally
                        {
                            previewAvatarDomain.EditFinish();
                        }
                    }
                    catch
                    {
                        previewAvatarDomain.Dispose();
                        throw;
                    }
                }
            }
            else if (previweing == target)
            {
                if (GUILayout.Button("Revert"))
                {
                    previweing = null;
                    previewAvatarDomain.Dispose();
                    previewAvatarDomain = null;
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Preview (Other Previewing)");
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}