using System;
using net.rs64.TexTransTool.Build;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace net.rs64.TexTransTool
{
    public class PreviewContext : ScriptableSingleton<PreviewContext>
    {
        [SerializeField]
        private Object previweing = null;

        protected PreviewContext()
        {
        }

        public static bool IsPreviewing(TextureTransformer transformer) => transformer == instance.previweing;
        public static bool IsPreviewing(AvatarDomainDefinition domainDefinition) => domainDefinition == instance.previweing;

        private void DrawApplyAndRevert<T>(T target, string previewMessage, Action<T> apply)
            where T : Object 
        {
            if (target == null) return;
            if (previweing == null)
            {
                if (GUILayout.Button(previewMessage))
                {
                    previweing = target;
                    try
                    {
                        apply(target);
                    }
                    catch
                    {
                        AnimationMode.StopAnimationMode();
                        previweing = null;
                        throw;
                    }
                }
            }
            else if (previweing == target)
            {
                if (GUILayout.Button("Revert"))
                {
                    previweing = null;
                    AnimationMode.StopAnimationMode();
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Preview (Other Previewing)");
                EditorGUI.EndDisabledGroup();
            }
        }

        public void DrawApplyAndRevert(TextureTransformer target)
        {
            DrawApplyAndRevert(target, "Preview", target1 =>
            {
                var previewDomain = new PreviewDomain(target.GetRenderers);

                try
                {
                    target1.Apply(previewDomain);
                }
                finally
                {
                    previewDomain.EditFinish();
                }
            });
        }
        
        public void DrawApplyAndRevert(AvatarDomainDefinition target)
        {
            DrawApplyAndRevert(target, "Preview - AvatarDomain-Apply", target1 =>
            {
                var previewAvatarDomain = new PreviewAvatarDomain(target.Avatar);
                try
                {
                    target.Apply(previewAvatarDomain);
                }
                finally
                {
                    previewAvatarDomain.EditFinish();
                }
            });
        }
    }
}