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

        public List<PropAndTexture> GetPropertyAndTextures(ITextureManager textureManager, Material material, PropertyBakeSetting bakeSetting)
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
            if (material.GetFloat("_UseMatCap") > 0.5f)
            {
                propEnvsDict.Add("_MatCapBlendMask", material.GetTexture("_MatCapBlendMask") as Texture2D);
            }
            if (material.GetFloat("_UseMatCap") > 0.5f)
            {
                propEnvsDict.Add("_MatCap2ndBlendMask", material.GetTexture("_MatCap2ndBlendMask") as Texture2D);
            }
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

                var texture = propEnvsDict.ContainsKey(TexPropName) ? propEnvsDict[TexPropName] : null;
                if (texture == null)
                {
                    if (AlreadyTex || bakeSetting == PropertyBakeSetting.BakeAllProperty)
                    {
                        propEnvsDict[TexPropName] = TexLU.CreateColorTex(Color);
                    }
                }
                else
                {
                    texture = texture is Texture2D ? textureManager.GetOriginalTexture2D(texture as Texture2D) : texture;
                    propEnvsDict[TexPropName] = TexLU.CreateMultipliedRenderTexture(texture, Color);
                }
            }
            void FloatMul(string TexPropName, string FloatProp, bool AlreadyTex)
            {
                var PropFloat = material.GetFloat(FloatProp);

                var propTex = propEnvsDict.ContainsKey(TexPropName) ? propEnvsDict[TexPropName] : null;
                if (propTex == null)
                {
                    if (AlreadyTex || bakeSetting == PropertyBakeSetting.BakeAllProperty)
                    {
                        propEnvsDict[TexPropName] = TexLU.CreateColorTex(new Color(PropFloat, PropFloat, PropFloat, PropFloat));
                    }
                }
                else
                {
                    propTex = propTex is Texture2D ? textureManager.GetOriginalTexture2D(propTex as Texture2D) : propTex;
                    propEnvsDict[TexPropName] = TexLU.CreateMultipliedRenderTexture(propTex, new Color(PropFloat, PropFloat, PropFloat, PropFloat));
                }
            }

            if (lilDifferenceRecorder.IsDifference_MainColor)
            {
                ColorMul("_MainTex", "_Color", lilDifferenceRecorder.IsAlreadyTex_MainColor);
            }
            if (lilDifferenceRecorder.IsDifference_MainTexHSVG)
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
            if (lilDifferenceRecorder.IsDifference_MainColor2nd && material.GetFloat("_UseMain2ndTex") > 0.5f)
            {
                ColorMul("_Main2ndTex", "_Color2nd", lilDifferenceRecorder.IsAlreadyTex_MainColor2nd);
            }
            if (lilDifferenceRecorder.IsDifference_MainColor3rd && material.GetFloat("_UseMain3rdTex") > 0.5f)
            {
                ColorMul("_Main3rdTex", "_Color3rd", lilDifferenceRecorder.IsAlreadyTex_MainColor3rd);
            }
            if (material.GetFloat("_UseShadow") > 0.5f)
            {
                if (lilDifferenceRecorder.IsDifference_ShadowStrength)
                {
                    FloatMul("_ShadowStrengthMask", "_ShadowStrength", lilDifferenceRecorder.IsAlreadyTex_ShadowStrength);
                }
                if (lilDifferenceRecorder.IsDifference_ShadowColor)
                {
                    ColorMul("_ShadowColorTex", "_ShadowColor", lilDifferenceRecorder.IsAlreadyTex_ShadowColor);
                }
                if (lilDifferenceRecorder.IsDifference_Shadow2ndColor)
                {
                    ColorMul("_Shadow2ndColorTex", "_Shadow2ndColor", lilDifferenceRecorder.IsAlreadyTex_Shadow2ndColor);
                }
                if (lilDifferenceRecorder.IsDifference_Shadow3rdColor)
                {
                    ColorMul("_Shadow3rdColorTex", "_Shadow3rdColor", lilDifferenceRecorder.IsAlreadyTex_Shadow3rdColor);
                }
            }
            if (material.GetFloat("_UseEmission") > 0.5f)
            {
                if (lilDifferenceRecorder.IsDifference_EmissionColor)
                {
                    ColorMul("_EmissionMap", "_EmissionColor", lilDifferenceRecorder.IsAlreadyTex_EmissionColor);
                }
                if (lilDifferenceRecorder.IsDifference_EmissionBlend)
                {
                    FloatMul("_EmissionBlendMask", "_EmissionBlend", lilDifferenceRecorder.IsAlreadyTex_EmissionBlend);
                }
            }
            if (material.GetFloat("_UseEmission2nd") > 0.5f)
            {
                if (lilDifferenceRecorder.IsDifference_Emission2ndColor)
                {
                    ColorMul("_Emission2ndMap", "_Emission2ndColor", lilDifferenceRecorder.IsAlreadyTex_Emission2ndColor);
                }
                if (lilDifferenceRecorder.IsDifference_Emission2ndBlend)
                {
                    FloatMul("_Emission2ndBlendMask", "_Emission2ndBlend", lilDifferenceRecorder.IsAlreadyTex_Emission2ndBlend);
                }
            }
            if (lilDifferenceRecorder.IsDifference_AnisotropyScale && material.GetFloat("_UseAnisotropy") > 0.5f)
            {
                FloatMul("_AnisotropyScaleMask", "_AnisotropyScale", lilDifferenceRecorder.IsAlreadyTex_AnisotropyScale);
            }
            if (lilDifferenceRecorder.IsDifference_BacklightColor && material.GetFloat("_UseBacklight") > 0.5f)
            {
                ColorMul("_BacklightColorTex", "_BacklightColor", lilDifferenceRecorder.IsAlreadyTex_BacklightColor);
            }
            if (material.GetFloat("_UseReflection") > 0.5f)
            {
                if (lilDifferenceRecorder.IsDifference_Smoothness)
                {
                    FloatMul("_SmoothnessTex", "_Smoothness", lilDifferenceRecorder.IsAlreadyTex_Smoothness);
                }
                if (lilDifferenceRecorder.IsDifference_Metallic)
                {
                    FloatMul("_MetallicGlossMap", "_Metallic", lilDifferenceRecorder.IsAlreadyTex_Metallic);
                }
                if (lilDifferenceRecorder.IsDifference_ReflectionColor)
                {
                    ColorMul("_ReflectionColorTex", "_ReflectionColor", lilDifferenceRecorder.IsAlreadyTex_ReflectionColor);
                }
            }
            if (material.GetFloat("_UseMatCap") > 0.5f)
            {
                FloatMul("_MatCapBlendMask", "_MatCapBlend", lilDifferenceRecorder.IsAlreadyTex_MatCapBlend);
            }
            if (material.GetFloat("_UseMatCap") > 0.5f)
            {
                FloatMul("_MatCap2ndBlendMask", "_MatCap2ndBlend", lilDifferenceRecorder.IsAlreadyTex_MatCap2ndBlend);
            }
            if (lilDifferenceRecorder.IsDifference_RimColor && material.GetFloat("_UseRim") > 0.5f)
            {
                ColorMul("_RimColorTex", "_RimColor", lilDifferenceRecorder.IsAlreadyTex_RimColor);
            }
            if (lilDifferenceRecorder.IsDifference_GlitterColor && material.GetFloat("_UseGlitter") > 0.5f)
            {
                ColorMul("_GlitterColorTex", "_GlitterColor", lilDifferenceRecorder.IsAlreadyTex_GlitterColor);
            }
            if (material.shader.name.Contains("Outline"))
            {
                if (lilDifferenceRecorder.IsDifference_OutlineColor)
                {
                    ColorMul("_OutlineTex", "_OutlineColor", lilDifferenceRecorder.IsAlreadyTex_OutlineColor);
                }
                if (lilDifferenceRecorder.IsDifference_OutlineTexHSVG)
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
                if (lilDifferenceRecorder.IsDifference_OutlineWidth)
                {
                    var floatProp = "_OutlineWidth";
                    var texPropName = "_OutlineWidthMask";

                    var outlineWidth = material.GetFloat(floatProp) / lilDifferenceRecorder._OutlineWidth;

                    var outlineWidthMask = propEnvsDict.ContainsKey(texPropName) ? propEnvsDict[texPropName] : null;
                    if (outlineWidthMask == null)
                    {
                        if (lilDifferenceRecorder.IsAlreadyTex_OutlineWidth || bakeSetting == PropertyBakeSetting.BakeAllProperty)
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

        MatCap系統はマスクだけマージする
        < マットキャップマスク(1,2)Tex * マットキャップブレンド(1,2)

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


        ファーや屈折、宝石、などの高負荷系統は、独自の設定が強いし、そもそもまとめるべきかというと微妙。分けといたほうが軽いとかがありそう...

        マテリアルをマージしない前提で、とりあえずアトラス化はする。

        */

        AtlasShaderRecorder lilDifferenceRecorder = new AtlasShaderRecorder();

        public void AddRecord(Material material)
        {
            if (material == null) return;

            lilDifferenceRecorder.AddRecord(material, "_MainTex", (material.GetColor("_Color"), material.GetColor("_MainTexHSVG")), ColorTupleEqualityComparer);
            if (material.GetFloat("_UseMain2ndTex") > 0.5f)
            {
                lilDifferenceRecorder.AddRecord(material, "_Main2ndTex", material.GetColor("_Color2nd"), ColorEqualityComparer);
            }
            if (material.GetFloat("_UseMain3rdTex") > 0.5f)
            {
                lilDifferenceRecorder.AddRecord(material, "_Main3rdTex", material.GetColor("_Color3rd"), ColorEqualityComparer);
            }
            if (material.GetFloat("_UseShadow") > 0.5f)
            {
                lilDifferenceRecorder.AddRecord(material, "_ShadowStrengthMask", material.GetFloat("_ShadowStrength"), FloatEqualityComparer);
                lilDifferenceRecorder.AddRecord(material, "_ShadowColorTex", material.GetColor("_ShadowColor"), ColorEqualityComparer);
                lilDifferenceRecorder.AddRecord(material, "_Shadow2ndColorTex", material.GetColor("_Shadow2ndColor"), ColorEqualityComparer);
                lilDifferenceRecorder.AddRecord(material, "_Shadow3rdColorTex", material.GetColor("_Shadow3rdColor"), ColorEqualityComparer);
            }
            if (material.GetFloat("_UseEmission") > 0.5f)
            {
                lilDifferenceRecorder.AddRecord(material, "_EmissionMap", material.GetColor("_EmissionColor"), ColorEqualityComparer);
                lilDifferenceRecorder.AddRecord(material, "_EmissionBlendMask", material.GetFloat("_EmissionBlend"), FloatEqualityComparer);
            }
            if (material.GetFloat("_UseEmission2nd") > 0.5f)
            {
                lilDifferenceRecorder.AddRecord(material, "_Emission2ndMap", material.GetColor("_Emission2ndColor"), ColorEqualityComparer);
                lilDifferenceRecorder.AddRecord(material, "_Emission2ndBlendMask", material.GetFloat("_Emission2ndBlend"), FloatEqualityComparer);
            }
            if (material.GetFloat("_UseAnisotropy") > 0.5f)
            {
                lilDifferenceRecorder.AddRecord(material, "_AnisotropyScaleMask", material.GetFloat("_AnisotropyScale"), FloatEqualityComparer);
            }
            if (material.GetFloat("_UseBacklight") > 0.5f)
            {
                lilDifferenceRecorder.AddRecord(material, "_BacklightColorTex", material.GetColor("_BacklightColor"), ColorEqualityComparer);
            }
            if (material.GetFloat("_UseReflection") > 0.5f)
            {
                lilDifferenceRecorder.AddRecord(material, "_SmoothnessTex", material.GetFloat("_Smoothness"), FloatEqualityComparer);
                lilDifferenceRecorder.AddRecord(material, "_MetallicGlossMap", material.GetFloat("_Metallic"), FloatEqualityComparer);
                lilDifferenceRecorder.AddRecord(material, "_ReflectionColorTex", material.GetColor("_ReflectionColor"), ColorEqualityComparer);
            }
            if (material.GetFloat("_UseMatCap") > 0.5f)
            {
                lilDifferenceRecorder.AddRecord(material, "_MatCapBlendMask", material.GetFloat("_MatCapBlend"), FloatEqualityComparer);
            }
            if (material.GetFloat("_UseMatCap2nd") > 0.5f)
            {
                lilDifferenceRecorder.AddRecord(material, "_MatCap2ndBlendMask", material.GetFloat("_MatCap2ndBlend"), FloatEqualityComparer);
            }
            if (material.GetFloat("_UseRim") > 0.5f)
            {
                lilDifferenceRecorder.AddRecord(material, "_RimColorTex", material.GetColor("_RimColor"), ColorEqualityComparer);
            }
            if (material.GetFloat("_UseGlitter") > 0.5f)
            {
                lilDifferenceRecorder.AddRecord(material, "_GlitterColorTex", material.GetColor("_GlitterColor"), ColorEqualityComparer);
            }
            if (material.shader.name.Contains("Outline"))
            {
                lilDifferenceRecorder.AddRecord(material, "_OutlineTex", (material.GetColor("_OutlineColor"), material.GetColor("_OutlineTexHSVG")), ColorTupleEqualityComparer);

                var record = lilDifferenceRecorder.AddRecord(material, "_OutlineWidthMask", material.GetFloat("_OutlineWidth"), FloatEqualityComparer);
                record.RecordValue = Mathf.Max(record.RecordValue, material.GetFloat("_OutlineWidth"));
            }
            if (material.shader.name.Contains("Gem"))
            {
                lilDifferenceRecorder.AddRecord(material, "_SmoothnessTex", material.GetFloat("_Smoothness"), FloatEqualityComparer);
            }
        }

        static bool ColorTupleEqualityComparer((Color, Color) l, (Color, Color) r)
        {
            return l.Item1 == r.Item1 && l.Item2 == r.Item2;
        }
        static bool ColorEqualityComparer(Color l, Color r)
        {
            return l == r;
        }
        static bool FloatEqualityComparer(float l, float r)
        {
            return Mathf.Approximately(l, r);
        }
        public void ClearRecord()
        {
            lilDifferenceRecorder = new AtlasShaderRecorder();
        }


    }
}
#endif
