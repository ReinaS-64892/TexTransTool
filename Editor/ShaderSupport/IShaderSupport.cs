#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;


namespace net.rs64.TexTransTool.ShaderSupport
{
    /// <summary>
    /// これらinterfaceは非常に実験的なAPIで予告なく変更や削除される可能性があります。
    ///
    /// この interface は Decal などのUIで Enum のように選択できる PropertyName を追加できるAPIで
    /// <see cref="ShaderName"/> に Shader の名前を、
    /// <see cref="GetPropertyNames"/> は PropertyName と DisplayName のペア(タプル)の配列を
    /// 用意することで追加できます。
    /// 参考 <see cref="liltoonSupport"/>
    /// </summary>
    public interface IShaderSupport
    {
        string ShaderName { get; }

        (string PropertyName, string DisplayName)[] GetPropertyNames { get; }// PropertyNames - DisplayName
    }


}
#endif
