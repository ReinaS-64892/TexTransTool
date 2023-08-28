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
        [SerializeField] string _propertyName;

#pragma warning disable CS0414 , IDE0052
        [SerializeField] bool _useCustomProperty;

        [SerializeField] string _shaderName;
        [SerializeField] int _propertyIndex;
#pragma warning restore CS0414 , IDE0052


        public PropertyName(string propertyName)
        {
            _propertyName = propertyName;
            _useCustomProperty = false;
            _shaderName = "";
            _propertyIndex = 0;
        }


        public static implicit operator string(PropertyName p) => p._propertyName;
    }
}
#endif
