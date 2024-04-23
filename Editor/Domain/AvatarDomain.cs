using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using net.rs64.TexTransTool.TextureStack;
using net.rs64.TexTransCore.Island;

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
        public AvatarDomain(GameObject avatarRoot, bool previewing, bool saveAsset = false)
        : this(avatarRoot, previewing, saveAsset ? new AssetSaver() : null) { }
        public AvatarDomain(GameObject avatarRoot, bool previewing, IAssetSaver assetSaver)
        : base(avatarRoot.GetComponentsInChildren<Renderer>(true).ToList(), previewing, assetSaver)
        {
            _avatarRoot = avatarRoot;
        }

        [SerializeField] GameObject _avatarRoot;
        public GameObject AvatarRoot => _avatarRoot;

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
