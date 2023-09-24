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
                    ExitPreview();
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Preview (Other Previewing)");
                EditorGUI.EndDisabledGroup();
            }
        }

        private void ExitPreview()
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
            DrawApplyAndRevert(target, "Preview", target1 =>
            {
                AnimationMode.BeginSampling();
                try
                {
                    var previewDomain = new RenderersDomain(target.GetRenderers, previewing: true, progressHandler: new ProgressHandler());
                    target1.Apply(previewDomain);
                    previewDomain.EditFinish();
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