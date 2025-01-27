
using nadena.dev.ndmf.preview;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransTool.Editor;
using UnityEngine;

namespace net.rs64.TexTransTool.NDMF
{
    internal static class NDMFPreviewToggleButtonDrawer
    {
        [TexTransInitialize]
        internal static void OverrideButton()
        {
            PreviewButtonDrawUtil.ExternalPreviewDrawer = DrawNDMFPreviewToggleButton;
        }

        static void DrawNDMFPreviewToggleButton(TexTransMonoBase texTransMonoBase)
        {
            if (texTransMonoBase.GetType() == typeof(TexTransGroup) || texTransMonoBase.GetType() == typeof(PreviewGroup)) { return; }

            TexTransPhase? phase = texTransMonoBase is not PhaseDefinition pd ? texTransMonoBase is TexTransBehavior ttb ? ttb.PhaseDefine : null : pd.TexTransPhase;
            if (phase is null) { return; }

            var previewNode = NDMFPlugin.s_togglablePreviewPhases[phase.Value];
            if (previewNode.IsEnabled.Value)
            {
                if (GUILayout.Button("Common:ndmf:DisableThisComponentPhasePreview".Glf(previewNode.DisplayName.Invoke())))
                { previewNode.IsEnabled.Value = !previewNode.IsEnabled.Value; }
            }
            else
            {
                if (GUILayout.Button("Common:ndmf:EnableThisComponentPhasePreview".Glf(previewNode.DisplayName.Invoke())))
                { previewNode.IsEnabled.Value = !previewNode.IsEnabled.Value; }
            }
        }

    }
}
