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

        private void OnEnable()
        {
            EditorApplication.update += () =>
            {
                if (!AnimationMode.InAnimationMode()) previweing = null;
            };
        }

        public static bool IsPreviewing(TextureTransformer transformer) => transformer == instance.previweing;
        public static bool IsPreviewing(AvatarDomainDefinition domainDefinition) => domainDefinition == instance.previweing;

        private void DrawApplyAndRevert<T>(T target, string previewMessage, Action<T> apply)
            where T : Object 
        {
            if (target == null) return;
            if (previweing == null && AnimationMode.InAnimationMode())
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button(previewMessage + " (Previewing Animation)");
                EditorGUI.EndDisabledGroup();
            }
            else if (previweing == null)
            {
                if (GUILayout.Button(previewMessage))
                {
                    previweing = target;
                    AnimationMode.StartAnimationMode();
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
                AnimationMode.BeginSampling();
                try
                {
                    var previewDomain = new RenderersDomain(target.GetRenderers, previewing: true);
                    target1.Apply(previewDomain);
                    previewDomain.EditFinish();
                }
                finally
                {
                    AnimationMode.EndSampling();
                }
            });
        }
        
        public void DrawApplyAndRevert(AvatarDomainDefinition target)
        {
            DrawApplyAndRevert(target, "Preview - AvatarDomain-Apply", target1 =>
            {
                AnimationMode.BeginSampling();
                try
                {
                    var previewAvatarDomain = new AvatarDomain(target.Avatar, previewing: true);
                    target.Apply(previewAvatarDomain);
                    previewAvatarDomain.EditFinish();
                }
                finally
                {
                    AnimationMode.EndSampling();
                }
            });
        }
    }
}