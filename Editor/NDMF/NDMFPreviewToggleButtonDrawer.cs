
using net.rs64.TexTransCore;
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
            if (NDMFPlugin.s_togglablePreviewTexTransBehaviors.ContainsKey(texTransBehavior.GetType()) is false) { return; }

            var isPreviewValue = NDMFPlugin.s_togglablePreviewTexTransBehaviors[texTransBehavior.GetType()].IsActive;
            var thisComponentName = texTransBehavior.GetType().Name;

            if (isPreviewValue.Value)
            {
                if (GUILayout.Button("Common:ndmf:DisableThisComponentTypePreview".Glf(thisComponentName)))
                { isPreviewValue.Value = !isPreviewValue.Value; }
            }
            else
            {
                if (GUILayout.Button("Common:ndmf:EnableThisComponentTypePreview".Glf(thisComponentName)))
                { isPreviewValue.Value = !isPreviewValue.Value; }
            }
        }

    }
}
