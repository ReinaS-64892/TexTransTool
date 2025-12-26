using System;
using net.rs64.TexTransTool.Preview.RealTime;
using net.rs64.TexTransTool.Preview;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool.Editor
{
    internal static class PreviewButtonDrawUtil
    {
        internal static Action<TexTransMonoBase> ExternalPreviewDrawer = null;
        public static void Draw(TexTransMonoBase target)
        {
            if (target == null) { return; }
#if !TTT_DISABLE_EXTERNAL_PREVIEW_DRAWER
            if (ExternalPreviewDrawer is not null) { ExternalPreviewDrawer(target); return; }
#endif
            if (target is TexTransBehavior ttr && RealTimePreviewContext.IsPreviewPossibleType(ttr))
            { DrawerRealTimePreviewEditorButton(target as TexTransBehavior); }
            else { OneTimePreviewContext.instance.DrawApplyAndRevert(target); }
        }

        internal static Action ExternalUnlitButton = null;
        public static void DrawUnlitButton()
        {
            if (ExternalUnlitButton is null) { return; }
            ExternalUnlitButton.Invoke();
        }

        public static void DrawerRealTimePreviewEditorButton(TexTransBehavior texTransRuntimeBehavior)
        {
            if (texTransRuntimeBehavior == null) { return; }
            var rpm = RealTimePreviewContext.instance;
            if (!rpm.IsPreview())
            {
                bool IsPossibleRealTimePreview = !OneTimePreviewContext.IsPreviewContains;
                IsPossibleRealTimePreview &= !AnimationMode.InAnimationMode();
                IsPossibleRealTimePreview |= rpm.IsPreview();

                EditorGUI.BeginDisabledGroup(!IsPossibleRealTimePreview);
                if (GUILayout.Button(IsPossibleRealTimePreview ? "SimpleDecal:button:RealTimePreview".Glc() : "Common:PreviewNotAvailable".Glc()))
                {
                    var domainRoot = DomainMarkerFinder.FindMarker(texTransRuntimeBehavior.gameObject);
                    if (domainRoot != null)
                    {
                        rpm.EnterRealtimePreview(domainRoot);
                    }
                    else
                    {
                        Debug.Log("Domain not found");
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                // EditorGUILayout.LabelField(RealTimePreviewManager.instance.LastDecalUpdateTime + "ms", GUILayout.Width(40));

                if (GUILayout.Button("SimpleDecal:button:ExitRealTimePreview".Glc()))
                {
                    rpm.ExitRealTimePreview();
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
