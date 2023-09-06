using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static net.rs64.TexTransTool.TextureLayerUtil;
using UnityEditor;
using System;
using net.rs64.TexTransTool.Build;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.TransTextureCore.Utils;

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
        public AvatarDomain(GameObject avatarRoot, bool AssetSaver = false, bool generateCustomMipMap = false, UnityEngine.Object OverrideAssetContainer = null)
        {
            _avatarRoot = avatarRoot;
            _renderers = avatarRoot.GetComponentsInChildren<Renderer>(true).ToList();
            _initialMaterials = RendererUtility.GetMaterials(_renderers);
            if (AssetSaver)
            {
                if (OverrideAssetContainer == null)
                {
                    Asset = ScriptableObject.CreateInstance<AvatarDomainAsset>();
                    AssetDatabase.CreateAsset(Asset, AssetSaveHelper.GenerateAssetPath("AvatarDomainAsset", ".asset"));
                }
                else
                {
                    Asset = ScriptableObject.CreateInstance<AvatarDomainAsset>();
                    Asset.OverrideContainer = OverrideAssetContainer;
                    Asset.name = "net.rs64.TexTransTool.AssetContainer";
                    Asset.AddSubObject(Asset);
                }
            };
            _generateCustomMipMap = generateCustomMipMap;
        }
        [SerializeField] GameObject _avatarRoot;
        [SerializeField] List<Renderer> _renderers;
        [SerializeField] List<Material> _initialMaterials;
        [SerializeField] List<TextureStack> _textureStacks = new List<TextureStack>();
        FlatMapDict<Material> _mapDict;
        [SerializeField] List<MatPair> _matModifies = new List<MatPair>();
        [SerializeField] bool _generateCustomMipMap;

        public AvatarDomainAsset Asset;
        public AvatarDomain GetBackUp()
        {
            return new AvatarDomain(_avatarRoot);
        }
        public void ResetMaterial()
        {
            var reversedMatModifiesDict = new Dictionary<Material, Material>();
            foreach (var MatPair in _matModifies) { reversedMatModifiesDict.Add(MatPair.SecondMaterial, MatPair.Material); }

            RendererUtility.ChangeMaterialForSerializedProperty(reversedMatModifiesDict, _avatarRoot, IgnoreTypes);

            _matModifies.Clear();

            RendererUtility.SetMaterials(_renderers, _initialMaterials);
        }
        private List<Material> GetFilteredMaterials()
        {
            return RendererUtility.GetMaterials(_renderers).Distinct().Where(I => I != null).ToList();
        }
        public void transferAsset(UnityEngine.Object UnityObject)
        {
            if (Asset != null) Asset.AddSubObject(UnityObject);
        }
        public void transferAsset(IEnumerable<UnityEngine.Object> UnityObjects)
        {
            foreach (var unityObject in UnityObjects)
            {
                transferAsset(unityObject);
            }
        }
        public void SetMaterial(Material Target, Material SetMat, bool isPaired)
        {
            if (isPaired)
            {
                RendererUtility.ChangeMaterialForRenderers(_renderers, Target, SetMat);
                if (_mapDict == null) _mapDict = new FlatMapDict<Material>();
                _mapDict.Add(Target, SetMat);
            }
            else
            {
                RendererUtility.ChangeMaterialForRenderers(_renderers, Target, SetMat);
            }

            transferAsset(SetMat);
        }

        public void SetMaterial(MatPair Pair, bool isPaired)
        {
            SetMaterial(Pair.Material, Pair.SecondMaterial, isPaired);
        }
        public void SetMaterials(IEnumerable<MatPair> pairs, bool isPaired)
        {
            foreach (var pair in pairs)
            {
                SetMaterial(pair, isPaired);
            }
        }


        /// <summary>
        /// ドメイン内のすべてのマテリアルのtextureをtargetからsetTexに変更する
        /// </summary>
        /// <param name="Target">差し替え元</param>
        /// <param name="SetTex">差し替え先</param>
        public List<MatPair> SetTexture(Texture2D Target, Texture2D SetTex)
        {
            var mats = GetFilteredMaterials();
            var targetAndSet = new List<MatPair>();
            foreach (var mat in mats)
            {
                var Textures = MaterialUtility.FiltalingUnused(MaterialUtility.GetPropAndTextures(mat), mat);

                if (Textures.ContainsValue(Target))
                {
                    var NewMat = UnityEngine.Object.Instantiate<Material>(mat);

                    foreach (var KVP in Textures)
                    {
                        if (KVP.Value == Target)
                        {
                            NewMat.SetTexture(KVP.Key, SetTex);
                        }
                    }

                    targetAndSet.Add(new MatPair(mat, NewMat));
                }
            }

            SetMaterials(targetAndSet, true);

            return targetAndSet;
        }
        /// <summary>
        /// ドメイン内のすべてのマテリアルのtextureをKeyからValueに変更する
        /// </summary>
        /// <param name="TargetAndSet"></param>
        public List<MatPair> SetTexture(Dictionary<Texture2D, Texture2D> TargetAndSet)
        {
            Dictionary<Material, Material> keyAndNotSavedMat = new Dictionary<Material, Material>();
            foreach (var KVP in TargetAndSet)
            {
                var notSavedMat = SetTexture(KVP.Key, KVP.Value);

                foreach (var matPar in notSavedMat)
                {
                    if (keyAndNotSavedMat.ContainsKey(matPar.Material) == false)
                    {
                        keyAndNotSavedMat.Add(matPar.Material, matPar.SecondMaterial);
                    }
                    else
                    {
                        keyAndNotSavedMat[matPar.Material] = matPar.SecondMaterial;
                    }
                }
            }
            return keyAndNotSavedMat.Select(i => new MatPair(i.Key, i.Value)).ToList();
        }
        public void AddTextureStack(Texture2D Dist, BlendTextures SetTex)
        {
            var stack = _textureStacks.Find(i => i.FirstTexture == Dist);
            if (stack == null)
            {
                stack = new TextureStack { FirstTexture = Dist };
                stack.Stack = SetTex;
                _textureStacks.Add(stack);
            }
            else
            {
                stack.Stack = SetTex;
            }

        }

        public void SaveTexture()
        {
            foreach (var stack in _textureStacks)
            {
                var dist = stack.FirstTexture;
                var setTex = stack.MergeStack();
                if (dist == null || setTex == null) continue;


                SortedList<int, Color[]> mip = null;
                if (_generateCustomMipMap)
                {
                    var usingUVdata = new List<TransTexture.TransData>();
                    foreach (var mat in FindUseMaterials(dist))
                    {
                        MatUseUvDataGet(usingUVdata, mat);
                    }
                    var primeRT = new RenderTexture(setTex.width, setTex.height, 32, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB);
                    TransTexture.TransTextureToRenderTexture(primeRT, setTex, usingUVdata);

                    var primeTex = primeRT.CopyTexture2D();

                    var distMip = setTex.GenerateMipList();
                    var setTexMip = primeTex.GenerateMipList();
                    MipMapUtils.MergeMip(distMip, setTexMip);

                    mip = distMip;
                }


                var copySetTex = setTex.CopySetting(dist, mip);
                SetTexture(dist, copySetTex);

                transferAsset(copySetTex);
            }

            var matModifiedDict = _mapDict.GetMapping;
            RendererUtility.ChangeMaterialForSerializedProperty(matModifiedDict, _avatarRoot, IgnoreTypes);
            _matModifies = matModifiedDict.Select(i => new MatPair(i.Key, i.Value)).ToList();
        }

        private void MatUseUvDataGet(List<TransTexture.TransData> UsingUVdata, Material Mat)
        {
            for (int i = 0; _renderers.Count > i; i++)
            {
                var render = _renderers[i];
                for (int j = 0; render.sharedMaterials.Length > j; j++)
                {
                    if (render.sharedMaterials[j] == Mat)
                    {
                        var mesh = render.GetMesh();
                        UsingUVdata.Add(new TransTexture.TransData(
                            mesh.GetSubTriangleIndex(j),
                            mesh.GetUVList(),
                            mesh.GetUVList(0)
                            )
                        );
                    }
                }
            }
        }

        public List<Material> FindUseMaterials(Texture2D Texture)
        {
            var mats = GetFilteredMaterials();
            List<Material> useMats = new List<Material>();
            foreach (var mat in mats)
            {
                var textures = MaterialUtility.FiltalingUnused(MaterialUtility.GetPropAndTextures(mat), mat);

                if (textures.ContainsValue(Texture))
                {
                    useMats.Add(mat);
                }
            }
            return useMats;
        }

        public class TextureStack
        {
            public Texture2D FirstTexture;
            [SerializeField] List<BlendTextures> StackTextures = new List<BlendTextures>();

            public BlendTextures Stack
            {
                set => StackTextures.Add(value);
            }

            public Texture2D MergeStack()
            {
                var size = FirstTexture.NativeSize();
                var rendererTexture = new RenderTexture(size.x, size.y, 32, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB);
                Graphics.Blit(FirstTexture, rendererTexture);

                rendererTexture.BlendBlit(StackTextures);

                rendererTexture.name = FirstTexture.name + "_MergedStack";
                return rendererTexture.CopyTexture2D();
            }

        }
    }

    public class FlatMapDict<TKeyValue>
    {
        Dictionary<TKeyValue, TKeyValue> _dict = new Dictionary<TKeyValue, TKeyValue>();
        Dictionary<TKeyValue, TKeyValue> _reverseDict = new Dictionary<TKeyValue, TKeyValue>();

        public void Add(TKeyValue key, TKeyValue value)
        {
            if (_reverseDict.TryGetValue(key, out var tKey))
            {
                _dict[tKey] = value;
                _reverseDict.Remove(key);
                _reverseDict.Add(value, tKey);
            }
            else
            {
                _dict.Add(key, value);
                _reverseDict.Add(value, key);
            }
        }
        public Dictionary<TKeyValue, TKeyValue> GetMapping => _dict;
    }
}