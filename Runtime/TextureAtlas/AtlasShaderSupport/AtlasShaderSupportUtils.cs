using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEngine;
using TexLU = net.rs64.TexTransCore.BlendTexture.TextureBlend;
using TexUT = net.rs64.TexTransCore.TransTextureCore.Utils.TextureUtility;

namespace net.rs64.TexTransTool.TextureAtlas
{
    internal class AtlasShaderSupportUtils
    {
        AtlasDefaultShaderSupport _defaultShaderSupport;
        List<IAtlasShaderSupport> _shaderSupports;

        public PropertyBakeSetting BakeSetting = PropertyBakeSetting.NotBake;
        public AtlasShaderSupportUtils()
        {
            _defaultShaderSupport = new AtlasDefaultShaderSupport();
            _shaderSupports = InterfaceUtility.GetInterfaceInstance<IAtlasShaderSupport>(new Type[] { typeof(AtlasDefaultShaderSupport) }).ToList();
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

    internal class AtlasShaderRecorder
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
    internal class TextureBaker
    {
        Material material;
        IOriginTexture textureManager;
        Dictionary<string, Texture> propEnvs;
        AtlasShaderRecorder atlasShaderRecorder;
        PropertyBakeSetting bakeSetting;
        public TextureBaker(IOriginTexture texManager, Dictionary<string, Texture> propEnvDict, Material mat, AtlasShaderRecorder shaderRecorder, PropertyBakeSetting bakeSettingEnum)
        {
            textureManager = texManager;
            propEnvs = propEnvDict;
            material = mat;
            atlasShaderRecorder = shaderRecorder;
            bakeSetting = bakeSettingEnum;
        }

        public void ColorMul(string texPropName, string colorPropName)
        {
            var color = material.GetColor(colorPropName);
            var record = atlasShaderRecorder.GetRecord(texPropName) as AtlasShaderRecorder.PropRecordAndValue<Color>;

            if (!record.IsDifferenceValue.HasValue) { return; }
            if (!record.IsDifferenceValue.Value) { return; }

            var texture = propEnvs.ContainsKey(texPropName) ? propEnvs[texPropName] : null;

            if (texture == null)
            {
                if (record.ContainsTexture || bakeSetting == PropertyBakeSetting.BakeAllProperty)
                {
                    propEnvs[texPropName] = TexUT.CreateColorTexForRT(color);
                }
            }
            else
            {
                var originTexture = texture is Texture2D ? textureManager.GetOriginTempRt(texture as Texture2D, texture.width) : texture;
                propEnvs[texPropName] = TexLU.CreateMultipliedRenderTexture(originTexture, color);
                if (originTexture is RenderTexture tempRT) { RenderTexture.ReleaseTemporary(tempRT); }
            }
        }

        public void FloatMul(string texPropName, string floatProp)
        {
            var propFloat = material.GetFloat(floatProp);
            var record = atlasShaderRecorder.GetRecord(texPropName) as AtlasShaderRecorder.PropRecordAndValue<float>;
            var propTex = propEnvs.ContainsKey(texPropName) ? propEnvs[texPropName] : null;

            if (!record.IsDifferenceValue.HasValue || !record.IsDifferenceValue.Value)
            {
                if (propTex == null && record.ContainsTexture)
                {
                    propEnvs[texPropName] = TexUT.CreateColorTexForRT(new Color(propFloat, propFloat, propFloat, propFloat));
                }

                return;
            }

            if (propTex == null)
            {
                if (record.ContainsTexture || bakeSetting == PropertyBakeSetting.BakeAllProperty)
                {
                    propEnvs[texPropName] = TexUT.CreateColorTexForRT(new Color(propFloat, propFloat, propFloat, propFloat));
                }
            }
            else
            {
                var originPropTex = propTex is Texture2D ? textureManager.GetOriginTempRt(propTex as Texture2D, propTex.width) : propTex;
                propEnvs[texPropName] = TexLU.CreateMultipliedRenderTexture(originPropTex, new Color(propFloat, propFloat, propFloat, propFloat));
                if (originPropTex is RenderTexture tempRT) { RenderTexture.ReleaseTemporary(tempRT); }
            }
        }

        public void ColorMulAndHSVG(string texPropName, string colorPropName, string hsvgPropName)
        {
            var color = material.GetColor(colorPropName);
            var record = atlasShaderRecorder.GetRecord(texPropName) as AtlasShaderRecorder.PropRecordAndTowValue<Color>;

            var texture = propEnvs.ContainsKey(texPropName) ? propEnvs[texPropName] : null;

            if (record.IsDifferenceValue.HasValue && record.IsDifferenceValue.Value)
            {
                if (texture == null)
                {
                    if (record.ContainsTexture || bakeSetting == PropertyBakeSetting.BakeAllProperty)
                    {
                        propEnvs[texPropName] = TexUT.CreateColorTexForRT(color);
                    }
                }
                else
                {
                    var originPropTex = texture is Texture2D ? textureManager.GetOriginTempRt(texture as Texture2D, texture.width) : texture;
                    texture = TexLU.CreateMultipliedRenderTexture(originPropTex, color);
                    if (originPropTex is RenderTexture tempRT) { RenderTexture.ReleaseTemporary(tempRT); }
                }
            }

            if (record.IsDifferenceValue2.HasValue && record.IsDifferenceValue2.Value)
            {
                var colorAdjustMask = propEnvs.ContainsKey("_MainColorAdjustMask") ? propEnvs["_MainColorAdjustMask"] : null;

                var colorAdjustMat = new Material(Shader.Find("Hidden/ColorAdjustShader"));
                if (colorAdjustMask != null) { colorAdjustMat.SetTexture("_Mask", colorAdjustMask); }
                colorAdjustMat.SetColor("_HSVG", material.GetColor(hsvgPropName));

                if (texture is Texture2D texture2d && texture2d != null)
                {
                    var textureRt = RenderTexture.GetTemporary(texture2d.width, texture2d.height, 0);
                    Graphics.Blit(texture2d, textureRt, colorAdjustMat);
                    texture = textureRt;
                }
                else if (texture is RenderTexture textureRt && textureRt != null)
                {
                    var swapRt = RenderTexture.GetTemporary(textureRt.descriptor);

                    Graphics.CopyTexture(textureRt, swapRt);
                    Graphics.Blit(swapRt, textureRt, colorAdjustMat);

                    RenderTexture.ReleaseTemporary(swapRt);
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

            var outlineWidthMask = propEnvs.ContainsKey(texPropName) ? propEnvs[texPropName] : null;

            if (!record.IsDifferenceValue.HasValue || !record.IsDifferenceValue.Value)
            {
                //ほかはテクスチャが存在しているがfloatの値が変わっていない場合にフォールバック用の値を書き込んだものを作らないといけない。
                if (outlineWidthMask == null && record.ContainsTexture)
                {
                    propEnvs[texPropName] = TexUT.CreateColorTexForRT(new Color(outlineWidth, outlineWidth, outlineWidth, outlineWidth));
                }

                return;
            }

            if (outlineWidthMask == null)
            {
                if (record.ContainsTexture || bakeSetting == PropertyBakeSetting.BakeAllProperty)
                {
                    outlineWidthMask = TexUT.CreateColorTexForRT(new Color(outlineWidth, outlineWidth, outlineWidth, outlineWidth));
                }
            }
            else
            {
                var originPropTex = outlineWidthMask is Texture2D ? textureManager.GetOriginTempRt(outlineWidthMask as Texture2D, outlineWidthMask.width) : outlineWidthMask;
                outlineWidthMask = TexLU.CreateMultipliedRenderTexture(originPropTex, new Color(outlineWidth, outlineWidth, outlineWidth, outlineWidth));
                if (originPropTex is RenderTexture tempRT) { RenderTexture.ReleaseTemporary(tempRT); }
            }
            propEnvs[texPropName] = outlineWidthMask;
        }
    }
}
