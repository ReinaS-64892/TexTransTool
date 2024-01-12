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
            _isObjectReplaceInvoke = isObjectReplaceInvoke.HasValue ? isObjectReplaceInvoke.Value : TTTConfig.isObjectReplaceInvoke;
        }

        bool _previewing;

        [SerializeField] GameObject _avatarRoot;
        public GameObject AvatarRoot => _avatarRoot;
        bool _isObjectReplaceInvoke;

        public override void EditFinish()
        {
            base.EditFinish();

            if (_previewing) { return; }

            var modMap = _objectMap.GetMapping;

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
        Dictionary<TKeyValue, TKeyValue> _dict = new();
        Dictionary<TKeyValue, TKeyValue> _reverseDict = new();
        HashSet<TKeyValue> _invalidObjects = new();

        public void Add(TKeyValue old, TKeyValue now)
        {
            if (_invalidObjects.Contains(old)) { return; }

            if (_reverseDict.TryGetValue(old, out var tKey))
            {
                //Mapping Update
                _dict[tKey] = now;
                _reverseDict.Remove(old);
                _reverseDict.Add(now, tKey);
            }
            else if (!_dict.ContainsKey(old))
            {
                //Mapping Add
                _dict.Add(old, now);
                _reverseDict.Add(now, old);
            }
            else
            {
                //InvalidMapping
                _invalidObjects.Add(old);

                _reverseDict.Remove(_dict[old]);
                _dict.Remove(old);
            }
        }
        public Dictionary<TKeyValue, TKeyValue> GetMapping => _dict;
    }

}
#endif
