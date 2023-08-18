#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AvatarDomainAsset : ScriptableObject
{
    [SerializeField] List<Object> SubAssets = new List<Object>();

    public void AddSubObject(Object UnityObject)
    {
        if (UnityObject != null && !SubAssets.Contains(UnityObject) && string.IsNullOrWhiteSpace(AssetDatabase.GetAssetPath(UnityObject)))
        {
            AssetDatabase.AddObjectToAsset(UnityObject, this);
            SubAssets.Add(UnityObject);
        }
    }
}
#endif
