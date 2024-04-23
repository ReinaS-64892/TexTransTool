using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Build;
using UnityEngine;

namespace net.rs64.TexTransTool.Preview.Custom
{
    [TTTCustomPreview(typeof(PreviewRenderer))]
    internal class PreviewRendererPreview : ITTTCustomPreview
    {
        public void Preview(TexTransBehavior texTransBehavior, IEditorCallDomain editorCallDomain)
        {
            if (texTransBehavior is not PreviewRenderer previewRenderer) { return; }
            if (editorCallDomain is not AvatarDomain avatarDomain) { Debug.LogError("ドメインが見つからない状態ではプレビューできません。"); return; }

            var refRenderer = previewRenderer.GetComponent<Renderer>();
            var root = avatarDomain.AvatarRoot;

            var targets = FindAtTargetRenderer(refRenderer, root);

            var phaseOnTf = AvatarBuildUtils.FindAtPhase(root);
            AvatarBuildUtils.WhiteList(phaseOnTf, targets);
            TexTransGroupPreview.ExecutePhases(editorCallDomain, phaseOnTf);
        }

        internal static HashSet<TexTransBehavior> FindAtTargetRenderer(Renderer targetRenderer, GameObject root)
        {
            return root.GetComponentsInChildren<TexTransBehavior>().Where(i => i.GetRenderers?.Any(i => i == targetRenderer) ?? false).Where(i => i is not TexTransGroup).ToHashSet();
        }
    }
}
