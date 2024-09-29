#if VRC_AVATAR && !NDMF_DEPEND_VERSION
using UnityEditor;
using UnityEngine;
using net.rs64.TexTransTool.Build;

#if NDMF_ERROR_REPORT
using System;
using nadena.dev.ndmf.localization;
using System.Collections.Generic;
#endif

#if NDMF
using nadena.dev.ndmf;
using net.rs64.TexTransTool;
[assembly: ExportsPlugin(typeof(ReportUnsupportedNDMFVersionPlugin))]
#endif

static class NDMFNotExistWarning
{
    [InitializeOnLoadMethod]
    static void Call()
    {
#if NDMF
        Debug.LogWarning("TexTransTool の対応している NDMF バージョンではありません！ NDMF をアップデートしてください！！！");
#else
        Debug.LogWarning("NDMF が環境に存在しません！！！ VRChat Avatar 用途では NDMF は必須なのでご注意ください！");
#endif
    }
}


#if NDMF
internal class ReportUnsupportedNDMFVersionPlugin : Plugin<ReportUnsupportedNDMFVersionPlugin>
{
    public override string QualifiedName => "net.rs64.tex-trans-tool";
    public override string DisplayName => "TexTransTool";
#if NDMF_ERROR_REPORT
    public override Texture2D LogoTexture => TTTImageAssets.Logo;
    public const string ErrorMessage_JP = "TexTransTool の対応している NDMF バージョンではありません！ NDMF をアップデートしてください！！！";
    public const string ErrorMessage_Description_JP = "TexTransTool がベータ版の場合は NDMF ベータ版が必要になる場合があります。";
    public const string ErrorMessage_EN = "This is no compatible NDMF version for TexTransTool! Please update NDMF!!!";
    public const string ErrorMessage_Description_EN = "If current TexTransTool is in beta version, may need the NDMF Beta version.";
#endif
    protected override void Configure()
    {
        InPhase(BuildPhase.Resolving).Run("Report Unsupported NDMF Version", ctx =>
        {
#if NDMF_ERROR_REPORT
            ErrorReport.ReportError(new Localizer("ja", () => new List<(string, Func<string, string>)>() {
                ("ja", s => {
                    switch (s)
                    {
                        case "MES": return ErrorMessage_JP;
                        case "MES:description": return ErrorMessage_Description_JP;
                        default: return null;
                    }
                }),
                ("en", s => {
                    switch (s)
                    {
                        case "MES": return ErrorMessage_EN;
                        case "MES:description": return ErrorMessage_Description_EN;
                        default: return null;
                    }
                })
            }), ErrorSeverity.NonFatal, "MES");
#endif
            AvatarBuildUtils.DestroyITexTransToolTags(ctx.AvatarRootObject);
        });
    }

}
#endif


#endif
