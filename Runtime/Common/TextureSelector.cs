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
        public Texture2D SelectTexture;

        #region V6SaveData
        [Obsolete("V6SaveData", true)] public SelectMode Mode = SelectMode.Absolute;
        [Obsolete("V6SaveData", true)]
        public enum SelectMode
        {
            Absolute,
            Relative,
        }



        [Obsolete("V6SaveData", true)][FormerlySerializedAs("TargetRenderer")] public Renderer RendererAsPath;
        [Obsolete("V6SaveData", true)][FormerlySerializedAs("MaterialSelect")] public int SlotAsPath = 0;
        [Obsolete("V6SaveData", true)][FormerlySerializedAs("TargetPropertyName")] public PropertyName PropertyNameAsPath = PropertyName.DefaultValue;

        #endregion V6SaveData

        internal Texture GetTextureWithLookAt<TObj>(IRendererTargeting targeting, TObj thisObject, Func<TObj, TextureSelector> getThis)
        where TObj : UnityEngine.Object
        {
            return targeting.LookAtGet(thisObject, i => getThis(i).SelectTexture);
        }
        internal IEnumerable<Renderer> ModificationTargetRenderers<TObj>(IRendererTargeting rendererTargeting, TObj thisObject, Func<TObj, TextureSelector> getThis)
        where TObj : UnityEngine.Object
        {
            var targetTex = GetTextureWithLookAt(rendererTargeting, thisObject, getThis);
            var targetTextures = rendererTargeting.GetAllTextures().Where(t => rendererTargeting.OriginalObjectEquals(t, targetTex)).ToHashSet();

            if (targetTextures.Any() is false) { yield break; }

            var containedHash = new Dictionary<Material, bool>();
            foreach (var r in rendererTargeting.EnumerateRenderers())
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

                    if (rendererTargeting.GetMaterialTextures(m).Any(targetTextures.Contains))
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
