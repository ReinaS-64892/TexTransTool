#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System.IO;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Decal
{
    public abstract class AbstractDecal : TextureTransformer
    {
        public List<Renderer> TargetRenderers = new List<Renderer> { null };
        public BlendType BlendType = BlendType.Normal;
        public Color Color = Color.white;
        public PropertyName TargetPropertyName = new PropertyName("_MainTex");
        public bool MultiRendererMode = false;
        public float Padding = 0.5f;
        public bool FastMode = true;
        public bool IsSeparateMatAndTexture;

        public virtual TextureWrap GetTextureWarp { get => TextureWrap.NotWrap; }

        public override List<Renderer> GetRenderers => TargetRenderers;


        public override void Apply(IDomain Domain)
        {
            if (Domain == null)
            {
                Debug.LogWarning("Decal : ドメインが存在しません。通常ではありえないエラーです。");
                return;
            }
            if (!IsPossibleApply)
            {
                Debug.LogWarning("Decal : デカールを張ることができない状態です。ターゲットレンダラーや、デカールテクスチャーなどが設定されているかどうかご確認ください。");
                return;
            }
            Dictionary<Texture2D, Texture> decalCompiledTextures = CompileDecal();

            if (!IsSeparateMatAndTexture)
            {
                foreach (var trp in decalCompiledTextures)
                {
                    Domain.AddTextureStack(trp.Key, new TextureLayerUtil.BlendTextures(trp.Value, BlendType));
                }
            }
            else
            {
                //分割する場合は特別処理。
                var decalBlendTextures = DecalBlend(decalCompiledTextures, BlendType);
                var materials = RendererUtility.GetMaterials(TargetRenderers).Distinct();
                CopyTexDescription(decalBlendTextures);

                var dictMat = GetDecalTextureSetMaterial(decalBlendTextures, materials, TargetPropertyName);
                
                foreach (var renderer in TargetRenderers)
                {
                    using (var serialized = new SerializedObject(renderer))
                    {
                        foreach (SerializedProperty property in serialized.FindProperty("m_Materials"))
                            if (property.objectReferenceValue is Material material &&
                                dictMat.TryGetValue(material, out var replacement))
                                Domain.SetSerializedProperty(property, replacement);

                        serialized.ApplyModifiedPropertiesWithoutUndo();
                    }
                }

                Domain.transferAssets(decalBlendTextures.Values);
                Domain.transferAssets(dictMat.Values);
            }
        }

        private static void CopyTexDescription(Dictionary<Texture2D, Texture2D> DecalBlendTextures)
        {
            foreach (var dist in DecalBlendTextures.Keys.ToArray())
            {
                DecalBlendTextures[dist] = DecalBlendTextures[dist].CopySetting(DecalBlendTextures[dist]);
            }
        }

        public static Dictionary<Material, Material> GetDecalTextureSetMaterial(Dictionary<Texture2D, Texture2D> DecalsBlendTextures, IEnumerable<Material> Materials, string TargetPropertyName)
        {
            var dictMat = new Dictionary<Material, Material>();
            foreach (var material in Materials)
            {
                if (!material.HasProperty(TargetPropertyName)) { continue; }
                var oldTex = material.GetTexture(TargetPropertyName) as Texture2D;

                if (oldTex == null) continue;
                if (!DecalsBlendTextures.ContainsKey(oldTex)) continue;

                var newMat = UnityEngine.Object.Instantiate(material);

                var NewTex = DecalsBlendTextures[oldTex];
                newMat.SetTexture(TargetPropertyName, NewTex);
                dictMat.Add(material, newMat);
            }

            return dictMat;
        }

        public static Dictionary<Texture2D, Texture2D> DecalBlend(Dictionary<Texture2D, Texture> DecalCompiledTextures, BlendType BlendType)
        {
            var decalBlendTextures = new Dictionary<Texture2D, Texture2D>();
            foreach (var texture in DecalCompiledTextures)
            {
                var blendTexture = TextureLayerUtil.BlendBlit(texture.Key, texture.Value, BlendType).CopyTexture2D();
                blendTexture.Apply();
                decalBlendTextures.Add(texture.Key, blendTexture);
            }

            return decalBlendTextures;
        }

        public abstract Dictionary<Texture2D, Texture> CompileDecal();


        [ContextMenu("ExtractDecalCompiledTexture")]
        public void ExtractDecalCompiledTexture()
        {
            if (!IsPossibleApply) { Debug.LogError("Applyできないためデカールをコンパイルできません。"); return; }


            var path = EditorUtility.OpenFolderPanel("ExtractDecalCompiledTexture", "Assets", "");
            if (string.IsNullOrEmpty(path) && !Directory.Exists(path)) return;

            var decalCompiledTextures = CompileDecal();
            foreach (var TexturePair in decalCompiledTextures)
            {
                var name = TexturePair.Key.name;
                Texture2D extractDCtex;
                switch (TexturePair.Value)
                {
                    case RenderTexture rt:
                        extractDCtex = rt.CopyTexture2D();
                        break;
                    case Texture2D tex:
                        extractDCtex = tex;
                        break;
                    default:
                        continue;
                }
                var pngByte = extractDCtex.EncodeToPNG();

                System.IO.File.WriteAllBytes(Path.Combine(path, name + ".png"), pngByte);

            }
        }

    }
}



#endif
