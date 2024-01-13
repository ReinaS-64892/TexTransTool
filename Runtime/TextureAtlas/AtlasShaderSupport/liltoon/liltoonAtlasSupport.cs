using System.Collections.Generic;
using UnityEngine;


namespace net.rs64.TexTransTool.TextureAtlas
{
    internal class liltoonAtlasSupport : IAtlasShaderSupport
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

        public List<PropAndTexture> GetPropertyAndTextures(IGetOriginTex2DManager textureManager, Material material, PropertyBakeSetting bakeSetting)
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

            var baker = new TextureBaker(textureManager, propEnvsDict, material, _lilDifferenceRecorder, bakeSetting);

            baker.ColorMulAndHSVG("_MainTex", "_Color", "_MainTexHSVG");
            if (material.GetFloat("_UseMain2ndTex") > 0.5f)
            {
                baker.ColorMul("_Main2ndTex", "_Color2nd");
            }
            if (material.GetFloat("_UseMain3rdTex") > 0.5f)
            {
                baker.ColorMul("_Main3rdTex", "_Color3rd");
            }
            if (material.GetFloat("_UseShadow") > 0.5f)
            {
                baker.FloatMul("_ShadowStrengthMask", "_ShadowStrength");
                baker.ColorMul("_ShadowColorTex", "_ShadowColor");
                baker.ColorMul("_Shadow2ndColorTex", "_Shadow2ndColor");
                baker.ColorMul("_Shadow3rdColorTex", "_Shadow3rdColor");
            }
            if (material.GetFloat("_UseEmission") > 0.5f)
            {
                baker.ColorMul("_EmissionMap", "_EmissionColor");
                baker.FloatMul("_EmissionBlendMask", "_EmissionBlend");
            }
            if (material.GetFloat("_UseEmission2nd") > 0.5f)
            {
                baker.ColorMul("_Emission2ndMap", "_Emission2ndColor");
                baker.FloatMul("_Emission2ndBlendMask", "_Emission2ndBlend");
            }
            if (material.GetFloat("_UseAnisotropy") > 0.5f)
            {
                baker.FloatMul("_AnisotropyScaleMask", "_AnisotropyScale");
            }
            if (material.GetFloat("_UseBacklight") > 0.5f)
            {
                baker.ColorMul("_BacklightColorTex", "_BacklightColor");
            }
            if (material.GetFloat("_UseReflection") > 0.5f)
            {
                baker.FloatMul("_SmoothnessTex", "_Smoothness");
                baker.FloatMul("_MetallicGlossMap", "_Metallic");
                baker.ColorMul("_ReflectionColorTex", "_ReflectionColor");
            }
            if (material.GetFloat("_UseMatCap") > 0.5f)
            {
                baker.FloatMul("_MatCapBlendMask", "_MatCapBlend");
            }
            if (material.GetFloat("_UseMatCap") > 0.5f)
            {
                baker.FloatMul("_MatCap2ndBlendMask", "_MatCap2ndBlend");
            }
            if (material.GetFloat("_UseRim") > 0.5f)
            {
                baker.ColorMul("_RimColorTex", "_RimColor");
            }
            if (material.GetFloat("_UseGlitter") > 0.5f)
            {
                baker.ColorMul("_GlitterColorTex", "_GlitterColor");
            }
            if (material.shader.name.Contains("Outline"))
            {
                baker.ColorMulAndHSVG("_OutlineTex", "_OutlineColor", "_OutlineTexHSVG");
                baker.OutlineWidthMul("_OutlineWidthMask", "_OutlineWidth");
            }

            var propAndTexture = new List<PropAndTexture>();
            foreach (var PropEnv in propEnvsDict)
            {
                propAndTexture.Add(new (PropEnv.Key, PropEnv.Value));
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

        AtlasShaderRecorder _lilDifferenceRecorder = new ();

        public void AddRecord(Material material)
        {
            if (material == null) return;

            _lilDifferenceRecorder.AddRecord(material, "_MainTex", material.GetColor("_Color"), material.GetColor("_MainTexHSVG"), ColorEqualityComparer);
            if (material.GetFloat("_UseMain2ndTex") > 0.5f)
            {
                _lilDifferenceRecorder.AddRecord(material, "_Main2ndTex", material.GetColor("_Color2nd"), ColorEqualityComparer);
            }
            if (material.GetFloat("_UseMain3rdTex") > 0.5f)
            {
                _lilDifferenceRecorder.AddRecord(material, "_Main3rdTex", material.GetColor("_Color3rd"), ColorEqualityComparer);
            }
            if (material.GetFloat("_UseShadow") > 0.5f)
            {
                _lilDifferenceRecorder.AddRecord(material, "_ShadowStrengthMask", material.GetFloat("_ShadowStrength"), FloatEqualityComparer);
                _lilDifferenceRecorder.AddRecord(material, "_ShadowColorTex", material.GetColor("_ShadowColor"), ColorEqualityComparer);
                _lilDifferenceRecorder.AddRecord(material, "_Shadow2ndColorTex", material.GetColor("_Shadow2ndColor"), ColorEqualityComparer);
                _lilDifferenceRecorder.AddRecord(material, "_Shadow3rdColorTex", material.GetColor("_Shadow3rdColor"), ColorEqualityComparer);
            }
            if (material.GetFloat("_UseEmission") > 0.5f)
            {
                _lilDifferenceRecorder.AddRecord(material, "_EmissionMap", material.GetColor("_EmissionColor"), ColorEqualityComparer);
                _lilDifferenceRecorder.AddRecord(material, "_EmissionBlendMask", material.GetFloat("_EmissionBlend"), FloatEqualityComparer);
            }
            if (material.GetFloat("_UseEmission2nd") > 0.5f)
            {
                _lilDifferenceRecorder.AddRecord(material, "_Emission2ndMap", material.GetColor("_Emission2ndColor"), ColorEqualityComparer);
                _lilDifferenceRecorder.AddRecord(material, "_Emission2ndBlendMask", material.GetFloat("_Emission2ndBlend"), FloatEqualityComparer);
            }
            if (material.GetFloat("_UseAnisotropy") > 0.5f)
            {
                _lilDifferenceRecorder.AddRecord(material, "_AnisotropyScaleMask", material.GetFloat("_AnisotropyScale"), FloatEqualityComparer);
            }
            if (material.GetFloat("_UseBacklight") > 0.5f)
            {
                _lilDifferenceRecorder.AddRecord(material, "_BacklightColorTex", material.GetColor("_BacklightColor"), ColorEqualityComparer);
            }
            if (material.GetFloat("_UseReflection") > 0.5f)
            {
                _lilDifferenceRecorder.AddRecord(material, "_SmoothnessTex", material.GetFloat("_Smoothness"), FloatEqualityComparer);
                _lilDifferenceRecorder.AddRecord(material, "_MetallicGlossMap", material.GetFloat("_Metallic"), FloatEqualityComparer);
                _lilDifferenceRecorder.AddRecord(material, "_ReflectionColorTex", material.GetColor("_ReflectionColor"), ColorEqualityComparer);
            }
            if (material.GetFloat("_UseMatCap") > 0.5f)
            {
                _lilDifferenceRecorder.AddRecord(material, "_MatCapBlendMask", material.GetFloat("_MatCapBlend"), FloatEqualityComparer);
            }
            if (material.GetFloat("_UseMatCap2nd") > 0.5f)
            {
                _lilDifferenceRecorder.AddRecord(material, "_MatCap2ndBlendMask", material.GetFloat("_MatCap2ndBlend"), FloatEqualityComparer);
            }
            if (material.GetFloat("_UseRim") > 0.5f)
            {
                _lilDifferenceRecorder.AddRecord(material, "_RimColorTex", material.GetColor("_RimColor"), ColorEqualityComparer);
            }
            if (material.GetFloat("_UseGlitter") > 0.5f)
            {
                _lilDifferenceRecorder.AddRecord(material, "_GlitterColorTex", material.GetColor("_GlitterColor"), ColorEqualityComparer);
            }
            if (material.shader.name.Contains("Outline"))
            {
                _lilDifferenceRecorder.AddRecord(material, "_OutlineTex", material.GetColor("_OutlineColor"), material.GetColor("_OutlineTexHSVG"), ColorEqualityComparer);

                var record = _lilDifferenceRecorder.AddRecord(material, "_OutlineWidthMask", material.GetFloat("_OutlineWidth"), FloatEqualityComparer);
                record.RecordValue = Mathf.Max(record.RecordValue, material.GetFloat("_OutlineWidth"));
            }
            if (material.shader.name.Contains("Gem"))
            {
                _lilDifferenceRecorder.AddRecord(material, "_SmoothnessTex", material.GetFloat("_Smoothness"), FloatEqualityComparer);
            }
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
            _lilDifferenceRecorder.ClearRecord();
        }

    }
}
