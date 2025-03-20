#nullable enable
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;
using net.rs64.TexTransTool.TextureAtlas.IslandRelocator;
using UnityEngine.Serialization;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.UVIsland;
using System.Runtime.InteropServices;
using System.Buffers.Binary;

namespace net.rs64.TexTransTool.TextureAtlas
{
    [AddComponentMenu(TTTName + "/" + MenuPath)]
    public sealed partial class AtlasTexture : TexTransRuntimeBehavior
    {
        internal const string ComponentName = "TTT AtlasTexture";
        internal const string MenuPath = ComponentName;
        internal override TexTransPhase PhaseDefine => TexTransPhase.Optimizing;


        [FormerlySerializedAs("TargetRoot")] public GameObject LimitCandidateMaterials;

        public List<MatSelector> SelectMatList = new List<MatSelector>();
        [Serializable]
        public struct MatSelector
        {
            public Material Material;
            public float MaterialFineTuningValue;

            #region V3SaveData
            [Obsolete("V3SaveData", true)][SerializeField] internal float AdditionalTextureSizeOffSet;
            #endregion
            #region V1SaveData
            [Obsolete("V1SaveData", true)][SerializeField] internal float TextureSizeOffSet;
            #endregion
        }

        public List<MaterialMergeGroup> MergeMaterialGroups = new();
        [Serializable]
        public class MaterialMergeGroup
        {
            public List<Material> Group = new();
            public Material? Reference;
        }
        public Material? AllMaterialMergeReference;



        public AtlasSetting AtlasSetting = new AtlasSetting();

        // internal bool TryCompileAtlasTextures(List<Renderer> nowTargetAllowedRenderer, List<Material> targetMaterials, IDomain domain, out AtlasData atlasData)
        // {
        //     Profiler.BeginSample("AtlasData and FindRenderers");
        //     var texManage = domain.GetTextureManager();
        //     atlasData = new AtlasData();


        //     //情報を集めるフェーズ
        //     var targetMaterialsHash = targetMaterials.ToHashSet();
        //     var sizePriorityDict = targetMaterials.ToDictionary(i => i, i => SelectMatList.First(m => domain.OriginEqual(m.Material, i)).MaterialFineTuningValue);

        //     atlasData.AtlasInMaterials = targetMaterialsHash;
        //     if (atlasData.AtlasInMaterials.Any() is false) { return false; }

        //     var atlasSetting = AtlasSetting;
        //     var propertyBakeSetting = atlasSetting.MergeMaterials ? atlasSetting.PropertyBakeSetting : PropertyBakeSetting.NotBake;
        //     Profiler.EndSample();

        //     Profiler.BeginSample("AtlasContext:ctor");
        //     var targeting = domain as IRendererTargeting;
        //     var targetRenderers = nowTargetAllowedRenderer
        //         .Where(r => targeting.GetMesh(r) != null)
        //         .Where(r => targeting.GetMaterials(r)
        //             .Where(i => i != null)
        //             .Cast<Material>()
        //             .Any(targetMaterials.Contains)
        //         ).ToArray();        //     //情報を集めるフェーズ
        //     var targetMaterialsHash = targetMaterials.ToHashSet();
        //     var sizePriorityDict = targetMaterials.ToDictionary(i => i, i => SelectMatList.First(m => domain.OriginEqual(m.Material, i)).MaterialFineTuningValue);

        //     atlasData.AtlasInMaterials = targetMaterialsHash;
        //     if (atlasData.AtlasInMaterials.Any() is false) { return false; }

        //     var atlasSetting = AtlasSetting;
        //     var propertyBakeSetting = atlasSetting.MergeMaterials ? atlasSetting.PropertyBakeSetting : PropertyBakeSetting.NotBake;
        //     Profiler.EndSample();

        //     Profiler.BeginSample("AtlasContext:ctor");
        //     var targeting = domain as IRendererTargeting;
        //     var targetRenderers = nowTargetAllowedRenderer
        //         .Where(r => targeting.GetMesh(r) != null)
        //         .Where(r => targeting.GetMaterials(r)
        //             .Where(i => i != null)
        //             .Cast<Material>()
        //             .Any(targetMaterials.Contains)
        //         ).ToArray();


        //     var atlasContext = new AtlasContext(targeting, targetRenderers, targetMaterialsHash, UVChannel.UV0);
        //     Profiler.EndSample();


        //     //アイランドまわり
        //     if (atlasSetting.PixelNormalize)
        //     {
        //         Profiler.BeginSample("AtlasReferenceData:PixelNormalize");
        //         foreach (var islandKV in atlasContext.IslandDict)
        //         {
        //             var material = atlasContext.MaterialToAtlasShaderTexDict[atlasContext.MaterialGroup[islandKV.Key.MaterialGroupID].First()];
        //             var refTex = material.TryGetValue("_MainTex", out var tex2D) ? tex2D.Texture : null;
        //             if (refTex == null) { continue; }
        //             foreach (var island in islandKV.Value) { NormalizeIsland(refTex.width, refTex.height, island); }
        //         }
        //         Profiler.EndSample();
        //     }


        //     Profiler.BeginSample("IslandFineTuning");
        //     var islandArray = atlasContext.Islands;
        //     var rectArray = new IslandRect[islandArray.Length];
        //     var index = 0;
        //     foreach (var islandKV in atlasContext.Islands)
        //     {
        //         rectArray[index] = new IslandRect(islandKV);
        //         index += 1;
        //     }

        //     Profiler.BeginSample("TextureIslandScaling");
        //     var textureIslandScale = new Dictionary<int, float>();
        //     var textureIslandAspect = new Dictionary<int, float>();
        //     for (var i = 0; atlasContext.MaterialGroup.Length > i; i += 1)
        //     {
        //         var tex = atlasContext.MaterialGroup[i].Select(m => m.mainTexture).Where(t => t != null).FirstOrDefault();
        //         if (tex != null)
        //         {
        //             var atlasTexPixelCount = atlasSetting.AtlasTextureSize * atlasSetting.AtlasTextureSize;
        //             var texPixelCount = tex.width * tex.height;
        //             textureIslandScale[i] = Mathf.Sqrt(texPixelCount / (float)atlasTexPixelCount);
        //             textureIslandAspect[i] = tex.width / tex.height;
        //         }
        //         else
        //         {
        //             textureIslandScale[i] = (float)0.01f;
        //             textureIslandAspect[i] = 1f;
        //         }

        //     }
        //     for (var i = 0; rectArray.Length > i; i += 1)
        //     {
        //         var subData = atlasContext.IslandSubData[i];
        //         rectArray[i].Size *= textureIslandScale[subData.MaterialGroupID];
        //         rectArray[i].Size.y *= textureIslandAspect[subData.MaterialGroupID];
        //     }
        //     Profiler.EndSample();

        //     var sizePriority = new float[islandArray.Length]; for (var i = 0; sizePriority.Length > i; i += 1) { sizePriority[i] = 1f; }
        //     var islandDescription = new IslandSelector.IslandDescription[islandArray.Length];
        //     for (var i = 0; islandDescription.Length > i; i += 1)
        //     {
        //         var md = atlasContext.MeshDataDict[atlasContext.NormalizeMeshes[atlasContext.Meshes[atlasContext.IslandSubData[i].MeshID]]];

        //         var vertex = md.Vertices;
        //         var uv = md.VertexUV;
        //         var renderer = md.ReferenceRenderer;
        //         islandDescription[i] = new IslandSelector.IslandDescription(vertex, uv, renderer, atlasContext.IslandSubData[i].SubMeshIndex);
        //     }

        //     for (var i = 0; rectArray.Length > i; i += 1)
        //     {
        //         var desc = islandDescription[i];
        //         var mat = desc.Renderer.sharedMaterials[desc.MaterialSlot];
        //         var materialGroupID = Array.FindIndex(atlasContext.MaterialGroup, i => i.Any(m => m == mat));
        //         if (materialGroupID == -1) { continue; }
        //         var group = atlasContext.MaterialGroup[materialGroupID];

        //         var materialFineTuningValue = 0f;
        //         var count = 0;
        //         foreach (var gm in group)
        //         {
        //             if (sizePriorityDict.TryGetValue(gm, out var priorityValue))
        //             {
        //                 materialFineTuningValue += priorityValue;
        //                 count += 1;
        //             }
        //         }
        //         materialFineTuningValue /= count;
        //         sizePriority[i] = materialFineTuningValue;
        //     }

        //     foreach (var islandFineTuner in atlasSetting.IslandFineTuners) { islandFineTuner?.IslandFineTuning(sizePriority, islandArray, islandDescription, domain); }
        //     for (var i = 0; sizePriority.Length > i; i += 1) { sizePriority[i] = Mathf.Clamp01(sizePriority[i]); }
        //     Profiler.EndSample();


        //     IAtlasIslandRelocator relocator = atlasSetting.AtlasIslandRelocator != null ? UnityEngine.Object.Instantiate(atlasSetting.AtlasIslandRelocator) : new NFDHPlasFC();


        //     Profiler.BeginSample("Relocation");
        //     var relocateManage = new IslandRelocationManager(relocator);
        //     relocateManage.Padding = atlasSetting.IslandPadding;
        //     relocateManage.ForceSizePriority = atlasSetting.ForceSizePriority;
        //     relocateManage.HeightDenominator = atlasSetting.HeightDenominator;

        //     var timer = System.Diagnostics.Stopwatch.StartNew();
        //     var relocatedRect = relocateManage.RelocateLoop(rectArray, sizePriority, out var relocateResult);
        //     timer.Stop();
        //     Profiler.EndSample();

        //     if (relocateResult.IsRelocateSuccess is false) { TTTRuntimeLog.Error("AtlasTexture:error:RelocationFailed"); }

        //     var rectTangleMove = relocator.RectTangleMove;

        //     if (relocator is UnityEngine.Object unityObject) { DestroyImmediate(unityObject); }

        //     if (atlasSetting.PixelNormalize)
        //     {
        //         if (Mathf.Approximately(atlasSetting.IslandPadding, 0)) { TTTRuntimeLog.Warning("AtlasTexture:warn:IslandPaddingIsZeroAndPixelNormalizeUsed"); }
        //         Profiler.BeginSample("AtlasReferenceData:PixelNormalize");
        //         for (var i = 0; relocatedRect.Length > i; i += 1)
        //         {
        //             relocatedRect[i] = NormalizeIsland(atlasSetting.AtlasTextureSize, atlasSetting.AtlasTextureSize, relocatedRect[i]);
        //         }
        //         Profiler.EndSample();
        //     }
        //     Profiler.BeginSample("IslandMinClamp");
        //     for (var i = 0; relocatedRect.Length > i; i += 1)
        //     {
        //         if (relocatedRect[i].Size.x <= 0.0001f) { relocatedRect[i].Size.x = 0.0001f; }
        //         if (relocatedRect[i].Size.y <= 0.0001f) { relocatedRect[i].Size.y = 0.0001f; }
        //     }//Islandが小さすぎると RectTangleMoveのコピーがうまくいかない
        //     Profiler.EndSample();

        //     //上側を削れるかを見る
        //     Profiler.BeginSample("IslandHight Calculate");
        //     var height = IslandRectUtility.CalculateIslandsMaxHeight(relocatedRect);
        //     var atlasTextureHeightSize = Mathf.Max(GetNormalizedMinHeightSize(atlasSetting.AtlasTextureSize, height), 4);//4以下はちょっと怪しい挙動しそうだからクランプ
        //     Debug.Assert(Mathf.IsPowerOfTwo(atlasTextureHeightSize));
        //     Profiler.EndSample();

        //     TTTRuntimeLog.Info("AtlasTexture:info:RelocateResult", 1 - height, relocateResult.PriorityDownScale, relocateResult.OverallDownScale, relocateResult.TotalRelocateCount, timer.ElapsedMilliseconds);


        //     //新しいUVを持つMeshを生成するフェーズ
        //     Profiler.BeginSample("MeshCompile");
        //     var compiledMeshes = new List<AtlasData.AtlasMeshHolder>();
        //     var normMeshes = atlasContext.Meshes.Select(m => atlasContext.NormalizeMeshes[m]).ToArray();
        //     var subSetMovedUV = new NativeArray<Vector2>[atlasContext.AtlasSubSets.Count];
        //     var scale = atlasSetting.AtlasTextureSize / atlasTextureHeightSize;
        //     for (int subSetIndex = 0; atlasContext.AtlasSubSets.Count > subSetIndex; subSetIndex += 1)
        //     {
        //         var subSet = atlasContext.AtlasSubSets[subSetIndex];
        //         var distMesh = atlasContext.Meshes[subSet.First(i => i.HasValue).Value.MeshID];
        //         var nmMesh = normMeshes[subSet.First(i => i.HasValue).Value.MeshID];
        //         var meshData = atlasContext.MeshDataDict[nmMesh];
        //         var newMesh = UnityEngine.Object.Instantiate<Mesh>(nmMesh);
        //         newMesh.name = "AtlasMesh_" + subSetIndex + "_" + nmMesh.name;


        //         var originLink = new LinkedList<Island>();
        //         var movedLink = new LinkedList<IslandRect>();


        //         for (var islandIndex = 0; relocatedRect.Length > islandIndex; islandIndex += 1)
        //         {
        //             if (subSet.Any(subData => atlasContext.IslandSubData[islandIndex] == subData))
        //             {
        //                 originLink.AddLast(atlasContext.Islands[islandIndex]);
        //                 movedLink.AddLast(relocatedRect[islandIndex]);
        //             }
        //         }

        //         var movedUV = new NativeArray<Vector2>(meshData.VertexUV, Allocator.TempJob);
        //         IslandUtility.IslandPoolMoveUV(meshData.VertexUV, movedUV, originLink.ToArray(), movedLink.ToArray());

        //         subSetMovedUV[subSetIndex] = movedUV;
        //         if (atlasSetting.AtlasTextureSize != atlasTextureHeightSize)
        //         {
        //             using (var aspectApplyUV = new NativeArray<Vector2>(movedUV, Allocator.Temp))
        //             {
        //                 var writer = aspectApplyUV.AsSpan();
        //                 foreach (var vi in originLink.SelectMany(i => i.GetVertexIndex()).Distinct())
        //                 {
        //                     writer[vi].x = movedUV[vi].x;
        //                     writer[vi].y = movedUV[vi].y * scale;
        //                 }
        //                 newMesh.SetUVs(0, aspectApplyUV);
        //             }
        //         }
        //         else
        //         {
        //             newMesh.SetUVs(0, movedUV);
        //         }
        //         if (atlasSetting.WriteOriginalUV)
        //         {
        //             var writeTarget = math.clamp(atlasSetting.OriginalUVWriteTargetChannel, 1, 7);
        //             if (newMesh.HasUV(writeTarget)) { TTTRuntimeLog.Info("AtlasTexture:warn:OriginalUVWriteTargetForAlreadyUV", writeTarget, distMesh); }
        //             newMesh.SetUVs(writeTarget, meshData.VertexUV);
        //         }

        //         compiledMeshes.Add(new AtlasData.AtlasMeshHolder(distMesh, UnityEngine.Object.Instantiate(nmMesh), newMesh, subSet.Select(i => i?.MaterialGroupID ?? -1).ToArray()));
        //     }
        //     atlasData.Meshes = compiledMeshes;
        //     Profiler.EndSample();

        //     //アトラス化したテクスチャーを生成するフェーズ
        //     var compiledAtlasTextures = new Dictionary<string, AsyncTexture2D>();

        //     Profiler.BeginSample("GetGroupedTextures");
        //     var groupedTextures = GetGroupedTextures(texManage, atlasSetting, atlasContext, propertyBakeSetting, out var containsProperty, out var bakePropMaxValue);
        //     Profiler.EndSample();

        //     Profiler.BeginSample("Texture synthesis");
        //     foreach (var propName in containsProperty)
        //     {
        //         var targetRT = TTRt.G(atlasSetting.AtlasTextureSize, atlasSetting.AtlasTextureSize, true, true, true, true);
        //         TextureUtility.FillColor(targetRT, atlasSetting.BackGroundColor);
        //         targetRT.name = "AtlasTex" + propName;
        //         Profiler.BeginSample("Draw:" + targetRT.name);
        //         foreach (var gTex in groupedTextures)
        //         {
        //             if (!gTex.Value.TryGetValue(propName, out var sTexture)) { continue; }

        //             var findMaterialID = gTex.Key;
        //             if (rectTangleMove)
        //             {
        //                 var findSubDataHash = atlasContext.AtlasSubMeshIndexIDHash.Where(i => i.MaterialGroupID == findMaterialID).ToHashSet();
        //                 var islandPairs = new Dictionary<Island, IslandRect>();
        //                 for (var islandIndex = 0; relocatedRect.Length > islandIndex; islandIndex += 1)
        //                 {
        //                     if (findSubDataHash.Contains(atlasContext.IslandSubData[islandIndex]))
        //                     {
        //                         islandPairs[atlasContext.Islands[islandIndex]] = relocatedRect[islandIndex];
        //                     }
        //                 }

        //                 TransMoveRectIsland(sTexture, targetRT, islandPairs, atlasSetting.IslandPadding);
        //             }
        //             else
        //             {
        //                 for (var subSetIndex = 0; atlasContext.AtlasSubSets.Count > subSetIndex; subSetIndex += 1)
        //                 {
        //                     var transTargets = atlasContext.AtlasSubSets[subSetIndex].Where(i => i.HasValue).Where(i => i.Value.MaterialGroupID == findMaterialID).Select(i => i.Value);
        //                     if (!transTargets.Any()) { continue; }

        //                     var triangles = new NativeArray<TriangleIndex>(transTargets.SelectMany(subData => atlasContext.IslandDict[subData].SelectMany(i => i.triangles)).ToArray(), Allocator.TempJob);
        //                     var originUV = atlasContext.MeshDataDict[atlasContext.NormalizeMeshes[atlasContext.Meshes[transTargets.First().MeshID]]].VertexUV;

        //                     var transData = new TransData(triangles, subSetMovedUV[subSetIndex], originUV);
        //                     ForTrans(targetRT, sTexture, transData, atlasSetting.GetTexScalePadding * 0.5f, null, true);

        //                     triangles.Dispose();
        //                 }
        //             }

        //         }
        //         Profiler.EndSample();

        //         if (atlasSetting.AtlasTextureSize != atlasTextureHeightSize)
        //         {
        //             var heightClampRt = TTRt.G(atlasSetting.AtlasTextureSize, atlasTextureHeightSize);
        //             heightClampRt.name = $"{targetRT.name}-heightClamp-TempRt-{heightClampRt.width}x{heightClampRt.height}";
        //             Graphics.CopyTexture(targetRT, 0, 0, 0, 0, heightClampRt.width, heightClampRt.height, heightClampRt, 0, 0, 0, 0);
        //             TTRt.R(targetRT);
        //             targetRT = heightClampRt;
        //         }

        //         var containsNormalMap = atlasContext.MaterialGroupToAtlasShaderTexDict.SelectMany(i => i.Value).Where(i => i.Key == propName).Any(i => i.Value.IsNormalMap);
        //         MipMapUtility.GenerateMips(targetRT, atlasSetting.DownScalingAlgorithm, containsNormalMap);

        //         Profiler.BeginSample("Readback");
        //         compiledAtlasTextures.Add(propName, new AsyncTexture2D(targetRT));
        //         Profiler.EndSample();

        //         TTRt.R(targetRT);
        //     }
        //     Profiler.EndSample();

        //     if (atlasSetting.AutoReferenceCopySetting && propertyBakeSetting == PropertyBakeSetting.NotBake)
        //     {
        //         Profiler.BeginSample("AutoReferenceCopySetting");
        //         var prop = containsProperty.ToArray();
        //         var refCopyDict = new Dictionary<string, string>();

        //         var gtHash = containsProperty.ToDictionary(p => p, p => groupedTextures.Where(i => i.Value.ContainsKey(p)).Select(i => i.Value[p]).ToHashSet());

        //         for (var i = 0; prop.Length > i; i += 1)
        //             for (var i2 = 0; prop.Length > i2; i2 += 1)
        //             {
        //                 if (i == i2) { continue; }

        //                 var copySource = prop[i];
        //                 var copyTarget = prop[i2];

        //                 var sTexHash = gtHash[copySource];
        //                 var tTexHash = gtHash[copyTarget];

        //                 if (sTexHash.SetEquals(tTexHash)) { refCopyDict.Remove(copySource); }

        //                 if (sTexHash.IsSupersetOf(tTexHash) is false) { continue; }
        //                 refCopyDict[copyTarget] = copySource;

        //                 // if (refCopyDict.ContainsKey(copySource) is false) { refCopyDict[copySource] = new(); }
        //                 // refCopyDict[copySource].Add(copyTarget);
        //             }


        //         var isContinue = true;
        //         var values = refCopyDict.Values.ToArray();
        //         while (isContinue)
        //         {
        //             foreach (var s in values)
        //             {
        //                 if (refCopyDict.ContainsKey(s))
        //                 {
        //                     foreach (var t in refCopyDict.Where(i => i.Value == s).ToArray())
        //                     {
        //                         refCopyDict[t.Key] = refCopyDict[s];
        //                     }
        //                     isContinue = true;
        //                 }
        //             }
        //             isContinue = false;
        //         }

        //         atlasData.ReferenceCopyDict = refCopyDict.GroupBy(i => i.Value).ToDictionary(i => i.Key, i => i.Select(k => k.Key).ToList());
        //         Profiler.EndSample();
        //     }
        //     if (atlasSetting.AutoMergeTextureSetting)
        //     {
        //         Profiler.BeginSample("AutoMergeTextureSetting");

        //         var prop = containsProperty.ToArray();
        //         var prop2MatIDs = prop.ToDictionary(p => p, p => groupedTextures.Where(gt => gt.Value.ContainsKey(p)).Select(gt => gt.Key).ToHashSet());

        //         var alreadySetting = new HashSet<string>();
        //         var margeSettingDict = new Dictionary<string, List<string>>();



        //         for (var i = 0; prop.Length > i; i += 1)
        //         {
        //             var mergeParent = prop[i];
        //             var mergedHash = prop2MatIDs[mergeParent].ToHashSet();
        //             var childList = new List<string>();

        //             for (var i2 = i; prop.Length > i2; i2 += 1)
        //             {
        //                 if (i == i2) { continue; }

        //                 var mergeChild = prop[i2];

        //                 if (alreadySetting.Contains(mergeChild)) { continue; }

        //                 var childHash = prop2MatIDs[mergeChild];

        //                 if (mergedHash.Overlaps(childHash)) { continue; }

        //                 childList.Add(mergeChild);
        //                 mergedHash.UnionWith(childHash);
        //                 alreadySetting.Add(mergeChild);
        //             }

        //             if (childList.Any()) { margeSettingDict[mergeParent] = childList; }
        //         }

        //         atlasData.MargeTextureDict = margeSettingDict;
        //         Profiler.EndSample();
        //     }

        //     Profiler.BeginSample("ReleaseGroupeTextures");
        //     foreach (var kv in groupedTextures.Values) { foreach (var tex in kv) { TTRt.R(tex.Value); } }
        //     groupedTextures = null;
        //     Profiler.EndSample();

        //     Profiler.BeginSample("TextureMaxSize");
        //     var texMaxDict = new Dictionary<string, int>();
        //     foreach (var atlasTexKV in atlasContext.MaterialToAtlasShaderTexDict.SelectMany(x => x.Value))
        //     {
        //         if (compiledAtlasTextures.ContainsKey(atlasTexKV.Key) is false) { continue; }
        //         if (texMaxDict.ContainsKey(atlasTexKV.Key) is false) { texMaxDict[atlasTexKV.Key] = 2; }
        //         if (atlasTexKV.Value.Texture == null) { continue; }
        //         texMaxDict[atlasTexKV.Key] = math.max(texMaxDict[atlasTexKV.Key], atlasTexKV.Value.Texture.width);
        //     }
        //     atlasData.SourceTextureMaxSize = texMaxDict;
        //     Profiler.EndSample();

        //     Profiler.BeginSample("Async Readback");
        //     atlasData.Textures = compiledAtlasTextures.ToDictionary(kv => kv.Key, kv => kv.Value.GetTexture2D());
        //     Profiler.EndSample();

        //     atlasData.MaterialID = atlasContext.MaterialGroup.Select(i => i.ToHashSet()).ToArray();
        //     atlasContext.Dispose();
        //     foreach (var movedUV in subSetMovedUV) { movedUV.Dispose(); }

        //     atlasData.BakePropMaxValue = bakePropMaxValue;

        //     return true;

        // }
        // static T NormalizeIsland<T>(int width, int height, T island) where T : IIslandRect
        // {
        //     var minPos = island.Pivot;
        //     var maxPos = island.Pivot + island.Size;
        //     island.Pivot = Normalize(width, height, minPos);
        //     island.Size = Normalize(width, height, maxPos) - island.Pivot;
        //     return island;
        // }
        // static Vector2 Normalize(int width, int height, Vector2 vector2)
        // {
        //     vector2.y = Mathf.Round(vector2.y * height) / height;
        //     vector2.x = Mathf.Round(vector2.x * width) / width;
        //     return vector2;
        // }
        // private static Dictionary<int, Dictionary<string, RenderTexture>> GetGroupedTextures(ITextureManager texManage, AtlasSetting atlasSetting, AtlasContext atlasContext, PropertyBakeSetting propertyBakeSetting, out HashSet<string> property, out Dictionary<string, float> bakePropMaxValue)
        // {
        //     foreach (var textures in atlasContext.MaterialGroupToAtlasShaderTexDict.Values)
        //     {
        //         foreach (var atlasShaderTexture in textures.Values)
        //         {
        //             if (atlasShaderTexture.Texture is Texture2D tex) texManage.PreloadOriginalTexture(tex);
        //         }
        //     }

        //     bakePropMaxValue = null;
        //     var downScalingAlgorithm = atlasSetting.DownScalingAlgorithm;
        //     switch (propertyBakeSetting)
        //     {
        //         default: { property = null; return null; }
        //         case PropertyBakeSetting.NotBake:
        //             {
        //                 var dict = atlasContext.MaterialGroupToAtlasShaderTexDict.ToDictionary(i => i.Key, i => ZipDictAndOffset(i.Value));
        //                 property = new HashSet<string>(dict.SelectMany(i => i.Value.Keys));
        //                 return dict;

        //                 Dictionary<string, RenderTexture> ZipDictAndOffset(Dictionary<string, AtlasShaderTexture> keyValuePairs)
        //                 {
        //                     var dict = new Dictionary<string, RenderTexture>();
        //                     var rtDict = new Dictionary<(Texture sTex, Vector2 tScale, Vector2 tTiling), RenderTexture>();
        //                     foreach (var kv in keyValuePairs)
        //                     {
        //                         if (kv.Value.Texture == null) { continue; }
        //                         var atlasTex = kv.Value;

        //                         var texHash = (atlasTex.Texture, atlasTex.TextureScale, atlasTex.TextureTranslation);
        //                         if (rtDict.ContainsKey(texHash)) { dict[kv.Key] = rtDict[texHash]; continue; }

        //                         var rt = GetOriginAtUseMip(texManage, atlasTex.Texture);

        //                         if (atlasTex.TextureScale != Vector2.one || atlasTex.TextureTranslation != Vector2.zero)
        //                         { rt.ApplyTextureST(atlasTex.TextureScale, atlasTex.TextureTranslation); }

        //                         MipMapUtility.GenerateMips(rt, downScalingAlgorithm);

        //                         rtDict[texHash] = dict[kv.Key] = rt;
        //                     }
        //                     return dict;


        //                 }        //     //情報を集めるフェーズ
        //     var targetMaterialsHash = targetMaterials.ToHashSet();
        //     var sizePriorityDict = targetMaterials.ToDictionary(i => i, i => SelectMatList.First(m => domain.OriginEqual(m.Material, i)).MaterialFineTuningValue);

        //     atlasData.AtlasInMaterials = targetMaterialsHash;
        //     if (atlasData.AtlasInMaterials.Any() is false) { return false; }

        //     var atlasSetting = AtlasSetting;
        //     var propertyBakeSetting = atlasSetting.MergeMaterials ? atlasSetting.PropertyBakeSetting : PropertyBakeSetting.NotBake;
        //     Profiler.EndSample();

        //     Profiler.BeginSample("AtlasContext:ctor");
        //     var targeting = domain as IRendererTargeting;
        //     var targetRenderers = nowTargetAllowedRenderer
        //         .Where(r => targeting.GetMesh(r) != null)
        //         .Where(r => targeting.GetMaterials(r)
        //             .Where(i => i != null)
        //             .Cast<Material>()
        //             .Any(targetMaterials.Contains)
        //         ).ToArray();


        //     var atlasContext = new AtlasContext(targeting, targetRenderers, targetMaterialsHash, UVChannel.UV0);
        //     Profiler.EndSample();

        //                 property = new HashSet<string>(atlasContext.MaterialToAtlasShaderTexDict
        //                         .SelectMany(i => i.Value)
        //                         .GroupBy(i => i.Key)
        //                         .Where(i => PropertyBakeSetting.BakeAllProperty == propertyBakeSetting || i.Any(st => st.Value.Texture != null))
        //                         .Select(i => i.Key)
        //                     );

        //                 var groupDict = new Dictionary<int, Dictionary<string, RenderTexture>>(atlasContext.MaterialGroup.Length);
        //                 var tmpMat = new Material(Shader.Find("Unlit/Texture"));

        //                 bakePropMaxValue = atlasContext.MaterialToAtlasShaderTexDict.Values.SelectMany(kv => kv)
        //                     .SelectMany(i => i.Value.BakeProperties)
        //                     .GroupBy(i => i.PropertyName)
        //                     .Where(i => i.First() is BakeFloat || i.First() is BakeRange)
        //                     .ToDictionary(i => i.Key, i => i.Max(p =>
        //                         {
        //                             if (p is BakeFloat bakeFloat) { return bakeFloat.Float; }
        //                             if (p is BakeRange bakeRange) { return bakeRange.Float; }
        //                             return 0;
        //                         }
        //                     )
        //                 );//一旦 Float として扱えるものだけの実装にしておく。Color がほしいシェーダーを作ってたりしたら issues たてて これをみたひと


        //                 for (var gi = 0; atlasContext.MaterialGroup.Length > gi; gi += 1)
        //                 {
        //                     var matGroup = atlasContext.MaterialGroup[gi];
        //                     var groupMat = matGroup.First();

        //                     var atlasTexDict = atlasContext.MaterialToAtlasShaderTexDict[groupMat];//テクスチャに関する情報が完全に同じでないと同じグループにならない。だから適当なものでよい。
        //                     var shaderSupport = atlasContext.AtlasShaderSupporters[groupMat];

        //                     tmpMat.shader = shaderSupport.BakeShader;

        //                     var texDict = groupDict[gi] = new();

        //                     foreach (var propName in property)
        //                     {
        //                         atlasTexDict.TryGetValue(propName, out var atlasTex);
        //                         var sTex = atlasTex?.Texture != null ? GetOriginAtUseMip(texManage, atlasTex.Texture) : null;

        //                         if (sTex != null && (atlasTex.TextureScale != Vector2.one || atlasTex.TextureTranslation != Vector2.zero)) { sTex.ApplyTextureST(atlasTex.TextureScale, atlasTex.TextureTranslation); }

        //                         if (shaderSupport.BakeShader == null)
        //                         {
        //                             if (sTex != null)
        //                             {
        //                                 MipMapUtility.GenerateMips(sTex, downScalingAlgorithm);
        //                                 texDict[propName] = sTex;
        //                             }
        //                             else
        //                             {
        //                                 var rt = texDict[propName] = TTRt.G(2);
        //                                 rt.name = $"AtlasTexColRT-2x2";
        //                             }
        //                             continue;
        //                         }

        //                         var bakedTex = sTex != null ? sTex.CloneTemp() : TTRt.G(2, "AtlasEmptyDefaultTexColRT-2x2");

        //                         if (atlasTex != null)
        //                         {
        //                             var bakePropertyDescriptions = shaderSupport.GetBakePropertyNames(propName);
        //                             foreach (var bakeProp in atlasTex.BakeProperties)
        //                             {
        //                                 bakeProp.WriteMaterial(tmpMat);

        //                                 var bakePropName = bakeProp.PropertyName;
        //                                 if (bakePropertyDescriptions.First(i => i.PropertyName == bakePropName).UseMaxValue)
        //                                 {
        //                                     var maxValPropName = bakePropName + "_MaxValue";
        //                                     if (tmpMat.HasProperty(maxValPropName) && bakePropMaxValue.TryGetValue(bakePropName, out var bakeMaxValue))
        //                                     {
        //                                         tmpMat.SetFloat(maxValPropName, bakeMaxValue);
        //                                     }
        //                                 }
        //                             }
        //                         }

        //                         tmpMat.EnableKeyword("Bake" + propName);
        //                         if (atlasTex == null) { tmpMat.EnableKeyword("Constraint_Invalid"); }

        //                         tmpMat.SetTexture(propName, sTex);
        //                         Graphics.Blit(sTex, bakedTex, tmpMat);
        //                         MipMapUtility.GenerateMips(bakedTex, downScalingAlgorithm);

        //                         texDict[propName] = bakedTex;

        //                         if (sTex != null) { TTRt.R(sTex); }
        //                         tmpMat.AllPropertyReset();
        //                         tmpMat.shaderKeywords = Array.Empty<string>();

        //                     }


        //                 }

        //                 return groupDict;
        //             }
        //     }
        // }

        // private static RenderTexture GetOriginAtUseMip(ITextureManager texManage, Texture atlasTex)
        // {
        //     Profiler.BeginSample("GetOriginAtUseMip");
        //     try
        //     {
        //         switch (atlasTex)
        //         {
        //             default:
        //                 {
        //                     var originSize = atlasTex.width;
        //                     Profiler.BeginSample("TTTRt.G");
        //                     var rt = TTRt.G(originSize, originSize, true, false, true, true);
        //                     Profiler.EndSample();
        //                     rt.name = $"{atlasTex.name}:GetOriginAtUseMip-TempRt-{rt.width}x{rt.height}";
        //                     rt.CopyFilWrap(atlasTex);
        //                     rt.filterMode = FilterMode.Trilinear;
        //                     Profiler.BeginSample("Graphics.Blit");
        //                     Graphics.Blit(atlasTex, rt);
        //                     Profiler.EndSample();
        //                     return rt;
        //                 }
        //             case Texture2D atlasTex2D:
        //                 {
        //                     Profiler.BeginSample("GetOriginalTextureSize");
        //                     var originSize = texManage.GetOriginalTextureSize(atlasTex2D);
        //                     Profiler.EndSample();

        //                     Profiler.BeginSample("TTTRt.G");
        //                     var rt = TTRt.G(originSize, originSize, true, false, true, true);
        //                     Profiler.EndSample();
        //                     rt.name = $"{atlasTex.name}:GetOriginAtUseMip-TempRt-{rt.width}x{rt.height}";
        //                     rt.CopyFilWrap(atlasTex);
        //                     rt.filterMode = FilterMode.Trilinear;
        //                     Profiler.BeginSample("Graphics.Blit");
        //                     texManage.WriteOriginalTexture(atlasTex2D, rt);
        //                     Profiler.EndSample();
        //                     return rt;
        //                 }

        //         }
        //     }
        //     finally
        //     {

        //         Profiler.EndSample();
        //     }
        // }

        internal override void Apply(IDomain domain)
        {
            domain.LookAt(this);

            if (SelectMatList.Any() is false) { TTTRuntimeLog.Info("AtlasTexture:info:TargetNotSet"); return; }

            var nowRenderers = GetTargetAllowedFilter(domain.EnumerateRenderer());
            var targetMaterials = GetTargetMaterials(domain, nowRenderers).ToHashSet();

            if (targetMaterials.Any() is false) { TTTRuntimeLog.Info("AtlasTexture:info:TargetNotFound"); return; }

            foreach (var mmg in MergeMaterialGroups) { if (mmg.Reference != null) { domain.LookAt(mmg.Reference); } }
            if (AllMaterialMergeReference != null) { domain.LookAt(AllMaterialMergeReference); }


            var sizePriorityDict = targetMaterials
                .ToDictionary(
                    i => i,
                    i => TTMath.Saturate(
                            SelectMatList.First(
                                m => domain.OriginEqual(m.Material, i)
                            ).MaterialFineTuningValue
                        )
                );


            var atlasSetting = AtlasSetting;

            var atlasTargeSize = GetAtlasTextureSize(atlasSetting);


            var targeting = domain as IRendererTargeting;
            var targetRenderers = nowRenderers
                .Where(r => targeting.GetMesh(r) != null)
                .Where(r => targeting.GetMaterials(r)
                    .Where(i => i != null)
                    .Cast<Material>()
                    .Any(targetMaterials.Contains)
                ).ToArray();


            using var atlasContext = new AtlasContext(
                targeting
                , targetRenderers
                , targetMaterials
                , atlasSetting.UsePrimaryMaximumTexture ? null : atlasSetting.PrimaryTextureProperty
                , atlasSetting.AtlasTargetUVChannel
                );



            //アイランドまわり
            if (atlasSetting.PixelNormalize)
            {
                atlasContext.SourceVirtualIslandNormalize();
            }


            var (movedVirtualIslandArray, relocateResult) = IslandProcessing(domain, atlasSetting, atlasContext, sizePriorityDict, out var relocationTime);
            if (relocateResult.IslandRelocationResult is null) { throw new Exception(); }
            if (relocateResult.IslandRelocationResult.IsSuccess is false) { throw new Exception(); }
            var source2MovedVirtualIsland = new Dictionary<IslandTransform, IslandTransform>();
            for (var i = 0; movedVirtualIslandArray.Length > i; i += 1)
            {
                source2MovedVirtualIsland[atlasContext.SourceVirtualIslands[i]] = movedVirtualIslandArray[i];
            }


            //上側を削れるかを見る
            var height = IslandRelocationManager.CalculateIslandsMaxHeight(movedVirtualIslandArray) + atlasSetting.IslandPadding;
            var atlasTextureHeightSize = Mathf.Max(GetNormalizedMinHeightSize(atlasTargeSize.y, height), 4);//4以下はちょっと怪しい挙動しそうだからクランプ
            Debug.Assert(Mathf.IsPowerOfTwo(atlasTextureHeightSize));

            TTLog.Info("AtlasTexture:info:RelocateResult", 1 - height, relocateResult.PriorityDownScale, relocateResult.OverallDownScale, relocateResult.TotalRelocateCount, relocationTime);
            var atlasedTextureSize = new Vector2Int(atlasTargeSize.x, atlasTextureHeightSize);

            //新しいUVを持つMeshを生成するフェーズ

            // SubSetIndex と対応する
            var atlasedMeshes = atlasContext.GenerateAtlasedMesh(atlasSetting, source2MovedVirtualIsland, atlasedTextureSize);


            //アトラス化したテクスチャーを生成するフェーズ
            var compiledAtlasTextures = new Dictionary<string, ITTRenderTexture>();

            // Profiler.BeginSample("GetGroupedTextures");
            var engine = domain.GetTexTransCoreEngineForUnity();
            using var groupedTextures = atlasContext.GetGroupedDiskOrRenderTextures(engine);
            var containsProperty = atlasContext.MaterialGroupingCtx.GetContainsAllProperties();

            // Profiler.EndSample();
            var loadedDiskTextures = new Dictionary<ITTDiskTexture, ITTRenderTexture>();

            // Profiler.BeginSample("Texture synthesis");
            foreach (var propName in containsProperty)
            {
                var targetRT = engine.CreateRenderTexture(atlasedTextureSize.x, atlasedTextureSize.y);
                engine.ColorFill(targetRT, atlasSetting.BackGroundColor.ToTTCore());
                // TextureUtility.FillColor(targetRT, atlasSetting.BackGroundColor);
                targetRT.Name = "AtlasTex" + propName;
                // Profiler.BeginSample("Draw:" + targetRT.name);
                foreach (var group in groupedTextures.GroupedTextures)
                {
                    if (!group.Value.TryGetValue(propName, out var sourceTexture)) { continue; }
                    var sourceRenderTexture = sourceTexture switch
                    {
                        ITTRenderTexture rt => rt,
                        ITTDiskTexture dt => loadedDiskTextures.ContainsKey(dt) ? loadedDiskTextures[dt] : LoadFullScale(dt),
                        _ => throw new InvalidCastException(),
                    };
                    ITTRenderTexture LoadFullScale(ITTDiskTexture diskTexture)
                    {
                        var loaded = engine.LoadTextureWidthFullScale(diskTexture);
                        loadedDiskTextures[diskTexture] = loaded;
                        return loaded;
                    }

                    var findMaterialID = group.Key;
                    if (relocateResult.IslandRelocationResult.IsRectangleMove)
                    {
                        var findSubIDHash = atlasContext.AtlasSubMeshIndexSetCtx.AtlasSubMeshIndexIDHash
                                                .Where(i => i.MaterialGroupID == findMaterialID).ToHashSet();

                        var drawTargetSourceVirtualIslandsHash = new HashSet<IslandTransform>();
                        foreach (var subID in findSubIDHash)
                        {
                            drawTargetSourceVirtualIslandsHash.UnionWith(
                                atlasContext.AtlasIslandCtx.OriginIslandDict[subID]
                                    .Select(i => atlasContext.AtlasIslandCtx.Origin2VirtualIsland[i])
                                );
                        }
                        var drawTargetSourceVirtualIslands = drawTargetSourceVirtualIslandsHash.ToArray();
                        var drawTargetMovedVirtualIslands = drawTargetSourceVirtualIslands.Select(i => source2MovedVirtualIsland[i]).ToArray();


                        TransMoveRectangle(
                            engine
                            , targetRT
                            , sourceRenderTexture
                            , drawTargetSourceVirtualIslands
                            , drawTargetMovedVirtualIslands
                            , atlasSetting.IslandPadding
                        );
                    }
                    else
                    {
                        throw new NotImplementedException();
                        // for (var subSetIndex = 0; atlasContext.AtlasSubSets.Count > subSetIndex; subSetIndex += 1)
                        // {
                        //     var transTargets = atlasContext.AtlasSubSets[subSetIndex].Where(i => i.HasValue).Where(i => i.Value.MaterialGroupID == findMaterialID).Select(i => i.Value);
                        //     if (!transTargets.Any()) { continue; }

                        //     var triangles = new NativeArray<TriangleIndex>(transTargets.SelectMany(subData => atlasContext.IslandDict[subData].SelectMany(i => i.triangles)).ToArray(), Allocator.TempJob);
                        //     var originUV = atlasContext.MeshDataDict[atlasContext.NormalizeMeshes[atlasContext.Meshes[transTargets.First().MeshID]]].VertexUV;

                        //     var transData = new TransData(triangles, subSetMovedUV[subSetIndex], originUV);
                        //     ForTrans(targetRT, sTexture, transData, atlasSetting.GetTexScalePadding * 0.5f, null, true);

                        //     triangles.Dispose();
                        // }preMesh
                    }

                }
                // Profiler.EndSample();

                // if (atlasSetting.AtlasTextureSize != atlasTextureHeightSize)
                // {
                //     var heightClampRt = TTRt.G(atlasSetting.AtlasTextureSize, atlasTextureHeightSize);
                //     heightClampRt.name = $"{targetRT.name}-heightClamp-TempRt-{heightClampRt.width}x{heightClampRt.height}";
                //     Graphics.CopyTexture(targetRT, 0, 0, 0, 0, heightClampRt.width, heightClampRt.height, heightClampRt, 0, 0, 0, 0);
                //     TTRt.R(targetRT);
                //     targetRT = heightClampRt;
                // }

                // var containsNormalMap = atlasContext.MaterialGroupToAtlasShaderTexDict.SelectMany(i => i.Value).Where(i => i.Key == propName).Any(i => i.Value.IsNormalMap);
                // MipMapUtility.GenerateMips(targetRT, atlasSetting.DownScalingAlgorithm, containsNormalMap);

                // Profiler.BeginSample("Readback");
                compiledAtlasTextures.Add(propName, targetRT);
                // Profiler.EndSample();

                // TTRt.R(targetRT);
            }

            // Profiler.EndSample();

            //     if (atlasSetting.AutoReferenceCopySetting && propertyBakeSetting == PropertyBakeSetting.NotBake)
            //     {
            //         Profiler.BeginSample("AutoReferenceCopySetting");
            //         var prop = containsProperty.ToArray();
            //         var refCopyDict = new Dictionary<string, string>();

            //         var gtHash = containsProperty.ToDictionary(p => p, p => groupedTextures.Where(i => i.Value.ContainsKey(p)).Select(i => i.Value[p]).ToHashSet());

            //         for (var i = 0; prop.Length > i; i += 1)
            //             for (var i2 = 0; prop.Length > i2; i2 += 1)
            //             {preMesh
            //                 if (i == i2) { continue; }

            //                 var copySource = prop[i];
            //                 var copyTarget = prop[i2];

            //                 var sTexHash = gtHash[copySource];
            //                 var tTexHash = gtHash[copyTarget];

            //                 if (sTexHash.SetEquals(tTexHash)) { refCopyDict.Remove(copySource); }

            //                 if (sTexHash.IsSupersetOf(tTexHash) is false) { continue; }
            //                 refCopyDict[copyTarget] = copySource;

            //                 // if (refCopyDict.ContainsKey(copySource) is false) { refCopyDict[copySource] = new(); }
            //                 // refCopyDict[copySource].Add(copyTarget);
            //             }


            //         var isContinue = true;
            //         var values = refCopyDict.Values.ToArray();
            //         while (isContinue)
            //         {
            //             foreach (var s in values)
            //             {
            //                 if (refCopyDict.ContainsKey(s))
            //                 {
            //                     foreach (var t in refCopyDict.Where(i => i.Value == s).ToArray())
            //                     {
            //                         refCopyDict[t.Key] = refCopyDict[s];
            //                     }
            //                     isContinue = true;
            //                 }
            //             }
            //             isContinue = false;
            //         }

            //         atlasData.ReferenceCopyDict = refCopyDict.GroupBy(i => i.Value).ToDictionary(i => i.Key, i => i.Select(k => k.Key).ToList());
            //         Profiler.EndSample();
            //     }
            //     if (atlasSetting.AutoMergeTextureSetting)
            //     {
            //         Profiler.BeginSample("AutoMergeTextureSetting");

            //         var prop = containsProperty.ToArray();
            //         var prop2MatIDs = prop.ToDictionary(p => p, p => groupedTextures.Where(gt => gt.Value.ContainsKey(p)).Select(gt => gt.Key).ToHashSet());

            //         var alreadySetting = new HashSet<string>();
            //         var margeSettingDict = new Dictionary<string, List<string>>();



            //         for (var i = 0; prop.Length > i; i += 1)
            //         {
            //             var mergeParent = prop[i];
            //             var mergedHash = prop2MatIDs[mergeParent].ToHashSet();
            //             var childList = new List<string>();

            //             for (var i2 = i; prop.Length > i2; i2 += 1)
            //             {
            //                 if (i == i2) { continue; }

            //                 var mergeChild = prop[i2];

            //                 if (alreadySetting.Contains(mergeChild)) { continue; }

            //                 var childHash = prop2MatIDs[mergeChild];

            //                 if (mergedHash.Overlaps(childHash)) { continue; }

            //                 childList.Add(mergeChild);
            //                 mergedHash.UnionWith(childHash);
            //                 alreadySetting.Add(mergeChild);
            //             }

            //             if (childList.Any()) { margeSettingDict[mergeParent] = childList; }
            //         }

            //         atlasData.MargeTextureDict = margeSettingDict;
            //         Profiler.EndSample();
            //     }

            //     Profiler.BeginSample("ReleaseGroupeTextures");
            //     foreach (var kv in groupedTextures.Values) { foreach (var tex in kv) { TTRt.R(tex.Value); } }
            //     groupedTextures = null;
            //     Profiler.EndSample();

            //     Profiler.BeginSample("TextureMaxSize");
            //     var texMaxDict = new Dictionary<string, int>();
            //     foreach (var atlasTexKV in atlasContext.MaterialToAtlasShaderTexDict.SelectMany(x => x.Value))
            //     {
            //         if (compiledAtlasTextures.ContainsKey(atlasTexKV.Key) is false) { continue; }
            //         if (texMaxDict.ContainsKey(atlasTexKV.Key) is false) { texMaxDict[atlasTexKV.Key] = 2; }
            //         if (atlasTexKV.Value.Texture == null) { continue; }
            //         texMaxDict[atlasTexKV.Key] = math.max(texMaxDict[atlasTexKV.Key], atlasTexKV.Value.Texture.width);
            //     }
            //     atlasData.SourceTextureMaxSize = texMaxDict;
            //     Profiler.EndSample();

            //     Profiler.BeginSample("Async Readback");
            //     atlasData.Textures = compiledAtlasTextures.ToDictionary(kv => kv.Key, kv => kv.Value.GetTexture2D());
            //     Profiler.EndSample();

            //     atlasData.MaterialID = atlasContext.MaterialGroup.Select(i => i.ToHashSet()).ToArray();
            //     atlasContext.Dispose();
            //     foreach (var movedUV in subSetMovedUV) { movedUV.Dispose(); }

            //     atlasData.BakePropMaxValue = bakePropMaxValue;

            //     return true;


            // Profiler.BeginSample("AtlasShaderSupportUtils:ctor");
            // var shaderSupport = new AtlasShaderSupportUtils();
            // Profiler.EndSample();

            //Mesh Change
            foreach (var renderer in targetRenderers)
            {
                var mesh = domain.GetMesh(renderer);
                if (mesh == null) { continue; }
                if (atlasContext.NormalizedMeshCtx.Origin2NormalizedMesh.ContainsKey(mesh) is false) { continue; }

                var meshID = atlasContext.NormalizedMeshCtx.Normalized2MeshID[atlasContext.NormalizedMeshCtx.Origin2NormalizedMesh[mesh]];
                var matIDs = domain.GetMaterials(renderer).Select(m => m != null ? atlasContext.MaterialGroupingCtx.GetMaterialGroupID(m) : -1).ToArray();

                var subSet = new AtlasSubMeshIndexID?[matIDs.Length];
                for (var i = 0; subSet.Length > i; i += 1)
                {
                    var matID = matIDs[i];
                    if (matID is -1) { subSet[i] = null; }
                    else { subSet[i] = new AtlasSubMeshIndexID(meshID, i, matID); }
                }
                // var atlasMeshHolder = atlasData.Meshes.FindAll(I => I.DistMesh == mesh).Find(I => I.MatIDs.SequenceEqual(matIDs));
                // if (atlasMeshHolder.AtlasMesh == null) { continue; }


                var identicalSubSetID = GetIdenticalSubSet(atlasContext.AtlasSubMeshIndexSetCtx.AtlasSubSets, subSet);

                int GetIdenticalSubSet(List<AtlasSubMeshIndexID?[]> atlasSubSetAll, AtlasSubMeshIndexID?[] findSource)
                {
                    return atlasSubSetAll.FindIndex(subSet =>
                    {

                        if (subSet.Length == findSource.Length && subSet.SequenceEqual(findSource)) { return true; }
                        if (AtlasSubMeshIndexIDSetContext.SubPartEqual(subSet, findSource) is false) { return false; }
                        return subSet.Length < findSource.Length is false;
                    });
                }
                if (identicalSubSetID is -1) { continue; }


                var atlasMesh = atlasedMeshes[identicalSubSetID];
                domain.SetMesh(renderer, atlasMesh);
            }
            domain.TransferAssets(atlasedMeshes);

            // //Texture Fine Tuning
            // var atlasTexFineTuningTargets = TexFineTuningUtility.InitTexFineTuning(atlasData.Textures);
            // SetSizeDataMaxSize(atlasTexFineTuningTargets, atlasData.SourceTextureMaxSize);
            // DefaultMargeTextureDictTuning(atlasTexFineTuningTargets, atlasData.MargeTextureDict);
            // DefaultRefCopyTuning(atlasTexFineTuningTargets, atlasData.ReferenceCopyDict);
            // foreach (var fineTuning in AtlasSetting.TextureFineTuning)
            // {
            //     fineTuning?.AddSetting(atlasTexFineTuningTargets);
            // }
            // var individualApplied = new HashSet<string>();
            // foreach (var individualTuning in AtlasSetting.TextureIndividualFineTuning)
            // {
            //     if (atlasTexFineTuningTargets.ContainsKey(individualTuning.TuningTarget) is false) { continue; }
            //     if (individualApplied.Contains(individualTuning.TuningTarget)) { continue; }
            //     individualApplied.Add(individualTuning.TuningTarget);

            //     var tuningTarget = atlasTexFineTuningTargets[individualTuning.TuningTarget];

            //     if (individualTuning.OverrideReferenceCopy) { tuningTarget.Get<ReferenceCopyData>().CopySource = individualTuning.CopyReferenceSource; }
            //     if (individualTuning.OverrideResize) { tuningTarget.Get<SizeData>().TextureSize = individualTuning.TextureSize; }
            //     if (individualTuning.OverrideCompression) { tuningTarget.Set(individualTuning.CompressionData); }
            //     if (individualTuning.OverrideMipMapRemove) { tuningTarget.Get<MipMapData>().UseMipMap = individualTuning.UseMipMap; }
            //     if (individualTuning.OverrideColorSpace) { tuningTarget.Get<ColorSpaceData>().Linear = individualTuning.Linear; }
            //     if (individualTuning.OverrideRemove) { tuningTarget.Get<RemoveData>().IsRemove = individualTuning.IsRemove; }
            //     if (individualTuning.OverrideMargeTexture) { tuningTarget.Get<MergeTextureData>().MargeParent = individualTuning.MargeRootProperty; }
            // }
            // TexFineTuningUtility.FinalizeTexFineTuning(atlasTexFineTuningTargets, domain.GetTextureManager());
            // var atlasTexture = atlasTexFineTuningTargets.ToDictionary(i => i.Key, i => i.Value.Texture2D);
            // domain.transferAssets(atlasTexture.Select(PaT => PaT.Value));
            var tex = compiledAtlasTextures.ToDictionary(i => i.Key, i => engine.DownloadToTexture2D(i.Value, true));
            foreach (var rt in compiledAtlasTextures.Values) { rt.Dispose(); }
            foreach (var t in tex.Values) { t.Apply(true); }

            //MaterialGenerate And Change
            var atlasMatOption = new AtlasMatGenerateOption() { ForceSetTexture = atlasSetting.ForceSetTexture };
            if (atlasSetting.UnsetTextures.Any())
            {
                var containsAllTexture = targetMaterials.SelectMany(mat => mat.GetAllTextureWithDictionary().Select(i => i.Value));
                atlasMatOption.UnsetTextures = atlasSetting.UnsetTextures.Select(i => i.GetTexture()).SelectMany(ot => containsAllTexture.Where(ct => domain.OriginEqual(ot, ct))).ToHashSet();
            }
            // if (AtlasSetting.MergeMaterials)
            // {
            //     if (AtlasSetting.PropertyBakeSetting != PropertyBakeSetting.NotBake) { atlasMatOption.BakedPropertyReset = AtlasSetting.BakedPropertyWriteMaxValue; }
            //     var mergeMat = AtlasSetting.MergeReferenceMaterial != null ? AtlasSetting.MergeReferenceMaterial : atlasData.AtlasInMaterials.First();
            //     Material generateMat = GenerateAtlasMat(mergeMat, atlasTexture, shaderSupport, atlasData.BakePropMaxValue, atlasMatOption);
            //     var matGroupGenerate = AtlasSetting.MaterialMergeGroups.Where(m => m.GroupMaterials.Any()).ToDictionary(m => m, m => GenerateAtlasMat(m.MergeReferenceMaterial != null ? m.MergeReferenceMaterial : m.GroupMaterials.First(), atlasTexture, shaderSupport, atlasData.BakePropMaxValue, atlasMatOption));

            //     domain.ReplaceMaterials(atlasData.AtlasInMaterials.ToDictionary(x => x, m => FindGroup(m)), false);

            //     Material FindGroup(Material material)
            //     {
            //         foreach (var matGroup in AtlasSetting.MaterialMergeGroups)
            //         {
            //             var index = matGroup.GroupMaterials.FindIndex(m => domain.OriginEqual(m, material));
            //             if (index != -1) { return matGroupGenerate[matGroup]; }
            //         }
            //         return generateMat;
            //     }
            // }
            // else
            {
                var materialMap = new Dictionary<Material, Material>();
                foreach (var distMat in targetMaterials)
                {
                    var generateMat = GenerateAtlasMat(distMat, tex, atlasMatOption);
                    materialMap.Add(distMat, generateMat);
                }
                domain.ReplaceMaterials(materialMap);
            }

            // foreach (var atlasMeshHolder in atlasData.Meshes) { UnityEngine.Object.DestroyImmediate(atlasMeshHolder.NormalizedMesh); }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct InputRect
        {
            public float SourcePositionX;
            public float SourcePositionY;
            public float SourceSizeX;
            public float SourceSizeY;

            public float SourceRotation;
            public float AlimentPadding11;
            public float AlimentPadding12;
            public float AlimentPadding13;


            public float TargetPositionX;
            public float TargetPositionY;

            public float TargetSizeX;
            public float TargetSizeY;

            public float TargetRotation;
            public float AlimentPadding21;
            public float AlimentPadding22;
            public float AlimentPadding23;
        }
        private static void TransMoveRectangle(
            ITexTransToolForUnity engine
            , ITTRenderTexture targetRT
            , ITTRenderTexture sourceTexture
            , IslandTransform[] drawTargetSourceVirtualIslands
            , IslandTransform[] drawTargetMovedVirtualIslands
            , float islandPadding
        )
        {

            var atlasHeightScale = targetRT.Hight / (float)targetRT.Width;
            var inputRecs = new InputRect[drawTargetSourceVirtualIslands.Length];
            for (var i = 0; inputRecs.Length > i; i += 1)
            {
                var sourceRect = drawTargetSourceVirtualIslands[i];
                var targetRect = drawTargetMovedVirtualIslands[i];

                inputRecs[i] = new InputRect
                {
                    SourcePositionX = sourceRect.Position.X,
                    SourcePositionY = sourceRect.Position.Y,
                    SourceSizeX = sourceRect.Size.X,
                    SourceSizeY = sourceRect.Size.Y,
                    SourceRotation = sourceRect.Rotation,

                    TargetPositionX = targetRect.Position.X,
                    TargetPositionY = targetRect.Position.Y,
                    TargetSizeX = targetRect.Size.X,
                    TargetSizeY = targetRect.Size.Y,
                    TargetRotation = targetRect.Rotation,
                };
            }
            Span<byte> mappingConstantBuffer = stackalloc byte[32];
            BitConverter.TryWriteBytes(mappingConstantBuffer.Slice(0, 4), (uint)targetRT.Width);
            BitConverter.TryWriteBytes(mappingConstantBuffer.Slice(4, 4), (uint)targetRT.Hight);
            BitConverter.TryWriteBytes(mappingConstantBuffer.Slice(8, 4), (uint)sourceTexture.Width);
            BitConverter.TryWriteBytes(mappingConstantBuffer.Slice(12, 4), (uint)sourceTexture.Hight);

            BitConverter.TryWriteBytes(mappingConstantBuffer.Slice(16, 4), islandPadding);
            BitConverter.TryWriteBytes(mappingConstantBuffer.Slice(20, 4), atlasHeightScale);
            BitConverter.TryWriteBytes(mappingConstantBuffer.Slice(24, 4), 0.0f);
            BitConverter.TryWriteBytes(mappingConstantBuffer.Slice(28, 4), 0.0f);


            using var inputRectBuffer = engine.UploadStorageBuffer<InputRect>(inputRecs);
            using var transMap = engine.CreateRenderTexture(targetRT.Width, targetRT.Hight, TexTransCoreTextureChannel.RG);
            using var scalingMap = engine.CreateRenderTexture(targetRT.Width, targetRT.Hight, TexTransCoreTextureChannel.R);
            using var isWriteMap = engine.CreateRenderTexture(targetRT.Width, targetRT.Hight, TexTransCoreTextureChannel.R);

            using (var mappingHandler = engine.GetComputeHandler(engine.GetExKeyQuery<IAtlasComputeKey>().RectangleTransMapping))
            {
                var gvBufID = mappingHandler.NameToID("gv");
                var transMapID = mappingHandler.NameToID("TransMap");
                var scalingMapID = mappingHandler.NameToID("ScalingMap");
                var writeMapID = mappingHandler.NameToID("WriteMap");
                var mappingRectBufID = mappingHandler.NameToID("MappingRect");

                mappingHandler.SetStorageBuffer(mappingRectBufID, inputRectBuffer);
                mappingHandler.UploadConstantsBuffer<byte>(gvBufID, mappingConstantBuffer);

                mappingHandler.SetTexture(transMapID, transMap);
                mappingHandler.SetTexture(scalingMapID, scalingMap);
                mappingHandler.SetTexture(writeMapID, isWriteMap);

                mappingHandler.Dispatch((uint)inputRecs.Length, 1, 1);
            }

            Span<uint> readTextureParm = stackalloc uint[4];
            readTextureParm[0] = (uint)sourceTexture.Width;
            readTextureParm[1] = (uint)sourceTexture.Hight;
            readTextureParm[2] = readTextureParm[3] = 0;

            using (var samplerHandler = engine.GetComputeHandler(engine.GetExKeyQuery<IAtlasSamplerComputeKey>().AtlasSamplerKey[engine.StandardComputeKey.DefaultSampler]))
            {
                var readTextureParmBufID = samplerHandler.NameToID("ReadTextureParm");
                var readTexID = samplerHandler.NameToID("ReadTex");

                var transMapID = samplerHandler.NameToID("TransMap");
                var scalingMapID = samplerHandler.NameToID("ScalingMap");
                var isWriteMapID = samplerHandler.NameToID("WriteMap");
                var targetTexID = samplerHandler.NameToID("TargetTex");

                samplerHandler.UploadConstantsBuffer<uint>(readTextureParmBufID, readTextureParm);
                samplerHandler.SetTexture(readTexID, sourceTexture);

                samplerHandler.SetTexture(transMapID, transMap);
                samplerHandler.SetTexture(scalingMapID, scalingMap);
                samplerHandler.SetTexture(isWriteMapID, isWriteMap);
                samplerHandler.SetTexture(targetTexID, targetRT);

                samplerHandler.DispatchWithTextureSize(targetRT);
            }
        }
        private static int GetNormalizedMinHeightSize(int atlasTextureSize, float height)
        {
            switch (height)
            {
                default: return atlasTextureSize;
                case < (1 / 32f): return atlasTextureSize / 32;
                case < (1 / 16f): return atlasTextureSize / 16;
                case < (1 / 8f): return atlasTextureSize / 8;
                case < (1 / 4f): return atlasTextureSize / 4;
                case < (1 / 2f): return atlasTextureSize / 2;
            }
        }
        static Vector2Int GetAtlasTextureSize(AtlasSetting atlasSetting)
        {
            var atlasTargeSize = new Vector2Int(atlasSetting.AtlasTextureSize, atlasSetting.AtlasTextureSize);
            if (atlasSetting.CustomAspect) { atlasTargeSize.y = atlasSetting.AtlasTextureHeightSize; }
            return atlasTargeSize;
        }
        private static (IslandTransform[] virtualIslandArray, IslandRelocationManager.RelocateResult relocateResult) IslandProcessing(
            IReplaceTracking domain
            , AtlasSetting atlasSetting
            , AtlasContext atlasContext
            , Dictionary<Material, float> sizePriorityDict
            , out long relocationTime
        )
        {
            var atlasTargeSize = GetAtlasTextureSize(atlasSetting);
            var virtualIslandArray = new IslandTransform[atlasContext.SourceVirtualIslands.Length];

            for (var i = 0; virtualIslandArray.Length > i; i += 1)
                virtualIslandArray[i] = atlasContext.SourceVirtualIslands[i].Clone();



            atlasContext.PrimaryTextureSizeScaling(virtualIslandArray, atlasTargeSize);

            var refOriginIslands = GetSourceVirtualIslandForRefOriginIslands(atlasContext);
            var islandDescription = GenerateIslandDescription(atlasContext, refOriginIslands);

            var sizePriority = new float[virtualIslandArray.Length]; for (var i = 0; sizePriority.Length > i; i += 1) { sizePriority[i] = 1f; }
            for (var i = 0; virtualIslandArray.Length > i; i += 1)
            {
                var refOriginIsland = refOriginIslands[i];
                var atlasSubMeshIndexID = atlasContext.AtlasIslandCtx.ReverseOriginDict[refOriginIsland];
                var group = atlasContext.MaterialGroupingCtx.GroupMaterials[atlasSubMeshIndexID.MaterialGroupID];

                var materialFineTuningValue = 0f;
                var count = 0;
                foreach (var gm in group)
                    if (sizePriorityDict.TryGetValue(gm, out var priorityValue))
                    {
                        materialFineTuningValue += priorityValue;
                        count += 1;
                    }

                materialFineTuningValue /= count;
                sizePriority[i] = materialFineTuningValue;
            }

            foreach (var islandFineTuner in atlasSetting.IslandFineTuners)
                islandFineTuner?.IslandFineTuning(sizePriority, refOriginIslands, islandDescription, domain);


            for (var i = 0; sizePriority.Length > i; i += 1)
                sizePriority[i] = TTMath.Saturate(sizePriority[i]);


            if (atlasSetting.AtlasIslandRelocator != null) { throw new NotImplementedException(); }
            var islandRelocator = new NFDHPlasFC() as IIslandRelocator;



            var relocateManage = new IslandRelocationManager(islandRelocator);
            relocateManage.Padding = atlasSetting.IslandPadding;
            relocateManage.ForceSizePriority = atlasSetting.ForceSizePriority;
            relocateManage.Height = (float)atlasTargeSize.y / atlasTargeSize.x;

            var timer = System.Diagnostics.Stopwatch.StartNew();
            relocateManage.RelocateLoop(atlasContext, virtualIslandArray, sizePriority, out var relocateResult);
            timer.Stop();
            relocationTime = timer.ElapsedMilliseconds;

            if (relocateResult.IslandRelocationResult is null || relocateResult.IslandRelocationResult.IsSuccess is false)
                TTLog.Error("AtlasTexture:error:RelocationFailed");


            var rectTangleMove = relocateResult.IslandRelocationResult?.IsRectangleMove ?? true;
            Debug.Assert(rectTangleMove is true);


            if (atlasSetting.PixelNormalize)
                if (TTMath.Approximately(atlasSetting.IslandPadding, 0))
                    TTLog.Warning("AtlasTexture:warn:IslandPaddingIsZeroAndPixelNormalizeUsed");

            PostVirtualIslandProcessing(virtualIslandArray, atlasTargeSize, atlasSetting.PixelNormalize);

            return (virtualIslandArray, relocateResult);
        }
        private static Island[] GetSourceVirtualIslandForRefOriginIslands(AtlasContext atlasContext)
        {
            var refOriginIslands = new Island[atlasContext.SourceVirtualIslands.Length];
            for (var i = 0; atlasContext.SourceVirtualIslands.Length > i; i += 1)
            {
                var refOriginIsland = atlasContext.ReverseSourceVirtualIsland2OriginIslands[atlasContext.SourceVirtualIslands[i]].First();
                refOriginIslands[i] = refOriginIsland;
            }

            return refOriginIslands;
        }
        private static IslandSelector.IslandDescription[] GenerateIslandDescription(AtlasContext atlasContext, Island[] refOriginIslands)
        {
            var islandDescription = new IslandSelector.IslandDescription[atlasContext.SourceVirtualIslands.Length];

            for (var i = 0; atlasContext.SourceVirtualIslands.Length > i; i += 1)
            {
                var refOriginIsland = refOriginIslands[i];
                var atlasSubMeshIndexID = atlasContext.AtlasIslandCtx.ReverseOriginDict[refOriginIsland];
                var md = atlasContext.NormalizedMeshCtx.GetMeshDataFromMeshID(atlasSubMeshIndexID.MeshID);

                var vertex = md.Vertices;
                var uv = md.VertexUV;
                var renderer = md.ReferenceRenderer;
                islandDescription[i] = new IslandSelector.IslandDescription(vertex, uv, renderer, atlasSubMeshIndexID.SubMeshIndex);
            }
            return islandDescription;
        }

        private static void PostVirtualIslandProcessing(IslandTransform[] virtualIslandArray, Vector2Int atlasTargeSize, bool pixelNormalize)
        {
            if (pixelNormalize)
            {
                for (var i = 0; virtualIslandArray.Length > i; i += 1)
                {
                    virtualIslandArray[i].Position = AtlasContext.NormalizeMin(atlasTargeSize.x, atlasTargeSize.y, virtualIslandArray[i].Position);
                }
            }

            for (var i = 0; virtualIslandArray.Length > i; i += 1)
            {
                if (virtualIslandArray[i].Size.X <= 0.0001f) { virtualIslandArray[i].Size.X = 0.0001f; }
                if (virtualIslandArray[i].Size.Y <= 0.0001f) { virtualIslandArray[i].Size.Y = 0.0001f; }
            }//Islandが小さすぎると RectTangleMoveのコピーがうまくいかない
        }

        internal List<Material> GetTargetMaterials(IDomain domain, List<Renderer> nowRenderers)
        {
            var nowContainsMatSet = new HashSet<Material>(RendererUtility.GetMaterials(nowRenderers).Where(i => i != null));
            var targetMaterials = nowContainsMatSet.Where(mat => SelectMatList.Any(smat => domain.OriginEqual(smat.Material, mat))).ToList();
            return targetMaterials;
        }

        // internal static void DefaultRefCopyTuning(Dictionary<string, TexFineTuningHolder> atlasTexFineTuningTargets, Dictionary<string, List<string>> referenceCopyDict)
        // {
        //     if (referenceCopyDict == null) { return; }

        //     foreach (var fineTuning in referenceCopyDict.Select(i => new ReferenceCopy(new(i.Key), i.Value.Select(i => new PropertyName(i)).ToList())))
        //     {
        //         fineTuning.AddSetting(atlasTexFineTuningTargets);
        //     }
        // }
        // internal static void DefaultMargeTextureDictTuning(Dictionary<string, TexFineTuningHolder> atlasTexFineTuningTargets, Dictionary<string, List<string>> margeTextureDict)
        // {
        //     if (margeTextureDict == null) { return; }

        //     foreach (var fineTuning in margeTextureDict.Select(i => new MergeTexture(new(i.Key), i.Value.Select(i => new PropertyName(i)).ToList())))
        //     {
        //         fineTuning.AddSetting(atlasTexFineTuningTargets);
        //     }

        // }
        // internal static void SetSizeDataMaxSize(Dictionary<string, TexFineTuningHolder> atlasTexFineTuningTargets, Dictionary<string, int> sourceTextureMaxSize)
        // {
        //     foreach (var texMax in sourceTextureMaxSize)
        //     {
        //         if (texMax.Key == "_MainTex") { continue; }
        //         var sizeData = atlasTexFineTuningTargets[texMax.Key].Get<SizeData>();
        //         sizeData.TextureSize = texMax.Value;
        //     }
        // }

        internal List<Renderer> GetTargetAllowedFilter(IEnumerable<Renderer> domainRenderers) { return domainRenderers.Where(i => AtlasAllowedRenderer(i, AtlasSetting.IncludeDisabledRenderer)).ToList(); }
        // private void TransMoveRectIsland<TIslandRect>(Texture sourceTex, RenderTexture targetRT, Dictionary<Island, TIslandRect> notAspectIslandPairs, float uvScalePadding) where TIslandRect : IIslandRect
        // {
        //     uvScalePadding *= 0.5f;
        //     using (var sUV = new NativeArray<Vector2>(notAspectIslandPairs.Count * 4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
        //     using (var tUV = new NativeArray<Vector2>(notAspectIslandPairs.Count * 4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
        //     using (var triangles = new NativeArray<TriangleIndex>(notAspectIslandPairs.Count * 2, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
        //     {
        //         var triSpan = triangles.AsSpan();
        //         var sUVSpan = sUV.AsSpan();
        //         var tUVSpan = tUV.AsSpan();

        //         var triIndex = 0;
        //         var vertIndex = 0;
        //         foreach (var islandPair in notAspectIslandPairs)
        //         {
        //             var (Origin, Moved) = (islandPair.Key, islandPair.Value);
        //             var rectScalePadding = Moved.UVScaleToRectScale(uvScalePadding);

        //             var originVertexes = Origin.GenerateRectVertexes(rectScalePadding);
        //             var movedVertexes = Moved.GenerateRectVertexes(rectScalePadding);

        //             triSpan[triIndex] = new(vertIndex + 0, vertIndex + 1, vertIndex + 2);
        //             triSpan[triIndex + 1] = new(vertIndex + 0, vertIndex + 2, vertIndex + 3);
        //             triIndex += 2;

        //             foreach (var v in originVertexes.Zip(movedVertexes, (o, m) => (o, m)))
        //             {
        //                 sUVSpan[vertIndex] = v.o;
        //                 tUVSpan[vertIndex] = v.m;
        //                 vertIndex += 1;
        //             }
        //         }

        //         TransTexture.ForTrans(targetRT, sourceTex, new TransData(triangles, tUV, sUV), argTexWrap: TextureWrap.Loop, NotTileNormalize: true);
        //     }
        // }

        private static Material GenerateAtlasMat(Material targetMat, Dictionary<string, Texture2D> atlasTex, AtlasMatGenerateOption option)
        {
            var editableTMat = UnityEngine.Object.Instantiate(targetMat);

            foreach (var texKV in atlasTex)
            {
                // if (supporter.IsConstraintValid(targetMat, texKV.Key) is false) { continue; }
                var tex = editableTMat.GetTexture(texKV.Key);
                if (tex == null) { tex = null; }//これは Unity側の Null を C# の Null にしてるやつ

                if (option.ForceSetTexture is false && tex == null) { continue; }
                if (tex is not Texture2D && tex is not RenderTexture && tex is not null) { continue; }
                // if (tex is RenderTexture rt && TTRt.IsTemp(rt) is false && TTRt2.IsTemp(rt) is false) { continue; }

                if (tex != null && option.UnsetTextures is not null && option.UnsetTextures.Contains(tex)) { continue; }

                editableTMat.SetTexture(texKV.Key, texKV.Value);

                // if (option.TextureScaleOffsetReset)
                // {
                //     editableTMat.SetTextureScale(texKV.Key, Vector2.one);
                //     editableTMat.SetTextureOffset(texKV.Key, Vector2.zero);
                // }
                // if (option.BakedPropertyReset && bakePropMaxValue != null)
                // {
                //     foreach (var bakedPropertyDescriptor in supporter.GetBakePropertyNames(texKV.Key))
                //     {
                //         if (bakePropMaxValue.ContainsKey(bakedPropertyDescriptor.PropertyName) is false || bakedPropertyDescriptor.UseMaxValue is false)
                //         { editableTMat.PropertyReset(bakedPropertyDescriptor.PropertyName); }
                //         else { editableTMat.SetFloat(bakedPropertyDescriptor.PropertyName, bakePropMaxValue[bakedPropertyDescriptor.PropertyName]); }
                //     }
                // }
            }

            // foreach (var postProcess in supporter.AtlasMaterialPostProses) { postProcess.Proses(editableTMat); }

            return editableTMat;
        }
        class AtlasMatGenerateOption
        {
            public bool ForceSetTexture = false;
            public HashSet<Texture>? UnsetTextures = null;
        }



        internal static List<Renderer> FilteredRenderers(GameObject targetRoot, bool includeDisabledRenderer)
        {
            return targetRoot.GetComponentsInChildren<Renderer>(true).Where(r => AtlasAllowedRenderer(r, includeDisabledRenderer)).ToList();
        }

        internal static bool AtlasAllowedRenderer(Renderer item, bool includeDisabledRenderer)
        {
            if (includeDisabledRenderer is false) { if (item.gameObject.activeInHierarchy is false || item.enabled is false) { return false; } }
            if (item.tag == "EditorOnly") return false;
            if (item.GetMesh() == null) return false;
            if (item.GetMesh().uv.Any() == false) return false;
            if (item.sharedMaterials.Length == 0) return false;
            if (item.GetComponent<AtlasExcludeTag>() != null) return false;

            return true;
        }
        internal override IEnumerable<Renderer> ModificationTargetRenderers(IRendererTargeting rendererTargeting)
        {
            var nowContainsMatSet = new HashSet<Material>(GetTargetAllowedFilter(rendererTargeting.EnumerateRenderer()).SelectMany(r => rendererTargeting.GetMaterials(r)).Where(i => i != null));
            var selectedMaterials = rendererTargeting.LookAtGet(this, at => at.SelectMatList.Select(sMat => sMat.Material).ToArray(), (l, r) => l.SequenceEqual(r));
            var targetMaterials = nowContainsMatSet.Where(mat => selectedMaterials.Any(sMat => rendererTargeting.OriginEqual(sMat, mat))).ToHashSet();
            return rendererTargeting.RendererFilterForMaterial(targetMaterials);
        }



    }
}
