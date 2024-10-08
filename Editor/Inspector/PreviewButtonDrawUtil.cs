using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransTool.Preview.RealTime;
using net.rs64.TexTransTool.Preview;

namespace net.rs64.TexTransTool.Editor
{
    internal static class PreviewButtonDrawUtil
    {
        internal static Action<TexTransBehavior> ExternalPreviewDrawer = null;
        public static void Draw(TexTransBehavior target)
        {
            if (target == null) { return; }

            if (ExternalPreviewDrawer is not null) { ExternalPreviewDrawer(target); return; }

            if (target is TexTransRuntimeBehavior ttr && RealTimePreviewContext.IsPreviewPossibleType(ttr))
            { TextureTransformerEditor.DrawerRealTimePreviewEditorButton(target as TexTransRuntimeBehavior); }
            else { OneTimePreviewContext.instance.DrawApplyAndRevert(target); }
        }
    }
}
