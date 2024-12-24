using System;
using System.Collections.Generic;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public abstract class TexTransMonoBase : MonoBehaviour, ITexTransToolTag
    {
        //v0.3.x == 0
        //v0.4.x == 1
        //v0.5.x == 2
        //v0.6.x == 3
        //v0.7.x == 4
        //v0.8.x == 5
        //v0.9.x == 6
        internal const int TTTDataVersion = 6;

        [HideInInspector, SerializeField] int _saveDataVersion = TTTDataVersion;
        int ITexTransToolTag.SaveDataVersion => _saveDataVersion;


        internal void OnDestroy()
        {
            DestroyCall.DestroyThis(this);
        }
    }
    internal static class DestroyCall
    {
        public static event Action<TexTransMonoBase> OnDestroy;
        public static void DestroyThis(TexTransMonoBase destroy) => OnDestroy?.Invoke(destroy);
    }
}
