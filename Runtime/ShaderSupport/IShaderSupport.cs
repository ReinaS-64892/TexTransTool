#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace net.rs64.TexTransTool.ShaderSupport
{
    public interface IShaderSupport
    {
        string ShaderName { get; }

        PropertyNameAndDisplayName[] GetPropatyNames { get; }
    }

    public struct PropertyNameAndDisplayName
    {
        public string PropertyName;
        public string DisplayName;
        public PropertyNameAndDisplayName(string PropertyName, string DisplayName)
        {
            this.PropertyName = PropertyName;
            this.DisplayName = DisplayName;
        }
    }


}
#endif