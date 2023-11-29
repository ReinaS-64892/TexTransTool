#if UNITY_EDITOR
using System;
using net.rs64.TexTransTool.Build;
using UnityEditor;
using UnityEditor.SceneManagement;
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
            AssemblyReloadEvents.beforeAssemblyReload -= ExitPreview;
            AssemblyReloadEvents.beforeAssemblyReload += ExitPreview;
            EditorSceneManager.sceneClosing -= ExitPreview;
            EditorSceneManager.sceneClosing += ExitPreview;
        }

        private void OnEnable()
        {
            EditorApplication.update += () =>
            {
                if (!AnimationMode.InAnimationMode()) previweing = null;
            };
        }

        public static bool IsPreviewing(TextureTransformer transformer) => transformer == instance.previweing;
        public static bool IsPreviewContains => instance.previweing != null;

        private void DrawApplyAndRevert<T>(T target, string previewMessage, Action<T> apply)
            where T : Object
        {
            if (target == null) return;
            if (previweing == null && AnimationMode.InAnimationMode())
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("(Other Previewing Or Previewing Animation)".GetLocalize());
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
                        EditorUtility.ClearProgressBar();
                        previweing = null;
                        throw;
                    }
                }
            }
            else if (previweing == target)
            {
                if (GUILayout.Button("Revert".GetLocalize()))
                {
                    ExitPreview();
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("(Other Previewing)".GetLocalize());
                EditorGUI.EndDisabledGroup();
            }
        }

        public void ExitPreview()
        {
            if (previweing == null) { return; }
            previweing = null;
            AnimationMode.StopAnimationMode();
        }
        public void ExitPreview(UnityEngine.SceneManagement.Scene scene, bool removingScene)
        {
            ExitPreview();
        }
        public void DrawApplyAndRevert(TextureTransformer target)
        {
            DrawApplyAndRevert(target, "Preview".GetLocalize(), target1 =>
            {
                AnimationMode.BeginSampling();
                try
                {
                    RenderersDomain renderersDomain = null;
                    var marker = DomainMarkerFinder.FindMarker(target1.gameObject);
                    if (marker != null) { renderersDomain = new AvatarDomain(marker, true, null, new ProgressHandler(), false, TTTConfig.UseImmediateTextureStack); }
                    else { renderersDomain = new RenderersDomain(target.GetRenderers, true, null, new ProgressHandler(), TTTConfig.UseImmediateTextureStack); }
                    target1.Apply(renderersDomain);
                    renderersDomain.EditFinish();
                }
                finally
                {
                    AnimationMode.EndSampling();
                }
            });
        }

    }
}
#endif