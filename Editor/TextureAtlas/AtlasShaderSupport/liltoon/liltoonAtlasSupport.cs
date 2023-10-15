#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TexLU = net.rs64.TexTransCore.BlendTexture.TextureBlendUtils;

using net.rs64.TexTransTool.Utils;


namespace net.rs64.TexTransTool.TextureAtlas
{
    public class liltoonAtlasSupport : IAtlasShaderSupport
    {
        public bool IsThisShader(Material material)
        {
            return material.shader.name.Contains("lilToon");
        }
        public void MaterialCustomSetting(Material material)
        {
            var mainTex = material.GetTexture("_MainTex") as Texture2D;
            material.SetTexture("_BaseMap", mainTex);
            material.SetTexture("_BaseColorMap", mainTex);
        }

        public List<PropAndTexture> GetPropertyAndTextures(Material material, PropertyBakeSetting bakeSetting)
        {
            var propEnvsDict = new Dictionary<string, Texture>();

            propEnvsDict.Add("_MainTex", material.GetTexture("_MainTex") as Texture2D);
            propEnvsDict.Add("_MainColorAdjustMask", material.GetTexture("_MainColorAdjustMask") as Texture2D);
            if (material.GetFloat("_UseMain2ndTex") > 0.5f)
            {
                propEnvsDict.Add("_Main2ndTex", material.GetTexture("_Main2ndTex") as Texture2D);
                propEnvsDict.Add("_Main2ndBlendMask", material.GetTexture("_Main2ndBlendMask") as Texture2D);
            }
            // PropertyAndTextures.Add(new PropAndTexture("_Main2ndDissolveMask", material.GetTexture("_Main2ndDissolveMask") as Texture2D));
            // PropertyAndTextures.Add(new PropAndTexture("_Main2ndDissolveNoiseMask", material.GetTexture("_Main2ndDissolveNoiseMask") as Texture2D));
            if (material.GetFloat("_UseMain3rdTex") > 0.5f)
            {
                propEnvsDict.Add("_Main3rdTex", material.GetTexture("_Main3rdTex") as Texture2D);
                propEnvsDict.Add("_Main3rdBlendMask", material.GetTexture("_Main3rdBlendMask") as Texture2D);
            }
            // PropertyAndTextures.Add(new PropAndTexture("_Main3rdDissolveMask", material.GetTexture("_Main3rdDissolveMask") as Texture2D));
            // PropertyAndTextures.Add(new PropAndTexture("_Main3rdDissolveNoiseMask", material.GetTexture("_Main3rdDissolveNoiseMask") as Texture2D));
            propEnvsDict.Add("_AlphaMask", material.GetTexture("_AlphaMask") as Texture2D);
            if (material.GetFloat("_UseBumpMap") > 0.5f)
            {
                propEnvsDict.Add("_BumpMap", material.GetTexture("_BumpMap") as Texture2D);
            }
            if (material.GetFloat("_UseBump2ndMap") > 0.5f)
            {
                propEnvsDict.Add("_Bump2ndMap", material.GetTexture("_Bump2ndMap") as Texture2D);
                propEnvsDict.Add("_Bump2ndScaleMask", material.GetTexture("_Bump2ndScaleMask") as Texture2D);
            }
            if (material.GetFloat("_UseAnisotropy") > 0.5f)
            {
                propEnvsDict.Add("_AnisotropyTangentMap", material.GetTexture("_AnisotropyTangentMap") as Texture2D);
                propEnvsDict.Add("_AnisotropyScaleMask", material.GetTexture("_AnisotropyScaleMask") as Texture2D);
            }
            if (material.GetFloat("_UseBacklight") > 0.5f)
            {
                propEnvsDict.Add("_BacklightColorTex", material.GetTexture("_BacklightColorTex") as Texture2D);
            }
            if (material.GetFloat("_UseShadow") > 0.5f)
            {
                propEnvsDict.Add("_ShadowStrengthMask", material.GetTexture("_ShadowStrengthMask") as Texture2D);
                propEnvsDict.Add("_ShadowBorderMask", material.GetTexture("_ShadowBorderMask") as Texture2D);
                propEnvsDict.Add("_ShadowBlurMask", material.GetTexture("_ShadowBlurMask") as Texture2D);
                propEnvsDict.Add("_ShadowColorTex", material.GetTexture("_ShadowColorTex") as Texture2D);
                propEnvsDict.Add("_Shadow2ndColorTex", material.GetTexture("_Shadow2ndColorTex") as Texture2D);
                propEnvsDict.Add("_Shadow3rdColorTex", material.GetTexture("_Shadow3rdColorTex") as Texture2D);
            }
            if (material.GetFloat("_UseReflection") > 0.5f)
            {
                propEnvsDict.Add("_SmoothnessTex", material.GetTexture("_SmoothnessTex") as Texture2D);
                propEnvsDict.Add("_MetallicGlossMap", material.GetTexture("_MetallicGlossMap") as Texture2D);
                propEnvsDict.Add("_ReflectionColorTex", material.GetTexture("_ReflectionColorTex") as Texture2D);
            }
            // PropertyAndTextures.Add(new PropAndTexture("_MatCapBlendMask", material.GetTexture("_MatCapBlendMask") as Texture2D));
            // PropertyAndTextures.Add(new PropAndTexture("_MatCap2ndBlendMask", material.GetTexture("_MatCap2ndBlendMask") as Texture2D));
            if (material.GetFloat("_UseRim") > 0.5f)
            {
                propEnvsDict.Add("_RimColorTex", material.GetTexture("_RimColorTex") as Texture2D);
            }
            if (material.GetFloat("_UseGlitter") > 0.5f)
            {
                propEnvsDict.Add("_GlitterColorTex", material.GetTexture("_GlitterColorTex") as Texture2D);
            }
            // PropertyAndTextures.Add(new PropAndTexture("_GlitterShapeTex", material.GetTexture("_GlitterShapeTex") as Texture2D));
            if (material.GetFloat("_UseEmission") > 0.5f)
            {
                propEnvsDict.Add("_EmissionMap", material.GetTexture("_EmissionMap") as Texture2D);
                propEnvsDict.Add("_EmissionBlendMask", material.GetTexture("_EmissionBlendMask") as Texture2D);
            }
            // PropertyAndTextures.Add(new PropAndTexture("_EmissionGradTex", material.GetTexture("_EmissionGradTex") as Texture2D));
            if (material.GetFloat("_UseEmission2nd") > 0.5f)
            {
                propEnvsDict.Add("_Emission2ndMap", material.GetTexture("_Emission2ndMap") as Texture2D);
                propEnvsDict.Add("_Emission2ndBlendMask", material.GetTexture("_Emission2ndBlendMask") as Texture2D);
            }
            // PropertyAndTextures.Add(new PropAndTexture("_Emission2ndGradTex", material.GetTexture("_Emission2ndGradTex") as Texture2D));
            // PropEnvsDict.Add("_ParallaxMap", material.GetTexture("_ParallaxMap") as Texture2D);
            if (material.GetFloat("_UseAudioLink") > 0.5f)
            {
                propEnvsDict.Add("_AudioLinkMask", material.GetTexture("_AudioLinkMask") as Texture2D);
            }
            // PropertyAndTextures.Add(new PropAndTexture("_DissolveMask", material.GetTexture("_DissolveMask") as Texture2D));
            // PropertyAndTextures.Add(new PropAndTexture("_DissolveNoiseMask", material.GetTexture("_DissolveNoiseMask") as Texture2D));
            if (material.shader.name.Contains("Outline"))
            {
                propEnvsDict.Add("_OutlineTex", material.GetTexture("_OutlineTex") as Texture2D);
                propEnvsDict.Add("_OutlineWidthMask", material.GetTexture("_OutlineWidthMask") as Texture2D);
                propEnvsDict.Add("_OutlineVectorTex", material.GetTexture("_OutlineVectorTex") as Texture2D);
            }
            if (material.shader.name.Contains("Gem"))
            {
                if (!propEnvsDict.ContainsKey("_SmoothnessTex")) propEnvsDict.Add("_SmoothnessTex", material.GetTexture("_SmoothnessTex") as Texture2D);
            }
            if (material.shader.name.Contains("Fur"))
            {
                propEnvsDict.Add("_FurLengthMask", material.GetTexture("_FurLengthMask") as Texture2D);
                propEnvsDict.Add("_FurMask", material.GetTexture("_FurMask") as Texture2D);
                propEnvsDict.Add("_FurNoiseMask", material.GetTexture("_FurNoiseMask") as Texture2D);
                propEnvsDict.Add("_FurVectorTex", material.GetTexture("_FurVectorTex") as Texture2D);
            }
            if (bakeSetting == PropertyBakeSetting.NotBake)
            {
                var propAndTexture2 = new List<PropAndTexture>();
                foreach (var PropEnv in propEnvsDict)
                {
                    propAndTexture2.Add(new PropAndTexture(PropEnv.Key, PropEnv.Value));
                }
                return propAndTexture2;
            }

            void ColorMul(string TexPropName, string ColorPropName, bool AlreadyTex)
            {
                var Color = material.GetColor(ColorPropName);

                var MainTex = propEnvsDict.ContainsKey(TexPropName) ? propEnvsDict[TexPropName] : null;
                if (MainTex == null)
                {
                    if (AlreadyTex || bakeSetting == PropertyBakeSetting.BakeAllProperty)
                    {
                        propEnvsDict[TexPropName] = TexLU.CreateColorTex(Color);
                    }
                }
                else
                {
                    propEnvsDict[TexPropName] = TexLU.CreateMultipliedRenderTexture(MainTex.TryGetUnCompress(), Color);
                }
            }
            void FloatMul(string TexPropName, string FloatProp, bool AlreadyTex)
            {
                var PropFloat = material.GetFloat(FloatProp);

                var PropTex = propEnvsDict.ContainsKey(TexPropName) ? propEnvsDict[TexPropName] : null;
                if (PropTex == null)
                {
                    if (AlreadyTex || bakeSetting == PropertyBakeSetting.BakeAllProperty)
                    {
                        propEnvsDict[TexPropName] = TexLU.CreateColorTex(new Color(PropFloat, PropFloat, PropFloat, PropFloat));
                    }
                }
                else
                {
                    propEnvsDict[TexPropName] = TexLU.CreateMultipliedRenderTexture(PropTex.TryGetUnCompress(), new Color(PropFloat, PropFloat, PropFloat, PropFloat));
                }
            }

            if (lilDifferenceRecordI.IsDifference_MainColor)
            {
                ColorMul("_MainTex", "_Color", lilDifferenceRecordI.IsAlreadyTex_MainColor);
            }
            if (lilDifferenceRecordI.IsDifference_MainTexHSVG)
            {
                var MainTex = propEnvsDict.ContainsKey("_MainTex") ? propEnvsDict["_MainTex"] : null;
                var ColorAdjustMask = propEnvsDict.ContainsKey("_MainColorAdjustMask") ? propEnvsDict["_MainColorAdjustMask"] : null;

                var Mat = new Material(Shader.Find("Hidden/ColorAdjustShader"));
                if (ColorAdjustMask != null) { Mat.SetTexture("_Mask", ColorAdjustMask); }
                Mat.SetColor("_HSVG", material.GetColor("_MainTexHSVG"));

                if (MainTex is Texture2D MainTex2d && MainTex2d != null)
                {
                    var MainTexRt = new RenderTexture(MainTex2d.width, MainTex2d.height, 0, RenderTextureFormat.ARGB32);
                    Graphics.Blit(MainTex2d, MainTexRt, Mat);
                    if (propEnvsDict.ContainsKey("_MainTex")) { propEnvsDict["_MainTex"] = MainTexRt; }
                    else { propEnvsDict.Add("_MainTex", MainTexRt); }
                }
                else if (MainTex is RenderTexture MainTexRt && MainTexRt != null)
                {
                    var SwapRt = new RenderTexture(MainTexRt.descriptor);

                    Graphics.CopyTexture(MainTex, SwapRt);
                    Graphics.Blit(SwapRt, MainTexRt, Mat);
                }
            }
            if (lilDifferenceRecordI.IsDifference_MainColor2nd && material.GetFloat("_UseMain2ndTex") > 0.5f)
            {
                ColorMul("_Main2ndTex", "_Color2nd", lilDifferenceRecordI.IsAlreadyTex_MainColor2nd);
            }
            if (lilDifferenceRecordI.IsDifference_MainColor3rd && material.GetFloat("_UseMain3rdTex") > 0.5f)
            {
                ColorMul("_Main3rdTex", "_Color3rd", lilDifferenceRecordI.IsAlreadyTex_MainColor3rd);
            }
            if (material.GetFloat("_UseShadow") > 0.5f)
            {
                if (lilDifferenceRecordI.IsDifference_ShadowStrength)
                {
                    FloatMul("_ShadowStrengthMask", "_ShadowStrength", lilDifferenceRecordI.IsAlreadyTex_ShadowStrength);
                }
                if (lilDifferenceRecordI.IsDifference_ShadowColor)
                {
                    ColorMul("_ShadowColorTex", "_ShadowColor", lilDifferenceRecordI.IsAlreadyTex_ShadowColor);
                }
                if (lilDifferenceRecordI.IsDifference_Shadow2ndColor)
                {
                    ColorMul("_Shadow2ndColorTex", "_Shadow2ndColor", lilDifferenceRecordI.IsAlreadyTex_Shadow2ndColor);
                }
                if (lilDifferenceRecordI.IsDifference_Shadow3rdColor)
                {
                    ColorMul("_Shadow3rdColorTex", "_Shadow3rdColor", lilDifferenceRecordI.IsAlreadyTex_Shadow3rdColor);
                }
            }
            if (material.GetFloat("_UseEmission") > 0.5f)
            {
                if (lilDifferenceRecordI.IsDifference_EmissionColor)
                {
                    ColorMul("_EmissionMap", "_EmissionColor", lilDifferenceRecordI.IsAlreadyTex_EmissionColor);
                }
                if (lilDifferenceRecordI.IsDifference_EmissionBlend)
                {
                    FloatMul("_EmissionBlendMask", "_EmissionBlend", lilDifferenceRecordI.IsAlreadyTex_EmissionBlend);
                }
            }
            if (material.GetFloat("_UseEmission2nd") > 0.5f)
            {
                if (lilDifferenceRecordI.IsDifference_Emission2ndColor)
                {
                    ColorMul("_Emission2ndMap", "_Emission2ndColor", lilDifferenceRecordI.IsAlreadyTex_Emission2ndColor);
                }
                if (lilDifferenceRecordI.IsDifference_Emission2ndBlend)
                {
                    FloatMul("_Emission2ndBlendMask", "_Emission2ndBlend", lilDifferenceRecordI.IsAlreadyTex_Emission2ndBlend);
                }
            }
            if (lilDifferenceRecordI.IsDifference_AnisotropyScale && material.GetFloat("_UseAnisotropy") > 0.5f)
            {
                FloatMul("_AnisotropyScaleMask", "_AnisotropyScale", lilDifferenceRecordI.IsAlreadyTex_AnisotropyScale);
            }
            if (lilDifferenceRecordI.IsDifference_BacklightColor && material.GetFloat("_UseBacklight") > 0.5f)
            {
                ColorMul("_BacklightColorTex", "_BacklightColor", lilDifferenceRecordI.IsAlreadyTex_BacklightColor);
            }
            if (material.GetFloat("_UseReflection") > 0.5f)
            {
                if (lilDifferenceRecordI.IsDifference_Smoothness)
                {
                    FloatMul("_SmoothnessTex", "_Smoothness", lilDifferenceRecordI.IsAlreadyTex_Smoothness);
                }
                if (lilDifferenceRecordI.IsDifference_Metallic)
                {
                    FloatMul("_MetallicGlossMap", "_Metallic", lilDifferenceRecordI.IsAlreadyTex_Metallic);
                }
                if (lilDifferenceRecordI.IsDifference_ReflectionColor)
                {
                    ColorMul("_ReflectionColorTex", "_ReflectionColor", lilDifferenceRecordI.IsAlreadyTex_ReflectionColor);
                }
            }
            if (lilDifferenceRecordI.IsDifference_RimColor && material.GetFloat("_UseRim") > 0.5f)
            {
                ColorMul("_RimColorTex", "_RimColor", lilDifferenceRecordI.IsAlreadyTex_RimColor);
            }
            if (lilDifferenceRecordI.IsDifference_GlitterColor && material.GetFloat("_UseGlitter") > 0.5f)
            {
                ColorMul("_GlitterColorTex", "_GlitterColor", lilDifferenceRecordI.IsAlreadyTex_GlitterColor);
            }
            if (material.shader.name.Contains("Outline"))
            {
                if (lilDifferenceRecordI.IsDifference_OutlineColor)
                {
                    ColorMul("_OutlineTex", "_OutlineColor", lilDifferenceRecordI.IsAlreadyTex_OutlineColor);
                }
                if (lilDifferenceRecordI.IsDifference_OutlineTexHSVG)
                {
                    var outlineTex = propEnvsDict.ContainsKey("_OutlineTex") ? propEnvsDict["_OutlineTex"] : null;

                    var Mat = new Material(Shader.Find("Hidden/ColorAdjustShader"));
                    Mat.SetColor("_HSVG", material.GetColor("_MainTexHSVG"));

                    if (outlineTex is Texture2D MainTex2d && MainTex2d != null)
                    {
                        var MainTexRt = new RenderTexture(MainTex2d.width, MainTex2d.height, 0, RenderTextureFormat.ARGB32);
                        Graphics.Blit(MainTex2d, MainTexRt, Mat);
                        if (propEnvsDict.ContainsKey("_OutlineTex")) { propEnvsDict["_OutlineTex"] = MainTexRt; }
                        else { propEnvsDict.Add("_OutlineTex", MainTexRt); }
                    }
                    else if (outlineTex is RenderTexture OutlineRt && OutlineRt != null)
                    {
                        var swapRt = new RenderTexture(OutlineRt.descriptor);
                        Graphics.CopyTexture(outlineTex, swapRt);
                        Graphics.Blit(swapRt, OutlineRt, Mat);
                    }
                }
                if (lilDifferenceRecordI.IsDifference_OutlineWidth)
                {
                    var floatProp = "_OutlineWidth";
                    var texPropName = "_OutlineWidthMask";

                    var outlineWidth = material.GetFloat(floatProp) / lilDifferenceRecordI._OutlineWidth;

                    var outlineWidthMask = propEnvsDict.ContainsKey(texPropName) ? propEnvsDict[texPropName] : null;
                    if (outlineWidthMask == null)
                    {
                        if (lilDifferenceRecordI.IsAlreadyTex_OutlineWidth || bakeSetting == PropertyBakeSetting.BakeAllProperty)
                        {
                            var newTex = TexLU.CreateColorTex(new Color(outlineWidth, outlineWidth, outlineWidth, outlineWidth));
                            if (propEnvsDict.ContainsKey(texPropName))
                            {
                                propEnvsDict[texPropName] = newTex;
                            }
                            else
                            {
                                propEnvsDict.Add(texPropName, newTex);
                            }

                        }
                    }
                    else
                    {
                        var newTex = propEnvsDict[texPropName] = TexLU.CreateMultipliedRenderTexture(outlineWidthMask, new Color(outlineWidth, outlineWidth, outlineWidth, outlineWidth));
                        if (propEnvsDict.ContainsKey(texPropName))
                        {
                            propEnvsDict[texPropName] = newTex;
                        }
                        else
                        {
                            propEnvsDict.Add(texPropName, newTex);
                        }

                    }
                }
            }


            var propAndTexture = new List<PropAndTexture>();
            foreach (var PropEnv in propEnvsDict)
            {
                propAndTexture.Add(new PropAndTexture(PropEnv.Key, PropEnv.Value));
            }
            return propAndTexture;
        }

        /*
        対応状況まとめ

        基本的にタイリングやオフセットには対応しない。
        色変更の系統は、白(1,1,1,1)の時、まとめる前と同じ見た目になるようにする。
        マスクなどの系統は、プロパティの値が1の時、まとめてないときと同じようになるようにする。  アウトラインを除く。


        色設定 ---

        < 色Tex * 色Color * (色調補正 * 色補正マスクTex)
        ただし、グラデーションは無理。

        2nd 3nd & BlendMask 2nd 3nd --
        < (2,3)Tex * (2,3)Color
        < (2,3)BlendMaskTex
        カラーなどはまとめるが...UV0でかつ、デカール化されてない場合のみ。

        Dissolve 2nd 3nd
        マットキャップと同じような感じで..適当にまとめるべきではない。

        影設定 ---

        < マスクと強度Tex * マスクと強度Float
        < 影色(1,2,3)Tex * 影色(1,2,3)Color
        マスクと強度　や影色等は色と加味してまとめるが、
        範囲やぼかしなどはどうしようもない。そもそもまとめるべきかというと微妙


        ぼかし量マスクやAOMapは私が全く使ったことがないため、勝手がわからないため保留。

        発光設定 ---

        発光 1nd 2nd --
        < 色/マスクTex * 色Color
        < マスクTex * マスクFloat
        UVModeがUV0の時はまとめる。

        グラデーションはまず無理。
        合成モードはどうにもできない。ユーザーが一番いいのを設定すべき。

        マスクも一応まとめはするが、どのような風になるかは保証できない。

        ノーマルマップ・光沢設定 ---

        ノーマルマップ 1nd 2nd & 異方性反射 --
        < ノーマルマップ(1,2)Tex
        < ノーマルマップ2のマスクと強度
        < 異方性反射ノーマルマップTex
        < 異方性反射強度Tex * 異方性反射強度Float
        まとめはするがどうなるかは保証できない。

        逆光ライト --
        < 色/マスクtex * 色Color

        反射 --
        < 滑らかさTex * 滑らかさFloat
        < 金属度Tex * 金属度Float
        < 色/マスクTex * 色Color
        はまとめる。

        MatCap系統は基本的に無理...そもそもこの方針だとできない。

        リムライト --
        < 色/マスクTex * 色Color

        ラメ --
        UV0の場合のみ。
        < 色/マスクTex * 色Color

        Shape周りは勝手がわからないため保留。

        拡張設定 ---

        輪郭線設定 --
        < 色Tex * 色Color * 色調補正
        < マスクと太さTex * マスクと太さFloat**

        ** 全体を見て全体の最大値を加味した値で補正する。ここだけ特殊な設定をする。

        UV0の時だけまとめる。
        < ノーマルマップ

        視差マップ --
        勝手がわからないため保留

        AudioLink --
        まとめれるものはない。

        Dissolve --
        そもそもまとめるべきではない。

        IDMask --
        ステンシル設定 --
        レンダリング設定 --
        ライトベイク設定 --
        テッセレーション --
        最適化 --

        これらはそもそもまとめれるものではない。


        ファーや屈折、宝石、などの高負荷系統は、独自の設定が強いし、そもそもまとめるべきかというと微妙。分けといたほうが軽いとかがありそうだから。

        マテリアルをマージしない前提で、とりあえずアトラス化はする。

        */

        class lilDifferenceRecord
        {
            public bool IsInitialized = false;

            public Color _Color;
            public bool IsDifference_MainColor;
            public bool IsAlreadyTex_MainColor;

            public Color _MainTexHSVG;
            public bool IsDifference_MainTexHSVG;

            public Color _Color2nd;
            public bool IsDifference_MainColor2nd;
            public bool IsAlreadyTex_MainColor2nd;

            public Color _Color3rd;
            public bool IsDifference_MainColor3rd;
            public bool IsAlreadyTex_MainColor3rd;

            public float _ShadowStrength;
            public bool IsDifference_ShadowStrength;
            public bool IsAlreadyTex_ShadowStrength;

            public Color _ShadowColor;
            public bool IsDifference_ShadowColor;
            public bool IsAlreadyTex_ShadowColor;

            public Color _Shadow2ndColor;
            public bool IsDifference_Shadow2ndColor;
            public bool IsAlreadyTex_Shadow2ndColor;

            public Color _Shadow3rdColor;
            public bool IsDifference_Shadow3rdColor;
            public bool IsAlreadyTex_Shadow3rdColor;

            public Color _EmissionColor;
            public bool IsDifference_EmissionColor;
            public bool IsAlreadyTex_EmissionColor;

            public float _EmissionBlend;
            public bool IsDifference_EmissionBlend;
            public bool IsAlreadyTex_EmissionBlend;

            public Color _Emission2ndColor;
            public bool IsDifference_Emission2ndColor;
            public bool IsAlreadyTex_Emission2ndColor;

            public float _Emission2ndBlend;
            public bool IsDifference_Emission2ndBlend;
            public bool IsAlreadyTex_Emission2ndBlend;

            public float _AnisotropyScale;
            public bool IsDifference_AnisotropyScale;
            public bool IsAlreadyTex_AnisotropyScale;

            public Color _BacklightColor;
            public bool IsDifference_BacklightColor;
            public bool IsAlreadyTex_BacklightColor;

            public float _Smoothness;
            public bool IsDifference_Smoothness;
            public bool IsAlreadyTex_Smoothness;

            public float _Metallic;
            public bool IsDifference_Metallic;
            public bool IsAlreadyTex_Metallic;

            public Color _ReflectionColor;
            public bool IsDifference_ReflectionColor;
            public bool IsAlreadyTex_ReflectionColor;

            public Color _RimColor;
            public bool IsDifference_RimColor;
            public bool IsAlreadyTex_RimColor;

            public Color _GlitterColor;
            public bool IsDifference_GlitterColor;
            public bool IsAlreadyTex_GlitterColor;

            public Color _OutlineColor;
            public bool IsDifference_OutlineColor;
            public bool IsAlreadyTex_OutlineColor;

            public Color _OutlineTexHSVG;
            public bool IsDifference_OutlineTexHSVG;


            public float _OutlineWidth;
            public bool IsDifference_OutlineWidth;
            public bool IsAlreadyTex_OutlineWidth;

        }
        lilDifferenceRecord lilDifferenceRecordI = new lilDifferenceRecord();

        public void AddRecord(Material material)
        {
            if (material == null) return;

            if (!lilDifferenceRecordI.IsInitialized)
            {
                lilDifferenceRecordI._Color = material.GetColor("_Color");
                lilDifferenceRecordI.IsDifference_MainColor = false;

                lilDifferenceRecordI._MainTexHSVG = material.GetColor("_MainTexHSVG");
                lilDifferenceRecordI.IsDifference_MainTexHSVG = false;

                lilDifferenceRecordI.IsAlreadyTex_MainColor = material.GetTexture("_MainTex") != null;

                if (material.GetFloat("_UseMain2ndTex") > 0.5f)
                {
                    lilDifferenceRecordI._Color2nd = material.GetColor("_Color2nd");
                    lilDifferenceRecordI.IsDifference_MainColor2nd = false;
                    lilDifferenceRecordI.IsAlreadyTex_MainColor2nd = material.GetTexture("_Main2ndTex") != null;
                }

                if (material.GetFloat("_UseMain3rdTex") > 0.5f)
                {
                    lilDifferenceRecordI._Color3rd = material.GetColor("_Color3rd");
                    lilDifferenceRecordI.IsDifference_MainColor3rd = false;
                    lilDifferenceRecordI.IsAlreadyTex_MainColor3rd = material.GetTexture("_Main3rdTex") != null;
                }

                if (material.GetFloat("_UseShadow") > 0.5f)
                {
                    lilDifferenceRecordI._ShadowStrength = material.GetFloat("_ShadowStrength");
                    lilDifferenceRecordI.IsDifference_ShadowStrength = false;
                    lilDifferenceRecordI.IsAlreadyTex_ShadowStrength = material.GetTexture("_ShadowStrengthMask") != null;

                    lilDifferenceRecordI._ShadowColor = material.GetColor("_ShadowColor");
                    lilDifferenceRecordI.IsDifference_ShadowColor = false;
                    lilDifferenceRecordI.IsAlreadyTex_ShadowColor = material.GetTexture("_ShadowColorTex") != null;

                    lilDifferenceRecordI._Shadow2ndColor = material.GetColor("_Shadow2ndColor");
                    lilDifferenceRecordI.IsDifference_Shadow2ndColor = false;
                    lilDifferenceRecordI.IsAlreadyTex_Shadow2ndColor = material.GetTexture("_Shadow2ndColorTex") != null;

                    lilDifferenceRecordI._Shadow3rdColor = material.GetColor("_Shadow3rdColor");
                    lilDifferenceRecordI.IsDifference_Shadow3rdColor = false;
                    lilDifferenceRecordI.IsAlreadyTex_Shadow3rdColor = material.GetTexture("_Shadow3rdColorTex") != null;
                }

                if (material.GetFloat("_UseEmission") > 0.5f)
                {
                    lilDifferenceRecordI._EmissionColor = material.GetColor("_EmissionColor");
                    lilDifferenceRecordI.IsDifference_EmissionColor = false;
                    lilDifferenceRecordI.IsAlreadyTex_EmissionColor = material.GetTexture("_EmissionMap") != null;

                    lilDifferenceRecordI._EmissionBlend = material.GetFloat("_EmissionBlend");
                    lilDifferenceRecordI.IsDifference_EmissionBlend = false;
                    lilDifferenceRecordI.IsAlreadyTex_EmissionBlend = material.GetTexture("_EmissionBlendMask") != null;
                }

                if (material.GetFloat("_UseEmission2nd") > 0.5f)
                {
                    lilDifferenceRecordI._Emission2ndColor = material.GetColor("_Emission2ndColor");
                    lilDifferenceRecordI.IsDifference_Emission2ndColor = false;
                    lilDifferenceRecordI.IsAlreadyTex_Emission2ndColor = material.GetTexture("_Emission2ndMap") != null;

                    lilDifferenceRecordI._Emission2ndBlend = material.GetFloat("_Emission2ndBlend");
                    lilDifferenceRecordI.IsDifference_Emission2ndBlend = false;
                    lilDifferenceRecordI.IsAlreadyTex_Emission2ndBlend = material.GetTexture("_Emission2ndBlendMask") != null;
                }

                if (material.GetFloat("_UseAnisotropy") > 0.5f)
                {
                    lilDifferenceRecordI._AnisotropyScale = material.GetFloat("_AnisotropyScale");
                    lilDifferenceRecordI.IsDifference_AnisotropyScale = false;
                    lilDifferenceRecordI.IsAlreadyTex_AnisotropyScale = material.GetTexture("_AnisotropyScaleMask") != null;
                }

                if (material.GetFloat("_UseBacklight") > 0.5f)
                {
                    lilDifferenceRecordI._BacklightColor = material.GetColor("_BacklightColor");
                    lilDifferenceRecordI.IsDifference_BacklightColor = false;
                    lilDifferenceRecordI.IsAlreadyTex_BacklightColor = material.GetTexture("_BacklightColorTex") != null;
                }

                if (material.GetFloat("_UseReflection") > 0.5f)
                {
                    lilDifferenceRecordI._Smoothness = material.GetFloat("_Smoothness");
                    lilDifferenceRecordI.IsDifference_Smoothness = false;
                    lilDifferenceRecordI.IsAlreadyTex_Smoothness = material.GetTexture("_SmoothnessTex") != null;

                    lilDifferenceRecordI._Metallic = material.GetFloat("_Metallic");
                    lilDifferenceRecordI.IsDifference_Metallic = false;
                    lilDifferenceRecordI.IsAlreadyTex_Metallic = material.GetTexture("_MetallicGlossMap") != null;

                    lilDifferenceRecordI._ReflectionColor = material.GetColor("_ReflectionColor");
                    lilDifferenceRecordI.IsDifference_ReflectionColor = false;
                    lilDifferenceRecordI.IsAlreadyTex_ReflectionColor = material.GetTexture("_ReflectionColorTex") != null;
                }

                if (material.GetFloat("_UseRim") > 0.5f)
                {
                    lilDifferenceRecordI._RimColor = material.GetColor("_RimColor");
                    lilDifferenceRecordI.IsDifference_RimColor = false;
                    lilDifferenceRecordI.IsAlreadyTex_RimColor = material.GetTexture("_RimColorTex") != null;
                }

                if (material.GetFloat("_UseGlitter") > 0.5f)
                {
                    lilDifferenceRecordI._GlitterColor = material.GetColor("_GlitterColor");
                    lilDifferenceRecordI.IsDifference_GlitterColor = false;
                    lilDifferenceRecordI.IsAlreadyTex_GlitterColor = material.GetTexture("_GlitterColorTex") != null;
                }

                if (material.shader.name.Contains("Outline"))
                {
                    lilDifferenceRecordI._OutlineColor = material.GetColor("_OutlineColor");
                    lilDifferenceRecordI.IsDifference_OutlineColor = false;
                    lilDifferenceRecordI.IsAlreadyTex_OutlineColor = material.GetTexture("_OutlineTex") != null;

                    lilDifferenceRecordI._OutlineTexHSVG = material.GetColor("_OutlineTexHSVG");
                    lilDifferenceRecordI.IsDifference_OutlineTexHSVG = false;

                    lilDifferenceRecordI._OutlineWidth = material.GetFloat("_OutlineWidth");
                    lilDifferenceRecordI.IsDifference_OutlineWidth = false;
                    lilDifferenceRecordI.IsAlreadyTex_OutlineWidth = material.GetTexture("_OutlineWidthMask") != null;
                }

                if (material.shader.name.Contains("Gem"))
                {
                    lilDifferenceRecordI._Smoothness = material.GetFloat("_Smoothness");
                    lilDifferenceRecordI.IsDifference_Smoothness = false;
                    lilDifferenceRecordI.IsAlreadyTex_Smoothness = material.GetTexture("_SmoothnessTex") != null;
                }

                lilDifferenceRecordI.IsInitialized = true;
            }
            else
            {
                if (lilDifferenceRecordI._Color != material.GetColor("_Color")) lilDifferenceRecordI.IsDifference_MainColor = true;
                if (material.GetTexture("_MainTex") != null) lilDifferenceRecordI.IsAlreadyTex_MainColor = true;
                if (lilDifferenceRecordI._MainTexHSVG != material.GetColor("_MainTexHSVG")) lilDifferenceRecordI.IsDifference_MainTexHSVG = true;
                if (material.GetFloat("_UseMain2ndTex") > 0.5f)
                {
                    if (lilDifferenceRecordI._Color2nd != material.GetColor("_Color2nd")) lilDifferenceRecordI.IsDifference_MainColor2nd = true;
                    if (material.GetTexture("_Main2ndTex") != null) lilDifferenceRecordI.IsAlreadyTex_MainColor2nd = true;
                }
                if (material.GetFloat("_UseMain3rdTex") > 0.5f)
                {
                    if (lilDifferenceRecordI._Color3rd != material.GetColor("_Color3rd")) lilDifferenceRecordI.IsDifference_MainColor3rd = true;
                    if (material.GetTexture("_Main3rdTex") != null) lilDifferenceRecordI.IsAlreadyTex_MainColor3rd = true;
                }
                if (material.GetFloat("_UseShadow") > 0.5f)
                {
                    if (!Mathf.Approximately(lilDifferenceRecordI._ShadowStrength, material.GetFloat("_ShadowStrength"))) lilDifferenceRecordI.IsDifference_ShadowStrength = true;
                    if (material.GetTexture("_ShadowStrengthMask") != null) lilDifferenceRecordI.IsAlreadyTex_ShadowStrength = true;
                    if (lilDifferenceRecordI._ShadowColor != material.GetColor("_ShadowColor")) lilDifferenceRecordI.IsDifference_ShadowColor = true;
                    if (material.GetTexture("_ShadowColorTex") != null) lilDifferenceRecordI.IsAlreadyTex_ShadowColor = true;
                    if (lilDifferenceRecordI._Shadow2ndColor != material.GetColor("_Shadow2ndColor")) lilDifferenceRecordI.IsDifference_Shadow2ndColor = true;
                    if (material.GetTexture("_Shadow2ndColorTex") != null) lilDifferenceRecordI.IsAlreadyTex_Shadow2ndColor = true;
                    if (lilDifferenceRecordI._Shadow3rdColor != material.GetColor("_Shadow3rdColor")) lilDifferenceRecordI.IsDifference_Shadow3rdColor = true;
                    if (material.GetTexture("_Shadow3rdColorTex") != null) lilDifferenceRecordI.IsAlreadyTex_Shadow3rdColor = true;
                }
                if (material.GetFloat("_UseEmission") > 0.5f)
                {
                    if (lilDifferenceRecordI._EmissionColor != material.GetColor("_EmissionColor")) lilDifferenceRecordI.IsDifference_EmissionColor = true;
                    if (material.GetTexture("_EmissionMap") != null) lilDifferenceRecordI.IsAlreadyTex_EmissionColor = true;
                    if (!Mathf.Approximately(lilDifferenceRecordI._EmissionBlend, material.GetFloat("_EmissionBlend"))) lilDifferenceRecordI.IsDifference_EmissionBlend = true;
                    if (material.GetTexture("_EmissionBlendMask") != null) lilDifferenceRecordI.IsAlreadyTex_EmissionBlend = true;
                }
                if (material.GetFloat("_UseEmission2nd") > 0.5f)
                {
                    if (lilDifferenceRecordI._Emission2ndColor != material.GetColor("_Emission2ndColor")) lilDifferenceRecordI.IsDifference_Emission2ndColor = true;
                    if (material.GetTexture("_Emission2ndMap") != null) lilDifferenceRecordI.IsAlreadyTex_Emission2ndColor = true;
                    if (!Mathf.Approximately(lilDifferenceRecordI._Emission2ndBlend, material.GetFloat("_Emission2ndBlend"))) lilDifferenceRecordI.IsDifference_Emission2ndBlend = true;
                    if (material.GetTexture("_Emission2ndBlendMask") != null) lilDifferenceRecordI.IsAlreadyTex_Emission2ndBlend = true;
                }
                if (material.GetFloat("_UseAnisotropy") > 0.5f)
                {
                    if (!Mathf.Approximately(lilDifferenceRecordI._AnisotropyScale, material.GetFloat("_AnisotropyScale"))) lilDifferenceRecordI.IsDifference_AnisotropyScale = true;
                    if (material.GetTexture("_AnisotropyScaleMask") != null) lilDifferenceRecordI.IsAlreadyTex_AnisotropyScale = true;
                }
                if (material.GetFloat("_UseBacklight") > 0.5f)
                {
                    if (lilDifferenceRecordI._BacklightColor != material.GetColor("_BacklightColor")) lilDifferenceRecordI.IsDifference_BacklightColor = true;
                    if (material.GetTexture("_BacklightColorTex") != null) lilDifferenceRecordI.IsAlreadyTex_BacklightColor = true;
                }
                if (material.GetFloat("_UseReflection") > 0.5f)
                {
                    if (!Mathf.Approximately(lilDifferenceRecordI._Smoothness, material.GetFloat("_Smoothness"))) lilDifferenceRecordI.IsDifference_Smoothness = true;
                    if (material.GetTexture("_SmoothnessTex") != null) lilDifferenceRecordI.IsAlreadyTex_Smoothness = true;
                    if (!Mathf.Approximately(lilDifferenceRecordI._Metallic, material.GetFloat("_Metallic"))) lilDifferenceRecordI.IsDifference_Metallic = true;
                    if (material.GetTexture("_MetallicGlossMap") != null) lilDifferenceRecordI.IsAlreadyTex_Metallic = true;
                    if (lilDifferenceRecordI._ReflectionColor != material.GetColor("_ReflectionColor")) lilDifferenceRecordI.IsDifference_ReflectionColor = true;
                    if (material.GetTexture("_ReflectionColorTex") != null) lilDifferenceRecordI.IsAlreadyTex_ReflectionColor = true;
                }

                if (material.GetFloat("_UseRim") > 0.5f)
                {
                    if (lilDifferenceRecordI._RimColor != material.GetColor("_RimColor")) lilDifferenceRecordI.IsDifference_RimColor = true;
                    if (material.GetTexture("_RimColorTex") != null) lilDifferenceRecordI.IsAlreadyTex_RimColor = true;

                }
                if (material.GetFloat("_UseGlitter") > 0.5f)
                {
                    if (lilDifferenceRecordI._GlitterColor != material.GetColor("_GlitterColor")) lilDifferenceRecordI.IsDifference_GlitterColor = true;
                    if (material.GetTexture("_GlitterColorTex") != null) lilDifferenceRecordI.IsAlreadyTex_GlitterColor = true;
                }
                if (material.shader.name.Contains("Outline"))
                {
                    if (lilDifferenceRecordI._OutlineColor != material.GetColor("_OutlineColor")) lilDifferenceRecordI.IsDifference_OutlineColor = true;
                    if (material.GetTexture("_OutlineTex") != null) lilDifferenceRecordI.IsAlreadyTex_OutlineColor = true;
                    if (lilDifferenceRecordI._OutlineTexHSVG != material.GetColor("_OutlineTexHSVG")) lilDifferenceRecordI.IsDifference_OutlineTexHSVG = true;
                    if (!Mathf.Approximately(lilDifferenceRecordI._OutlineWidth, material.GetFloat("_OutlineWidth"))) { lilDifferenceRecordI.IsDifference_OutlineWidth = true; lilDifferenceRecordI._OutlineWidth = Mathf.Max(lilDifferenceRecordI._OutlineWidth, material.GetFloat("_OutlineWidth")); }
                    if (material.GetTexture("_OutlineWidthMask") != null) lilDifferenceRecordI.IsAlreadyTex_OutlineWidth = true;
                }
                if (material.shader.name.Contains("Gem"))
                {
                    if (!Mathf.Approximately(lilDifferenceRecordI._Smoothness, material.GetFloat("_Smoothness"))) lilDifferenceRecordI.IsDifference_Smoothness = true;
                    if (material.GetTexture("_SmoothnessTex") != null) lilDifferenceRecordI.IsAlreadyTex_Smoothness = true;
                }
            }

        }

        public void ClearRecord()
        {
            lilDifferenceRecordI = new lilDifferenceRecord();
        }


    }
}
#endif
