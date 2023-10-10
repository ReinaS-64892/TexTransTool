#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransTool;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlendUtils;
namespace net.rs64.TexTransTool.MultiLayerImage
{

    [AddComponentMenu("TexTransTool/MultiLayer/TTT MultiLayerImageCanvas")]
    public class MultiLayerImageCanvas : TextureTransformer
    {
        public Texture2D ReplaceTarget;
        public List<Renderer> PreviewRenderer;

        public override List<Renderer> GetRenderers => PreviewRenderer;

        public override bool IsPossibleApply => true;

        public override TexTransPhase PhaseDefine => TexTransPhase.BeforeUVModification;

        public Vector2Int TextureSize = new Vector2Int(2048, 2048);

        public override void Apply([NotNull] IDomain domain)
        {
            var Canvas = new RenderTexture(TextureSize.x, TextureSize.y, 0);
            var canvasDescription = new CanvasDescription() { CanvasSize = TextureSize };

            var Layers = transform.GetChildren()
            .Select(I => I.GetComponent<AbstractLayer>())
            .Reverse()
            .Where(I => I.Visible)
            .SelectMany(I => I.EvaluateTexture(canvasDescription))
            .ToArray();

            foreach (var layer in Layers)
            {
                domain.AddTextureStack(ReplaceTarget, layer);
            }

            // Canvas.BlendBlit(Layers);
            // var resultTex = Canvas.CopyTexture2D(OverrideUseMip: true);
            // var mat = ReferenceRenderer.sharedMaterials[ReferenceMaterialSlot];
            // var newMat = Instantiate(mat);
            // newMat.SetTexture(ReferencePropertyName, resultTex);
            // domain.ReplaceMaterials(new Dictionary<Material, Material>() { { mat, newMat } });
        }

        public class CanvasDescription
        {
            public Vector2Int CanvasSize;
        }
    }
}
#endif