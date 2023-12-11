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
            _useMaterialReplaceEvent = !previewing;
            if (_useMaterialReplaceEvent) { _liners = avatarRoot.GetComponentsInChildren<IMaterialReplaceEventLiner>().ToArray(); }
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
            _useMaterialReplaceEvent = !previewing;
            if (_useMaterialReplaceEvent) { _liners = avatarRoot.GetComponentsInChildren<IMaterialReplaceEventLiner>().ToArray(); }
            _isObjectReplaceInvoke = isObjectReplaceInvoke.HasValue ? isObjectReplaceInvoke.Value : TTTConfig.isObjectReplaceInvoke;
        }

        bool _useMaterialReplaceEvent;
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
            if (!_useMaterialReplaceEvent) { return; }
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

            if (_isObjectReplaceInvoke)
            {
                var matModifiedDict = _matMap.GetMapping;
                var texModifiedDict = _texMap.GetMapping;
                var meshModifiedDict = _meshMap.GetMapping;

                foreach (var component in _avatarRoot.GetComponentsInChildren<Component>())
                {
                    if (component == null) continue;
                    if (s_ignoreTypes.Contains(component.GetType())) continue;

                    using (var serializeObj = new SerializedObject(component))
                    {
                        var iter = serializeObj.GetIterator();
                        while (iter.Next(true))
                        {
                            if (iter.propertyType != SerializedPropertyType.ObjectReference) continue;
                            switch (iter.objectReferenceValue)
                            {
                                case Material originalMat:
                                    {
                                        if (!matModifiedDict.TryGetValue(originalMat, out var value)) { continue; }
                                        SetSerializedProperty(iter, value);
                                        break;
                                    }
                                case Texture2D originalTexture2D:
                                    {
                                        if (!texModifiedDict.TryGetValue(originalTexture2D, out var value)) { continue; }
                                        SetSerializedProperty(iter, value);
                                        break;
                                    }
                                case Mesh originalMesh:
                                    {
                                        if (!meshModifiedDict.TryGetValue(originalMesh, out var value)) { continue; }
                                        SetSerializedProperty(iter, value);
                                        break;
                                    }
                                default:
                                    break;
                            }

                        }

                        serializeObj.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
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
