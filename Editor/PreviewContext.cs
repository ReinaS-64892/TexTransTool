using System;
using net.rs64.TexTransTool.Build;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace net.rs64.TexTransTool
{
    internal class PreviewContext : ScriptableSingleton<PreviewContext>
    {
        [SerializeField]
        private Object previweing = null;

        protected PreviewContext()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= ExitPreview;
            AssemblyReloadEvents.beforeAssemblyReload += ExitPreview;
            EditorSceneManager.sceneClosing -= ExitPreview;
            EditorSceneManager.sceneClosing += ExitPreview;
            DestroyCall.OnDestroy -= DestroyObserve;
            DestroyCall.OnDestroy += DestroyObserve;
        }

        private void OnEnable()
        {
            EditorApplication.update += () =>
            {
                if (!AnimationMode.InAnimationMode()) previweing = null;
            };
        }

        public static bool IsPreviewing(TexTransBehavior transformer) => transformer == instance.previweing;
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
                    StartPreview(target, apply);
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
                if (GUILayout.Button("Other Previewing. Override Preview this".GetLocalize()))
                {
                    ExitPreview();
                    StartPreview(target, apply);
                }
            }

            void StartPreview(T target, Action<T> apply)
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
        public void DrawApplyAndRevert(TexTransBehavior target)
        {
            DrawApplyAndRevert(target, "Preview".GetLocalize(), targetTTBehavior =>
            {
                AnimationMode.BeginSampling();
                try
                {
                    RenderersDomain previewDomain = null;
                    var marker = DomainMarkerFinder.FindMarker(targetTTBehavior.gameObject);
                    if (marker != null) { previewDomain = new AvatarDomain(marker, true, false, true); }
                    else { previewDomain = new RenderersDomain(target.GetRenderers, true, false, true); }

                    if (targetTTBehavior is TexTransGroup abstractTexTransGroup)
                    {
                        var phaseOnTf = AvatarBuildUtils.FindAtPhase(abstractTexTransGroup.gameObject);
                        foreach (var tf in phaseOnTf[TexTransPhase.BeforeUVModification]) { tf.Apply(previewDomain); }
                        previewDomain.MergeStack();
                        foreach (var tf in phaseOnTf[TexTransPhase.UVModification]) { tf.Apply(previewDomain); }
                        foreach (var tf in phaseOnTf[TexTransPhase.AfterUVModification]) { tf.Apply(previewDomain); }
                        foreach (var tf in phaseOnTf[TexTransPhase.UnDefined]) { tf.Apply(previewDomain); }
                    }
                    else
                    {
                        targetTTBehavior.Apply(previewDomain);
                    }

                    previewDomain.EditFinish();
                }
                finally
                {
                    AnimationMode.EndSampling();
                }
            });
        }

        public void DestroyObserve(TexTransBehavior texTransBehavior)
        {
            if (IsPreviewing(texTransBehavior)) { instance.ExitPreview(); }
        }

    }
}
