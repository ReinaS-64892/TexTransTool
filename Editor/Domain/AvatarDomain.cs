#if UNITY_EDITOR
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static net.rs64.TexTransTool.TextureLayerUtil;
using System;
using net.rs64.TexTransTool.Build;
using net.rs64.TexTransTool.Utils;
using JetBrains.Annotations;

namespace net.rs64.TexTransTool
{
    [System.Serializable]
    public class AvatarDomain : IDomain
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
        public AvatarDomain(GameObject avatarRoot, bool saveAssets = false, UnityEngine.Object OverrideAssetContainer = null)
        {
            _avatarRoot = avatarRoot;
            _renderers = avatarRoot.GetComponentsInChildren<Renderer>(true).ToList();
            if (saveAssets) AssetSaver = new AssetSaver(OverrideAssetContainer);
        }
        [SerializeField] GameObject _avatarRoot;
        [SerializeField] List<Renderer> _renderers;
        [SerializeField] TextureStacks _textureStacks = new TextureStacks();
        FlatMapDict<Material> _mapDict;
        [CanBeNull] public AssetSaver AssetSaver;


        public void transferAsset(UnityEngine.Object UnityObject)
        {
            AssetSaver?.transferAsset(UnityObject);
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

        public void SetMesh(Renderer renderer, Mesh mesh)
        {
            switch (renderer)
            {
                case SkinnedMeshRenderer skinnedRenderer:
                {
                    skinnedRenderer.sharedMesh = mesh;
                    break;
                }
                case MeshRenderer meshRenderer:
                {
                    meshRenderer.GetComponent<MeshFilter>().sharedMesh = mesh;
                    break;
                }
                default:
                    throw new ArgumentException($"Unexpected Renderer Type: {renderer.GetType()}", nameof(renderer));
            }
        }

        public void AddTextureStack(Texture2D Dist, BlendTextures SetTex)
        {
            _textureStacks.AddTextureStack(Dist, SetTex);
        }
        public void SetTexture(Texture2D Target, Texture2D SetTex)
        {
            var matPeas = RendererUtility.SetTexture(_renderers, Target, SetTex);
            this.SetMaterials(matPeas, true);
        }

        public void EditFinish()
        {
            foreach (var stackResult in _textureStacks.MargeStacks())
            {
                if (stackResult.FirstTexture == null || stackResult.MargeTexture == null) continue;
                SetTexture(stackResult.FirstTexture, stackResult.MargeTexture);
                transferAsset(stackResult.MargeTexture);
            }

            if (_mapDict != null)
            {
                var matModifiedDict = _mapDict.GetMapping;
                RendererUtility.ChangeMaterialForSerializedProperty(matModifiedDict, _avatarRoot, IgnoreTypes);
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
#endif
