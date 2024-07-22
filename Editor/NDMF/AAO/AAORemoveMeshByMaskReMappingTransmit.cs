#if UNITY_EDITOR
using nadena.dev.ndmf;
using net.rs64.TexTransTool.NDMF;
using net.rs64.TexTransTool.Build;
using UnityEngine;
using System;
using System.Collections.Generic;
using net.rs64.TexTransTool.TextureAtlas;
using net.rs64.TexTransCore.Utils;
using net.rs64.TexTransCore;
using UnityEditor;
using System.Linq;


namespace net.rs64.TexTransTool.NDMF
{
    internal class AAORemoveMeshByMaskReMappingTransmit : MonoBehaviour, ITexTransToolTag, IAtlasReMappingReceiver
    {
        [HideInInspector, SerializeField] int _saveDataVersion = TexTransBehavior.TTTDataVersion;
        int ITexTransToolTag.SaveDataVersion => _saveDataVersion;

        internal static Type TargetType = AppDomain.CurrentDomain.GetAssemblies()
            .First(a => a.GetName().Name == "com.anatawa12.avatar-optimizer.runtime").GetType("Anatawa12.AvatarOptimizer.RemoveMeshByMask");

        public void ReMappingReceive(Mesh normalizedOriginalMesh, Mesh atlasMesh)
        {
            Component transmitTarget = GetComponent(TargetType);
            if (transmitTarget == null) { return; }

            var materialFieldRefection = TargetType.GetField("materials", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Array materials = materialFieldRefection.GetValue(transmitTarget) as Array;
            int length = materials.Length;

            for (var subMeshIndex = 0; length > subMeshIndex; subMeshIndex += 1)
            {
                object materialSlot = materials.GetValue(subMeshIndex);
                var enabledRef = materialSlot.GetType().GetField("enabled", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var maskRef = materialSlot.GetType().GetField("mask", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                bool enabled = (bool)enabledRef.GetValue(materialSlot);
                Texture2D mask = maskRef.GetValue(materialSlot) as Texture2D;

                if (enabled is false) { continue; }
                using (TTRt.U(out var tmpRt, mask.width, mask.height, true))
                {

                    var tri = atlasMesh.GetSubTriangleIndex(subMeshIndex);
                    var sUV = normalizedOriginalMesh.GetUVList();
                    var tUV = atlasMesh.GetUVList();

                    var transData = new TransTexture.TransData<Vector2>(tri, tUV, sUV);
                    TransTexture.ForTrans(tmpRt, mask, transData, 5f, TextureWrap.Loop, true);
                    var remappedMask = tmpRt.CopyTexture2D();
                    maskRef.SetValue(materialSlot, remappedMask);
                    // AssetDatabase.CreateAsset(remappedMask,AssetDatabase.GenerateUniqueAssetPath("Assets/RemappedMask.asset"));
                }
                materials.SetValue(materialSlot, subMeshIndex);
            }
        }
    }
}
#endif
