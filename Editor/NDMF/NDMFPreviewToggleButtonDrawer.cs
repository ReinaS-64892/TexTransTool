
using System;
using nadena.dev.ndmf.preview;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransTool.Editor;
using net.rs64.TexTransTool.IslandSelector;
using UnityEngine;

namespace net.rs64.TexTransTool.NDMF
{
    internal static class NDMFPreviewToggleButtonDrawer
    {
        [TexTransInitialize]
        internal static void OverrideButton()
        {
            PreviewButtonDrawUtil.ExternalPreviewDrawer = DrawNDMFPreviewToggleButton;
            PreviewButtonDrawUtil.ExternalUnlitButton = DrawNDMFEverythingUnlitTextureButton;
        }

        static void DrawNDMFPreviewToggleButton(TexTransMonoBase texTransMonoBase)
        {
            switch (texTransMonoBase)
            {
                case PreviewGroup:
                case TexTransGroup:
                    { return; }
#if NDMF_1_6_8_OR_NEWER
                case AbstractIslandSelector islandSelector:
                    {
                        if (PreviewIslandSelector.PreviewTarget.Value != islandSelector)
                        {
                            if (GUILayout.Button("Start preview IslandSelector this"))
                                PreviewIslandSelector.PreviewTarget.Value = islandSelector;
                        }
                        else
                        {
                            if (GUILayout.Button("Exit preview IslandSelector this"))
                                PreviewIslandSelector.PreviewTarget.Value = null;
                        }
                        return;
                    }
#endif
                case TexTransBehavior ttb:
                    {
                        var phase = ttb.PhaseDefine;

                        var previewNode = NDMFPlugin.s_togglablePreviewPhases[phase];
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
                        return;
                    }

                default: return;
            }
        }
        private static void DrawNDMFEverythingUnlitTextureButton()
        {
            var previewNode = EverythingUnlitTexture.s_EverythingUnlitTexture;
            if (previewNode.IsEnabled.Value)
            {
                if (GUILayout.Button("Everything Unlit/Texture disable"))
                { previewNode.IsEnabled.Value = !previewNode.IsEnabled.Value; }
            }
            else
            {
                if (GUILayout.Button("Everything Unlit/Texture enable"))
                { previewNode.IsEnabled.Value = !previewNode.IsEnabled.Value; }
            }
        }

    }
}
