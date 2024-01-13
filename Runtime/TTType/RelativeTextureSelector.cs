using UnityEngine;
using System;
namespace net.rs64.TexTransTool
{
    [Serializable]
    public class RelativeTextureSelector
    {
        public Renderer TargetRenderer;
        public int MaterialSelect = 0;
        public PropertyName TargetPropertyName = PropertyName.DefaultValue;

        internal Texture2D GetTexture()
        {
            var DistMaterials = TargetRenderer.sharedMaterials;

            if (DistMaterials.Length <= MaterialSelect) return null;
            var DistMat = DistMaterials[MaterialSelect];

            return DistMat.GetTexture(TargetPropertyName) as Texture2D;
        }
    }
}