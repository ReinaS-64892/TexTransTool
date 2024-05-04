using UnityEngine;
using System;
using UnityEngine.Serialization;
using System.Collections.Generic;
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
        [FormerlySerializedAs("TargetRenderer")] public Renderer RendererAsPath;
        [FormerlySerializedAs("MaterialSelect")] public int SlotAsPath = 0;
        [FormerlySerializedAs("TargetPropertyName")] public PropertyName PropertyNameAsPath = PropertyName.DefaultValue;

        internal Texture GetTexture()
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

                        return DistMat.GetTexture(PropertyNameAsPath);
                    }
                default: { return null; }
            }
        }

        internal IEnumerable<UnityEngine.Object> GetDependency()
        {
            switch (Mode)
            {
                default: yield break;
                case SelectMode.Absolute: yield return SelectTexture; yield break;
                case SelectMode.Relative:
                    {
                        yield return RendererAsPath;

                        var DistMaterials = RendererAsPath.sharedMaterials;
                        if (DistMaterials.Length <= SlotAsPath) { yield return DistMaterials[SlotAsPath]; }

                        yield return GetTexture();
                        yield break;
                    }
            }
        }
        internal int GetDependencyHash()
        {
            var hash = 0;
            hash ^= (int)Mode;
            switch (Mode)
            {

                case SelectMode.Absolute:
                    {
                        hash ^= SelectTexture?.GetInstanceID() ?? 0;
                        break;
                    }
                case SelectMode.Relative:
                    {
                        if (RendererAsPath == null) { break; }
                        hash ^= RendererAsPath?.GetInstanceID() ?? 0;
                        var DistMaterials = RendererAsPath.sharedMaterials;
                        if (DistMaterials.Length <= SlotAsPath) { break; }
                        hash ^= SlotAsPath;
                        hash ^= DistMaterials[SlotAsPath]?.GetInstanceID() ?? 0;

                        hash ^= GetTexture()?.GetInstanceID() ?? 0;
                        break;
                    }
            }

            return hash;
        }

    }
}
