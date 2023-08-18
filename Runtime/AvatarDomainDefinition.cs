#if UNITY_EDITOR
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static Rs64.TexTransTool.TextureLayerUtil;
using System;
using UnityEditor;

namespace Rs64.TexTransTool
{
    [RequireComponent(typeof(AbstractTexTransGroup))]
    public class AvatarDomainDefinition : MonoBehaviour
    {
        public GameObject Avatar;
        public bool GenereatCustomMipMap;
        [SerializeField] public AbstractTexTransGroup TexTransGroup;
        [SerializeField] protected AvatarDomain CacheDomain;

        [SerializeField] bool _IsSelfCallApply;
        public virtual bool IsSelfCallApply => _IsSelfCallApply;
        public virtual AvatarDomain GetDomain()
        {
            return new AvatarDomain(Avatar.GetComponentsInChildren<Renderer>(true).ToList(), true, GenereatCustomMipMap);
        }
        protected void Reset()
        {
            TexTransGroup = GetComponent<AbstractTexTransGroup>();
        }
        public virtual void Apply()
        {
            if (TexTransGroup == null) Reset();
            if (TexTransGroup.IsApply) return;
            if (Avatar == null) return;
            CacheDomain = GetDomain();
            _IsSelfCallApply = true;
            TexTransGroup.Apply(CacheDomain);
            CacheDomain.SaveTexture();
        }

        public virtual void Revart()
        {
            if (_IsSelfCallApply == false) return;
            if (TexTransGroup == null) Reset();
            if (!TexTransGroup.IsApply) return;
            _IsSelfCallApply = false;
            CacheDomain.ResetMaterial();
            TexTransGroup.Revart(CacheDomain);
            AssetSaveHelper.DeletAsset(CacheDomain.Asset);
            CacheDomain = null;
        }
    }
    [System.Serializable]
    public class AvatarDomain
    {
        /*
        AssetSaverがtrueのとき
        渡されたアセットはすべて保存する。
        アセットに保存されていない物を渡すのが前提。

        マテリアルで渡された場合、マテリアルは保存するが、マテリアルの持つテクスチャーは保存しないため、保存の必要がある場合個別でテクスチャーを渡す必要がある。

        基本テクスチャは圧縮して渡す
        ただし、スタックに入れるものは圧縮の必要はない。
        */
        public AvatarDomain(List<Renderer> Renderers, bool AssetSaver = false, bool genereatCustomMipMap = false)
        {
            _Renderers = Renderers;
            _initialMaterials = Utils.GetMaterials(Renderers);
            if (AssetSaver)
            {
                Asset = ScriptableObject.CreateInstance<AvatarDomainAsset>();
                AssetDatabase.CreateAsset(Asset, AssetSaveHelper.GenereatAssetPath("AvatarDomainAsset", ".asset"));
            };
            GenereatCustomMipMap = genereatCustomMipMap;
        }
        [SerializeField] List<Renderer> _Renderers;
        [SerializeField] List<Material> _initialMaterials;
        [SerializeField] List<TextureStack> _TextureStacks = new List<TextureStack>();
        [SerializeField] bool GenereatCustomMipMap;

        public AvatarDomainAsset Asset;
        public AvatarDomain GetBackUp()
        {
            return new AvatarDomain(_Renderers);
        }
        private List<Material> GetFiltedMaterials()
        {
            return Utils.GetMaterials(_Renderers).Distinct().Where(I => I != null).ToList();
        }
        public void transferAsset(UnityEngine.Object UnityObject)
        {
            if (Asset != null) Asset.AddSubObject(UnityObject);
        }
        public void SetMaterial(Material Target, Material SetMat)
        {
            foreach (var Renderer in _Renderers)
            {
                var Materials = Renderer.sharedMaterials;
                var IsEdit = false;
                foreach (var Index in Enumerable.Range(0, Materials.Length))
                {
                    if (Materials[Index] == Target)
                    {
                        Materials[Index] = SetMat;
                        IsEdit = true;
                    }
                }
                if (IsEdit)
                {
                    Renderer.sharedMaterials = Materials;
                }
            }
            transferAsset(SetMat);
        }
        public void SetMaterial(MatPea Pea)
        {
            SetMaterial(Pea.Material, Pea.SecndMaterial);
        }
        public void SetMaterials(IEnumerable<MatPea> peas)
        {
            foreach (var pea in peas)
            {
                SetMaterial(pea);
            }
        }

        public void ResetMaterial()
        {
            Utils.SetMaterials(_Renderers, _initialMaterials);
        }
        /// <summary>
        /// ドメイン内のすべてのマテリアルのtextureをtargetからsetTexに変更する
        /// </summary>
        /// <param name="Target">差し替え元</param>
        /// <param name="SetTex">差し替え先</param>
        public List<MatPea> SetTexture(Texture2D Target, Texture2D SetTex)
        {
            var Mats = GetFiltedMaterials();
            var TargetAndSet = new List<MatPea>();
            foreach (var Mat in Mats)
            {
                var Textures = MaterialUtil.FiltalingUnused(MaterialUtil.GetPropAndTextures(Mat), Mat);

                if (Textures.ContainsValue(Target))
                {
                    var NewMat = UnityEngine.Object.Instantiate<Material>(Mat);

                    foreach (var KVP in Textures)
                    {
                        if (KVP.Value == Target)
                        {
                            NewMat.SetTexture(KVP.Key, SetTex);
                        }
                    }

                    TargetAndSet.Add(new MatPea(Mat, NewMat));
                }
            }

            SetMaterials(TargetAndSet);

            return TargetAndSet;
        }
        /// <summary>
        /// ドメイン内のすべてのマテリアルのtextureをKeyからValueに変更する
        /// </summary>
        /// <param name="TargetAndSet"></param>
        public List<MatPea> SetTexture(Dictionary<Texture2D, Texture2D> TargetAndSet)
        {
            Dictionary<Material, Material> KeyAndNotSavedMat = new Dictionary<Material, Material>();
            foreach (var KVP in TargetAndSet)
            {
                var NotSavedMat = SetTexture(KVP.Key, KVP.Value);

                foreach (var MatPar in NotSavedMat)
                {
                    if (KeyAndNotSavedMat.ContainsKey(MatPar.Material) == false)
                    {
                        KeyAndNotSavedMat.Add(MatPar.Material, MatPar.SecndMaterial);
                    }
                    else
                    {
                        KeyAndNotSavedMat[MatPar.Material] = MatPar.SecndMaterial;
                    }
                }
            }
            return KeyAndNotSavedMat.Select(i => new MatPea(i.Key, i.Value)).ToList();
        }
        public void AddTextureStack(Texture2D Dist, BlendTextures SetTex)
        {
            var Stack = _TextureStacks.Find(i => i.FirstTexture == Dist);
            if (Stack == null)
            {
                Stack = new TextureStack { FirstTexture = Dist };
                Stack.Stack = SetTex;
                _TextureStacks.Add(Stack);
            }
            else
            {
                Stack.Stack = SetTex;
            }

        }

        public void SaveTexture()
        {
            foreach (var Stack in _TextureStacks)
            {
                var Dist = Stack.FirstTexture;
                var SetTex = Stack.MargeStack();
                if (Dist == null || SetTex == null) continue;

                if (Dist.width != SetTex.width || Dist.height != SetTex.height)
                {
                    SetTex = TextureLayerUtil.ResizeTexture(SetTex, new Vector2Int(Dist.width, Dist.height));
                }

                SortedList<int, Color[]> Mip = null;
                if (GenereatCustomMipMap)
                {
                    var UsingUVdata = new List<TransTexture.TransUVData>();
                    foreach (var Mat in FindUseMaterials(Dist))
                    {
                        MatUseUvDataGet(UsingUVdata, Mat);
                    }
                    var PrimeRT = new RenderTexture(SetTex.width, SetTex.height, 32, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB);
                    TransTexture.TransTextureToRenderTexture(PrimeRT, SetTex, UsingUVdata);

                    var PrimeTex = PrimeRT.CopyTexture2D();

                    var DistMip = SetTex.GenereatMiplist();
                    var SetTexMip = PrimeTex.GenereatMiplist();
                    MipMapUtili.MargeMip(DistMip, SetTexMip);

                    Mip = DistMip;
                }


                var CopySetTex = SetTex.CopySetting(Dist, Mip);
                SetTexture(Dist, CopySetTex);

                transferAsset(CopySetTex);
            }

        }

        private void MatUseUvDataGet(List<TransTexture.TransUVData> UsingUVdata, Material Mat)
        {
            for (int i = 0; _Renderers.Count > i; i++)
            {
                var render = _Renderers[i];
                for (int j = 0; render.sharedMaterials.Length > j; j++)
                {
                    if (render.sharedMaterials[j] == Mat)
                    {
                        var mesh = render.GetMesh();
                        UsingUVdata.Add(new TransTexture.TransUVData(
                            Utils.ToList(mesh.GetTriangles(j)),
                            mesh.uv,
                            mesh.uv
                            )
                        );
                    }
                }
            }
        }

        public List<Material> FindUseMaterials(Texture2D Texture)
        {
            var Mats = GetFiltedMaterials();
            List<Material> UseMats = new List<Material>();
            foreach (var Mat in Mats)
            {
                var Textures = MaterialUtil.FiltalingUnused(MaterialUtil.GetPropAndTextures(Mat), Mat);

                if (Textures.ContainsValue(Texture))
                {
                    UseMats.Add(Mat);
                }
            }
            return UseMats;
        }

        public class TextureStack
        {
            public Texture2D FirstTexture;
            [SerializeField] List<BlendTextures> StackTextures = new List<BlendTextures>();

            public BlendTextures Stack
            {
                set => StackTextures.Add(value);
            }

            public Texture2D MargeStack()
            {
                var Size = FirstTexture.NativeSize();
                var RendererTexture = new RenderTexture(Size.x, Size.y, 32, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB);
                Graphics.Blit(FirstTexture, RendererTexture);

                RendererTexture.BlendBlit(StackTextures);

                return RendererTexture.CopyTexture2D();
            }

        }
    }
}
#endif