using UnityEngine;
using System;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.Utils;
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

        internal void LookThis(ILookingObject lookingObject) { if (Mode == SelectMode.Relative) { lookingObject.LookAt(RendererAsPath); } }
        internal IEnumerable<Renderer> ModificationTargetRenderers(IEnumerable<Renderer> domainRenderers)
        {
            var targetTex = GetTexture() as Texture2D;
            return FindModificationTargetRenderers(domainRenderers, targetTex);
        }

        private static IEnumerable<Renderer> FindModificationTargetRenderers(IEnumerable<Renderer> domainRenderers, Texture2D targetTex)
        {
            if (targetTex == null) { return Array.Empty<Renderer>(); }
            var mats = RendererUtility.GetFilteredMaterials(domainRenderers);
            var targetMatHash = new HashSet<Material>();

            foreach (var mat in mats)
            {
                if (targetMatHash.Contains(mat)) { continue; }
                var dict = mat.GetAllTexture2D();
                if (dict.ContainsValue(targetTex)) { targetMatHash.Add(mat); }
            }
            return domainRenderers.Where(i => i.sharedMaterials.Any(targetMatHash.Contains));
        }
        internal void LookAtCalling(ILookingObject lookingObject) { lookingObject.LookAt(GetTexture()); }
    }
}
