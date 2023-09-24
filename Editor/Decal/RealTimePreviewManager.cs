#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using net.rs64.TexTransTool.Decal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static net.rs64.TexTransTool.TextureLayerUtil;

namespace net.rs64.TexTransTool
{
    public class RealTimePreviewManager : ScriptableSingleton<RealTimePreviewManager>
    {
        private List<AbstractDecal> PreviewList = new List<AbstractDecal>();
        private Dictionary<Material, Dictionary<string, ((Texture2D SouseTexture, RenderTexture TargetTexture), List<BlendTextures> Decals)>> Previews = new Dictionary<Material, Dictionary<string, ((Texture2D SouseTexture, RenderTexture TargetTexture), List<BlendTextures> Decals)>>();
        protected RealTimePreviewManager()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= ExitPreview;
            AssemblyReloadEvents.beforeAssemblyReload += ExitPreview;
            EditorSceneManager.sceneClosing -= ExitPreview;
            EditorSceneManager.sceneClosing += ExitPreview;
        }

        public void AddPreviewRenderTexture(Material material, string PropertyName, RenderTexture renderTexture, BlendType blendType)
        {
            if (Previews.ContainsKey(material))
            {
                if (Previews[material].ContainsKey(PropertyName))
                {
                    Previews[material][PropertyName].Decals.Add(new BlendTextures(renderTexture, blendType));
                }
                else
                {
                    var newTarget = new RenderTexture(renderTexture.descriptor);
                    var souseTexture = material.GetTexture(PropertyName) as Texture2D;
                    material.SetTexture(PropertyName, newTarget);
                    Previews[material].Add(PropertyName, ((souseTexture, newTarget), new List<BlendTextures>() { new BlendTextures(renderTexture, blendType) }));
                }
            }
            else
            {
                var editableMat = Instantiate(material);
                var souseTexture = material.GetTexture(PropertyName) as Texture2D;
                var newTarget = new RenderTexture(renderTexture.descriptor);
                editableMat.SetTexture(PropertyName, newTarget);
                Previews.Add(editableMat, new Dictionary<string, ((Texture2D SouseTexture, RenderTexture TargetTexture), List<BlendTextures> Decals)>() { { PropertyName, ((souseTexture, newTarget), new List<BlendTextures>() { new BlendTextures(renderTexture, blendType) }) } });
            }
        }

        public void UpdatePreviewTexture(Material material, string PropertyName)
        {
            if (!Previews.ContainsKey(material)) { return; }
            if (!Previews[material].ContainsKey(PropertyName)) { return; }

            var target = Previews[material][PropertyName];
            var targetRt = target.Item1.TargetTexture;
            targetRt.Release();
            var souseTex = target.Item1.SouseTexture;
            Graphics.Blit(souseTex, targetRt);

            TextureLayerUtil.BlendBlit(targetRt, target.Decals);
        }

        private void ExitPreview()
        {
            PreviewList.Clear();
            Previews.Clear();
        }
        public void ExitPreview(UnityEngine.SceneManagement.Scene scene, bool removingScene)
        {
            ExitPreview();
        }
    }
}
#endif