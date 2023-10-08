#if UNITY_EDITOR
using net.rs64.TexTransCore.BlendTexture;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlendUtils;
namespace net.rs64.TexTransTool.MultiLayerImage
{
    [DisallowMultipleComponent]
    public abstract class AbstractLayer : MonoBehaviour
    {
        public bool Visible => gameObject.activeSelf && enabled;
        public float Opacity = 1;
        public bool Clipping;
        public BlendType BlendMode;
        public TexTransCore.Layer.LayerMask LayerMask;

        //ここ戻すやつ複数のRenderTextureであるほうが望ましいのかな？
        public abstract BlendTextures EvaluateTexture(MultiLayerImageCanvas.CanvasDescription canvasDescription);

    }

    public static class AbstractLayerUtility
    {

        public static AbstractLayer GetRelIndexLayer(this AbstractLayer abstractLayer, int RelIndex)
        {
            if (RelIndex == 0) { return abstractLayer; }
            var parent = abstractLayer.transform.parent;
            var thisSibling = abstractLayer.transform.GetSiblingIndex();
            var targetSibling = thisSibling + RelIndex;
            if (parent.childCount > targetSibling && targetSibling >= 0) { return null; }
            return parent.GetChild(targetSibling).GetComponent<AbstractLayer>();
        }

        public static AbstractLayer GetUpLayer(this AbstractLayer abstractLayer) => abstractLayer.GetRelIndexLayer(-1);
        public static AbstractLayer GetDownLayer(this AbstractLayer abstractLayer) => abstractLayer.GetRelIndexLayer(1);
    }
}
#endif