#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.ShaderSupport;
using UnityEngine;
using TexLU = net.rs64.TexTransCore.BlendTexture.TextureBlendUtils;

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
            if (!propRecords.ContainsKey(texturePropertyName)) { propRecords[texturePropertyName] = new PropRecordAndValue<Value>(); }

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
        public PropRecordAndTowValue<Value> AddRecord<Value>(Material mat, string texturePropertyName, Value additionalPropertyValue, Value additionalPropertyValue2, ValueEqualityComparer<Value> equalityComparer)
        where Value : struct
        {
            if (!mat.HasProperty(texturePropertyName)) { return null; }
            if (!propRecords.ContainsKey(texturePropertyName)) { propRecords[texturePropertyName] = new PropRecordAndTowValue<Value>(); }

            var record = propRecords[texturePropertyName];
            var vRecord = record as PropRecordAndTowValue<Value>;

            var texture = mat.GetTexture(texturePropertyName) as Texture2D;
            vRecord.ContainsTexture |= texture != null;

            if (!vRecord.IsDifferenceValue.HasValue)
            {
                vRecord.IsDifferenceValue = false;
                vRecord.RecordValue = additionalPropertyValue;
                vRecord.RecordValue2 = additionalPropertyValue2;
            }
            else
            {
                vRecord.IsDifferenceValue |= !equalityComparer.Invoke(vRecord.RecordValue, additionalPropertyValue);
                vRecord.IsDifferenceValue2 |= !equalityComparer.Invoke(vRecord.RecordValue2, additionalPropertyValue2);
            }
            return vRecord;
        }

        public PropRecord GetRecord(string texturePropertyName)
        {
            if (!propRecords.ContainsKey(texturePropertyName)) { return null; }
            return propRecords[texturePropertyName];
        }


        //テクスチャーのプロパティに付随する何かが値の違いがあるかやテクスチャが存在するかなどを記録する
        public abstract class PropRecord
        {
            public bool ContainsTexture;
        }
        public class PropRecordAndValue<Value> : PropRecord where Value : struct
        {
            public bool? IsDifferenceValue;
            public Value RecordValue;
        }
        public class PropRecordAndTowValue<Value> : PropRecord where Value : struct
        {
            public bool? IsDifferenceValue;
            public Value RecordValue;
            public bool? IsDifferenceValue2;
            public Value RecordValue2;
        }

        public void ClearRecord() { propRecords.Clear(); }
    }
    public class TextureBaker
    {
        Material material;
        ITextureManager textureManager;
        Dictionary<string, Texture> propEnvs;
        AtlasShaderRecorder atlasShaderRecorder;
        PropertyBakeSetting bakeSetting;
        public TextureBaker(ITextureManager texManager, Dictionary<string, Texture> propEnvDict, Material mat, AtlasShaderRecorder shaderRecorder, PropertyBakeSetting bakeSettingEnum)
        {
            textureManager = texManager;
            propEnvs = propEnvDict;
            material = mat;
            atlasShaderRecorder = shaderRecorder;
            bakeSetting = bakeSettingEnum;
        }

        public void ColorMul(string texPropName, string colorPropName)
        {
            var Color = material.GetColor(colorPropName);
            var record = atlasShaderRecorder.GetRecord(texPropName) as AtlasShaderRecorder.PropRecordAndValue<Color>;

            if (!record.IsDifferenceValue.HasValue) { return; }
            if (!record.IsDifferenceValue.Value) { return; }

            var texture = propEnvs.ContainsKey(texPropName) ? propEnvs[texPropName] : null;

            if (texture == null)
            {
                if (record.ContainsTexture || bakeSetting == PropertyBakeSetting.BakeAllProperty)
                {
                    propEnvs[texPropName] = TexLU.CreateColorTex(Color);
                }
            }
            else
            {
                var originTexture = texture is Texture2D ? textureManager.GetOriginalTexture2D(texture as Texture2D) : texture;
                propEnvs[texPropName] = TexLU.CreateMultipliedRenderTexture(originTexture, Color);
            }
        }

        public void FloatMul(string texPropName, string floatProp)
        {
            var PropFloat = material.GetFloat(floatProp);
            var record = atlasShaderRecorder.GetRecord(texPropName) as AtlasShaderRecorder.PropRecordAndValue<float>;

            if (!record.IsDifferenceValue.HasValue) { return; }
            if (!record.IsDifferenceValue.Value) { return; }

            var propTex = propEnvs.ContainsKey(texPropName) ? propEnvs[texPropName] : null;
            if (propTex == null)
            {
                if (record.ContainsTexture || bakeSetting == PropertyBakeSetting.BakeAllProperty)
                {
                    propEnvs[texPropName] = TexLU.CreateColorTex(new Color(PropFloat, PropFloat, PropFloat, PropFloat));
                }
            }
            else
            {
                var originPropTex = propTex is Texture2D ? textureManager.GetOriginalTexture2D(propTex as Texture2D) : propTex;
                propEnvs[texPropName] = TexLU.CreateMultipliedRenderTexture(originPropTex, new Color(PropFloat, PropFloat, PropFloat, PropFloat));
            }
        }

        public void ColorMulAndHSVG(string texPropName, string colorPropName, string hsvgPropName)
        {
            var Color = material.GetColor(colorPropName);
            var record = atlasShaderRecorder.GetRecord(texPropName) as AtlasShaderRecorder.PropRecordAndTowValue<Color>;

            var texture = propEnvs.ContainsKey(texPropName) ? propEnvs[texPropName] : null;

            if (record.IsDifferenceValue.HasValue && record.IsDifferenceValue.Value)
            {
                if (texture == null)
                {
                    if (record.ContainsTexture || bakeSetting == PropertyBakeSetting.BakeAllProperty)
                    {
                        propEnvs[texPropName] = TexLU.CreateColorTex(Color);
                    }
                }
                else
                {
                    var originTexture = texture is Texture2D ? textureManager.GetOriginalTexture2D(texture as Texture2D) : texture;
                    texture = TexLU.CreateMultipliedRenderTexture(originTexture, Color);
                }
            }

            if (record.IsDifferenceValue2.HasValue && record.IsDifferenceValue2.Value)
            {
                var ColorAdjustMask = propEnvs.ContainsKey("_MainColorAdjustMask") ? propEnvs["_MainColorAdjustMask"] : null;

                var colorAdjustMat = new Material(Shader.Find("Hidden/ColorAdjustShader"));
                if (ColorAdjustMask != null) { colorAdjustMat.SetTexture("_Mask", ColorAdjustMask); }
                colorAdjustMat.SetColor("_HSVG", material.GetColor(hsvgPropName));

                if (texture is Texture2D texture2d && texture2d != null)
                {
                    var textureRt = new RenderTexture(texture2d.width, texture2d.height, 0);
                    Graphics.Blit(texture2d, textureRt, colorAdjustMat);
                    texture = textureRt;
                }
                else if (texture is RenderTexture textureRt && textureRt != null)
                {
                    var SwapRt = RenderTexture.GetTemporary(textureRt.descriptor);

                    Graphics.CopyTexture(textureRt, SwapRt);
                    Graphics.Blit(SwapRt, textureRt, colorAdjustMat);

                    RenderTexture.ReleaseTemporary(SwapRt);
                    texture = textureRt;
                }
                UnityEngine.Object.DestroyImmediate(colorAdjustMat);
            }


            propEnvs[texPropName] = texture;
        }

        public void OutlineWidthMul(string texPropName, string floatProp)
        {
            var record = atlasShaderRecorder.GetRecord(texPropName) as AtlasShaderRecorder.PropRecordAndValue<float>;
            var outlineWidth = material.GetFloat(floatProp) / record.RecordValue;

            if (record.IsDifferenceValue.HasValue) { return; }
            if (!record.IsDifferenceValue.Value) { return; }

            var outlineWidthMask = propEnvs.ContainsKey(texPropName) ? propEnvs[texPropName] : null;
            if (outlineWidthMask == null)
            {
                if (record.ContainsTexture || bakeSetting == PropertyBakeSetting.BakeAllProperty)
                {
                    outlineWidthMask = TexLU.CreateColorTex(new Color(outlineWidth, outlineWidth, outlineWidth, outlineWidth));
                }
            }
            else
            {
                outlineWidthMask = TexLU.CreateMultipliedRenderTexture(outlineWidthMask, new Color(outlineWidth, outlineWidth, outlineWidth, outlineWidth));
            }
            propEnvs[texPropName] = outlineWidthMask;
        }
    }
}
#endif
