#if UNITY_EDITOR
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using net.rs64.TexTransTool.Build;

namespace net.rs64.TexTransTool
{
    /// <summary>
    /// This class almost same as <see cref="AvatarDomain"/> but this class does support reverting
    /// The caller must call <see cref="AnimationMode.BeginSampling"/> and <see cref="AnimationMode.EndSampling"/>
    /// </summary>
    [System.Serializable]
    public class PreviewAvatarDomain : PreviewDomain
    {
        static HashSet<Type> IgnoreTypes = new HashSet<Type> { typeof(Transform), typeof(AvatarDomainDefinition) };

        public PreviewAvatarDomain(GameObject avatarRoot)
            : base(avatarRoot.GetComponentsInChildren<Renderer>(true).ToList())
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
}
#endif
