
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

        static void DrawNDMFPreviewToggleButton(TexTransBehavior texTransBehavior)
        {
            if (texTransBehavior.GetType() == typeof(TexTransGroup) || texTransBehavior.GetType() == typeof(PreviewGroup)) { return; }

            var phase = texTransBehavior is not PhaseDefinition pd ? texTransBehavior.PhaseDefine : pd.TexTransPhase;
            TogglablePreviewNode previewNode;
            switch (phase)
            {
                default: { return; }
                case TexTransPhase.BeforeUVModification:
                    { previewNode = NDMFPlugin.s_togglablePreviewPhases[TexTransPhase.BeforeUVModification]; break; }
                case TexTransPhase.UVModification:
                case TexTransPhase.AfterUVModification:
                case TexTransPhase.UnDefined:
                    { previewNode = NDMFPlugin.s_togglablePreviewPhases[TexTransPhase.UVModification]; break; }
                case TexTransPhase.Optimizing:
                    { previewNode = NDMFPlugin.s_togglablePreviewPhases[TexTransPhase.Optimizing]; break; }
            }

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
