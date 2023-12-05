#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.ShaderSupport;
using UnityEngine;
using YamlDotNet.Core.Tokens;

namespace net.rs64.TexTransTool.TextureAtlas
{
    public class AtlasShaderSupportUtils
    {
        AtlasDefaultShaderSupport _defaultShaderSupport;
        List<IAtlasShaderSupport> _shaderSupports;

        public PropertyBakeSetting BakeSetting = PropertyBakeSetting.NotBake;
        public AtlasShaderSupportUtils(bool DefaultShaderSupportForGetAllTexture = false)
        {
            _defaultShaderSupport = new AtlasDefaultShaderSupport() { GetAllTexture = DefaultShaderSupportForGetAllTexture };
            _shaderSupports = InterfaceUtility.GetInterfaceInstance<IAtlasShaderSupport>(new Type[] { typeof(AtlasDefaultShaderSupport) });
        }

        public void AddRecord(Material material)
        {
            var supportShaderI = FindSupportI(material);
            if (supportShaderI != null)
            {
                supportShaderI.AddRecord(material);
            }
        }
        public void ClearRecord()
        {
            foreach (var i in _shaderSupports)
            {
                i.ClearRecord();
            }
        }

        public List<PropAndTexture> GetTextures(Material material, ITextureManager textureManager)
        {
            List<PropAndTexture> allTex;
            var supportShaderI = FindSupportI(material);

            if (supportShaderI != null) { allTex = supportShaderI.GetPropertyAndTextures(textureManager, material, BakeSetting); }
            else { allTex = _defaultShaderSupport.GetPropertyAndTextures(textureManager, material, BakeSetting); }

            var textures = new List<PropAndTexture>();
            foreach (var tex in allTex)
            {
                if (tex.Texture != null)
                {
                    textures.Add(tex);
                }
            }

            return textures;
        }
        public void MaterialCustomSetting(Material material)
        {
            var supportShaderI = FindSupportI(material);
            if (supportShaderI != null)
            {
                supportShaderI.MaterialCustomSetting(material);
            }
        }
        public IAtlasShaderSupport FindSupportI(Material material)
        {
            return _shaderSupports.Find(i => { return i.IsThisShader(material); });
        }
    }

    public class AtlasShaderRecorder
    {
        Dictionary<string, PropRecord> propRecords = new Dictionary<string, PropRecord>();


        //ここではテクスチャーは Texture2Dのみ扱う

        //Color
        //NormalizedFloat
        //UnNormalizedFloat

        public delegate bool ValueEqualityComparer<Value>(Value l, Value r);

        public PropRecordAndValue<Value> AddRecord<Value>(Material mat, string texturePropertyName, Value additionalPropertyValue, ValueEqualityComparer<Value> equalityComparer)
        where Value : struct
        {
            if (!mat.HasProperty(texturePropertyName)) { return null; }
            if (propRecords.ContainsKey(texturePropertyName)) { propRecords[texturePropertyName] = new PropRecordAndValue<Value>(); }

            var record = propRecords[texturePropertyName];
            var vRecord = record as PropRecordAndValue<Value>;

            var texture = mat.GetTexture(texturePropertyName) as Texture2D;
            vRecord.ContainsTexture |= texture != null;

            if (!vRecord.IsDifferenceValue.HasValue)
            {
                vRecord.IsDifferenceValue = false;
                vRecord.RecordValue = additionalPropertyValue;
            }
            else
            {
                vRecord.IsDifferenceValue |= !equalityComparer.Invoke(vRecord.RecordValue, additionalPropertyValue);
            }
            return vRecord;
        }

        public PropRecordAndValue<Value> GetRecord<Value>(string texturePropertyName) where Value : struct
        {
            if (!propRecords.ContainsKey(texturePropertyName)) { return null; }
            return propRecords[texturePropertyName] as PropRecordAndValue<Value>;
        }


        //テクスチャーのプロパティに付随する何かが値の違いがあるかやテクスチャが存在するかなどを記録する
        public abstract class PropRecord
        {
            public bool ContainsTexture;
            public bool? IsDifferenceValue;
        }
        public class PropRecordAndValue<Value> : PropRecord
        where Value : struct
        {
            public Value RecordValue;
        }
    }
}
#endif
