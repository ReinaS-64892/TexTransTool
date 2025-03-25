using System;
using System.Linq;
using net.rs64.TexTransTool.Preview.Custom;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace net.rs64.TexTransTool.Preview
{
    internal class OneTimePreviewContext : ScriptableSingleton<OneTimePreviewContext>
    {
        [SerializeField]
        private Object previweing = null;
        private IDisposable previweingAssets = null;

        public event Action<Object> OnPreviewEnter;
        public event Action OnPreviewExit;
        protected OneTimePreviewContext()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= ExitPreview;
            AssemblyReloadEvents.beforeAssemblyReload += ExitPreview;
            EditorSceneManager.sceneClosing -= ExitPreview;
            EditorSceneManager.sceneClosing += ExitPreview;
            MonoBehaviourCallProvider.OnDestroy -= DestroyObserve;
            MonoBehaviourCallProvider.OnDestroy += DestroyObserve;
        }

        private void OnEnable()
        {
            EditorApplication.update += () =>
            {
                if (!AnimationMode.InAnimationMode()) previweing = null;
            };
        }

        public static bool IsPreviewing(TexTransMonoBase transformer) => transformer == instance.previweing;
        public static bool IsPreviewContains => instance.previweing != null;

        private void DrawApplyAndRevert<T>(T target, Func<T, IDisposable> apply)
            where T : Object
        {
            if (target == null) return;
            if (previweing == null && AnimationMode.InAnimationMode())
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Common:PreviewNotAvailable".Glc());
                EditorGUI.EndDisabledGroup();
            }
            else if (previweing == null)
            {
                if (GUILayout.Button("Common:Preview".Glc()))
                {
                    StartPreview(target, apply);
                }
            }
            else if (previweing == target)
            {
                if (GUILayout.Button("Common:ExitPreview".Glc()))
                {
                    ExitPreview();
                }
            }
            else
            {
                if (GUILayout.Button("Common:OverridePreviewThis".Glc()))
                {
                    ExitPreview();
                    StartPreview(target, apply);
                }
            }
        }
        public void DrawApplyAndRevert(TexTransMonoBase target)
        {
            DrawApplyAndRevert(target, TexTransBehaviorApply);
        }
        public void ApplyTexTransBehavior(TexTransMonoBase target)
        {
            StartPreview(target, TexTransBehaviorApply);
        }

        void StartPreview<T>(T target, Func<T, IDisposable> applyAction) where T : Object
        {
            previweing = target;
            previweingAssets?.Dispose();
            previweingAssets = null;
            OnPreviewEnter?.Invoke(previweing);
            AnimationMode.StartAnimationMode();
            try
            {
                previweingAssets = applyAction(target);
            }
            catch
            {
                AnimationMode.StopAnimationMode();
                EditorUtility.ClearProgressBar();
                previweing = null;
                throw;
            }
        }
        static IDisposable TexTransBehaviorApply(TexTransMonoBase targetTTBehavior)
        {
            AnimationMode.BeginSampling();
            try
            {
                EditorUtility.DisplayProgressBar("TexTransBehaviorApply", "", 0f);
                Profiler.BeginSample("TexTransBehaviorApply: " + targetTTBehavior.GetType() + " " + targetTTBehavior.gameObject.name);
                (UnityAnimationPreviewDomain domain, TempAssetHolder holder) previewDomainHolder = (null, null);
                EditorUtility.DisplayProgressBar("FindMarker", "", 0.01f);
                Profiler.BeginSample("FindMarker");
                var marker = DomainMarkerFinder.FindMarker(targetTTBehavior.gameObject);
                Profiler.EndSample();
                EditorUtility.DisplayProgressBar("Create Domain", "", 0.1f);
                if (marker != null)
                { previewDomainHolder = UnityAnimationPreviewDomain.Create(marker.GetComponentsInChildren<Renderer>(true).ToList()); }
                else { Debug.LogError("Domainが見つかりません!!!"); return null; }

                EditorUtility.DisplayProgressBar("Preview Apply", "", 0.2f);
                //カスタムプレビューとエディターコールビヘイビアは違うから注意
                try
                {
                    if (!TTTCustomPreviewUtility.TryExecutePreview(targetTTBehavior, marker, previewDomainHolder.domain))
                    {
                        if (targetTTBehavior is TexTransBehavior ttb) ttb.Apply(previewDomainHolder.domain);
                    }
                }
                finally
                {
                    EditorUtility.DisplayProgressBar("Edit Finish", "", 0.95f);
                    previewDomainHolder.domain.Dispose();
                }
                return previewDomainHolder.holder;
            }
            finally
            {
                Profiler.EndSample();
                AnimationMode.EndSampling();
                EditorUtility.ClearProgressBar();
            }

        }

        public void ExitPreview()
        {
            if (previweing == null) { return; }
            previweing = null;
            previweingAssets?.Dispose();
            previweingAssets = null;
            AnimationMode.StopAnimationMode();
            OnPreviewExit?.Invoke();
        }
        public void ExitPreview(UnityEngine.SceneManagement.Scene scene, bool removingScene)
        {
            ExitPreview();
        }
        public void DestroyObserve(TexTransMonoBase texTransBehavior)
        {
            if (IsPreviewing(texTransBehavior)) { instance.ExitPreview(); }
        }

    }
}
