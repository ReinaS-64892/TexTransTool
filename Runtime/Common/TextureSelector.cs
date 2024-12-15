using UnityEngine;
using System;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Utils;
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
                        if (RendererAsPath == null) return null;
                        var DistMaterials = RendererAsPath.sharedMaterials;

                        if (DistMaterials.Length <= SlotAsPath) return null;
                        var DistMat = DistMaterials[SlotAsPath];

                        if (DistMat.HasProperty(PropertyNameAsPath) is false) return null;
                        return DistMat.GetTexture(PropertyNameAsPath);
                    }
                default: { return null; }
            }
        }

        internal void LookThis(ILookingObject lookingObject) { if (Mode == SelectMode.Relative) { lookingObject.LookAt(RendererAsPath); } }
        internal IEnumerable<Renderer> ModificationTargetRenderers(IEnumerable<Renderer> domainRenderers, OriginEqual replaceTracking)
        {
            var targetTex = GetTexture();
            var domainMaterials = RendererUtility.GetFilteredMaterials(domainRenderers);
            var targetTextures = domainMaterials.SelectMany(m => Memoize.Memo(m, MaterialUtility.GetAllTexture).Values).Where(t => replaceTracking(t, targetTex)).ToHashSet();

            if (targetTextures.Any() is false) { yield break; }

            var containedHash = new Dictionary<Material, bool>();
            foreach (var r in domainRenderers)
            {
                var mats = r.sharedMaterials;
                if (mats.Any(containedHash.GetValueOrDefault)) // キャッシュに true になるものがあったら、調査をすべてスキップしてあった事にする。
                {
                    yield return r;
                    continue;
                }

                foreach (var m in mats)
                {
                    if (containedHash.ContainsKey(m)) { continue; }//このコードパスに来るってことは キャッシュにあるものが true であることはない。
                    var dict = Memoize.Memo(m, MaterialUtility.GetAllTexture);
                    if (dict.Values.Any(targetTextures.Contains))
                    {
                        containedHash.Add(m, true);
                        yield return r;
                        break;
                    }
                    else { containedHash.Add(m, false); }
                }
            }
        }

        internal void LookAtCalling(ILookingObject lookingObject) { lookingObject.LookAt(GetTexture()); }
    }
}
