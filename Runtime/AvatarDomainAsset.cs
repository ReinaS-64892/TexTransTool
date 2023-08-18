#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AvatarDomainAsset : ScriptableObject
{
    [SerializeField] List<Object> SubAssets;

    public void AddSubObject(Object UnityObject)
    {
        if (!SubAssets.Contains(UnityObject))
        {
            AssetDatabase.AddObjectToAsset(UnityObject, this);
        }
    }
}
#endif
