#if UNITY_EDITOR
using UnityEngine;
using System;
namespace net.rs64.TexTransTool
{
    [Serializable]
    internal class RelativeTextureSelector
    {
        public Renderer TargetRenderer;
        public int MaterialSelect = 0;
        public PropertyName TargetPropertyName = PropertyName.DefaultValue;

        public Texture2D GetTexture()
        {
            var DistMaterials = TargetRenderer.sharedMaterials;

            if (DistMaterials.Length <= MaterialSelect) return null;
            var DistMat = DistMaterials[MaterialSelect];

            return DistMat.GetTexture(TargetPropertyName) as Texture2D;
        }
    }
}
#endif
