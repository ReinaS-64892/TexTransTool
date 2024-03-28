using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore.TransTextureCore;
using net.rs64.TexTransCore.Island;
using Island = net.rs64.TexTransCore.Island.Island;
using static net.rs64.TexTransCore.TransTextureCore.TransTexture;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;
using net.rs64.TexTransTool.TextureAtlas.IslandRelocator;
using UnityEngine.Serialization;
using Unity.Collections;
using Unity.Mathematics;
using net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject;

namespace net.rs64.TexTransTool.TextureAtlas
{
    [AddComponentMenu(TTTName + "/" + MenuPath)]
    public sealed class AtlasTexture : TexTransRuntimeBehavior
    {
        internal const string ComponentName = "TTT AtlasTexture";
        internal const string MenuPath = ComponentName;
        public GameObject TargetRoot;
        public List<Renderer> Renderers => FilteredRenderers(TargetRoot, AtlasSetting.IncludeDisabledRenderer);
        public List<MatSelector> SelectMatList = new List<MatSelector>();
        public MaterialToIslandFineTuningMode MaterialToIslandFineTuningModeSelect;

        public AtlasSetting AtlasSetting = new AtlasSetting();

        internal override bool IsPossibleApply => TargetRoot != null;

        internal override List<Renderer> GetRenderers => Renderers;

        internal override TexTransPhase PhaseDefine => TexTransPhase.Optimizing;


        public enum MaterialToIslandFineTuningMode
        {
            SizeOffset = 0,
            SizePriority = 1,
        }

        #region V0SaveData
        [Obsolete("V0SaveData", true)] public List<AtlasTexture> MigrationV0ObsoleteChannelsRef;
        [Obsolete("V0SaveData", true)] public List<Material> SelectReferenceMat;//OrderedHashSetにしたかったけどシリアライズの都合で
        [Obsolete("V0SaveData", true)] public List<MatSelectorV0> MatSelectors = new List<MatSelectorV0>();
        [Serializable]
        [Obsolete("V0SaveData", true)]
        public class MatSelectorV0
        {
            public Material Material;
            public bool IsTarget = false;
            public int AtlasChannel = 0;
            public float TextureSizeOffSet = 1;
        }
        [Obsolete("V0SaveData", true)][SerializeField] internal List<AtlasSetting> AtlasSettings = new List<AtlasSetting>() { new AtlasSetting() };
        [Obsolete("V0SaveData", true)] public bool UseIslandCache = true;
        #endregion


        // public override bool IsPossibleCompile => TargetRoot != null && AtlasSettings.Count > 0;
        /*
        TargetRenderers 対象となるのはメッシュが存在しマテリアルスロットにNullが含まれていないかつ、無効化されておらず、エディターオンリーではないもの。

        MaterialReference レンダラーから集めた重複のないマテリアルの配列に対してのインデックス。

        MeshReference 上のメッシュ版。

        AtlasMeshData すべてのレンダラーをMeshReferenceとMaterialReferenceに変換し、まったく同じReferenceを持つものを消した物


        AtlasSettings アトラス化するときの細かい設定

        SelectRefsMat インスペクター上で表示されているマテリアルたちの配列。
        MatSelectors SelectRefsMatに含まれているマテリアルの参照を持ち マテリアルがターゲットであるか、大きさのオフセットやどのChannelに属しているかの情報を持っている。

        MatData MatSelectorsをMaterialReferenceとTextureSizeOffSet、PropAndTexturesにしたもの。
        このMaterialReferenceはSelectRefsMatを使ってインデックスに変換している。

        MeshAndMatRef MeshReferenceとマテリアルスロット分のMaterialReferenceをもち、適応するときこれをもとに、マテリアル違いやマテリアルの一部違いなども識別して適応する。

        */

        struct AtlasData
        {
            public Dictionary<string, Texture2D> Textures;
            public List<AtlasMeshAndDist> Meshes;
            public List<Material> AtlasInMaterials;
            public HashSet<Material>[] MaterialID;

            public struct AtlasMeshAndDist
            {
                public Mesh DistMesh;
                public Mesh AtlasMesh;
                public int[] MatIDs;

                public AtlasMeshAndDist(Mesh distMesh, Mesh atlasMesh, int[] mats)
                {
                    DistMesh = distMesh;
                    AtlasMesh = atlasMesh;
                    MatIDs = mats;
                }
            }
        }
        [Serializable]
        public struct MatSelector
        {
            public Material Material;
            [FormerlySerializedAs("AdditionalTextureSizeOffSet")] public float MaterialFineTuningValue;

            #region V1SaveData
            [Obsolete("V1SaveData", true)][SerializeField] internal float TextureSizeOffSet;
            #endregion

        }

        bool TryCompileAtlasTextures(IDomain domain, out AtlasData atlasData)
        {
            var texManage = domain.GetTextureManager();
            atlasData = new AtlasData();


            //情報を集めるフェーズ
            var NowContainsMatSet = new HashSet<Material>(RendererUtility.GetMaterials(Renderers));
            var targetMaterials = NowContainsMatSet.Distinct().Where(i => i != null)
            .Where(mat => SelectMatList.Any(smat => domain.OriginEqual(smat.Material, mat))).ToList();

            atlasData.AtlasInMaterials = targetMaterials;
            var atlasSetting = AtlasSetting;
            var propertyBakeSetting = atlasSetting.MergeMaterials ? atlasSetting.PropertyBakeSetting : PropertyBakeSetting.NotBake;
            // var atlasReferenceData = new AtlasReferenceData(targetMaterials, Renderers);
            var atlasContext = new AtlasContext(targetMaterials, Renderers, propertyBakeSetting != PropertyBakeSetting.NotBake);


            //サブメッシュより多いスロットの存在可否
            // if (atlasReferenceData.AtlasMeshDataList.Any(i => i.Triangles.Count < i.MaterialIndex.Length)) { TTTRuntimeLog.Warning("AtlasTexture:error:MoreMaterialSlotThanSubMesh"); }


            //ターゲットとなるマテリアルやそのマテリアルが持つテクスチャを引き出すフェーズ
            // shaderSupports.BakeSetting = atlasSetting.MergeMaterials ? atlasSetting.PropertyBakeSetting : PropertyBakeSetting.NotBake;
            // var materialTextures = new Dictionary<Material, List<PropAndTexture>>();
            // foreach (var mat in targetMaterials) { shaderSupports.AddRecord(mat); }
            // foreach (var mat in targetMaterials) { materialTextures[mat] = shaderSupports.GetTextures(mat, texManage); }



            var islandSizeOffset = GetTextureSizeOffset(SelectMatList.Select(i => i.Material), atlasSetting);

            //アイランドまわり
            if (atlasSetting.PixelNormalize)
            {
                foreach (var islandKV in atlasContext.IslandDict)
                {
                    var material = atlasContext.MaterialToAtlasShaderDict[atlasContext.MaterialGroup[islandKV.Key.MaterialGroupID].First()];
                    var refTex = material.TryGetValue("_MainTex", out var tex2D) ? tex2D.Texture2D : null;
                    if (refTex == null) { continue; }
                    foreach (var island in islandKV.Value)
                    {
                        island.Pivot.y = Mathf.Round(island.Pivot.y * refTex.height) / refTex.height;
                        island.Pivot.x = Mathf.Round(island.Pivot.x * refTex.width) / refTex.width;
                    }
                }
            }
            var islandArray = atlasContext.Islands;
            var rectArray = new IslandRect[islandArray.Length];
            var index = 0;
            foreach (var islandKV in atlasContext.Islands)
            {
                rectArray[index] = new IslandRect(islandKV);
                index += 1;
            }

            var sizePriority = new float[islandArray.Length]; for (var i = 0; sizePriority.Length > i; i += 1) { sizePriority[i] = 1f; }
            var islandDescription = new IslandSelector.IslandDescription[islandArray.Length];
            for (var i = 0; islandDescription.Length > i; i += 1)
            {
                var md = atlasContext.MeshDataDict[atlasContext.NormalizeMeshes[atlasContext.Meshes[atlasContext.IslandSubData[i].MeshID]]];

                var vertex = md.Vertices;
                var uv = md.VertexUV;
                var renderer = md.ReferenceRenderer;
                islandDescription[i] = new IslandSelector.IslandDescription(vertex, uv, renderer, atlasContext.IslandSubData[i].SubMeshIndex);
            }

            for (var i = 0; rectArray.Length > i; i += 1)
            {
                var desc = islandDescription[i];
                var mat = desc.Renderer.sharedMaterials[desc.MaterialSlot];
                var materialGroupID = Array.FindIndex(atlasContext.MaterialGroup, i => i.Any(m => m == mat));
                if (materialGroupID == -1) { continue; }
                var group = atlasContext.MaterialGroup[materialGroupID];
                var targets = SelectMatList.Where(i => group.Any(m => domain.OriginEqual(m, i.Material)));

                var materialFineTuningValue = 0f;
                var count = 0;
                foreach (var selector in targets)
                {
                    materialFineTuningValue += selector.MaterialFineTuningValue;
                    count += 1;
                }
                materialFineTuningValue /= count;

                switch (MaterialToIslandFineTuningModeSelect)
                {
                    case MaterialToIslandFineTuningMode.SizeOffset: { rectArray[i].Size *= materialFineTuningValue; break; }
                    case MaterialToIslandFineTuningMode.SizePriority: { sizePriority[i] = materialFineTuningValue; break; }
                    default: break;
                }

            }
            // foreach (var mat in SelectMatList)
            // {
            //     var textureSizeOffSet = islandSizeOffset[mat.Material];
            //     for (var i = 0; rectArray.Length > i; i += 1)
            //     {
            //         var desc = islandDescription[i];
            //         var matSlotRef = desc.MaterialSlot;
            //         if (!domain.OriginEqual(desc.Renderer.sharedMaterials[matSlotRef], mat.Material)) { continue; }

            //         rectArray[i].Size *= textureSizeOffSet;

            //         switch (MaterialToIslandFineTuningModeSelect)
            //         {
            //             case MaterialToIslandFineTuningMode.SizeOffset: { rectArray[i].Size *= mat.MaterialFineTuningValue; break; }
            //             case MaterialToIslandFineTuningMode.SizePriority: { sizePriority[i] = mat.MaterialFineTuningValue; break; }
            //             default: break;
            //         }
            //     }
            // }

            foreach (var islandFineTuner in atlasSetting.IslandFineTuners) { islandFineTuner.IslandFineTuning(sizePriority, rectArray, islandArray, islandDescription, domain); }
            for (var i = 0; sizePriority.Length > i; i += 1) { sizePriority[i] = Mathf.Clamp01(sizePriority[i]); }


            IAtlasIslandRelocator relocator = atlasSetting.AtlasIslandRelocator != null ? UnityEngine.Object.Instantiate(atlasSetting.AtlasIslandRelocator) : new NFDHPlasFC();

            relocator.Padding = atlasSetting.IslandPadding;
            var rectTangleMove = relocator.RectTangleMove;
            var relocatedRect = rectArray.ToArray();

            if (!relocator.Relocation(relocatedRect)) { relocatedRect = RelocateLoop(rectArray, sizePriority, relocator, relocatedRect); }

            IslandRect[] RelocateLoop(IslandRect[] rectArray, float[] sizePriority, IAtlasIslandRelocator relocator, IslandRect[] relocatedRect)
            {
                if (sizePriority.Any(f => !Mathf.Approximately(1, f)))
                {
                    var priorityMinSize = rectArray.ToArray();
                    for (var i = 0; priorityMinSize.Length > i; i += 1) { priorityMinSize[i].Size *= sizePriority[i]; }

                    for (var priLerp = 1f; 0 < priLerp; priLerp -= 0.05f)
                    {
                        for (var i = 0; rectArray.Length > i; i += 1)
                        {
                            if (relocatedRect[i].Is90Rotation)
                            {
                                var size = Vector3.Lerp(priorityMinSize[i].Size, rectArray[i].Size, priLerp);
                                (size.x, size.y) = (size.y, size.x);
                                relocatedRect[i].Size = size;
                            }
                            else
                            {
                                relocatedRect[i].Size = Vector3.Lerp(priorityMinSize[i].Size, rectArray[i].Size, priLerp);
                            }
                        }
                        // Debug.Log("priLerp-" + priLerp);
                        if (relocator.Relocation(relocatedRect)) { return relocatedRect; }
                    }
                    relocatedRect = priorityMinSize;
                }

                ScaleApplyDown(Mathf.Sqrt(0.5f / IslandRectUtility.CalculateAllAreaSum(relocatedRect)));
                var preRelocated = relocatedRect.ToArray();

                var safetyCount = 0;
                while (relocator.Relocation(relocatedRect) && safetyCount < 2048)//失敗するかセーフティにかかるまで、失敗したら前回の物を使用する
                {
                    safetyCount += 1;
                    // Debug.Log("safetyCount-" + safetyCount);

                    for (int i = 0; relocatedRect.Length > i; i++) { preRelocated[i] = relocatedRect[i]; }
                    ScaleApplyUp(1.01f);
                }

                void ScaleApplyDown(float scaleDownStep)
                {
                    for (int i = 0; relocatedRect.Length > i; i++)
                    {
                        relocatedRect[i].Size *= scaleDownStep;
                    }
                }
                void ScaleApplyUp(float scaleUpStep)
                {
                    for (int i = 0; relocatedRect.Length > i; i++)
                    {
                        var size = relocatedRect[i].Size *= scaleUpStep;


                        var maxLength = 0.99f - atlasSetting.IslandPadding - atlasSetting.IslandPadding;
                        if (size.x > maxLength || size.y > maxLength)//一つ大きいのがあるとすべて使いきれなくなってしまうために、これは必要。
                        {
                            var max = Mathf.Max(size.x, size.y);
                            size *= maxLength / max;
                            relocatedRect[i].Size = size;
                        }
                    }
                }

                return preRelocated;
            }



            if (atlasSetting.PixelNormalize)
            {
                for (var i = 0; relocatedRect.Length > i; i += 1)
                {
                    var island = relocatedRect[i];
                    island.Pivot.y = Mathf.Round(island.Pivot.y * atlasSetting.AtlasTextureSize) / atlasSetting.AtlasTextureSize;
                    island.Pivot.x = Mathf.Round(island.Pivot.x * atlasSetting.AtlasTextureSize) / atlasSetting.AtlasTextureSize;
                    relocatedRect[i] = island;
                }
            }
            for (var i = 0; relocatedRect.Length > i; i += 1)
            {
                if (relocatedRect[i].Size.x <= 0.0001f) { relocatedRect[i].Size.x = 0.0001f; }
                if (relocatedRect[i].Size.y <= 0.0001f) { relocatedRect[i].Size.y = 0.0001f; }
            }//Islandが小さすぎると RectTangleMoveのコピーがうまくいかない

            //上側を削れるかを見る
            var height = IslandRectUtility.CalculateIslandsMaxHeight(relocatedRect);
            var atlasTextureHeightSize = Mathf.Max(GetHeightSize(atlasSetting.AtlasTextureSize, height), 4);//4以下はちょっと怪しい挙動しそうだからクランプ

            // var areaSum = IslandRectUtility.CalculateAllAreaSum(islandRectPool.Values);
            // Debug.Log(areaSum + ":AreaSum" + "-" + height + ":height");

            var aspectIslandsRectPool = GetAspectIslandRect(relocatedRect, atlasSetting, atlasTextureHeightSize);


            //新しいUVを持つMeshを生成するフェーズ
            var compiledMeshes = new List<AtlasData.AtlasMeshAndDist>();
            var normMeshes = atlasContext.Meshes.Select(m => atlasContext.NormalizeMeshes[m]).ToArray();
            var subSetMovedUV = new NativeArray<Vector2>[atlasContext.AtlasSubSets.Count];
            for (int subSetIndex = 0; atlasContext.AtlasSubSets.Count > subSetIndex; subSetIndex += 1)
            {
                var subSet = atlasContext.AtlasSubSets[subSetIndex];
                var distMesh = atlasContext.Meshes[subSet.First(i => i.HasValue).Value.MeshID];
                var nmMesh = normMeshes[subSet.First(i => i.HasValue).Value.MeshID];
                var meshData = atlasContext.MeshDataDict[nmMesh];
                var newMesh = UnityEngine.Object.Instantiate<Mesh>(nmMesh);
                newMesh.name = "AtlasMesh_" + subSetIndex + "_" + nmMesh.name;


                var originLink = new LinkedList<Island>();
                var movedLink = new LinkedList<IslandRect>();


                for (var islandIndex = 0; aspectIslandsRectPool.Length > islandIndex; islandIndex += 1)
                {
                    if (subSet.Any(subData => atlasContext.IslandSubData[islandIndex] == subData))
                    {
                        originLink.AddLast(atlasContext.Islands[islandIndex]);
                        movedLink.AddLast(aspectIslandsRectPool[islandIndex]);
                    }
                }

                // var thisTag = new AtlasSlotRef(amdIndex, slotIndex);
                // if (containsSlotRefs.Contains(thisTag)) { foreach (var islandKV in AtlasReferenceData.IslandFind(thisTag, aspectIslandsRectPool)) { movedPool.Add(islandKV.Key, islandKV.Value); } }
                // else
                // {
                //     var msmRef = new MSMRef(atlasMeshData.ReferenceMesh, slotIndex, atlasMeshData.MaterialIndex[slotIndex]);
                //     var identicalTag = atlasReferenceData.FindIdenticalSlotRef(containsSlotRefs, msmRef);

                //     if (!identicalTag.HasValue) { continue; }
                //     foreach (var islandKV in AtlasReferenceData.IslandFind(identicalTag.Value, aspectIslandsRectPool)) { movedPool.Add(islandKV.Key, islandKV.Value); }
                // }


                var movedUV = new NativeArray<Vector2>(meshData.VertexUV, Allocator.Temp);
                IslandUtility.IslandPoolMoveUV(meshData.VertexUV, movedUV, originLink.ToArray(), movedLink.ToArray());

                newMesh.SetUVs(0, movedUV);
                if (AtlasSetting.WriteOriginalUV) { newMesh.SetUVs(1, meshData.VertexUV); }
                subSetMovedUV[subSetIndex] = movedUV;

                compiledMeshes.Add(new AtlasData.AtlasMeshAndDist(distMesh, newMesh, subSet.Select(i => i?.MaterialGroupID ?? -1).ToArray()));
            }
            atlasData.Meshes = compiledMeshes;


            //アトラス化したテクスチャーを生成するフェーズ
            var compiledAtlasTextures = new Dictionary<string, Texture2D>();

            var propertyNames = atlasContext.MaterialToAtlasShaderDict.Values.SelectMany(i => i.Keys).ToHashSet();
            var groupedTextures = atlasContext.MaterialGroup
            .Select(mg => (Array.IndexOf(atlasContext.MaterialGroup, mg), mg.Select(m => atlasContext.MaterialToAtlasShaderDict[m])))
            .Select(mg => (mg.Item1, ZipDict(mg.Item2)))
            .ToDictionary(i => i.Item1, i => i.Item2);

            Dictionary<string, Texture2D> ZipDict(IEnumerable<Dictionary<string, AtlasShaderTexture2D>> keyValuePairs)
            {
                var dict = new Dictionary<string, Texture2D>();
                foreach (var kv in keyValuePairs.SelectMany(i => i))
                {
                    if (kv.Value.Texture2D == null) { continue; }
                    dict[kv.Key] = kv.Value.Texture2D;
                }
                return dict;
            }



            foreach (var propName in propertyNames)
            {
                var targetRT = RenderTexture.GetTemporary(atlasSetting.AtlasTextureSize, atlasTextureHeightSize, 32);
                targetRT.Clear();
                targetRT.name = "AtlasTex" + propName;
                foreach (var gTex in groupedTextures)
                {
                    // var souseProp2Tex = gTex.MatPropKV.Value.Find(I => I.PropertyName == propName);
                    // if (souseProp2Tex == null) continue;
                    if (!gTex.Value.TryGetValue(propName, out var sTexture)) { continue; }
                    Texture souseTex = sTexture is Texture2D ? texManage.GetOriginTempRt(sTexture as Texture2D, sTexture.width) : sTexture;

                    // var findMaterial = MatPropKV.Key;
                    var findMaterialID = gTex.Key;
                    if (rectTangleMove)
                    {
                        var findSubDataHash = atlasContext.AtlasSubAll.Where(i => i.MaterialGroupID == findMaterialID).ToHashSet();
                        var islandPairs = new Dictionary<Island, IslandRect>();
                        for (var islandIndex = 0; relocatedRect.Length > islandIndex; islandIndex += 1)
                        {
                            if (findSubDataHash.Contains(atlasContext.IslandSubData[islandIndex]))
                            {
                                islandPairs[atlasContext.Islands[islandIndex]] = relocatedRect[islandIndex];
                            }
                        }

                        TransMoveRectIsland(souseTex, targetRT, islandPairs, atlasSetting.IslandPadding);
                    }
                    else
                    {
                        for (var subSetIndex = 0; atlasContext.AtlasSubSets.Count > subSetIndex; subSetIndex += 1)
                        {
                            var transTargets = atlasContext.AtlasSubSets[subSetIndex].Where(i => i.HasValue).Where(i => i.Value.MaterialGroupID == findMaterialID).Select(i => i.Value);
                            if (!transTargets.Any()) { continue; }

                            var triangles = transTargets.SelectMany(subData => atlasContext.IslandDict[subData].SelectMany(i => i.triangles));
                            var originUV = atlasContext.MeshDataDict[atlasContext.NormalizeMeshes[atlasContext.Meshes[transTargets.First().MeshID]]].VertexUV;

                            var transData = new TransData<Vector2>(triangles, subSetMovedUV[subSetIndex], originUV);
                            ForTrans(targetRT, souseTex, transData, atlasSetting.GetTexScalePadding * 0.5f, null, true);
                        }
                    }

                    // UnityEditor.AssetDatabase.CreateAsset(targetRT.CopyTexture2D(), UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/temp.asset"));

                    if (sTexture is Texture2D && souseTex is RenderTexture tempRt) { RenderTexture.ReleaseTemporary(tempRt); }
                }

                compiledAtlasTextures.Add(propName, targetRT.CopyTexture2D());
                RenderTexture.ReleaseTemporary(targetRT);
            }
            // foreach (var matData in materialTextures)
            // {
            //     foreach (var pTex in matData.Value)
            //     {
            //         if (pTex.Texture == null) { continue; }
            //         switch (pTex.Texture)
            //         {
            //             case RenderTexture renderTexture:
            //                 {
            //                     RenderTexture.ReleaseTemporary(renderTexture);
            //                     break;
            //                 }
            //             case Texture2D texture2D:
            //             default:
            //                 { break; }
            //         }
            //     }
            // }

            atlasData.Textures = compiledAtlasTextures;
            atlasData.MaterialID = atlasContext.MaterialGroup.Select(i => i.ToHashSet()).ToArray();
            atlasContext.Dispose();
            foreach (var movedUV in subSetMovedUV) { movedUV.Dispose(); }

            return true;
        }

        private static Dictionary<Material, float> GetTextureSizeOffset(IEnumerable<Material> targetMaterialSelectors, AtlasSetting atlasSetting)
        {
            float atlasTexPixelCount = atlasSetting.AtlasTextureSize * atlasSetting.AtlasTextureSize;
            var islandSizeOffset = new Dictionary<Material, float>();
            foreach (var material in targetMaterialSelectors)
            {
                var tex = material.mainTexture;
                float textureSizeOffset;
                if (tex != null)
                {
                    textureSizeOffset = tex.width * tex.height / atlasTexPixelCount;
                }
                else { textureSizeOffset = (float)0.01f; }

                islandSizeOffset[material] = Mathf.Sqrt(textureSizeOffset);
            }

            return islandSizeOffset;
        }

        private static IslandRect[] GetAspectIslandRect(IslandRect[] islandRectPool, AtlasSetting atlasSetting, int atlasTextureHeightSize)
        {
            if (atlasTextureHeightSize == atlasSetting.AtlasTextureSize) { return islandRectPool; }

            var heightSizeScale = atlasSetting.AtlasTextureSize / (float)atlasTextureHeightSize;
            var aspectIslands = new IslandRect[islandRectPool.Length];
            for (var i = 0; islandRectPool.Length > i; i += 1)
            {
                var islandRect = islandRectPool[i];
                islandRect.Pivot.y *= heightSizeScale;
                islandRect.Size.y *= heightSizeScale;
                aspectIslands[i] = islandRect;
            }
            return aspectIslands;
        }

        private static int GetHeightSize(int atlasTextureSize, float height)
        {
            switch (height)
            {
                default: return atlasTextureSize;
                case < (1 / 8f): return atlasTextureSize / 8;
                case < (1 / 4f): return atlasTextureSize / 4;
                case < (1 / 2f): return atlasTextureSize / 2;
            }
        }

        internal override void Apply(IDomain domain = null)
        {
            if (!IsPossibleApply)
            {
                TTTRuntimeLog.Error("AtlasTexture:error:TTTNotExecutable");
                return;
            }

            domain.ProgressStateEnter("AtlasTexture");
            domain.ProgressUpdate("CompileAtlasTexture", 0f);

            if (!TryCompileAtlasTextures(domain, out var atlasData)) { return; }

            domain.ProgressUpdate("MeshChange", 0.5f);

            var nowRenderers = Renderers;

            var shaderSupport = new AtlasShaderSupportUtils();

            //Mesh Change
            foreach (var renderer in nowRenderers)
            {
                var mesh = renderer.GetMesh();
                var matIDs = renderer.sharedMaterials.Select(i => Array.FindIndex(atlasData.MaterialID, mh => mh.Contains(i)));
                var atlasMeshAndDist = atlasData.Meshes.FindAll(I => I.DistMesh == mesh).Find(I => I.MatIDs.SequenceEqual(matIDs));
                if (atlasMeshAndDist.AtlasMesh == null) { continue; }

                var atlasMesh = atlasMeshAndDist.AtlasMesh;
                domain.SetMesh(renderer, atlasMesh);
                domain.TransferAsset(atlasMesh);
            }

            domain.ProgressUpdate("Texture Fine Tuning", 0.75f);

            //Texture Fine Tuning
            var atlasTexFineTuningTargets = TexFineTuningUtility.ConvertForTargets(atlasData.Textures);
            TexFineTuningUtility.InitTexFineTuning(atlasTexFineTuningTargets);
            foreach (var fineTuning in AtlasSetting.TextureFineTuning)
            {
                fineTuning.AddSetting(atlasTexFineTuningTargets);
            }
            TexFineTuningUtility.FinalizeTexFineTuning(atlasTexFineTuningTargets);
            var atlasTexture = TexFineTuningUtility.ConvertForPropAndTexture2D(atlasTexFineTuningTargets);
            domain.transferAssets(atlasTexture.Select(PaT => PaT.Texture2D));

            //CompressDelegation
            foreach (var atlasTexFTData in atlasTexFineTuningTargets)
            {
                var tex = atlasTexFTData.Texture2D;
                var compressSetting = atlasTexFTData.TuningDataList.Find(I => I is CompressionQualityData) as CompressionQualityData;
                if (compressSetting == null) { continue; }
                var compressSettingTuple = (CompressionQualityApplicant.GetTextureFormat(tex, compressSetting), (int)compressSetting.CompressionQuality);
                domain.GetTextureManager().TextureCompressDelegation(compressSettingTuple, atlasTexFTData.Texture2D);
            }


            domain.ProgressUpdate("MaterialGenerate And Change", 0.9f);

            //MaterialGenerate And Change
            if (AtlasSetting.MergeMaterials)
            {
                var mergeMat = AtlasSetting.MergeReferenceMaterial != null ? AtlasSetting.MergeReferenceMaterial : atlasData.AtlasInMaterials.First();
                Material generateMat = GenerateAtlasMat(mergeMat, atlasTexture, shaderSupport, AtlasSetting.ForceSetTexture);

                domain.ReplaceMaterials(atlasData.AtlasInMaterials.ToDictionary(x => x, _ => generateMat), rendererOnly: true);
            }
            else
            {
                var materialMap = new Dictionary<Material, Material>();
                foreach (var MatSelector in atlasData.AtlasInMaterials)
                {
                    var distMat = MatSelector;
                    var generateMat = GenerateAtlasMat(distMat, atlasTexture, shaderSupport, AtlasSetting.ForceSetTexture);
                    materialMap.Add(distMat, generateMat);
                }
                domain.ReplaceMaterials(materialMap);
            }

            domain.ProgressUpdate("End", 1);
            domain.ProgressStateExit();
        }

        private void TransMoveRectIsland<TIslandRect>(Texture souseTex, RenderTexture targetRT, Dictionary<Island, TIslandRect> notAspectIslandPairs, float uvScalePadding) where TIslandRect : IIslandRect
        {
            uvScalePadding *= 0.5f;
            var targetAspect = targetRT.width / (float)targetRT.height;
            var sUV = new List<Vector2>();
            var tUV = new List<Vector2>();
            var triangles = new List<TriangleIndex>();

            var nawIndex = 0;
            foreach (var islandPair in notAspectIslandPairs)
            {
                var (Origin, Moved) = (islandPair.Key, islandPair.Value);
                var rectScalePadding = Moved.UVScaleToRectScale(uvScalePadding);

                var originVertexes = Origin.GenerateRectVertexes(rectScalePadding);
                var movedVertexes = Moved.GenerateRectVertexes(rectScalePadding);

                triangles.Add(new(nawIndex + 0, nawIndex + 1, nawIndex + 2));
                triangles.Add(new(nawIndex + 0, nawIndex + 2, nawIndex + 3));
                nawIndex += 4;
                sUV.AddRange(originVertexes);
                tUV.AddRange(movedVertexes.Select(v => new Vector2(v.x, v.y * targetAspect)));
            }

            TransTexture.ForTrans(targetRT, souseTex, new TransData<Vector2>(triangles, tUV, sUV), argTexWrap: TextureWrap.Loop);

        }
        internal static float Frac(float v) { return v > 0 ? v - Mathf.Floor(v) : v - Mathf.Ceil(v); }

        private static Material GenerateAtlasMat(Material targetMat, List<PropAndTexture2D> atlasTex, AtlasShaderSupportUtils shaderSupport, bool forceSetTexture)
        {
            var editableTMat = UnityEngine.Object.Instantiate(targetMat);

            editableTMat.SetTextures(atlasTex, forceSetTexture);
            //TODO : これどっかでいい感じに誰かがやらないといけない
            //editableTMat.RemoveUnusedProperties();
            var supporter = shaderSupport.GetAtlasShaderSupporter(editableTMat);
            //TODO : なんとかして
            return editableTMat;
        }



        internal static List<Renderer> FilteredRenderers(GameObject targetRoot, bool includeDisabledRenderer)
        {
            var result = new List<Renderer>();
            foreach (var item in targetRoot.GetComponentsInChildren<Renderer>(includeDisabledRenderer))
            {
                if (item.tag == "EditorOnly") continue;
                if (item.GetMesh() == null) continue;
                if (item.GetMesh().uv.Any() == false) continue;
                if (item.sharedMaterials.Length == 0) continue;
                if (item.sharedMaterials.Any(Mat => Mat == null)) continue;
                if (item.GetComponent<AtlasExcludeTag>() != null) continue;

                result.Add(item);
            }
            return result;
        }

    }
}
