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

        internal Texture GetTextureWithLookAt<TObj>(IRendererTargeting targeting, TObj thisObject, Func<TObj, TextureSelector> getThis)
        where TObj : UnityEngine.Object
        {
            var mode = targeting.LookAtGet(thisObject, i => getThis(i).Mode);
            switch (mode)
            {
                case SelectMode.Absolute:
                    {
                        return targeting.LookAtGet(thisObject, i => getThis(i).SelectTexture);
                    }
                case SelectMode.Relative:
                    {
                        var rendererAsPath = targeting.LookAtGet(thisObject, i => getThis(i).RendererAsPath);
                        if (rendererAsPath == null) return null;
                        var domainsRendererAsPath = targeting.GetDomainsRenderers(rendererAsPath).FirstOrDefault();
                        if (domainsRendererAsPath == null) return null;
                        var distMaterials = targeting.GetMaterials(domainsRendererAsPath);

                        var slotAsPath = targeting.LookAtGet(thisObject, i => getThis(i).SlotAsPath);
                        if (distMaterials.Length <= slotAsPath) return null;
                        var distMat = distMaterials[slotAsPath];

                        var propertyNameAsPath = targeting.LookAtGet(thisObject, i => getThis(i).PropertyNameAsPath);
                        if (targeting.LookAtGet(distMat, m => m.HasProperty(propertyNameAsPath)) is false) return null;
                        return targeting.LookAtGet(distMat, m => m.GetTexture(propertyNameAsPath));
                    }
                default: { return null; }
            }
        }
        internal IEnumerable<Renderer> ModificationTargetRenderers<TObj>(IRendererTargeting rendererTargeting, TObj thisObject, Func<TObj, TextureSelector> getThis)
        where TObj : UnityEngine.Object
        {
            var targetTex = GetTextureWithLookAt(rendererTargeting, thisObject, getThis);
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

                    if (rendererTargeting.GetMaterialTexture(m).Any(targetTextures.Contains))
                    {
                        containedHash.Add(m, true);
                        yield return r;
                        break;
                    }
                    else { containedHash.Add(m, false); }
                }
            }
        }
    }
}
