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
        internal IEnumerable<Renderer> ModificationTargetRenderers(IRendererTargeting rendererTargeting)
        {
            var targetTex = GetTexture();
            var targetTextures = rendererTargeting.GetAllTextures().Where(t => rendererTargeting.OriginEqual(t, targetTex)).ToHashSet();

            if (targetTextures.Any() is false) { yield break; }

            var containedHash = new Dictionary<Material, bool>();
            foreach (var r in rendererTargeting.EnumerateRenderer())
            {
                var mats = rendererTargeting.GetMaterials(r).Where(i => i != null);
                if (mats.Any(containedHash.GetValueOrDefault)) // キャッシュに true になるものがあったら、調査をすべてスキップしてあった事にする。
                {
                    yield return r;
                    continue;
                }

                foreach (var m in mats)
                {
                    if (containedHash.ContainsKey(m)) { continue; }//このコードパスに来るってことは キャッシュにあるものが true であることはない。

                    if (m.GetAllTexture<Texture>().Any(targetTextures.Contains))
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
