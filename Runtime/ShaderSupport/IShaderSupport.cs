#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Rs64.TexTransTool.ShaderSupport
{
    public interface IShaderSupport
    {
        string SupprotShaderName { get; }
        string DisplayShaderName { get; }

        void AddRecord(Material material);
        void ClearRecord();

        List<PropAndTexture> GetPropertyAndTextures(Material material, bool IsGNTFMP = false);
        void MaterialCustomSetting(Material material);

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