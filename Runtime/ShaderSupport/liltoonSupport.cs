#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Rs64.TexTransTool.ShaderSupport
{
    public class liltoonSupport : IShaderSupport
    {
        public string SupprotShaderName => "lilToon";

        public void MaterialCustomSetting(Material material)
        {
            var MainTex = material.GetTexture("_MainTex") as Texture2D;
            material.SetTexture("_BaseMap", MainTex);
            material.SetTexture("_BaseColorMap", MainTex);
        }

        public List<PropAndTexture> GetPropertyAndTextures(Material material, bool IsGNTFMP = false)
        {
            var PropertyAndTextures = new List<PropAndTexture>();

            PropertyAndTextures.Add(new PropAndTexture("_MainTex", material.GetTexture("_MainTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Main2ndTex", material.GetTexture("_Main2ndTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Main2ndBlendMask", material.GetTexture("_Main2ndBlendMask") as Texture2D));
            // PropertyAndTextures.Add(new PropAndTexture("_Main2ndDissolveMask", material.GetTexture("_Main2ndDissolveMask") as Texture2D));
            // PropertyAndTextures.Add(new PropAndTexture("_Main2ndDissolveNoiseMask", material.GetTexture("_Main2ndDissolveNoiseMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Main3rdTex", material.GetTexture("_Main3rdTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Main3rdBlendMask", material.GetTexture("_Main3rdBlendMask") as Texture2D));
            // PropertyAndTextures.Add(new PropAndTexture("_Main3rdDissolveMask", material.GetTexture("_Main3rdDissolveMask") as Texture2D));
            // PropertyAndTextures.Add(new PropAndTexture("_Main3rdDissolveNoiseMask", material.GetTexture("_Main3rdDissolveNoiseMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_AlphaMask", material.GetTexture("_AlphaMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_BumpMap", material.GetTexture("_BumpMap") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Bump2ndMap", material.GetTexture("_Bump2ndMap") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Bump2ndScaleMask", material.GetTexture("_Bump2ndScaleMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_AnisotropyTangentMap", material.GetTexture("_AnisotropyTangentMap") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_AnisotropyScaleMask", material.GetTexture("_AnisotropyScaleMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_BacklightColorTex", material.GetTexture("_BacklightColorTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_ShadowStrengthMask", material.GetTexture("_ShadowStrengthMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_ShadowBorderMask", material.GetTexture("_ShadowBorderMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_ShadowBlurMask", material.GetTexture("_ShadowBlurMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_ShadowColorTex", material.GetTexture("_ShadowColorTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Shadow2ndColorTex", material.GetTexture("_Shadow2ndColorTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Shadow3rdColorTex", material.GetTexture("_Shadow3rdColorTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_SmoothnessTex", material.GetTexture("_SmoothnessTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_MetallicGlossMap", material.GetTexture("_MetallicGlossMap") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_ReflectionColorTex", material.GetTexture("_ReflectionColorTex") as Texture2D));
            // PropertyAndTextures.Add(new PropAndTexture("_MatCapBlendMask", material.GetTexture("_MatCapBlendMask") as Texture2D));
            // PropertyAndTextures.Add(new PropAndTexture("_MatCap2ndBlendMask", material.GetTexture("_MatCap2ndBlendMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_RimColorTex", material.GetTexture("_RimColorTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_GlitterColorTex", material.GetTexture("_GlitterColorTex") as Texture2D));
            // PropertyAndTextures.Add(new PropAndTexture("_GlitterShapeTex", material.GetTexture("_GlitterShapeTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_EmissionMap", material.GetTexture("_EmissionMap") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_EmissionBlendMask", material.GetTexture("_EmissionBlendMask") as Texture2D));
            // PropertyAndTextures.Add(new PropAndTexture("_EmissionGradTex", material.GetTexture("_EmissionGradTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Emission2ndMap", material.GetTexture("_Emission2ndMap") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_Emission2ndBlendMask", material.GetTexture("_Emission2ndBlendMask") as Texture2D));
            // PropertyAndTextures.Add(new PropAndTexture("_Emission2ndGradTex", material.GetTexture("_Emission2ndGradTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_ParallaxMap", material.GetTexture("_ParallaxMap") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_AudioLinkMask", material.GetTexture("_AudioLinkMask") as Texture2D));
            // PropertyAndTextures.Add(new PropAndTexture("_DissolveMask", material.GetTexture("_DissolveMask") as Texture2D));
            // PropertyAndTextures.Add(new PropAndTexture("_DissolveNoiseMask", material.GetTexture("_DissolveNoiseMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_OutlineTex", material.GetTexture("_OutlineTex") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_OutlineWidthMask", material.GetTexture("_OutlineWidthMask") as Texture2D));
            PropertyAndTextures.Add(new PropAndTexture("_OutlineVectorTex", material.GetTexture("_OutlineVectorTex") as Texture2D));

            return PropertyAndTextures;
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
            public Color _Color;
            public bool IsDifference_MainColor;

            public Color _MainTexHSVG;
            public bool IsDifference_MainTexHSVG;

            public Color _Color2nd;
            public bool IsDifference_MainColor2nd;

            public Color _Color3rd;
            public bool IsDifference_MainColor3rd;

            public float _ShadowStrength;
            public bool IsDifference_ShadowStrength;

            public Color _ShadowColor;
            public bool IsDifference_ShadowColor;

            public Color _Shadow2ndColor;
            public bool IsDifference_Shadow2ndColor;

            public Color _Shadow3rdColor;
            public bool IsDifference_Shadow3rdColor;

            public Color _EmissionColor;
            public bool IsDifference_EmissionColor;

            public float _EmissionBlend;
            public bool IsDifference_EmissionBlend;

            public Color _Emission2ndColor;
            public bool IsDifference_Emission2ndColor;

            public float _Emission2ndBlend;
            public bool IsDifference_Emission2ndBlend;

            public float _AnisotropyScale;
            public bool IsDifference_AnisotropyScale;

            public Color _BacklightColor;
            public bool IsDifference_BacklightColor;

            public float _Smoothness;
            public bool IsDifference_Smoothness;

            public float _Metallic;
            public bool IsDifference_Metallic;

            public Color _ReflectionColor;
            public bool IsDifference_ReflectionColor;

            public Color _RimColor;
            public bool IsDifference_RimColor;

            public Color _GlitterColor;
            public bool IsDifference_GlitterColor;

            public Color _OutlineColor;
            public bool IsDifference_OutlineColor;

            public float _OutlineWidth;
            public bool IsDifference_OutlineWidth;

        }
        public void PropatyDataStack(Material material)
        {
        }

        public void StackClear()
        {
        }
    }
}
#endif