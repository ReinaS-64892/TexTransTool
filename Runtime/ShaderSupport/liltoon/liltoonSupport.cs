#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Rs64.TexTransTool;
using TexLU = Rs64.TexTransTool.TextureLayerUtil;


namespace Rs64.TexTransTool.ShaderSupport
{
    public class liltoonSupport : IShaderSupport
    {
        public string SupprotShaderName => "lilToon";

        public string DisplayShaderName => SupprotShaderName;

        public PropertyNameAndDisplayName[] GetPropatyNames
        {
            get
            {
                return new PropertyNameAndDisplayName[]{
                new PropertyNameAndDisplayName("_MainTex", "MainTexture"),
                new PropertyNameAndDisplayName("_EmissionMap", "EmissionMap"),

                new PropertyNameAndDisplayName("_Main2ndTex", "2ndMainTexture"),
                new PropertyNameAndDisplayName("_Main3rdTex", "3rdMainTexture"),

                new PropertyNameAndDisplayName("_Emission2ndMap", "Emission2ndMap"),


                new PropertyNameAndDisplayName("_MainColorAdjustMask", "MainColorAdjustMask"),
                new PropertyNameAndDisplayName("_Main2ndBlendMask", "2ndMainBlendMask"),
                new PropertyNameAndDisplayName("_Main3rdBlendMask", "3rdMainBlendMask"),
                new PropertyNameAndDisplayName("_AlphaMask", "AlphaMask"),
                new PropertyNameAndDisplayName("_BumpMap", "NormalMap"),
                new PropertyNameAndDisplayName("_Bump2ndMap", "2ndNormalMap"),
                new PropertyNameAndDisplayName("_Bump2ndScaleMask", "2ndNormalScaleMask"),
                new PropertyNameAndDisplayName("_AnisotropyTangentMap", "AnisotropyTangentMap"),
                new PropertyNameAndDisplayName("_AnisotropyScaleMask", "AnisotropyScaleMask"),
                new PropertyNameAndDisplayName("_BacklightColorTex", "BacklightColorTex"),
                new PropertyNameAndDisplayName("_ShadowStrengthMask", "ShadowStrengthMask"),
                new PropertyNameAndDisplayName("_ShadowBorderMask", "ShadowBorderMask"),
                new PropertyNameAndDisplayName("_ShadowBlurMask", "ShadowBlurMask"),
                new PropertyNameAndDisplayName("_ShadowColorTex", "ShadowColorTex"),
                new PropertyNameAndDisplayName("_Shadow2ndColorTex", "Shadow2ndColorTex"),
                new PropertyNameAndDisplayName("_Shadow3rdColorTex", "Shadow3rdColorTex"),
                new PropertyNameAndDisplayName("_SmoothnessTex", "SmoothnessTex"),
                new PropertyNameAndDisplayName("_MetallicGlossMap", "MetallicGlossMap"),
                new PropertyNameAndDisplayName("_ReflectionColorTex", "ReflectionColorTex"),
                new PropertyNameAndDisplayName("_RimColorTex", "RimColorTex"),
                new PropertyNameAndDisplayName("_GlitterColorTex", "GlitterColorTex"),
                new PropertyNameAndDisplayName("_EmissionBlendMask", "EmissionBlendMask"),
                new PropertyNameAndDisplayName("_Emission2ndBlendMask", "Emission2ndBlendMask"),
                new PropertyNameAndDisplayName("_AudioLinkMask", "AudioLinkMask"),
                new PropertyNameAndDisplayName("_OutlineTex", "OutlineTex"),
                new PropertyNameAndDisplayName("_OutlineWidthMask", "OutlineWidthMask"),
                new PropertyNameAndDisplayName("_OutlineVectorTex", "OutlineVectorTex"),
                };
            }
        }

        public void MaterialCustomSetting(Material material)
        {
            var MainTex = material.GetTexture("_MainTex") as Texture2D;
            material.SetTexture("_BaseMap", MainTex);
            material.SetTexture("_BaseColorMap", MainTex);
        }

        public List<PropAndTexture> GetPropertyAndTextures(Material material, bool IsGNTFMP = false)
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
            void ColorMul(string TexPropName, string ColorPorpName, bool AradeyTex)
            {
                var Color = material.GetColor(ColorPorpName);

                var MainTex = propEnvsDict.ContainsKey(TexPropName) ? propEnvsDict[TexPropName] : null;
                if (MainTex == null)
                {
                    if (AradeyTex || IsGNTFMP)
                    {
                        propEnvsDict[TexPropName] = TexLU.CreateColorTex(Color);
                    }
                }
                else
                {
                    propEnvsDict[TexPropName] = TexLU.CreatMuldRenderTexture(MainTex, Color);
                }
            }
            void FloatMul(string TexPropName, string FloatProp, bool AradeyTex)
            {
                var Propfloat = material.GetFloat(FloatProp);

                var PropTex = propEnvsDict.ContainsKey(TexPropName) ? propEnvsDict[TexPropName] : null;
                if (PropTex == null)
                {
                    if (AradeyTex || IsGNTFMP)
                    {
                        propEnvsDict[TexPropName] = TexLU.CreateColorTex(new Color(Propfloat, Propfloat, Propfloat, Propfloat));
                    }
                }
                else
                {
                    propEnvsDict[TexPropName] = TexLU.CreatMuldRenderTexture(PropTex, new Color(Propfloat, Propfloat, Propfloat, Propfloat));
                }
            }

            if (lilDifferenceRecordI.IsDifference_MainColor)
            {
                ColorMul("_MainTex", "_Color", lilDifferenceRecordI.IsAredyTex_MainColor);
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
                ColorMul("_Main2ndTex", "_Color2nd", lilDifferenceRecordI.IsAredyTex_MainColor2nd);
            }
            if (lilDifferenceRecordI.IsDifference_MainColor3rd && material.GetFloat("_UseMain3rdTex") > 0.5f)
            {
                ColorMul("_Main3rdTex", "_Color3rd", lilDifferenceRecordI.IsAredyTex_MainColor3rd);
            }
            if (material.GetFloat("_UseShadow") > 0.5f)
            {
                if (lilDifferenceRecordI.IsDifference_ShadowStrength)
                {
                    FloatMul("_ShadowStrengthMask", "_ShadowStrength", lilDifferenceRecordI.IsAredyTex_ShadowStrength);
                }
                if (lilDifferenceRecordI.IsDifference_ShadowColor)
                {
                    ColorMul("_ShadowColorTex", "_ShadowColor", lilDifferenceRecordI.IsAredyTex_ShadowColor);
                }
                if (lilDifferenceRecordI.IsDifference_Shadow2ndColor)
                {
                    ColorMul("_Shadow2ndColorTex", "_Shadow2ndColor", lilDifferenceRecordI.IsAredyTex_Shadow2ndColor);
                }
                if (lilDifferenceRecordI.IsDifference_Shadow3rdColor)
                {
                    ColorMul("_Shadow3rdColorTex", "_Shadow3rdColor", lilDifferenceRecordI.IsAredyTex_Shadow3rdColor);
                }
            }
            if (material.GetFloat("_UseEmission") > 0.5f)
            {
                if (lilDifferenceRecordI.IsDifference_EmissionColor)
                {
                    ColorMul("_EmissionMap", "_EmissionColor", lilDifferenceRecordI.IsAredyTex_EmissionColor);
                }
                if (lilDifferenceRecordI.IsDifference_EmissionBlend)
                {
                    FloatMul("_EmissionBlendMask", "_EmissionBlend", lilDifferenceRecordI.IsAredyTex_EmissionBlend);
                }
            }
            if (material.GetFloat("_UseEmission2nd") > 0.5f)
            {
                if (lilDifferenceRecordI.IsDifference_Emission2ndColor)
                {
                    ColorMul("_Emission2ndMap", "_Emission2ndColor", lilDifferenceRecordI.IsAredyTex_Emission2ndColor);
                }
                if (lilDifferenceRecordI.IsDifference_Emission2ndBlend)
                {
                    FloatMul("_Emission2ndBlendMask", "_Emission2ndBlend", lilDifferenceRecordI.IsAredyTex_Emission2ndBlend);
                }
            }
            if (lilDifferenceRecordI.IsDifference_AnisotropyScale && material.GetFloat("_UseAnisotropy") > 0.5f)
            {
                FloatMul("_AnisotropyScaleMask", "_AnisotropyScale", lilDifferenceRecordI.IsAredyTex_AnisotropyScale);
            }
            if (lilDifferenceRecordI.IsDifference_BacklightColor && material.GetFloat("_UseBacklight") > 0.5f)
            {
                ColorMul("_BacklightColorTex", "_BacklightColor", lilDifferenceRecordI.IsAredyTex_BacklightColor);
            }
            if (material.GetFloat("_UseReflection") > 0.5f)
            {
                if (lilDifferenceRecordI.IsDifference_Smoothness)
                {
                    FloatMul("_SmoothnessTex", "_Smoothness", lilDifferenceRecordI.IsAredyTex_Smoothness);
                }
                if (lilDifferenceRecordI.IsDifference_Metallic)
                {
                    FloatMul("_MetallicGlossMap", "_Metallic", lilDifferenceRecordI.IsAredyTex_Metallic);
                }
                if (lilDifferenceRecordI.IsDifference_ReflectionColor)
                {
                    ColorMul("_ReflectionColorTex", "_ReflectionColor", lilDifferenceRecordI.IsAredyTex_ReflectionColor);
                }
            }
            if (lilDifferenceRecordI.IsDifference_RimColor && material.GetFloat("_UseRim") > 0.5f)
            {
                ColorMul("_RimColorTex", "_RimColor", lilDifferenceRecordI.IsAredyTex_RimColor);
            }
            if (lilDifferenceRecordI.IsDifference_GlitterColor && material.GetFloat("_UseGlitter") > 0.5f)
            {
                ColorMul("_GlitterColorTex", "_GlitterColor", lilDifferenceRecordI.IsAredyTex_GlitterColor);
            }
            if (material.shader.name.Contains("Outline"))
            {
                if (lilDifferenceRecordI.IsDifference_OutlineColor)
                {
                    ColorMul("_OutlineTex", "_OutlineColor", lilDifferenceRecordI.IsAredyTex_OutlineColor);
                }
                if (lilDifferenceRecordI.IsDifference_OutlineTexHSVG)
                {
                    var OutlineTex = propEnvsDict.ContainsKey("_OutlineTex") ? propEnvsDict["_OutlineTex"] : null;

                    var Mat = new Material(Shader.Find("Hidden/ColorAdjustShader"));
                    Mat.SetColor("_HSVG", material.GetColor("_MainTexHSVG"));

                    if (OutlineTex is Texture2D MainTex2d && MainTex2d != null)
                    {
                        var MainTexRt = new RenderTexture(MainTex2d.width, MainTex2d.height, 0, RenderTextureFormat.ARGB32);
                        Graphics.Blit(MainTex2d, MainTexRt, Mat);
                        if (propEnvsDict.ContainsKey("_OutlineTex")) { propEnvsDict["_OutlineTex"] = MainTexRt; }
                        else { propEnvsDict.Add("_OutlineTex", MainTexRt); }
                    }
                    else if (OutlineTex is RenderTexture OutlineRt && OutlineRt != null)
                    {
                        var SwapRt = new RenderTexture(OutlineRt.descriptor);
                        Graphics.CopyTexture(OutlineTex, SwapRt);
                        Graphics.Blit(SwapRt, OutlineRt, Mat);
                    }
                }
                if (lilDifferenceRecordI.IsDifference_OutlineWidth)
                {
                    var FloatProp = "_OutlineWidth";
                    var TexPropName = "_OutlineWidthMask";

                    var Outlinewidth = material.GetFloat(FloatProp) / lilDifferenceRecordI._OutlineWidth;

                    var OutlinewidthMask = propEnvsDict.ContainsKey(TexPropName) ? propEnvsDict[TexPropName] : null;
                    if (OutlinewidthMask == null)
                    {
                        if (lilDifferenceRecordI.IsAredyTex_OutlineWidth || IsGNTFMP)
                        {
                            var newtex = TexLU.CreateColorTex(new Color(Outlinewidth, Outlinewidth, Outlinewidth, Outlinewidth));
                            if (propEnvsDict.ContainsKey(TexPropName))
                            {
                                propEnvsDict[TexPropName] = newtex;
                            }
                            else
                            {
                                propEnvsDict.Add(TexPropName, newtex);
                            }

                        }
                    }
                    else
                    {
                        var newtex = propEnvsDict[TexPropName] = TexLU.CreatMuldRenderTexture(OutlinewidthMask, new Color(Outlinewidth, Outlinewidth, Outlinewidth, Outlinewidth));
                        if (propEnvsDict.ContainsKey(TexPropName))
                        {
                            propEnvsDict[TexPropName] = newtex;
                        }
                        else
                        {
                            propEnvsDict.Add(TexPropName, newtex);
                        }

                    }
                }
            }


            var PropAndTexture = new List<PropAndTexture>();
            foreach (var PropEnv in propEnvsDict)
            {
                PropAndTexture.Add(new PropAndTexture(PropEnv.Key, PropEnv.Value));
            }
            return PropAndTexture;
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
        */

        class lilDifferenceRecord
        {
            public bool IsInitilized = false;

            public Color _Color;
            public bool IsDifference_MainColor;
            public bool IsAredyTex_MainColor;

            public Color _MainTexHSVG;
            public bool IsDifference_MainTexHSVG;

            public Color _Color2nd;
            public bool IsDifference_MainColor2nd;
            public bool IsAredyTex_MainColor2nd;

            public Color _Color3rd;
            public bool IsDifference_MainColor3rd;
            public bool IsAredyTex_MainColor3rd;

            public float _ShadowStrength;
            public bool IsDifference_ShadowStrength;
            public bool IsAredyTex_ShadowStrength;

            public Color _ShadowColor;
            public bool IsDifference_ShadowColor;
            public bool IsAredyTex_ShadowColor;

            public Color _Shadow2ndColor;
            public bool IsDifference_Shadow2ndColor;
            public bool IsAredyTex_Shadow2ndColor;

            public Color _Shadow3rdColor;
            public bool IsDifference_Shadow3rdColor;
            public bool IsAredyTex_Shadow3rdColor;

            public Color _EmissionColor;
            public bool IsDifference_EmissionColor;
            public bool IsAredyTex_EmissionColor;

            public float _EmissionBlend;
            public bool IsDifference_EmissionBlend;
            public bool IsAredyTex_EmissionBlend;

            public Color _Emission2ndColor;
            public bool IsDifference_Emission2ndColor;
            public bool IsAredyTex_Emission2ndColor;

            public float _Emission2ndBlend;
            public bool IsDifference_Emission2ndBlend;
            public bool IsAredyTex_Emission2ndBlend;

            public float _AnisotropyScale;
            public bool IsDifference_AnisotropyScale;
            public bool IsAredyTex_AnisotropyScale;

            public Color _BacklightColor;
            public bool IsDifference_BacklightColor;
            public bool IsAredyTex_BacklightColor;

            public float _Smoothness;
            public bool IsDifference_Smoothness;
            public bool IsAredyTex_Smoothness;

            public float _Metallic;
            public bool IsDifference_Metallic;
            public bool IsAredyTex_Metallic;

            public Color _ReflectionColor;
            public bool IsDifference_ReflectionColor;
            public bool IsAredyTex_ReflectionColor;

            public Color _RimColor;
            public bool IsDifference_RimColor;
            public bool IsAredyTex_RimColor;

            public Color _GlitterColor;
            public bool IsDifference_GlitterColor;
            public bool IsAredyTex_GlitterColor;

            public Color _OutlineColor;
            public bool IsDifference_OutlineColor;
            public bool IsAredyTex_OutlineColor;

            public Color _OutlineTexHSVG;
            public bool IsDifference_OutlineTexHSVG;


            public float _OutlineWidth;
            public bool IsDifference_OutlineWidth;
            public bool IsAredyTex_OutlineWidth;

        }
        lilDifferenceRecord lilDifferenceRecordI = new lilDifferenceRecord();

        public void AddRecord(Material material)
        {
            if (material == null) return;

            if (!lilDifferenceRecordI.IsInitilized)
            {
                lilDifferenceRecordI._Color = material.GetColor("_Color");
                lilDifferenceRecordI.IsDifference_MainColor = false;

                lilDifferenceRecordI._MainTexHSVG = material.GetColor("_MainTexHSVG");
                lilDifferenceRecordI.IsDifference_MainTexHSVG = false;

                lilDifferenceRecordI.IsAredyTex_MainColor = material.GetTexture("_MainTex") != null;

                if (material.GetFloat("_UseMain2ndTex") > 0.5f)
                {
                    lilDifferenceRecordI._Color2nd = material.GetColor("_Color2nd");
                    lilDifferenceRecordI.IsDifference_MainColor2nd = false;
                    lilDifferenceRecordI.IsAredyTex_MainColor2nd = material.GetTexture("_Main2ndTex") != null;
                }

                if (material.GetFloat("_UseMain3rdTex") > 0.5f)
                {
                    lilDifferenceRecordI._Color3rd = material.GetColor("_Color3rd");
                    lilDifferenceRecordI.IsDifference_MainColor3rd = false;
                    lilDifferenceRecordI.IsAredyTex_MainColor3rd = material.GetTexture("_Main3rdTex") != null;
                }

                if (material.GetFloat("_UseShadow") > 0.5f)
                {
                    lilDifferenceRecordI._ShadowStrength = material.GetFloat("_ShadowStrength");
                    lilDifferenceRecordI.IsDifference_ShadowStrength = false;
                    lilDifferenceRecordI.IsAredyTex_ShadowStrength = material.GetTexture("_ShadowStrengthMask") != null;

                    lilDifferenceRecordI._ShadowColor = material.GetColor("_ShadowColor");
                    lilDifferenceRecordI.IsDifference_ShadowColor = false;
                    lilDifferenceRecordI.IsAredyTex_ShadowColor = material.GetTexture("_ShadowColorTex") != null;

                    lilDifferenceRecordI._Shadow2ndColor = material.GetColor("_Shadow2ndColor");
                    lilDifferenceRecordI.IsDifference_Shadow2ndColor = false;
                    lilDifferenceRecordI.IsAredyTex_Shadow2ndColor = material.GetTexture("_Shadow2ndColorTex") != null;

                    lilDifferenceRecordI._Shadow3rdColor = material.GetColor("_Shadow3rdColor");
                    lilDifferenceRecordI.IsDifference_Shadow3rdColor = false;
                    lilDifferenceRecordI.IsAredyTex_Shadow3rdColor = material.GetTexture("_Shadow3rdColorTex") != null;
                }

                if (material.GetFloat("_UseEmission") > 0.5f)
                {
                    lilDifferenceRecordI._EmissionColor = material.GetColor("_EmissionColor");
                    lilDifferenceRecordI.IsDifference_EmissionColor = false;
                    lilDifferenceRecordI.IsAredyTex_EmissionColor = material.GetTexture("_EmissionMap") != null;

                    lilDifferenceRecordI._EmissionBlend = material.GetFloat("_EmissionBlend");
                    lilDifferenceRecordI.IsDifference_EmissionBlend = false;
                    lilDifferenceRecordI.IsAredyTex_EmissionBlend = material.GetTexture("_EmissionBlendMask") != null;
                }

                if (material.GetFloat("_UseEmission2nd") > 0.5f)
                {
                    lilDifferenceRecordI._Emission2ndColor = material.GetColor("_Emission2ndColor");
                    lilDifferenceRecordI.IsDifference_Emission2ndColor = false;
                    lilDifferenceRecordI.IsAredyTex_Emission2ndColor = material.GetTexture("_Emission2ndMap") != null;

                    lilDifferenceRecordI._Emission2ndBlend = material.GetFloat("_Emission2ndBlend");
                    lilDifferenceRecordI.IsDifference_Emission2ndBlend = false;
                    lilDifferenceRecordI.IsAredyTex_Emission2ndBlend = material.GetTexture("_Emission2ndBlendMask") != null;
                }

                if (material.GetFloat("_UseAnisotropy") > 0.5f)
                {
                    lilDifferenceRecordI._AnisotropyScale = material.GetFloat("_AnisotropyScale");
                    lilDifferenceRecordI.IsDifference_AnisotropyScale = false;
                    lilDifferenceRecordI.IsAredyTex_AnisotropyScale = material.GetTexture("_AnisotropyScaleMask") != null;
                }

                if (material.GetFloat("_UseBacklight") > 0.5f)
                {
                    lilDifferenceRecordI._BacklightColor = material.GetColor("_BacklightColor");
                    lilDifferenceRecordI.IsDifference_BacklightColor = false;
                    lilDifferenceRecordI.IsAredyTex_BacklightColor = material.GetTexture("_BacklightColorTex") != null;
                }

                if (material.GetFloat("_UseReflection") > 0.5f)
                {
                    lilDifferenceRecordI._Smoothness = material.GetFloat("_Smoothness");
                    lilDifferenceRecordI.IsDifference_Smoothness = false;
                    lilDifferenceRecordI.IsAredyTex_Smoothness = material.GetTexture("_SmoothnessTex") != null;

                    lilDifferenceRecordI._Metallic = material.GetFloat("_Metallic");
                    lilDifferenceRecordI.IsDifference_Metallic = false;
                    lilDifferenceRecordI.IsAredyTex_Metallic = material.GetTexture("_MetallicGlossMap") != null;

                    lilDifferenceRecordI._ReflectionColor = material.GetColor("_ReflectionColor");
                    lilDifferenceRecordI.IsDifference_ReflectionColor = false;
                    lilDifferenceRecordI.IsAredyTex_ReflectionColor = material.GetTexture("_ReflectionColorTex") != null;
                }

                if (material.GetFloat("_UseRim") > 0.5f)
                {
                    lilDifferenceRecordI._RimColor = material.GetColor("_RimColor");
                    lilDifferenceRecordI.IsDifference_RimColor = false;
                    lilDifferenceRecordI.IsAredyTex_RimColor = material.GetTexture("_RimColorTex") != null;
                }

                if (material.GetFloat("_UseGlitter") > 0.5f)
                {
                    lilDifferenceRecordI._GlitterColor = material.GetColor("_GlitterColor");
                    lilDifferenceRecordI.IsDifference_GlitterColor = false;
                    lilDifferenceRecordI.IsAredyTex_GlitterColor = material.GetTexture("_GlitterColorTex") != null;
                }

                if (material.shader.name.Contains("Outline"))
                {
                    lilDifferenceRecordI._OutlineColor = material.GetColor("_OutlineColor");
                    lilDifferenceRecordI.IsDifference_OutlineColor = false;
                    lilDifferenceRecordI.IsAredyTex_OutlineColor = material.GetTexture("_OutlineTex") != null;

                    lilDifferenceRecordI._OutlineTexHSVG = material.GetColor("_OutlineTexHSVG");
                    lilDifferenceRecordI.IsDifference_OutlineTexHSVG = false;

                    lilDifferenceRecordI._OutlineWidth = material.GetFloat("_OutlineWidth");
                    lilDifferenceRecordI.IsDifference_OutlineWidth = false;
                    lilDifferenceRecordI.IsAredyTex_OutlineWidth = material.GetTexture("_OutlineWidthMask") != null;
                }

                lilDifferenceRecordI.IsInitilized = true;
            }
            else
            {
                if (lilDifferenceRecordI._Color != material.GetColor("_Color")) lilDifferenceRecordI.IsDifference_MainColor = true;
                if (material.GetTexture("_MainTex") != null) lilDifferenceRecordI.IsAredyTex_MainColor = true;
                if (lilDifferenceRecordI._MainTexHSVG != material.GetColor("_MainTexHSVG")) lilDifferenceRecordI.IsDifference_MainTexHSVG = true;
                if (material.GetFloat("_UseMain2ndTex") > 0.5f)
                {
                    if (lilDifferenceRecordI._Color2nd != material.GetColor("_Color2nd")) lilDifferenceRecordI.IsDifference_MainColor2nd = true;
                    if (material.GetTexture("_Main2ndTex") != null) lilDifferenceRecordI.IsAredyTex_MainColor2nd = true;
                }
                if (material.GetFloat("_UseMain3rdTex") > 0.5f)
                {
                    if (lilDifferenceRecordI._Color3rd != material.GetColor("_Color3rd")) lilDifferenceRecordI.IsDifference_MainColor3rd = true;
                    if (material.GetTexture("_Main3rdTex") != null) lilDifferenceRecordI.IsAredyTex_MainColor3rd = true;
                }
                if (material.GetFloat("_UseShadow") > 0.5f)
                {
                    if (!Mathf.Approximately(lilDifferenceRecordI._ShadowStrength, material.GetFloat("_ShadowStrength"))) lilDifferenceRecordI.IsDifference_ShadowStrength = true;
                    if (material.GetTexture("_ShadowStrengthMask") != null) lilDifferenceRecordI.IsAredyTex_ShadowStrength = true;
                    if (lilDifferenceRecordI._ShadowColor != material.GetColor("_ShadowColor")) lilDifferenceRecordI.IsDifference_ShadowColor = true;
                    if (material.GetTexture("_ShadowColorTex") != null) lilDifferenceRecordI.IsAredyTex_ShadowColor = true;
                    if (lilDifferenceRecordI._Shadow2ndColor != material.GetColor("_Shadow2ndColor")) lilDifferenceRecordI.IsDifference_Shadow2ndColor = true;
                    if (material.GetTexture("_Shadow2ndColorTex") != null) lilDifferenceRecordI.IsAredyTex_Shadow2ndColor = true;
                    if (lilDifferenceRecordI._Shadow3rdColor != material.GetColor("_Shadow3rdColor")) lilDifferenceRecordI.IsDifference_Shadow3rdColor = true;
                    if (material.GetTexture("_Shadow3rdColorTex") != null) lilDifferenceRecordI.IsAredyTex_Shadow3rdColor = true;
                }
                if (material.GetFloat("_UseEmission") > 0.5f)
                {
                    if (lilDifferenceRecordI._EmissionColor != material.GetColor("_EmissionColor")) lilDifferenceRecordI.IsDifference_EmissionColor = true;
                    if (material.GetTexture("_EmissionMap") != null) lilDifferenceRecordI.IsAredyTex_EmissionColor = true;
                    if (!Mathf.Approximately(lilDifferenceRecordI._EmissionBlend, material.GetFloat("_EmissionBlend"))) lilDifferenceRecordI.IsDifference_EmissionBlend = true;
                    if (material.GetTexture("_EmissionBlendMask") != null) lilDifferenceRecordI.IsAredyTex_EmissionBlend = true;
                }
                if (material.GetFloat("_UseEmission2nd") > 0.5f)
                {
                    if (lilDifferenceRecordI._Emission2ndColor != material.GetColor("_Emission2ndColor")) lilDifferenceRecordI.IsDifference_Emission2ndColor = true;
                    if (material.GetTexture("_Emission2ndMap") != null) lilDifferenceRecordI.IsAredyTex_Emission2ndColor = true;
                    if (!Mathf.Approximately(lilDifferenceRecordI._Emission2ndBlend, material.GetFloat("_Emission2ndBlend"))) lilDifferenceRecordI.IsDifference_Emission2ndBlend = true;
                    if (material.GetTexture("_Emission2ndBlendMask") != null) lilDifferenceRecordI.IsAredyTex_Emission2ndBlend = true;
                }
                if (material.GetFloat("_UseAnisotropy") > 0.5f)
                {
                    if (!Mathf.Approximately(lilDifferenceRecordI._AnisotropyScale, material.GetFloat("_AnisotropyScale"))) lilDifferenceRecordI.IsDifference_AnisotropyScale = true;
                    if (material.GetTexture("_AnisotropyScaleMask") != null) lilDifferenceRecordI.IsAredyTex_AnisotropyScale = true;
                }
                if (material.GetFloat("_UseBacklight") > 0.5f)
                {
                    if (lilDifferenceRecordI._BacklightColor != material.GetColor("_BacklightColor")) lilDifferenceRecordI.IsDifference_BacklightColor = true;
                    if (material.GetTexture("_BacklightColorTex") != null) lilDifferenceRecordI.IsAredyTex_BacklightColor = true;
                }
                if (material.GetFloat("_UseReflection") > 0.5f)
                {
                    if (!Mathf.Approximately(lilDifferenceRecordI._Smoothness, material.GetFloat("_Smoothness"))) lilDifferenceRecordI.IsDifference_Smoothness = true;
                    if (material.GetTexture("_SmoothnessTex") != null) lilDifferenceRecordI.IsAredyTex_Smoothness = true;
                    if (!Mathf.Approximately(lilDifferenceRecordI._Metallic, material.GetFloat("_Metallic"))) lilDifferenceRecordI.IsDifference_Metallic = true;
                    if (material.GetTexture("_MetallicGlossMap") != null) lilDifferenceRecordI.IsAredyTex_Metallic = true;
                    if (lilDifferenceRecordI._ReflectionColor != material.GetColor("_ReflectionColor")) lilDifferenceRecordI.IsDifference_ReflectionColor = true;
                    if (material.GetTexture("_ReflectionColorTex") != null) lilDifferenceRecordI.IsAredyTex_ReflectionColor = true;
                }

                if (material.GetFloat("_UseRim") > 0.5f)
                {
                    if (lilDifferenceRecordI._RimColor != material.GetColor("_RimColor")) lilDifferenceRecordI.IsDifference_RimColor = true;
                    if (material.GetTexture("_RimColorTex") != null) lilDifferenceRecordI.IsAredyTex_RimColor = true;

                }
                if (material.GetFloat("_UseGlitter") > 0.5f)
                {
                    if (lilDifferenceRecordI._GlitterColor != material.GetColor("_GlitterColor")) lilDifferenceRecordI.IsDifference_GlitterColor = true;
                    if (material.GetTexture("_GlitterColorTex") != null) lilDifferenceRecordI.IsAredyTex_GlitterColor = true;
                }
                if (material.shader.name.Contains("Outline"))
                {
                    if (lilDifferenceRecordI._OutlineColor != material.GetColor("_OutlineColor")) lilDifferenceRecordI.IsDifference_OutlineColor = true;
                    if (material.GetTexture("_OutlineTex") != null) lilDifferenceRecordI.IsAredyTex_OutlineColor = true;
                    if (lilDifferenceRecordI._OutlineTexHSVG != material.GetColor("_OutlineTexHSVG")) lilDifferenceRecordI.IsDifference_OutlineTexHSVG = true;
                    if (!Mathf.Approximately(lilDifferenceRecordI._OutlineWidth, material.GetFloat("_OutlineWidth"))) { lilDifferenceRecordI.IsDifference_OutlineWidth = true; lilDifferenceRecordI._OutlineWidth = Mathf.Max(lilDifferenceRecordI._OutlineWidth, material.GetFloat("_OutlineWidth")); }
                    if (material.GetTexture("_OutlineWidthMask") != null) lilDifferenceRecordI.IsAredyTex_OutlineWidth = true;
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