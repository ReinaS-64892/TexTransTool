using UnityEngine;
using UnityEditor;
using Unity.Collections;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransCore;
using System;

namespace net.rs64.TexTransTool.MultiLayerImage
{

    public abstract class TTTImportedImage : ScriptableObject
    {
        public TTTImportedCanvasDescription CanvasDescription;
        public Texture2D PreviewTexture;


        // ここでの writeTarget は CanvasSize と同じことが前提
        public virtual void LoadImage<TTCE>(ITTImportedCanvasSource importSource, TTCE ttce, ITTRenderTexture writeTarget)
        where TTCE : ITexTransComputeKeyQuery
        , ITexTransGetComputeHandler
        , ITexTransCopyRenderTexture
        , ITexTransCreateTexture
        , ITexTransRenderTextureIO
        , ITexTransRenderTextureUploadToCreate
        {
            var ppB = EnginUtil.GetPixelParByte(CanvasDescription.ImportedImageFormat, TexTransCoreTextureChannel.RGBA);
            var length = CanvasDescription.Width * CanvasDescription.Height * ppB;
            using var na = new NativeArray<byte>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            LoadImage(importSource, na.AsSpan());
            ttce.UploadTexture<byte>(writeTarget, na.AsSpan(), CanvasDescription.ImportedImageFormat);
        }

        public abstract void LoadImage(ITTImportedCanvasSource importSource, Span<byte> writeTarget);
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(TTTImportedImage))]
    public class TTTImportedPngEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();
            EditorGUILayout.LabelField("Debugなどでこのオブジェクトの中身を見ようとしないでください!!!、UnityEditorが停止します!!!");
            var thisTarget = target as TTTImportedImage;
            // if (thisTarget.PreviewTexture != null) { EditorGUI.DrawTextureTransparent(EditorGUILayout.GetControlRect(GUILayout.Height(400)), thisTarget.PreviewTexture, ScaleMode.ScaleToFit); }

        }
    }
#endif
}
