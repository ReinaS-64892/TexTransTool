#if UNITY_EDITOR
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using JetBrains.Annotations;
using net.rs64.TexTransTool.Build;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.TextureStack;

namespace net.rs64.TexTransTool
{
    /// <summary>
    /// This is an IDomain implementation that applies to whole the specified GameObject.
    ///
    /// If <see cref="Previewing"/> is true, This will call <see cref="AnimationMode.AddPropertyModification"/>
    /// everytime modifies some property so you can revert those changes with <see cref="AnimationMode.StopAnimationMode"/>.
    /// This class doesn't call <see cref="AnimationMode.BeginSampling"/> and <see cref="AnimationMode.EndSampling"/>
    /// so user must call those if needed.
    /// </summary>
    internal class AvatarDomain : RenderersDomain
    {
        static readonly HashSet<Type> s_ignoreTypes = new HashSet<Type> { typeof(Transform), typeof(SkinnedMeshRenderer), typeof(MeshRenderer) };

        public AvatarDomain(GameObject avatarRoot,
                            bool previewing,
                            [CanBeNull] IAssetSaver saver = null,
                            IProgressHandling progressHandler = null,
                            bool? isObjectReplaceInvoke = null
                            ) : base(avatarRoot.GetComponentsInChildren<Renderer>(true).ToList(), previewing, saver, progressHandler)
        {
            _avatarRoot = avatarRoot;
            _previewing = previewing;
            if (!_previewing) { _liners = avatarRoot.GetComponentsInChildren<IMaterialReplaceEventLiner>().ToArray(); }
            _isObjectReplaceInvoke = isObjectReplaceInvoke.HasValue ? isObjectReplaceInvoke.Value : TTTConfig.isObjectReplaceInvoke;
        }
        public AvatarDomain(GameObject avatarRoot,
                            bool previewing,
                            [CanBeNull] IAssetSaver saver,
                            IProgressHandling progressHandler,
                            ITextureManager textureManager,
                            IStackManager stackManager,
                            bool? isObjectReplaceInvoke = null
                            ) : base(avatarRoot.GetComponentsInChildren<Renderer>(true).ToList(), previewing, saver, progressHandler, textureManager, stackManager)
        {
            _avatarRoot = avatarRoot;
            _previewing = previewing;
            if (!_previewing) { _liners = avatarRoot.GetComponentsInChildren<IMaterialReplaceEventLiner>().ToArray(); }
            _isObjectReplaceInvoke = isObjectReplaceInvoke.HasValue ? isObjectReplaceInvoke.Value : TTTConfig.isObjectReplaceInvoke;
        }

        bool _previewing;
        IMaterialReplaceEventLiner[] _liners;

        [SerializeField] GameObject _avatarRoot;
        public GameObject AvatarRoot => _avatarRoot;
        bool _isObjectReplaceInvoke;
        [NotNull] FlatMapDict<Material> _matMap = new FlatMapDict<Material>();
        [NotNull] FlatMapDict<Texture2D> _texMap = new FlatMapDict<Texture2D>();
        [NotNull] FlatMapDict<Mesh> _meshMap = new FlatMapDict<Mesh>();

        public override void ReplaceMaterials(Dictionary<Material, Material> mapping, bool rendererOnly = false)
        {
            base.ReplaceMaterials(mapping, rendererOnly);

            if (!rendererOnly)
            {
                foreach (var keyValuePair in mapping)
                {
                    _matMap.Add(keyValuePair.Key, keyValuePair.Value);
                    InvokeMaterialReplace(keyValuePair.Key, keyValuePair.Value);
                }
            }
        }

        private void InvokeMaterialReplace(Material key, Material value)
        {
            if (_previewing) { return; }
            foreach (var liner in _liners)
            {
                liner.MaterialReplace(key, value);
            }
        }

        public override void SetTexture(Texture2D target, Texture2D setTex)
        {
            base.SetTexture(target, setTex);
            _texMap.Add(target, setTex);
        }
        public override void SetMesh(Renderer renderer, Mesh mesh)
        {
            base.SetMesh(renderer, mesh);
            _meshMap.Add(renderer.GetMesh(), mesh);
        }

        public override void EditFinish()
        {
            base.EditFinish();

            if (_previewing) { return; }

            var modMap = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
            foreach (var map in _matMap.GetMapping) { modMap.Add(map.Key, map.Value); }
            foreach (var map in _texMap.GetMapping) { modMap.Add(map.Key, map.Value); }
            foreach (var map in _meshMap.GetMapping) { modMap.Add(map.Key, map.Value); }

#if NDMF_1_3_x
            foreach (var replaceKV in modMap)
            {
                nadena.dev.ndmf.ObjectRegistry.RegisterReplacedObject(replaceKV.Key, replaceKV.Value);
            }
#endif
            if (_isObjectReplaceInvoke)
            {
                SerializedObjectCrawler.ReplaceSerializedObjects(_avatarRoot, modMap);
            }
        }
    }

    internal class FlatMapDict<TKeyValue>
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
            else if (!_dict.ContainsKey(key))
            {
                _dict.Add(key, value);
                _reverseDict.Add(value, key);
            }
        }
        public Dictionary<TKeyValue, TKeyValue> GetMapping => _dict;
    }

}
#endif
