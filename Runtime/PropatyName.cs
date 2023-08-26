#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace net.rs64.TexTransTool
{
    [Serializable]
    public struct PropertyName
    {
        [SerializeField] string _propatyName;

#pragma warning disable CS0414 , IDE0052
        [SerializeField] bool _useCustomProperty;

        [SerializeField] string _shaderName;
        [SerializeField] int _propatyIndex;
#pragma warning restore CS0414 , IDE0052


        public PropertyName(string propatyName)
        {
            _propatyName = propatyName;
            _useCustomProperty = false;
            _shaderName = "";
            _propatyIndex = 0;
        }


        public static implicit operator string(PropertyName p) => p._propatyName;
    }
}
#endif