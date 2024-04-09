using UnityEngine;
using System;
using UnityEngine.Serialization;
namespace net.rs64.TexTransTool
{
    [Serializable]
    public class TextureSelector
    {
        public SelectMode Mode = SelectMode.Absolute;
        public enum SelectMode
        {
            Absolute,
            Relative,
        }


        //Absolute
        public Texture2D SelectTexture;

        //Relative
        [FormerlySerializedAs("TargetRenderer")]public Renderer RendererAsPath;
        [FormerlySerializedAs("MaterialSelect")]public int SlotAsPath = 0;
        [FormerlySerializedAs("TargetPropertyName")]public PropertyName PropertyNameAsPath = PropertyName.DefaultValue;

        internal Texture2D GetTexture()
        {
            switch (Mode)
            {
                case SelectMode.Absolute:
                    {
                        return SelectTexture;
                    }
                case SelectMode.Relative:
                    {
                        var DistMaterials = RendererAsPath.sharedMaterials;

                        if (DistMaterials.Length <= SlotAsPath) return null;
                        var DistMat = DistMaterials[SlotAsPath];

                        return DistMat.GetTexture(PropertyNameAsPath) as Texture2D;
                    }
                default: { return null; }
            }
        }

    }
}
