#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;


namespace net.rs64.TexTransTool.ShaderSupport
{
    internal interface IShaderSupport
    {
        string ShaderName { get; }

        (string PropertyName, string DisplayName)[] GetPropertyNames { get; }// PropertyNames - DisplayName
    }


}
#endif
