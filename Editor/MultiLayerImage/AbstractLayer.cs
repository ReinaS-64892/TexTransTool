#if UNITY_EDITOR
using System.Collections.Generic;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransTool.Utils;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlendUtils;
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
        public TexTransCore.Layer.LayerMask LayerMask;
        public abstract IEnumerable<BlendTextures> EvaluateTexture(MultiLayerImageCanvas.CanvasDescription canvasDescription);

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