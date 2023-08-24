#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Rs64.TexTransTool
{
    [Serializable]
    public struct PropertyName
    {
        [SerializeField] string _propatyName;

        [SerializeField] bool _useCustomProperty;

        [SerializeField] string _shaderName;
        [SerializeField] int _propatyIndex;


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