#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal class AvatarDomainAsset : ScriptableObject
    {
        public UnityEngine.Object OverrideContainer;
        [SerializeField] List<Object> _subAssets = new List<Object>();

        public void AddSubObject(Object unityObject)
        {
            if (unityObject != null && !_subAssets.Contains(unityObject) && !AssetDatabase.Contains(unityObject))
            {
                AssetDatabase.AddObjectToAsset(unityObject, OverrideContainer == null ? this : OverrideContainer);
                _subAssets.Add(unityObject);
            }
            EditorUtility.SetDirty(this);
        }
    }
}
#endif
