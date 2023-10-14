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

        [Range(0, 1)] public float Opacity = 1;
        public bool Clipping;
        public BlendType BlendMode;
        public LayerMask LayerMask;
        public abstract void EvaluateTexture(LayerStack layerStack);

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



    }
}
#endif