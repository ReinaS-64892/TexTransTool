#if UNITY_EDITOR && AAO_CONTAINS
using UnityEngine;
using net.rs64.TexTransTool.TextureAtlas;
using net.rs64.TexTransCore.Utils;
using net.rs64.TexTransCore;
using Anatawa12.AvatarOptimizer;


namespace net.rs64.TexTransTool.NDMF
{
    [AddComponentMenu("")]
    internal class TransmitReMappingToAAORemoveMeshByMask : MonoBehaviour, ITexTransToolTag, IAtlasReMappingReceiver
    {
        [HideInInspector, SerializeField] int _saveDataVersion = TexTransBehavior.TTTDataVersion;
        int ITexTransToolTag.SaveDataVersion => _saveDataVersion;

        public void ReMappingReceive(Mesh normalizedOriginalMesh, Mesh atlasMesh)
        {
            var transmitTarget = GetComponent<RemoveMeshByMask>();
            if (transmitTarget == null) { return; }

            var materials = transmitTarget.Materials;
            int length = materials.Length;

            for (var subMeshIndex = 0; length > subMeshIndex; subMeshIndex += 1)
            {
                var materialSlot = materials[subMeshIndex];

                if (materialSlot.Enabled is false) { continue; }

                var mask = materialSlot.Mask;
                using (TTRt.U(out var tmpRt, mask.width, mask.height, true))
                {
                    var tri = atlasMesh.GetSubTriangleIndex(subMeshIndex);
                    var sUV = normalizedOriginalMesh.GetUVList();
                    var tUV = atlasMesh.GetUVList();

                    var transData = new TransTexture.TransData<Vector2>(tri, tUV, sUV);
                    TransTexture.ForTrans(tmpRt, mask, transData, 5f, TextureWrap.Loop, true);
                    materialSlot.Mask = tmpRt.CopyTexture2D();
                    // AssetDatabase.CreateAsset(remappedMask,AssetDatabase.GenerateUniqueAssetPath("Assets/RemappedMask.asset"));
                }
                materials[subMeshIndex] = materialSlot;
            }
            transmitTarget.Materials = materials;
        }
    }
}
#endif
