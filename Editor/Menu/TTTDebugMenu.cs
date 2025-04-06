#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransTool.Build;
using net.rs64.TexTransTool.Migration;
using net.rs64.TexTransTool.MultiLayerImage.Importer;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal sealed class TTTDebugMenu : TTTMenu.ITTTConfigWindow
    {
        public void OnGUI()
        {
            EditorGUILayout.HelpBox("This is a Debug Menu !!!, If you do not understand it, do not touch this menu !!!\nこれはデバッグメニューです!!! 理解できないのであればこのメニューは使用しないでください!!!", MessageType.Warning);

            ReloadReloadTTBlendAndCompute();
            UnityNativeLeakDetectionMode();
            PreviewUtilityDraw();
            CanvasImportedImagePreviewDebug();
            DebugManualBake();
            DebugMigrator();
        }


        static void PreviewUtilityDraw()
        {
            EditorGUILayout.LabelField("Preview Utility");
            using var iS = new EditorGUI.IndentLevelScope(1);
            if (GUILayout.Button("RePreview")) { Editor.OtherMenuItem.PreviewUtility.RePreview(); }
            if (GUILayout.Button("Exit Preview")) { Editor.OtherMenuItem.PreviewUtility.ExitPreviews(); }
        }

        static void ReloadReloadTTBlendAndCompute()
        {
            EditorGUILayout.LabelField("TTBlendAndCompute");
            using var iS = new EditorGUI.IndentLevelScope(1);
            if (GUILayout.Button("ReloadReloadTTBlendAndCompute") is false) { return; }
            var reloadTargets = AssetDatabase.FindAssets("t:TTBlendUnityObject")
             .Concat(AssetDatabase.FindAssets("t:TTComputeUnityObject"))
             .Concat(AssetDatabase.FindAssets("t:TTComputeOperator"))
             .Concat(AssetDatabase.FindAssets("t:TTComputeDownScalingUnityObject"))
             .Concat(AssetDatabase.FindAssets("t:TTComputeUpScalingUnityObject")).Distinct();

            foreach (var asset in reloadTargets)
                AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(asset));
            ComputeObjectUtility.ComputeObjectsInit();
        }
        static void UnityNativeLeakDetectionMode()
        {
            EditorGUILayout.LabelField("UnityNativeLeakDetection");
            using var iS = new EditorGUI.IndentLevelScope(1);
            var bMode = UnsafeUtility.GetLeakDetectionMode();
            var aMode = (NativeLeakDetectionMode)EditorGUILayout.EnumPopup("UnityNativeLeakDetectionMode", bMode);
            if (aMode == bMode) { return; }
            UnsafeUtility.SetLeakDetectionMode(aMode);
        }
        static void CanvasImportedImagePreviewDebug()
        {
            EditorGUILayout.LabelField("CanvasImportedImagePreviewManager");
            using var iS = new EditorGUI.IndentLevelScope(1);
            if (GUILayout.Button("ReInitialize")) { CanvasImportedImagePreviewManager.Reinitialize(); }
            if (GUILayout.Button("InvalidatesCacheAll")) { CanvasImportedImagePreviewManager.InvalidatesCacheAll(); }
        }
        static void DebugManualBake()
        {
            EditorGUILayout.LabelField("DebugManualBake");
            using var iS = new EditorGUI.IndentLevelScope(1);

            if (GUILayout.Button("Run TexTransTool-Only-ManualBake to the selected GameObject")) { ManualBake.ManualBakeSelected(); }
        }
        static void DebugMigrator()
        {
            EditorGUILayout.LabelField("Debug Migrator");
            using var iS = new EditorGUI.IndentLevelScope(1);
            if (GUILayout.Button("Do Project Migration"))
                if (EditorUtility.DisplayDialog("Debug Migrator", "プロジェクト全体の Migration を実行しますか？\nバックアップをお忘れなく。", "実行する (Do)", "実行しない (Cancel)"))
                    AAOMigrator.MigrateEverything();

        }
    }
}
