#nullable enable
using UnityEditor;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransCore.MultiLayerImageCanvas;
using System;
using UnityEngine;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransCore;
using net.rs64.TexTransTool.TextureAtlas.Editor;
namespace net.rs64.TexTransTool.Editor.MultiLayerImage
{
    [CustomEditor(typeof(AbstractLayer), true)]
    [CanEditMultipleObjects]
    internal class AbstractLayerEditor : TexTransMonoBaseEditor
    {
        (NotWorkDomain domain, UnityDiskUtil diskUtil, ITTRenderTexture previewRt)? _imageLayerPreviewResult = null;
        private bool _needUpdate;

        void OnEnable() { _needUpdate = true; GenerateImageLayerPreview(); }
        void GenerateImageLayerPreview()
        {
            // TTT が初期化されてない状態で実行すると失敗するため
            if (ComputeObjectUtility.BlendingObject == null) { return; }
            _needUpdate = false;

            var isMultiple = targets.Length != 1;
            if (isMultiple) { return; }

            if (_imageLayerPreviewResult != null)
            {
                _imageLayerPreviewResult.Value.previewRt.Dispose();
                _imageLayerPreviewResult.Value.diskUtil.Dispose();
                _imageLayerPreviewResult.Value.domain.Dispose();
                _imageLayerPreviewResult = null;
            }

            var layer = target as AbstractLayer;
            if (layer is null) { return; }

            var previewCanvasSize = (1024, 1024);
            var diskUtil = new UnityDiskUtil(true);
            var domain = new NotWorkDomain(Array.Empty<Renderer>(), new TTCEUnityWithTTT4Unity(diskUtil));
            var engine = domain.GetTexTransCoreEngineForUnity();

            var layerObject = layer.GetLayerObject(new(domain, previewCanvasSize));

            if (layerObject is ImageLayer<ITexTransToolForUnity> imageLayer)
                try
                {
                    var previewTex = engine.CreateRenderTexture(previewCanvasSize.Item1, previewCanvasSize.Item2);
                    imageLayer.GetImage(engine, previewTex);
                    imageLayer.AlphaMask.Masking(engine, previewTex);


                    // これをそのままインスペクターに描画しようとすると薄くなってしまうから Linear 空間にすることでごまかす。
                    engine.GammaToLinear(previewTex);

                    _imageLayerPreviewResult = (domain, diskUtil, previewTex);
                }
                finally { imageLayer.Dispose(); }
            else layerObject.Dispose();
        }
        void OnDisable()
        {
            if (_imageLayerPreviewResult == null) { return; }
            _imageLayerPreviewResult.Value.previewRt.Dispose();
            _imageLayerPreviewResult.Value.domain.Dispose();
            _imageLayerPreviewResult = null;
        }
        protected sealed override void OnTexTransComponentInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            DrawCommonProperties();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            DrawInnerProperties();
            _needUpdate |= EditorGUI.EndChangeCheck();
        }

        // GeneralCommonLayerSetting
        protected void DrawCommonProperties()
        {
            var blendTypeKey = serializedObject.FindProperty(nameof(AbstractLayer.BlendTypeKey));
            var opacity = serializedObject.FindProperty(nameof(AbstractLayer.Opacity));
            var clipping = serializedObject.FindProperty(nameof(AbstractLayer.Clipping));
            var layerMask = serializedObject.FindProperty(nameof(AbstractLayer.LayerMask));

            EditorGUILayout.PropertyField(blendTypeKey, "AbstractLayer:prop:BlendTypeKey".Glc());
            EditorGUILayout.PropertyField(opacity, "AbstractLayer:prop:Opacity".Glc());
            EditorGUILayout.PropertyField(clipping, "AbstractLayer:prop:Clipping".Glc());
            EditorGUILayout.PropertyField(layerMask, "AbstractLayer:prop:LayerMask".Glc());
        }

        protected virtual void DrawInnerProperties()
        {
            var iterator = serializedObject.GetIterator();
            iterator.NextVisible(true); // m_script
            for (int i = 0; i < 4; i++) // GeneralCommonLayerSetting
            {
                iterator.NextVisible(false);
            }
            while (iterator.NextVisible(false))
            {
                EditorGUILayout.PropertyField(iterator);
            }
        }

        public override bool HasPreviewGUI() { return _imageLayerPreviewResult is not null; }
        public override void DrawPreview(Rect previewArea)
        {
            if (_needUpdate) { GenerateImageLayerPreview(); }
            if (_imageLayerPreviewResult is not null)
            {
                var previewUrt = _imageLayerPreviewResult.Value.domain.GetTexTransCoreEngineForUnity().GetReferenceRenderTexture(_imageLayerPreviewResult.Value.previewRt);
                EditorGUI.DrawTextureTransparent(previewArea, previewUrt, ScaleMode.ScaleToFit);
            }
        }

    }
}
