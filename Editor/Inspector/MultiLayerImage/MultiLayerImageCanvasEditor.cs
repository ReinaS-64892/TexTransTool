#nullable enable
using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransCore;
using net.rs64.TexTransCoreEngineForUnity;
using System;
using UnityEngine;

namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(MultiLayerImageCanvas))]
    internal class MultiLayerImageCanvasEditor : UnityEditor.Editor
    {
        ITTRenderTexture? _imageLayerPreviewResult = null;

        void OnEnable() { GenerateImageLayerPreview(); }
        void GenerateImageLayerPreview()
        {
            // TTT が初期化されてない状態で実行すると失敗するため
            if (ComputeObjectUtility.BlendingObject == null) { return; }

            var isMultiple = targets.Length != 1;
            if (isMultiple) { return; }

            if (_imageLayerPreviewResult != null) { _imageLayerPreviewResult.Dispose(); _imageLayerPreviewResult = null; }

            var mlic = target as MultiLayerImageCanvas;
            if (mlic is null) { return; }

            var previewCanvasSize = (1024, 1024);
            var domain = new TextureAtlas.Editor.NotWorkDomain(Array.Empty<Renderer>(), new TextureManager(true));
            _imageLayerPreviewResult = mlic.EvaluateCanvas(new(domain, previewCanvasSize));

            // これをそのままインスペクターに描画しようとすると薄くなってしまうから Linear 空間にすることでごまかす。
            domain.GetTexTransCoreEngineForUnity().GammaToLinear(_imageLayerPreviewResult);
        }
        void OnDisable()
        {
            if (_imageLayerPreviewResult == null) { return; }
            _imageLayerPreviewResult.Dispose();
            _imageLayerPreviewResult = null;
        }
        public override void OnInspectorGUI()
        {
            TextureTransformerEditor.DrawOldSaveDataVersionWarning(target as TexTransMonoBase);
            TextureTransformerEditor.DrawerWarning("MultiLayerImageCanvas".GetLocalize());

            var sTarget = serializedObject;

            EditorGUILayout.PropertyField(sTarget.FindProperty("TextureSelector"));

            PreviewButtonDrawUtil.Draw(target as TexTransBehavior);


            sTarget.ApplyModifiedProperties();
        }

        public override bool HasPreviewGUI() { return true; }
        public override void DrawPreview(Rect previewArea)
        {
            if (_imageLayerPreviewResult is null) { GenerateImageLayerPreview(); }
            if (_imageLayerPreviewResult is not null)
            {
                EditorGUI.DrawTextureTransparent(previewArea, _imageLayerPreviewResult.Unwrap(), ScaleMode.ScaleToFit);
            }
        }

    }
}
