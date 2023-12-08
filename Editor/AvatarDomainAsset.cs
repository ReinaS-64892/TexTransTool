#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal class AvatarDomainAsset : ScriptableObject
    {
        public UnityEngine.Object OverrideContainer;
        [SerializeField] List<Object> SubAssets = new List<Object>();

        public void AddSubObject(Object UnityObject)
        {
            if (UnityObject != null && !SubAssets.Contains(UnityObject) && !AssetDatabase.Contains(UnityObject))
            {
                AssetDatabase.AddObjectToAsset(UnityObject, OverrideContainer == null ? this : OverrideContainer);
                SubAssets.Add(UnityObject);
            }
            EditorUtility.SetDirty(this);
        }
    }
}
#endif
