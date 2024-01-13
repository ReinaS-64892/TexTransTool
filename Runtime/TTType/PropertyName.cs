using System;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    [Serializable]
    public struct PropertyName
    {
        [SerializeField] string _propertyName;

#pragma warning disable CS0414 , IDE0052
        [SerializeField] bool _useCustomProperty;
        [SerializeField] string _shaderName;
#pragma warning restore CS0414 , IDE0052


        public PropertyName(string propertyName)
        {
            _propertyName = propertyName;
            _useCustomProperty = false;
            _shaderName = "DefaultShader";
        }

        public const string MainTex = "_MainTex";
        public static PropertyName DefaultValue => new PropertyName(MainTex);
        public override string ToString()
        {
            return (string)this;
        }

        public static implicit operator string(PropertyName p) => p._propertyName;
    }
}