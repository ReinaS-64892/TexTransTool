#if UNITY_EDITOR
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static net.rs64.TexTransTool.TextureLayerUtil;
using UnityEditor;
using System;
using net.rs64.TexTransTool.Bulige;

namespace net.rs64.TexTransTool
{
    [System.Serializable]
    public class AvatarDomain
    {
        static Type[] IgnoreTypes = new Type[] { typeof(Transform), typeof(AvatarDomainDefinition) };
        /*
        AssetSaverがtrueのとき
        渡されたアセットはすべて保存する。
        アセットに保存されていない物を渡すのが前提。

        マテリアルで渡された場合、マテリアルは保存するが、マテリアルの持つテクスチャーは保存しないため、保存の必要がある場合個別でテクスチャーを渡す必要がある。

        基本テクスチャは圧縮して渡す
        ただし、スタックに入れるものは圧縮の必要はない。
        */
        public AvatarDomain(GameObject avatarRoot, bool AssetSaver = false, bool genereatCustomMipMap = false, UnityEngine.Object OverrideAssetContainer = null)
        {
            _avatarRoot = avatarRoot;
            _renderers = avatarRoot.GetComponentsInChildren<Renderer>(true).ToList();
            _initialMaterials = Utils.GetMaterials(_renderers);
            if (AssetSaver)
            {
                if (OverrideAssetContainer == null)
                {
                    Asset = ScriptableObject.CreateInstance<AvatarDomainAsset>();
                    AssetDatabase.CreateAsset(Asset, AssetSaveHelper.GenereatAssetPath("AvatarDomainAsset", ".asset"));
                }
                else
                {
                    Asset = ScriptableObject.CreateInstance<AvatarDomainAsset>();
                    Asset.OverrideContainer = OverrideAssetContainer;
                    Asset.name = "net.rs64.TexTransTool.AssetContainer";
                    Asset.AddSubObject(Asset);
                }
            };
            _genereatCustomMipMap = genereatCustomMipMap;
        }
        [SerializeField] GameObject _avatarRoot;
        [SerializeField] List<Renderer> _renderers;
        [SerializeField] List<Material> _initialMaterials;
        [SerializeField] List<TextureStack> _textureStacks = new List<TextureStack>();
        FlatMapDict<Material> _MapDict;
        [SerializeField] List<MatPea> MatModifids = new List<MatPea>();
        [SerializeField] bool _genereatCustomMipMap;
        Dictionary<SerializedObject, SerializedProperty[]> _cashMaterialPropertys;

        public AvatarDomainAsset Asset;
        public AvatarDomain GetBackUp()
        {
            return new AvatarDomain(_avatarRoot);
        }
        public void ResetMaterial()
        {
            var RevarsdMatModifaidDict = new Dictionary<Material, Material>();
            foreach (var MatPea in MatModifids) { RevarsdMatModifaidDict.Add(MatPea.SecndMaterial, MatPea.Material); }

            Utils.ChangeMaterialPropetys(RevarsdMatModifaidDict, _avatarRoot, IgnoreTypes);

            MatModifids.Clear();

            Utils.SetMaterials(_renderers, _initialMaterials);
        }
        private List<Material> GetFiltedMaterials()
        {
            return Utils.GetMaterials(_renderers).Distinct().Where(I => I != null).ToList();
        }
        public void transferAsset(UnityEngine.Object UnityObject)
        {
            if (Asset != null) Asset.AddSubObject(UnityObject);
        }
        public void transferAsset(IEnumerable<UnityEngine.Object> UnityObjects)
        {
            foreach (var UnityObject in UnityObjects)
            {
                transferAsset(UnityObject);
            }
        }
        public void SetMaterial(Material Target, Material SetMat, bool isPaird)
        {
            if (isPaird)
            {
                Utils.ChangeMaterialRendereas(_renderers, Target, SetMat);
                if(_MapDict == null) _MapDict = new FlatMapDict<Material>();
                _MapDict.Add(Target, SetMat);
            }
            else
            {
                Utils.ChangeMaterialRendereas(_renderers, Target, SetMat);
            }

            transferAsset(SetMat);
        }

        public void SetMaterial(MatPea Pea, bool isPaird)
        {
            SetMaterial(Pea.Material, Pea.SecndMaterial, isPaird);
        }
        public void SetMaterials(IEnumerable<MatPea> peas, bool isPaird)
        {
            foreach (var pea in peas)
            {
                SetMaterial(pea, isPaird);
            }
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

            SetMaterials(TargetAndSet, true);

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
            var Stack = _textureStacks.Find(i => i.FirstTexture == Dist);
            if (Stack == null)
            {
                Stack = new TextureStack { FirstTexture = Dist };
                Stack.Stack = SetTex;
                _textureStacks.Add(Stack);
            }
            else
            {
                Stack.Stack = SetTex;
            }

        }

        public void SaveTexture()
        {
            foreach (var Stack in _textureStacks)
            {
                var Dist = Stack.FirstTexture;
                var SetTex = Stack.MargeStack();
                if (Dist == null || SetTex == null) continue;


                SortedList<int, Color[]> Mip = null;
                if (_genereatCustomMipMap)
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

            var matModifaidDict = _MapDict.GetMapping;
            Utils.ChangeMaterialPropetys(matModifaidDict, _avatarRoot, IgnoreTypes);
            MatModifids = matModifaidDict.Select(i => new MatPea(i.Key, i.Value)).ToList();
        }

        private void MatUseUvDataGet(List<TransTexture.TransUVData> UsingUVdata, Material Mat)
        {
            for (int i = 0; _renderers.Count > i; i++)
            {
                var render = _renderers[i];
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

                RendererTexture.name = FirstTexture.name + "_MargedStack";
                return RendererTexture.CopyTexture2D();
            }

        }
    }

    public class FlatMapDict<TkeyValu>
    {
        Dictionary<TkeyValu, TkeyValu> _dict = new Dictionary<TkeyValu, TkeyValu>();
        Dictionary<TkeyValu, TkeyValu> _revastDict = new Dictionary<TkeyValu, TkeyValu>();

        public void Add(TkeyValu key, TkeyValu value)
        {
            if (_revastDict.TryGetValue(key, out var tkey))
            {
                _dict[tkey] = value;
                _revastDict.Remove(key);
                _revastDict.Add(value, tkey);
            }
            else
            {
                _dict.Add(key, value);
                _revastDict.Add(value, key);
            }
        }
        public IReadOnlyDictionary<TkeyValu, TkeyValu> GetMapping => _dict;
    }
}
#endif