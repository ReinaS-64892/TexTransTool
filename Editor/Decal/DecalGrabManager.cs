#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.Decal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static net.rs64.TexTransCore.BlendTexture.TextureBlend;

namespace net.rs64.TexTransTool
{
    public class DecalGrabManager : ScriptableSingleton<DecalGrabManager>
    {

        AbstractDecal GrabTarget;
        public AbstractDecal NowGrabDecal => GrabTarget;
        SceneView GrabView;

        DecalGrabManager()
        {
            EditorApplication.update -= UpdateGrab;
            EditorApplication.update += UpdateGrab;
        }
        public void Grab(AbstractDecal abstractDecal)
        {
            GrabTarget = abstractDecal;
            GrabView = SceneView.lastActiveSceneView;
        }
        public void Drop(AbstractDecal abstractDecal)
        {
            if (abstractDecal != GrabTarget) { return; }
            GrabTarget = null;
            GrabView = null;
            EditorUtility.SetDirty(abstractDecal);
        }

        private void UpdateGrab()
        {
            if (GrabTarget == null || GrabView == null) { return; }
            GrabTarget.transform.position = GrabView.camera.transform.position;
            GrabTarget.transform.rotation = GrabView.camera.transform.rotation;
        }
    }
}
#endif