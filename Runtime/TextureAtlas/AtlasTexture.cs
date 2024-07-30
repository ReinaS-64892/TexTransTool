using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.Island;
using Island = net.rs64.TexTransCore.Island.Island;
using static net.rs64.TexTransCore.TransTexture;
using net.rs64.TexTransCore.Utils;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;
using net.rs64.TexTransTool.TextureAtlas.IslandRelocator;
using UnityEngine.Serialization;
using Unity.Collections;
using net.rs64.TexTransTool.TextureAtlas.AtlasScriptableObject;
using UnityEngine.Profiling;
using net.rs64.TexTransCore.MipMap;
using Unity.Mathematics;
using net.rs64.TexTransCore.BlendTexture;

namespace net.rs64.TexTransTool.TextureAtlas
{
    [AddComponentMenu(TTTName + "/" + MenuPath)]
    public sealed class AtlasTexture : TexTransRuntimeBehavior
    {
        internal const string ComponentName = "TTT AtlasTexture";
        internal const string MenuPath = ComponentName;
        [FormerlySerializedAs("TargetRoot")] public GameObject LimitCandidateMaterials;
        public List<MatSelector> SelectMatList = new List<MatSelector>();
        public AtlasSetting AtlasSetting = new AtlasSetting();

        internal override bool IsPossibleApply => SelectMatList.Any();


        internal override TexTransPhase PhaseDefine => TexTransPhase.Optimizing;

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

        internal class AtlasData
        {
            public Dictionary<string, Texture2D> Textures;
            public Dictionary<string, int> SourceTextureMaxSize;
            public List<AtlasMeshHolder> Meshes;
            public List<Material> AtlasInMaterials;
            public HashSet<Material>[] MaterialID;
            public Dictionary<string, List<string>> ReferenceCopyDict;
            public Dictionary<string, List<string>> MargeTextureDict;
            public Dictionary<string, bool> AlphaContainsDict;

            public struct AtlasMeshHolder
            {
                public Mesh DistMesh;
                public Mesh NormalizedMesh;
                public Mesh AtlasMesh;
                public int[] MatIDs;

                public AtlasMeshHolder(Mesh distMesh, Mesh normalizedMesh, Mesh atlasMesh, int[] mats)
                {
                    DistMesh = distMesh;
                    NormalizedMesh = normalizedMesh;
                    AtlasMesh = atlasMesh;
                    MatIDs = mats;
                }
            }
        }
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

        internal bool TryCompileAtlasTextures(List<Renderer> nowTargetAllowedRenderer, IDomain domain, out AtlasData atlasData)
        {
            Profiler.BeginSample("AtlasData and FindRenderers");
            var texManage = domain.GetTextureManager();
            atlasData = new AtlasData();


            //情報を集めるフェーズ
            var nowContainsMatSet = new HashSet<Material>(RendererUtility.GetMaterials(nowTargetAllowedRenderer).Where(i => i != null));
            var targetMaterials = nowContainsMatSet.Where(mat => SelectMatList.Any(smat => domain.OriginEqual(smat.Material, mat))).ToList();
            var sizePriorityDict = targetMaterials.ToDictionary(i => i, i => SelectMatList.First(m => domain.OriginEqual(m.Material, i)).MaterialFineTuningValue);

            atlasData.AtlasInMaterials = targetMaterials;
            var atlasSetting = AtlasSetting;
            var propertyBakeSetting = atlasSetting.MergeMaterials ? atlasSetting.PropertyBakeSetting : PropertyBakeSetting.NotBake;
            Profiler.EndSample();

            Profiler.BeginSample("AtlasContext:ctor");
            var atlasContext = new AtlasContext(targetMaterials, nowTargetAllowedRenderer, propertyBakeSetting != PropertyBakeSetting.NotBake);
            Profiler.EndSample();

            //アイランドまわり
            if (atlasSetting.PixelNormalize)
            {
                Profiler.BeginSample("AtlasReferenceData:PixelNormalize");
                foreach (var islandKV in atlasContext.IslandDict)
                {
                    var material = atlasContext.MaterialToAtlasShaderTexDict[atlasContext.MaterialGroup[islandKV.Key.MaterialGroupID].First()];
                    var refTex = material.TryGetValue("_MainTex", out var tex2D) ? tex2D.Texture : null;
                    if (refTex == null) { continue; }
                    foreach (var island in islandKV.Value) { NormalizeIsland(refTex.width, refTex.height, island); }
                }
                Profiler.EndSample();
            }


            Profiler.BeginSample("IslandFineTuning");
            var islandArray = atlasContext.Islands;
            var rectArray = new IslandRect[islandArray.Length];
            var index = 0;
            foreach (var islandKV in atlasContext.Islands)
            {
                rectArray[index] = new IslandRect(islandKV);
                index += 1;
            }

            Profiler.BeginSample("TextureIslandScaling");
            var textureIslandScale = new Dictionary<int, float>();
            var textureIslandAspect = new Dictionary<int, float>();
            for (var i = 0; atlasContext.MaterialGroup.Length > i; i += 1)
            {
                var tex = atlasContext.MaterialGroup[i].Select(m => m.mainTexture).Where(t => t != null).FirstOrDefault();
                if (tex != null)
                {
                    var atlasTexPixelCount = atlasSetting.AtlasTextureSize * atlasSetting.AtlasTextureSize;
                    var texPixelCount = tex.width * tex.height;
                    textureIslandScale[i] = Mathf.Sqrt(texPixelCount / (float)atlasTexPixelCount);
                    textureIslandAspect[i] = tex.width / tex.height;
                }
                else
                {
                    textureIslandScale[i] = (float)0.01f;
                    textureIslandAspect[i] = 1f;
                }

            }
            for (var i = 0; rectArray.Length > i; i += 1)
            {
                var subData = atlasContext.IslandSubData[i];
                rectArray[i].Size *= textureIslandScale[subData.MaterialGroupID];
                rectArray[i].Size.y *= textureIslandAspect[subData.MaterialGroupID];
            }
            Profiler.EndSample();

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

                var materialFineTuningValue = 0f;
                var count = 0;
                foreach (var gm in group)
                {
                    if (sizePriorityDict.TryGetValue(gm, out var priorityValue))
                    {
                        materialFineTuningValue += priorityValue;
                        count += 1;
                    }
                }
                materialFineTuningValue /= count;
                sizePriority[i] = materialFineTuningValue;
            }

            foreach (var islandFineTuner in atlasSetting.IslandFineTuners) { islandFineTuner?.IslandFineTuning(sizePriority, islandArray, islandDescription, domain); }
            for (var i = 0; sizePriority.Length > i; i += 1) { sizePriority[i] = Mathf.Clamp01(sizePriority[i]); }
            Profiler.EndSample();


            IAtlasIslandRelocator relocator = atlasSetting.AtlasIslandRelocator != null ? UnityEngine.Object.Instantiate(atlasSetting.AtlasIslandRelocator) : new NFDHPlasFC();


            Profiler.BeginSample("Relocation");
            var relocateManage = new IslandRelocationManager(relocator);
            relocateManage.Padding = atlasSetting.IslandPadding;
            relocateManage.ForceSizePriority = atlasSetting.ForceSizePriority;

            var timer = System.Diagnostics.Stopwatch.StartNew();
            var relocatedRect = relocateManage.RelocateLoop(rectArray, sizePriority, out var relocateResult);
            timer.Stop();
            Profiler.EndSample();

            if (relocateResult.IsRelocateSuccess is false) { TTTRuntimeLog.Error("AtlasTexture:error:RelocationFailed"); }

            var rectTangleMove = relocator.RectTangleMove;

            if (relocator is UnityEngine.Object unityObject) { DestroyImmediate(unityObject); }

            if (atlasSetting.PixelNormalize)
            {
                if (Mathf.Approximately(atlasSetting.IslandPadding, 0)) { TTTRuntimeLog.Warning("AtlasTexture:warn:IslandPaddingIsZeroAndPixelNormalizeUsed"); }
                Profiler.BeginSample("AtlasReferenceData:PixelNormalize");
                for (var i = 0; relocatedRect.Length > i; i += 1)
                {
                    relocatedRect[i] = NormalizeIsland(atlasSetting.AtlasTextureSize, atlasSetting.AtlasTextureSize, relocatedRect[i]);
                }
                Profiler.EndSample();
            }
            Profiler.BeginSample("IslandMinClamp");
            for (var i = 0; relocatedRect.Length > i; i += 1)
            {
                if (relocatedRect[i].Size.x <= 0.0001f) { relocatedRect[i].Size.x = 0.0001f; }
                if (relocatedRect[i].Size.y <= 0.0001f) { relocatedRect[i].Size.y = 0.0001f; }
            }//Islandが小さすぎると RectTangleMoveのコピーがうまくいかない
            Profiler.EndSample();

            //上側を削れるかを見る
            Profiler.BeginSample("IslandHight Calculate");
            var height = IslandRectUtility.CalculateIslandsMaxHeight(relocatedRect);
            var atlasTextureHeightSize = Mathf.Max(GetNormalizedMinHeightSize(atlasSetting.AtlasTextureSize, height), 4);//4以下はちょっと怪しい挙動しそうだからクランプ
            Debug.Assert(Mathf.IsPowerOfTwo(atlasTextureHeightSize));
            Profiler.EndSample();

            TTTRuntimeLog.Info("AtlasTexture:info:RelocateResult", 1 - height, relocateResult.PriorityDownScale, relocateResult.OverallDownScale, relocateResult.TotalRelocateCount, timer.ElapsedMilliseconds);


            //新しいUVを持つMeshを生成するフェーズ
            Profiler.BeginSample("MeshCompile");
            var compiledMeshes = new List<AtlasData.AtlasMeshHolder>();
            var normMeshes = atlasContext.Meshes.Select(m => atlasContext.NormalizeMeshes[m]).ToArray();
            var subSetMovedUV = new NativeArray<Vector2>[atlasContext.AtlasSubSets.Count];
            var scale = atlasSetting.AtlasTextureSize / atlasTextureHeightSize;
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


                for (var islandIndex = 0; relocatedRect.Length > islandIndex; islandIndex += 1)
                {
                    if (subSet.Any(subData => atlasContext.IslandSubData[islandIndex] == subData))
                    {
                        originLink.AddLast(atlasContext.Islands[islandIndex]);
                        movedLink.AddLast(relocatedRect[islandIndex]);
                    }
                }

                var movedUV = new NativeArray<Vector2>(meshData.VertexUV, Allocator.Temp);
                IslandUtility.IslandPoolMoveUV(meshData.VertexUV, movedUV, originLink.ToArray(), movedLink.ToArray());

                subSetMovedUV[subSetIndex] = movedUV;
                if (atlasSetting.AtlasTextureSize != atlasTextureHeightSize)
                {
                    using (var aspectApplyUV = new NativeArray<Vector2>(movedUV, Allocator.Temp))
                    {
                        var writer = aspectApplyUV.AsSpan();
                        foreach (var vi in originLink.SelectMany(i => i.GetVertexIndex()).Distinct())
                        {
                            writer[vi].x = movedUV[vi].x;
                            writer[vi].y = movedUV[vi].y * scale;
                        }
                        newMesh.SetUVs(0, aspectApplyUV);
                    }
                }
                else
                {
                    newMesh.SetUVs(0, movedUV);
                }
                if (atlasSetting.WriteOriginalUV) { newMesh.SetUVs(math.clamp(atlasSetting.OriginalUVWriteTargetChannel, 1, 7), meshData.VertexUV); }

                compiledMeshes.Add(new AtlasData.AtlasMeshHolder(distMesh, UnityEngine.Object.Instantiate(nmMesh), newMesh, subSet.Select(i => i?.MaterialGroupID ?? -1).ToArray()));
            }
            atlasData.Meshes = compiledMeshes;
            Profiler.EndSample();

            //アトラス化したテクスチャーを生成するフェーズ
            var compiledAtlasTextures = new Dictionary<string, AsyncTexture2D>();

            Profiler.BeginSample("GetGroupedTextures");
            var groupedTextures = GetGroupedTextures(atlasSetting, atlasContext, propertyBakeSetting, out var containsProperty, texManage);
            Profiler.EndSample();

            Profiler.BeginSample("Texture synthesis");
            foreach (var propName in containsProperty)
            {
                var targetRT = TTRt.G(atlasSetting.AtlasTextureSize, atlasSetting.AtlasTextureSize, false, true, true, true);
                TextureBlend.ColorBlit(targetRT, atlasSetting.BackGroundColor);
                targetRT.name = "AtlasTex" + propName;
                Profiler.BeginSample("Draw:" + targetRT.name);
                foreach (var gTex in groupedTextures)
                {
                    if (!gTex.Value.TryGetValue(propName, out var sTexture)) { continue; }

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

                        TransMoveRectIsland(sTexture, targetRT, islandPairs, atlasSetting.IslandPadding);
                    }
                    else
                    {
                        for (var subSetIndex = 0; atlasContext.AtlasSubSets.Count > subSetIndex; subSetIndex += 1)
                        {
                            var transTargets = atlasContext.AtlasSubSets[subSetIndex].Where(i => i.HasValue).Where(i => i.Value.MaterialGroupID == findMaterialID).Select(i => i.Value);
                            if (!transTargets.Any()) { continue; }

                            var triangles = new NativeArray<TriangleIndex>(transTargets.SelectMany(subData => atlasContext.IslandDict[subData].SelectMany(i => i.triangles)).ToArray(), Allocator.TempJob);
                            var originUV = atlasContext.MeshDataDict[atlasContext.NormalizeMeshes[atlasContext.Meshes[transTargets.First().MeshID]]].VertexUV;

                            var transData = new TransData<Vector2>(triangles, subSetMovedUV[subSetIndex], originUV);
                            ForTrans(targetRT, sTexture, transData, atlasSetting.GetTexScalePadding * 0.5f, null, true);

                            triangles.Dispose();
                        }
                    }

                }
                Profiler.EndSample();

                if (atlasSetting.AtlasTextureSize != atlasTextureHeightSize)
                {
                    var heightClampRt = TTRt.G(atlasSetting.AtlasTextureSize, atlasTextureHeightSize);
                    heightClampRt.name = $"{targetRT.name}-heightClamp-TempRt-{heightClampRt.width}x{heightClampRt.height}";
                    Graphics.CopyTexture(targetRT, 0, 0, 0, 0, heightClampRt.width, heightClampRt.height, heightClampRt, 0, 0, 0, 0);
                    TTRt.R(targetRT);
                    targetRT = heightClampRt;
                }
                MipMapUtility.GenerateMips(targetRT, atlasSetting.DownScalingAlgorithm);

                Profiler.BeginSample("Readback");
                compiledAtlasTextures.Add(propName, new AsyncTexture2D(targetRT));
                Profiler.EndSample();

                TTRt.R(targetRT);
            }
            Profiler.EndSample();

            if (atlasSetting.AutoReferenceCopySetting && propertyBakeSetting == PropertyBakeSetting.NotBake)
            {
                Profiler.BeginSample("AutoReferenceCopySetting");
                var prop = containsProperty.ToArray();
                var refCopyDict = new Dictionary<string, string>();

                var gtHash = containsProperty.ToDictionary(p => p, p => groupedTextures.Where(i => i.Value.ContainsKey(p)).Select(i => i.Value[p]).ToHashSet());

                for (var i = 0; prop.Length > i; i += 1)
                    for (var i2 = 0; prop.Length > i2; i2 += 1)
                    {
                        if (i == i2) { continue; }

                        var copySource = prop[i];
                        var copyTarget = prop[i2];

                        var sTexHash = gtHash[copySource];
                        var tTexHash = gtHash[copyTarget];

                        if (sTexHash.SetEquals(tTexHash)) { refCopyDict.Remove(copySource); }

                        if (sTexHash.IsSupersetOf(tTexHash) is false) { continue; }
                        refCopyDict[copyTarget] = copySource;

                        // if (refCopyDict.ContainsKey(copySource) is false) { refCopyDict[copySource] = new(); }
                        // refCopyDict[copySource].Add(copyTarget);
                    }


                var isContinue = true;
                var values = refCopyDict.Values.ToArray();
                while (isContinue)
                {
                    foreach (var s in values)
                    {
                        if (refCopyDict.ContainsKey(s))
                        {
                            foreach (var t in refCopyDict.Where(i => i.Value == s).ToArray())
                            {
                                refCopyDict[t.Key] = refCopyDict[s];
                            }
                            isContinue = true;
                        }
                    }
                    isContinue = false;
                }

                atlasData.ReferenceCopyDict = refCopyDict.GroupBy(i => i.Value).ToDictionary(i => i.Key, i => i.Select(k => k.Key).ToList());
                Profiler.EndSample();
            }
            if (atlasSetting.AutoMergeTextureSetting)
            {
                Profiler.BeginSample("AutoMergeTextureSetting");

                var prop = containsProperty.ToArray();
                var prop2MatIDs = prop.ToDictionary(p => p, p => groupedTextures.Where(gt => gt.Value.ContainsKey(p)).Select(gt => gt.Key).ToHashSet());

                var alreadySetting = new HashSet<string>();
                var margeSettingDict = new Dictionary<string, List<string>>();



                for (var i = 0; prop.Length > i; i += 1)
                {
                    var mergeParent = prop[i];
                    var mergedHash = prop2MatIDs[mergeParent].ToHashSet();
                    var childList = new List<string>();

                    for (var i2 = i; prop.Length > i2; i2 += 1)
                    {
                        if (i == i2) { continue; }

                        var mergeChild = prop[i2];

                        if (alreadySetting.Contains(mergeChild)) { continue; }

                        var childHash = prop2MatIDs[mergeChild];

                        if (mergedHash.Overlaps(childHash)) { continue; }

                        childList.Add(mergeChild);
                        mergedHash.UnionWith(childHash);
                        alreadySetting.Add(mergeChild);
                    }

                    if (childList.Any()) { margeSettingDict[mergeParent] = childList; }
                }

                atlasData.MargeTextureDict = margeSettingDict;
                Profiler.EndSample();
            }

            Profiler.BeginSample("ReleaseGroupeTextures");
            foreach (var kv in groupedTextures.Values) { foreach (var tex in kv) { TTRt.R(tex.Value); } }
            groupedTextures = null;
            Profiler.EndSample();

            Profiler.BeginSample("TextureMaxSize");
            var texMaxDict = new Dictionary<string, int>();
            foreach (var atlasTexKV in atlasContext.MaterialToAtlasShaderTexDict.SelectMany(x => x.Value))
            {
                if (compiledAtlasTextures.ContainsKey(atlasTexKV.Key) is false) { continue; }
                if (texMaxDict.ContainsKey(atlasTexKV.Key) is false) { texMaxDict[atlasTexKV.Key] = 2; }
                if (atlasTexKV.Value.Texture == null) { continue; }
                texMaxDict[atlasTexKV.Key] = math.max(texMaxDict[atlasTexKV.Key], atlasTexKV.Value.Texture.width);
            }
            atlasData.SourceTextureMaxSize = texMaxDict;
            Profiler.EndSample();

            Profiler.BeginSample("Async Readback");
            atlasData.Textures = compiledAtlasTextures.ToDictionary(kv => kv.Key, kv => kv.Value.GetTexture2D());
            Profiler.EndSample();

            atlasData.MaterialID = atlasContext.MaterialGroup.Select(i => i.ToHashSet()).ToArray();
            atlasContext.Dispose();
            foreach (var movedUV in subSetMovedUV) { movedUV.Dispose(); }

            return true;

        }
        static T NormalizeIsland<T>(int width, int height, T island) where T : IIslandRect
        {
            var minPos = island.Pivot;
            var maxPos = island.Pivot + island.Size;
            island.Pivot = Normalize(width, height, minPos);
            island.Size = Normalize(width, height, maxPos) - island.Pivot;
            return island;
        }
        static Vector2 Normalize(int width, int height, Vector2 vector2)
        {
            vector2.y = Mathf.Round(vector2.y * height) / height;
            vector2.x = Mathf.Round(vector2.x * width) / width;
            return vector2;
        }
        private static Dictionary<int, Dictionary<string, RenderTexture>> GetGroupedTextures(AtlasSetting atlasSetting, AtlasContext atlasContext, PropertyBakeSetting propertyBakeSetting, out HashSet<string> property, ITextureManager texManage)
        {
            var downScalingAlgorithm = atlasSetting.DownScalingAlgorithm;
            switch (propertyBakeSetting)
            {
                default: { property = null; return null; }
                case PropertyBakeSetting.NotBake:
                    {
                        var dict = atlasContext.MaterialGroupToAtlasShaderTexDict.ToDictionary(i => i.Key, i => ZipDictAndOffset(i.Value));
                        property = new HashSet<string>(dict.SelectMany(i => i.Value.Keys));
                        return dict;

                        Dictionary<string, RenderTexture> ZipDictAndOffset(Dictionary<string, AtlasShaderTexture2D> keyValuePairs)
                        {
                            var dict = new Dictionary<string, RenderTexture>();
                            var rtDict = new Dictionary<(Texture sTex, Vector2 tScale, Vector2 tTiling), RenderTexture>();
                            foreach (var kv in keyValuePairs)
                            {
                                if (kv.Value.Texture == null) { continue; }
                                var atlasTex = kv.Value;

                                var texHash = (atlasTex.Texture, atlasTex.TextureScale, atlasTex.TextureTranslation);
                                if (rtDict.ContainsKey(texHash)) { dict[kv.Key] = rtDict[texHash]; continue; }

                                var rt = GetOriginAtUseMip(texManage, atlasTex.Texture);

                                if (atlasTex.TextureScale != Vector2.one || atlasTex.TextureTranslation != Vector2.zero)
                                { rt.ApplyTextureST(atlasTex.TextureScale, atlasTex.TextureTranslation); }

                                MipMapUtility.GenerateMips(rt, downScalingAlgorithm);

                                rtDict[texHash] = dict[kv.Key] = rt;
                            }
                            return dict;


                        }
                    }
                case PropertyBakeSetting.Bake:
                case PropertyBakeSetting.BakeAllProperty:
                    {
                        property = new HashSet<string>(atlasContext.MaterialToAtlasShaderTexDict
                                .SelectMany(i => i.Value)
                                .GroupBy(i => i.Key)
                                .Where(i => PropertyBakeSetting.BakeAllProperty == propertyBakeSetting || i.Any(st => st.Value.Texture != null))
                                .Select(i => i.Key)
                            );

                        var groupDict = new Dictionary<int, Dictionary<string, RenderTexture>>(atlasContext.MaterialGroup.Length);
                        var tmpMat = new Material(Shader.Find("Unlit/Texture"));

                        var bakePropMaxValue = atlasContext.MaterialToAtlasShaderTexDict.Values.SelectMany(kv => kv)
                            .SelectMany(i => i.Value.BakeProperties)
                            .GroupBy(i => i.PropertyName)
                            .Where(i => i.First() is BakeFloat || i.First() is BakeRange)
                            .ToDictionary(i => i.Key, i => i.Max(p =>
                                {
                                    if (p is BakeFloat bakeFloat) { return bakeFloat.Float; }
                                    if (p is BakeRange bakeRange) { return bakeRange.Float; }
                                    return 0;
                                }
                            )
                        );//一旦 Float として扱えるものだけの実装にしておく。


                        for (var gi = 0; atlasContext.MaterialGroup.Length > gi; gi += 1)
                        {
                            var matGroup = atlasContext.MaterialGroup[gi];
                            var groupMat = matGroup.First();

                            var atlasTexDict = atlasContext.MaterialToAtlasShaderTexDict[groupMat];//テクスチャに関する情報が完全に同じでないと同じグループにならない。だから適当なものでよい。
                            var shaderSupport = atlasContext.AtlasShaderSupporters[groupMat];

                            tmpMat.shader = shaderSupport.BakeShader;

                            var texDict = groupDict[gi] = new();

                            foreach (var propName in property)
                            {
                                atlasTexDict.TryGetValue(propName, out var atlasTex);
                                var sTex = atlasTex?.Texture != null ? GetOriginAtUseMip(texManage, atlasTex.Texture) : null;

                                if (sTex != null && (atlasTex.TextureScale != Vector2.one || atlasTex.TextureTranslation != Vector2.zero)) { sTex.ApplyTextureST(atlasTex.TextureScale, atlasTex.TextureTranslation); }

                                if (shaderSupport.BakeShader == null)
                                {
                                    if (sTex != null)
                                    {
                                        MipMapUtility.GenerateMips(sTex, downScalingAlgorithm);
                                        texDict[propName] = sTex;
                                    }
                                    else
                                    {
                                        var rt = texDict[propName] = TTRt.G(2);
                                        rt.name = $"AtlasTexColRT-2x2";
                                    }
                                    continue;
                                }

                                var bakedTex = sTex != null ? sTex.CloneTemp() : TTRt.G(2, "AtlasEmptyDefaultTexColRT-2x2");


                                if (atlasTex != null)
                                {
                                    foreach (var bakeProp in atlasTex.BakeProperties)
                                    {
                                        bakeProp.WriteMaterial(tmpMat);

                                        var bakePropName = bakeProp.PropertyName;
                                        var maxValPropName = bakePropName + "_MaxValue";
                                        if (tmpMat.HasProperty(maxValPropName) && bakePropMaxValue.TryGetValue(bakePropName, out var bakeMaxValue))
                                        {
                                            tmpMat.SetFloat(maxValPropName, bakeMaxValue);
                                        }
                                    }
                                }

                                tmpMat.EnableKeyword("Bake" + propName);
                                if (atlasTex == null) { tmpMat.EnableKeyword("Constraint_Invalid"); }

                                tmpMat.SetTexture(propName, sTex);
                                Graphics.Blit(sTex, bakedTex, tmpMat);
                                MipMapUtility.GenerateMips(bakedTex, downScalingAlgorithm);

                                texDict[propName] = bakedTex;

                                if (sTex != null) { TTRt.R(sTex); }
                                tmpMat.AllPropertyReset();
                                tmpMat.shaderKeywords = Array.Empty<string>();

                            }


                        }

                        return groupDict;
                    }
            }
        }

        private static RenderTexture GetOriginAtUseMip(ITextureManager texManage, Texture atlasTex)
        {
            switch (atlasTex)
            {
                default:
                    {
                        var originSize = atlasTex.width;
                        var rt = TTRt.G(originSize, originSize, true, false, true, true);
                        rt.name = $"{atlasTex.name}:GetOriginAtUseMip-TempRt-{rt.width}x{rt.height}";
                        rt.CopyFilWrap(atlasTex);
                        rt.filterMode = FilterMode.Trilinear;
                        Graphics.Blit(atlasTex, rt);
                        return rt;
                    }
                case Texture2D atlasTex2D:
                    {
                        var originSize = texManage.GetOriginalTextureSize(atlasTex2D);
                        var rt = TTRt.G(originSize, originSize, true, false, true, true);
                        rt.name = $"{atlasTex.name}:GetOriginAtUseMip-TempRt-{rt.width}x{rt.height}";
                        rt.CopyFilWrap(atlasTex);
                        rt.filterMode = FilterMode.Trilinear;
                        texManage.WriteOriginalTexture(atlasTex2D, rt);
                        return rt;
                    }

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

        internal override void Apply(IDomain domain = null)
        {
            if (!IsPossibleApply)
            {
                TTTRuntimeLog.Error("AtlasTexture:error:TTTNotExecutable");
                return;
            }
            var nowRenderers = GetTargetAllowedFilter(domain.EnumerateRenderer());

            Profiler.BeginSample("TryCompileAtlasTextures");
            if (!TryCompileAtlasTextures(nowRenderers, domain, out var atlasData)) { Profiler.EndSample(); return; }
            Profiler.EndSample();

            Profiler.BeginSample("AtlasShaderSupportUtils:ctor");
            var shaderSupport = new AtlasShaderSupportUtils();
            Profiler.EndSample();

            //Mesh Change
            foreach (var renderer in nowRenderers)
            {
                var mesh = renderer.GetMesh();
                var matIDs = renderer.sharedMaterials.Select(i => Array.FindIndex(atlasData.MaterialID, mh => mh.Contains(i)));
                var atlasMeshHolder = atlasData.Meshes.FindAll(I => I.DistMesh == mesh).Find(I => I.MatIDs.SequenceEqual(matIDs));
                if (atlasMeshHolder.AtlasMesh == null) { continue; }

                try
                {
                    var reMappingCallTargets = renderer.GetComponents<IAtlasReMappingReceiver>();
                    foreach (var target in reMappingCallTargets.Where(i => i != null)) { target.ReMappingReceive(atlasMeshHolder.NormalizedMesh, atlasMeshHolder.AtlasMesh); }
                }
                catch (Exception e) { TTTRuntimeLog.Exception(e); }

                var atlasMesh = atlasMeshHolder.AtlasMesh;
                domain.SetMesh(renderer, atlasMesh);
                domain.TransferAsset(atlasMesh);
            }

            //Texture Fine Tuning
            var atlasTexFineTuningTargets = TexFineTuningUtility.InitTexFineTuning(atlasData.Textures);
            SetSizeDataMaxSize(atlasTexFineTuningTargets, atlasData.SourceTextureMaxSize);
            DefaultMargeTextureDictTuning(atlasTexFineTuningTargets, atlasData.MargeTextureDict);
            DefaultRefCopyTuning(atlasTexFineTuningTargets, atlasData.ReferenceCopyDict);
            foreach (var fineTuning in AtlasSetting.TextureFineTuning)
            {
                fineTuning?.AddSetting(atlasTexFineTuningTargets);
            }
            var individualApplied = new HashSet<string>();
            foreach (var individualTuning in AtlasSetting.TextureIndividualFineTuning)
            {
                if (atlasTexFineTuningTargets.ContainsKey(individualTuning.TuningTarget) is false) { continue; }
                if (individualApplied.Contains(individualTuning.TuningTarget)) { continue; }
                individualApplied.Add(individualTuning.TuningTarget);

                var tuningTarget = atlasTexFineTuningTargets[individualTuning.TuningTarget];

                if (individualTuning.OverrideAsReferenceCopy) { tuningTarget.Get<ReferenceCopyData>().CopySource = individualTuning.CopyReferenceSource; }
                if (individualTuning.OverrideResize) { tuningTarget.Get<SizeData>().TextureSize = individualTuning.TextureSize; }
                if (individualTuning.OverrideCompression) { tuningTarget.Set(individualTuning.CompressionData); }
                if (individualTuning.OverrideMipMapRemove) { tuningTarget.Get<MipMapData>().UseMipMap = individualTuning.UseMipMap; }
                if (individualTuning.OverrideColorSpace) { tuningTarget.Get<ColorSpaceData>().Linear = individualTuning.Linear; }
                if (individualTuning.OverrideAsRemove) { tuningTarget.Get<RemoveData>(); }
                if (individualTuning.OverrideAsMargeTexture) { tuningTarget.Get<MergeTextureData>().MargeParent = individualTuning.MargeRootProperty; }
            }
            TexFineTuningUtility.FinalizeTexFineTuning(atlasTexFineTuningTargets);
            var atlasTexture = atlasTexFineTuningTargets.ToDictionary(i => i.Key, i => i.Value.Texture2D);
            domain.transferAssets(atlasTexture.Select(PaT => PaT.Value));

            //CompressDelegation
            foreach (var atlasTexFTData in atlasTexFineTuningTargets)
            {
                var compressSetting = atlasTexFTData.Value.Find<TextureCompressionData>();
                if (compressSetting == null) { continue; }
                domain.GetTextureManager().DeferredTextureCompress(compressSetting, atlasTexFTData.Value.Texture2D);
            }


            //MaterialGenerate And Change
            if (AtlasSetting.MergeMaterials)
            {
                var mergeMat = AtlasSetting.MergeReferenceMaterial != null ? AtlasSetting.MergeReferenceMaterial : atlasData.AtlasInMaterials.First();
                Material generateMat = GenerateAtlasMat(mergeMat, atlasTexture, shaderSupport, AtlasSetting.ForceSetTexture);
                var matGroupGenerate = AtlasSetting.MaterialMergeGroups.ToDictionary(m => m, m => GenerateAtlasMat(m.MergeReferenceMaterial, atlasTexture, shaderSupport, AtlasSetting.ForceSetTexture));

                domain.ReplaceMaterials(atlasData.AtlasInMaterials.ToDictionary(x => x, m => FindGroup(m)), false);

                Material FindGroup(Material material)
                {
                    foreach (var matGroup in AtlasSetting.MaterialMergeGroups)
                    {
                        var index = matGroup.GroupMaterials.FindIndex(m => domain.OriginEqual(m, material));
                        if (index != -1) { return matGroupGenerate[matGroup]; }
                    }
                    return generateMat;
                }
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

            foreach (var atlasMeshHolder in atlasData.Meshes) { UnityEngine.Object.DestroyImmediate(atlasMeshHolder.NormalizedMesh); }
        }

        internal static void DefaultRefCopyTuning(Dictionary<string, TexFineTuningHolder> atlasTexFineTuningTargets, Dictionary<string, List<string>> referenceCopyDict)
        {
            if (referenceCopyDict == null) { return; }

            foreach (var fineTuning in referenceCopyDict.Select(i => new ReferenceCopy(new(i.Key), i.Value.Select(i => new PropertyName(i)).ToList())))
            {
                fineTuning.AddSetting(atlasTexFineTuningTargets);
            }
        }
        internal static void DefaultMargeTextureDictTuning(Dictionary<string, TexFineTuningHolder> atlasTexFineTuningTargets, Dictionary<string, List<string>> margeTextureDict)
        {
            if (margeTextureDict == null) { return; }

            foreach (var fineTuning in margeTextureDict.Select(i => new MergeTexture(new(i.Key), i.Value.Select(i => new PropertyName(i)).ToList())))
            {
                fineTuning.AddSetting(atlasTexFineTuningTargets);
            }

        }
        internal static void SetSizeDataMaxSize(Dictionary<string, TexFineTuningHolder> atlasTexFineTuningTargets, Dictionary<string, int> sourceTextureMaxSize)
        {
            foreach (var texMax in sourceTextureMaxSize)
            {
                if (texMax.Key == "_MainTex") { continue; }
                var sizeData = atlasTexFineTuningTargets[texMax.Key].Get<SizeData>();
                sizeData.TextureSize = texMax.Value;
            }
        }

        internal List<Renderer> GetTargetAllowedFilter(IEnumerable<Renderer> domainRenderers) { return domainRenderers.Where(i => AtlasAllowedRenderer(i, AtlasSetting.IncludeDisabledRenderer)).ToList(); }
        private void TransMoveRectIsland<TIslandRect>(Texture sourceTex, RenderTexture targetRT, Dictionary<Island, TIslandRect> notAspectIslandPairs, float uvScalePadding) where TIslandRect : IIslandRect
        {
            uvScalePadding *= 0.5f;
            using (var sUV = new NativeArray<Vector2>(notAspectIslandPairs.Count * 4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
            using (var tUV = new NativeArray<Vector2>(notAspectIslandPairs.Count * 4, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
            using (var triangles = new NativeArray<TriangleIndex>(notAspectIslandPairs.Count * 2, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
            {
                var triSpan = triangles.AsSpan();
                var sUVSpan = sUV.AsSpan();
                var tUVSpan = tUV.AsSpan();

                var triIndex = 0;
                var vertIndex = 0;
                foreach (var islandPair in notAspectIslandPairs)
                {
                    var (Origin, Moved) = (islandPair.Key, islandPair.Value);
                    var rectScalePadding = Moved.UVScaleToRectScale(uvScalePadding);

                    var originVertexes = Origin.GenerateRectVertexes(rectScalePadding);
                    var movedVertexes = Moved.GenerateRectVertexes(rectScalePadding);

                    triSpan[triIndex] = new(vertIndex + 0, vertIndex + 1, vertIndex + 2);
                    triSpan[triIndex + 1] = new(vertIndex + 0, vertIndex + 2, vertIndex + 3);
                    triIndex += 2;

                    foreach (var v in originVertexes.Zip(movedVertexes, (o, m) => (o, m)))
                    {
                        sUVSpan[vertIndex] = v.o;
                        tUVSpan[vertIndex] = v.m;
                        vertIndex += 1;
                    }
                }

                TransTexture.ForTrans(targetRT, sourceTex, new TransData<Vector2>(triangles, tUV, sUV), argTexWrap: TextureWrap.Loop);
            }
        }

        private static Material GenerateAtlasMat(Material targetMat, Dictionary<string, Texture2D> atlasTex, AtlasShaderSupportUtils shaderSupport, bool forceSetTexture)
        {
            var supporter = shaderSupport.GetAtlasShaderSupporter(targetMat);
            var editableTMat = UnityEngine.Object.Instantiate(targetMat);

            foreach (var texKV in atlasTex)
            {
                if (supporter.IsConstraintValid(targetMat, texKV.Key) is false) { continue; }
                var tex = editableTMat.GetTexture(texKV.Key);

                if (forceSetTexture is false && tex == null) { continue; }
                if (tex is not Texture2D && tex is not RenderTexture) { continue; }
                if (tex is RenderTexture rt && TTRt.IsTemp(rt) is false) { continue; }

                editableTMat.SetTexture(texKV.Key, texKV.Value);
            }

            foreach (var postProcess in supporter.AtlasMaterialPostProses) { postProcess.Proses(editableTMat); }

            return editableTMat;
        }



        internal static List<Renderer> FilteredRenderers(GameObject targetRoot, bool includeDisabledRenderer)
        {
            return targetRoot.GetComponentsInChildren<Renderer>(true).Where(r => AtlasAllowedRenderer(r, includeDisabledRenderer)).ToList();
        }

        internal static bool AtlasAllowedRenderer(Renderer item, bool includeDisabledRenderer)
        {
            if (includeDisabledRenderer is false) { if (item.gameObject.activeInHierarchy is false) { return false; } }
            if (item.tag == "EditorOnly") return false;
            if (item.GetMesh() == null) return false;
            if (item.GetMesh().uv.Any() == false) return false;
            if (item.sharedMaterials.Length == 0) return false;
            if (item.sharedMaterials.Any(Mat => Mat == null)) return false;
            if (item.GetComponent<AtlasExcludeTag>() != null) return false;

            return true;
        }
        internal override IEnumerable<Renderer> ModificationTargetRenderers(IEnumerable<Renderer> domainRenderers, OriginEqual replaceTracking)
        {
            var nowContainsMatSet = new HashSet<Material>(RendererUtility.GetMaterials(GetTargetAllowedFilter(domainRenderers)).Where(i => i != null));
            var targetMaterials = nowContainsMatSet.Where(mat => SelectMatList.Any(smat => replaceTracking(smat.Material, mat))).ToHashSet();
            return domainRenderers.Where(r => r.sharedMaterials.Any(targetMaterials.Contains));
        }
    }
}
