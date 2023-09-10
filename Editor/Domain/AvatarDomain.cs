#if UNITY_EDITOR
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using JetBrains.Annotations;
using net.rs64.TexTransTool.Build;

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
    [System.Serializable]
    public class AvatarDomain : RenderersDomain
    {
        static HashSet<Type> IgnoreTypes = new HashSet<Type> { typeof(Transform), typeof(AvatarDomainDefinition) };

        public AvatarDomain(GameObject avatarRoot, bool previewing, [CanBeNull] IAssetSaver saver = null)
            : base(avatarRoot.GetComponentsInChildren<Renderer>(true).ToList(), previewing, saver)
        {
            _avatarRoot = avatarRoot;
        }

        [SerializeField] GameObject _avatarRoot;
        FlatMapDict<Material> _mapDict;

        public override void SetMaterial(Material target, Material set, bool isPaired)
        {
            base.SetMaterial(target, set, isPaired);
            if (isPaired)
            {
                if (_mapDict == null) _mapDict = new FlatMapDict<Material>();
                _mapDict.Add(target, set);
            }
        }

        public override void EditFinish()
        {
            base.EditFinish();

            if (_mapDict == null) return;

            var matModifiedDict = _mapDict.GetMapping;

            foreach (var component in _avatarRoot.GetComponentsInChildren<Component>())
            {
                if (IgnoreTypes.Contains(component.GetType())) continue;

                using (var serializeObj = new SerializedObject(component))
                {
                    var iter = serializeObj.GetIterator();
                    while (iter.Next(true))
                    {
                        if (iter.propertyType != SerializedPropertyType.ObjectReference) continue;
                        if (!(iter.objectReferenceValue is Material originalMat)) continue;
                        if (!matModifiedDict.TryGetValue(originalMat, out var value)) continue;

                        AddPropertyModification(component, iter.propertyPath, originalMat);
                        iter.objectReferenceValue = value;
                    }

                    serializeObj.ApplyModifiedPropertiesWithoutUndo();
                }
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
