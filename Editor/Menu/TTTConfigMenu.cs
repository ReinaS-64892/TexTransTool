#nullable enable
using System;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal sealed class TTTConfigMenu : TTTMenu.ITTTConfigWindow
    {
        public void OnGUI()
        {
            var globalSettings = TTTGlobalConfig.instance;
            EditorGUILayout.LabelField("Global Settings");

            using (new EditorGUI.IndentLevelScope(1))
            {
                if (Localize.Languages is not null)
                {
                    var bIndex = Array.IndexOf(Localize.Languages, globalSettings.Language);
                    var aIndex = EditorGUILayout.Popup("Language", bIndex, Localize.Languages);
                    if (bIndex != aIndex) { globalSettings.Language = Localize.Languages[aIndex]; }
                }
            }

            var projectSettings = TTTProjectConfig.instance;
            EditorGUILayout.LabelField("Project Settings");

            using (new EditorGUI.IndentLevelScope(1))
            {
                projectSettings.InternalRenderTextureFormat = (TexTransCore.TexTransCoreTextureFormat)EditorGUILayout.Popup(
                        "TTTMenu:TTTConfigMenu:InternalRenderTextureFormat".Glc(),
                        (int)projectSettings.InternalRenderTextureFormat,
                        GetInternalRenderTextureFormatOption()
                    );

                var displayVRAMIcon = EditorGUILayout.Toggle(
                        "TTTMenu:TTTConfigMenu:DisplayVRAMIcon".Glc(),
                        projectSettings.DisplayVRAMIcon
                    );
                if (projectSettings.DisplayVRAMIcon != displayVRAMIcon)
                {
                    if (displayVRAMIcon)
                    {
                        projectSettings.DisplayVRAMIcon = displayVRAMIcon;
                        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    }
                    else
                    {
                        if (EditorUtility.DisplayDialog(
                            "Warning",
                            "TTTMenu:TTTConfigMenu:DisplayVRAMIcon:DisableWarningMessage".GetLocalize(),
                            "OK", "Cancel"
                        ))
                        {
                            projectSettings.DisplayVRAMIcon = displayVRAMIcon;
                            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                        }

                    }
                }


                EditorGUILayout.LabelField("Experimental");
                using (new EditorGUI.IndentLevelScope(1))
                {
                    projectSettings.TexTransCoreEngineBackend = (TTTProjectConfig.TexTransCoreEngineBackendEnum)EditorGUILayout.EnumPopup(
                            "TTTMenu:TTTConfigMenu:TexTransCoreEngineBackend".Glc(),
                            projectSettings.TexTransCoreEngineBackend
                        );
                }
            }
        }

        GUIContent[]? InternalRenderTextureFormatOptionGuiContents;
        GUIContent[] GetInternalRenderTextureFormatOption()
        {
            InternalRenderTextureFormatOptionGuiContents ??= new GUIContent[4];

            InternalRenderTextureFormatOptionGuiContents[0] = "TTTMenu:TTTConfigMenu:InternalRenderTextureFormat:Byte".Glc();
            InternalRenderTextureFormatOptionGuiContents[1] = "TTTMenu:TTTConfigMenu:InternalRenderTextureFormat:UShort".Glc();
            InternalRenderTextureFormatOptionGuiContents[2] = "TTTMenu:TTTConfigMenu:InternalRenderTextureFormat:Half".Glc();
            InternalRenderTextureFormatOptionGuiContents[3] = "TTTMenu:TTTConfigMenu:InternalRenderTextureFormat:Float".Glc();
            return InternalRenderTextureFormatOptionGuiContents;
        }

    }
}
