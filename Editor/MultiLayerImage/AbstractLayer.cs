#if UNITY_EDITOR
using System.Collections.Generic;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlendUtils;
using static net.rs64.TexTransTool.MultiLayerImage.MultiLayerImageCanvas;
using LayerMask = net.rs64.TexTransCore.Layer.LayerMask;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    [DisallowMultipleComponent]
    public abstract class AbstractLayer : MonoBehaviour, ITexTransToolTag
    {
        public bool Visible { get => gameObject.activeSelf; set => gameObject.SetActive(value); }

        [HideInInspector, SerializeField] int _saveDataVersion = ToolUtils.ThiSaveDataVersion;
        public int SaveDataVersion => _saveDataVersion;

        public float Opacity = 1;
        public bool Clipping;
        public BlendType BlendMode;
        public LayerMask LayerMask;
        public abstract void EvaluateTexture(LayerStack layerStack);

        public static void DrawMask(LayerMask layerMask, Vector2Int canvasSize, RenderTexture tex)
        {
            if (layerMask.MaskTexture != null)
            {
                var tempRt = RenderTexture.GetTemporary(tex.descriptor);
                ColorBlit(tempRt, new Color(1, 1, 1, layerMask.DefaultMaskColor));
                DrawOffsetEvaluateTexture(tempRt, layerMask.MaskTexture, layerMask.MaskPivot, canvasSize);
                MaskDrawRenderTexture(tex, tempRt);
                RenderTexture.ReleaseTemporary(tempRt);
            }
        }

        public static void DrawClipping(LayerStack layerStack, RenderTexture tex)
        {
            var index = layerStack.Stack.Count;
            var findEnd = false;
            while (!findEnd)
            {
                index -= 1;
                if (index < 0) { break; }
                var downLayer = layerStack.Stack[index];
                if (downLayer.RefLayer is LayerFolder layerFolder && layerFolder.PassThrough) { break; }
                if (downLayer.RefLayer.Clipping) { continue; }
                if (downLayer.BlendTextures.Texture == null) { break; }
                findEnd = true;
            }

            if (findEnd == false || index < 0)
            {
                TextureBlendUtils.MultipleRenderTexture(tex, new Color(0, 0, 0, 0));
            }
            else
            {
                var refBlendLayer = layerStack.Stack[index];
                MaskDrawRenderTexture(tex, refBlendLayer.BlendTextures.Texture);
            }
        }

        public static void DrawOffsetEvaluateTexture(RenderTexture DrawTarget, Texture2D targetTexture, Vector2Int texturePivot, Vector2Int canvasSize)
        {
            var RightUpPos = texturePivot + new Vector2Int(targetTexture.width, targetTexture.height);
            var Pivot = texturePivot;
            if (RightUpPos != canvasSize || Pivot != Vector2Int.zero)
            {
                TextureOffset(DrawTarget, targetTexture, new Vector2((float)RightUpPos.x / canvasSize.x, (float)RightUpPos.y / canvasSize.y), new Vector2((float)Pivot.x / canvasSize.x, (float)Pivot.y / canvasSize.y));
            }
            else
            {
                Graphics.Blit(targetTexture, DrawTarget);
            }
        }

        public static void TextureOffset(RenderTexture tex, Texture texture, Vector2 RightUpPos, Vector2 Pivot)
        {
            var triangle = new List<TriangleIndex>() { new TriangleIndex(0, 1, 2), new TriangleIndex(2, 1, 3) };
            var souse = new List<Vector2>() { Vector2.zero, new Vector2(0, 1), new Vector2(1, 0), Vector2.one };
            var target = new List<Vector2>() { Pivot, new Vector2(Pivot.x, RightUpPos.y), new Vector2(RightUpPos.x, Pivot.y), RightUpPos };

            var TransData = new TransTexture.TransData(triangle, target, souse);
            TransTexture.TransTextureToRenderTexture(tex, texture, TransData);
        }



    }
}
#endif