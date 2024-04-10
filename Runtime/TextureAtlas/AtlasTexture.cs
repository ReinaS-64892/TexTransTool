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
using Unity.Profiling;
using UnityEngine.Profiling;

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
        public AtlasSetting AtlasSetting = new AtlasSetting();

        internal override bool IsPossibleApply => TargetRoot != null;

        internal override List<Renderer> GetRenderers => Renderers;

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
            Profiler.BeginSample("AtlasData and FindRenderers");
            var texManage = domain.GetTextureManager();
            atlasData = new AtlasData();


            //情報を集めるフェーズ
            var NowContainsMatSet = new HashSet<Material>(RendererUtility.GetMaterials(Renderers));
            var targetMaterials = NowContainsMatSet.Distinct().Where(i => i != null)
            .Where(mat => SelectMatList.Any(smat => domain.OriginEqual(smat.Material, mat))).ToList();

            atlasData.AtlasInMaterials = targetMaterials;
            var atlasSetting = AtlasSetting;
            var propertyBakeSetting = atlasSetting.MergeMaterials ? atlasSetting.PropertyBakeSetting : PropertyBakeSetting.NotBake;
            Profiler.EndSample();

            Profiler.BeginSample("AtlasContext:ctor");
            var atlasContext = new AtlasContext(targetMaterials, Renderers, propertyBakeSetting != PropertyBakeSetting.NotBake);
            Profiler.EndSample();

            var islandSizeOffset = GetTextureSizeOffset(SelectMatList.Select(i => i.Material), atlasSetting);

            //アイランドまわり
            if (atlasSetting.PixelNormalize)
            {
                Profiler.BeginSample("AtlasReferenceData:PixelNormalize");
                foreach (var islandKV in atlasContext.IslandDict)
                {
                    var material = atlasContext.MaterialToAtlasShaderTexDict[atlasContext.MaterialGroup[islandKV.Key.MaterialGroupID].First()];
                    var refTex = material.TryGetValue("_MainTex", out var tex2D) ? tex2D.Texture2D : null;
                    if (refTex == null) { continue; }
                    foreach (var island in islandKV.Value)
                    {
                        island.Pivot.y = Mathf.Round(island.Pivot.y * refTex.height) / refTex.height;
                        island.Pivot.x = Mathf.Round(island.Pivot.x * refTex.width) / refTex.width;
                    }
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
                var targets = SelectMatList.Where(i => group.Any(m => domain.OriginEqual(m, i.Material)));

                var materialFineTuningValue = 0f;
                var count = 0;
                foreach (var selector in targets)
                {
                    materialFineTuningValue += selector.MaterialFineTuningValue;
                    count += 1;
                }
                materialFineTuningValue /= count;
                sizePriority[i] = materialFineTuningValue;
            }

            foreach (var islandFineTuner in atlasSetting.IslandFineTuners) { islandFineTuner.IslandFineTuning(sizePriority, islandArray, islandDescription, domain); }
            for (var i = 0; sizePriority.Length > i; i += 1) { sizePriority[i] = Mathf.Clamp01(sizePriority[i]); }
            Profiler.EndSample();


            IAtlasIslandRelocator relocator = atlasSetting.AtlasIslandRelocator != null ? UnityEngine.Object.Instantiate(atlasSetting.AtlasIslandRelocator) : new NFDHPlasFC();
            relocator.Padding = atlasSetting.IslandPadding;

            Profiler.BeginSample("Relocation");
            var relocatedRect = RelocateLoop(rectArray, sizePriority, relocator, atlasSetting.ForceSizePriority, atlasSetting.IslandPadding);
            Profiler.EndSample();

            var rectTangleMove = relocator.RectTangleMove;

            if (relocator is UnityEngine.Object unityObject) { DestroyImmediate(unityObject); }

            if (atlasSetting.PixelNormalize)
            {
                Profiler.BeginSample("AtlasReferenceData:PixelNormalize");
                for (var i = 0; relocatedRect.Length > i; i += 1)
                {
                    var island = relocatedRect[i];
                    island.Pivot.y = Mathf.Round(island.Pivot.y * atlasSetting.AtlasTextureSize) / atlasSetting.AtlasTextureSize;
                    island.Pivot.x = Mathf.Round(island.Pivot.x * atlasSetting.AtlasTextureSize) / atlasSetting.AtlasTextureSize;
                    relocatedRect[i] = island;
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

            // var areaSum = IslandRectUtility.CalculateAllAreaSum(islandRectPool.Values);
            // Debug.Log(areaSum + ":AreaSum" + "-" + height + ":height");



            //新しいUVを持つMeshを生成するフェーズ
            Profiler.BeginSample("MeshCompile");
            var compiledMeshes = new List<AtlasData.AtlasMeshAndDist>();
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
                if (AtlasSetting.WriteOriginalUV) { newMesh.SetUVs(1, meshData.VertexUV); }

                compiledMeshes.Add(new AtlasData.AtlasMeshAndDist(distMesh, newMesh, subSet.Select(i => i?.MaterialGroupID ?? -1).ToArray()));
            }
            atlasData.Meshes = compiledMeshes;
            Profiler.EndSample();

            //アトラス化したテクスチャーを生成するフェーズ
            var compiledAtlasTextures = new Dictionary<string, AsyncTexture2D>();

            Profiler.BeginSample("GetGroupedTextures");
            var groupedTextures = GetGroupedTextures(atlasContext, propertyBakeSetting, out var containsProperty);
            Profiler.EndSample();

            Dictionary<int, Dictionary<string, RenderTexture>> GetGroupedTextures(AtlasContext atlasContext, PropertyBakeSetting propertyBakeSetting, out HashSet<string> property)
            {
                switch (propertyBakeSetting)
                {
                    default: { property = null; return null; }
                    case PropertyBakeSetting.NotBake:
                        {
                            var dict = atlasContext.MaterialGroup
                                .Select(mg => (Array.IndexOf(atlasContext.MaterialGroup, mg), mg.Select(m => atlasContext.MaterialToAtlasShaderTexDict[m])))
                                .Select(mg => (mg.Item1, ZipDictAndOffset(mg.Item2)))
                                .ToDictionary(i => i.Item1, i => i.Item2);

                            property = new HashSet<string>(dict.SelectMany(i => i.Value.Keys));
                            return dict;

                            Dictionary<string, RenderTexture> ZipDictAndOffset(IEnumerable<Dictionary<string, AtlasShaderTexture2D>> keyValuePairs)
                            {
                                var dict = new Dictionary<string, RenderTexture>();
                                foreach (var kv in keyValuePairs.SelectMany(i => i).GroupBy(i => i.Key))
                                {
                                    if (kv.Any(i => i.Value.Texture2D != null) == false) { continue; }
                                    var atlasTex = kv.First(i => i.Value.Texture2D != null).Value;
                                    if (atlasTex.TextureScale == Vector2.zero && atlasTex.TextureTranslation == Vector2.zero)
                                    {
                                        dict[kv.Key] = texManage.GetOriginTempRt(atlasTex.Texture2D);
                                    }
                                    else
                                    {
                                        var tex = atlasTex.Texture2D;

                                        var originTex = texManage.GetOriginTempRt(tex);
                                        originTex.ApplyTextureST(atlasTex.TextureScale, atlasTex.TextureTranslation);

                                        dict[kv.Key] = originTex;
                                    }
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
                                    .Where(i => PropertyBakeSetting.BakeAllProperty == propertyBakeSetting || i.Any(st => st.Value.Texture2D != null))
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
                                    var sTex = atlasTex?.Texture2D != null ? texManage.GetOriginTempRt(atlasTex.Texture2D) : null;

                                    if (sTex != null) { sTex.ApplyTextureST(atlasTex.TextureScale, atlasTex.TextureTranslation); }
                                    var bakedTex = sTex != null ? sTex.CloneTemp() : RenderTexture.GetTemporary(2, 2);

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

                                    texDict[propName] = bakedTex;

                                    if (sTex != null) { RenderTexture.ReleaseTemporary(sTex); }
                                    tmpMat.AllPropertyReset();
                                    tmpMat.shaderKeywords = Array.Empty<string>();

                                }


                            }

                            return groupDict;
                        }
                }
            }



            Profiler.BeginSample("Texture synthesis");
            foreach (var propName in containsProperty)
            {
                var targetRT = RenderTexture.GetTemporary(atlasSetting.AtlasTextureSize, atlasSetting.AtlasTextureSize, 32);
                targetRT.Clear();
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
                    var heightClampRt = RenderTexture.GetTemporary(atlasSetting.AtlasTextureSize, atlasTextureHeightSize);
                    Graphics.CopyTexture(targetRT, 0, 0, 0, 0, heightClampRt.width, heightClampRt.height, heightClampRt, 0, 0, 0, 0);
                    RenderTexture.ReleaseTemporary(targetRT);
                    targetRT = heightClampRt;
                }

                Profiler.BeginSample("Readback");
                compiledAtlasTextures.Add(propName, new AsyncTexture2D(targetRT));
                Profiler.EndSample();

                RenderTexture.ReleaseTemporary(targetRT);
            }
            Profiler.EndSample();
            foreach (var kv in groupedTextures.Values) { foreach (var tex in kv) { RenderTexture.ReleaseTemporary(tex.Value); } }
            groupedTextures = null;

            Profiler.BeginSample("Async Readback");
            atlasData.Textures = compiledAtlasTextures.ToDictionary(kv => kv.Key, kv => kv.Value.GetTexture2D());
            Profiler.EndSample();

            atlasData.MaterialID = atlasContext.MaterialGroup.Select(i => i.ToHashSet()).ToArray();
            atlasContext.Dispose();
            foreach (var movedUV in subSetMovedUV) { movedUV.Dispose(); }

            return true;

        }

        private static IslandRect[] RelocateLoop(IslandRect[] rectArray, float[] sizePriority, IAtlasIslandRelocator relocator, bool forceSizePriority, float padding)
        {
            var relocatedRect = rectArray.ToArray();
            var refRect = rectArray;

            if (forceSizePriority)
            {
                refRect = GetPriorityMinSizeRectArray(rectArray, sizePriority);
                if (relocator.Relocation(refRect)) { return refRect; }
            }
            else
            {
                if (relocator.Relocation(relocatedRect)) { return relocatedRect; }

                if (sizePriority.Any(f => !Mathf.Approximately(1, f)))
                {
                    Profiler.BeginSample("Priority");
                    var priorityMinSize = GetPriorityMinSizeRectArray(rectArray, sizePriority);

                    for (var lerpValue = 1f; 0 < lerpValue; lerpValue -= 0.05f)
                    {
                        LerpPriority(relocatedRect, priorityMinSize, rectArray, lerpValue);
                        if (relocator.Relocation(relocatedRect)) { Profiler.EndSample(); return relocatedRect; }
                    }
                    refRect = priorityMinSize;
                    Profiler.EndSample();
                }
            }



            Profiler.BeginSample("Expand");

            var refRectSize = IslandRectUtility.CalculateAllAreaSum(refRect);

            var lastRelocated = relocatedRect.ToArray();

            for (var size = 0.5f; size >= 0; size -= 0.01f)
            {
                for (var i = 0; rectArray.Length > i; i += 1) { relocatedRect[i] = refRect[i]; }
                ScaleApplyDown(relocatedRect, Mathf.Sqrt(size / refRectSize));

                if (ExpandLoop(relocator, relocatedRect, lastRelocated)) { break; }
            }

            Profiler.EndSample();

            return lastRelocated;


            static void LerpPriority(IslandRect[] write, IslandRect[] min, IslandRect[] max, float lerpValue)
            {
                for (var i = 0; max.Length > i; i += 1)
                {
                    var size = Vector3.Lerp(min[i].Size, max[i].Size, lerpValue);
                    if (write[i].Is90Rotation) { (size.x, size.y) = (size.y, size.x); }
                    write[i].Size = size;
                }
            }

            bool ExpandLoop(IAtlasIslandRelocator relocator, IslandRect[] relocatedRect, IslandRect[] lastRelocated)
            {
                var safetyCount = 0;
                if (!relocator.Relocation(relocatedRect)) { return false; }
                while (relocator.Relocation(relocatedRect) && safetyCount < 2048)//失敗するかセーフティにかかるまで、続けて失敗したら前回の物を使用する方針
                {
                    safetyCount += 1;
                    for (int i = 0; relocatedRect.Length > i; i++) { lastRelocated[i] = relocatedRect[i]; }
                    ScaleApplyUp(relocatedRect, 1.01f);
                }
                return true;
            }

            static void ScaleApplyDown(IslandRect[] rect, float scaleDownStep)
            {
                for (int i = 0; rect.Length > i; i++)
                {
                    rect[i].Size *= scaleDownStep;
                }
            }
            void ScaleApplyUp(IslandRect[] rect, float scaleUpStep)
            {
                for (int i = 0; rect.Length > i; i++)
                {
                    var size = rect[i].Size *= scaleUpStep;


                    var maxLength = 0.99f - padding - padding;
                    if (size.x > maxLength || size.y > maxLength)//一つ大きいのがあるとすべて使いきれなくなってしまうために、これは必要。
                    {
                        var max = Mathf.Max(size.x, size.y);
                        size *= maxLength / max;
                        rect[i].Size = size;
                    }
                }
            }


        }

        private static IslandRect[] GetPriorityMinSizeRectArray(IslandRect[] rectArray, float[] sizePriority)
        {
            var priorityMinSize = rectArray.ToArray();
            for (var i = 0; priorityMinSize.Length > i; i += 1) { priorityMinSize[i].Size *= sizePriority[i]; }

            return priorityMinSize;
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

            domain.ProgressStateEnter("AtlasTexture");
            domain.ProgressUpdate("CompileAtlasTexture", 0f);

            Profiler.BeginSample("TryCompileAtlasTextures");
            if (!TryCompileAtlasTextures(domain, out var atlasData)) { Profiler.EndSample(); return; }
            Profiler.EndSample();

            domain.ProgressUpdate("MeshChange", 0.5f);

            var nowRenderers = Renderers;

            Profiler.BeginSample("AtlasShaderSupportUtils:ctor");
            var shaderSupport = new AtlasShaderSupportUtils();
            Profiler.EndSample();

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
                var matGroupGenerate = AtlasSetting.MaterialMargeGroups.ToDictionary(m => m, m => GenerateAtlasMat(m.MargeReferenceMaterial, atlasTexture, shaderSupport, AtlasSetting.ForceSetTexture));

                domain.ReplaceMaterials(atlasData.AtlasInMaterials.ToDictionary(x => x, m => FindGroup(m)), true);

                Material FindGroup(Material material)
                {
                    foreach (var matGroup in AtlasSetting.MaterialMargeGroups)
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
                domain.ReplaceMaterials(materialMap, true);
            }

            domain.ProgressUpdate("End", 1);
            domain.ProgressStateExit();
        }

        private void TransMoveRectIsland<TIslandRect>(Texture souseTex, RenderTexture targetRT, Dictionary<Island, TIslandRect> notAspectIslandPairs, float uvScalePadding) where TIslandRect : IIslandRect
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

                TransTexture.ForTrans(targetRT, souseTex, new TransData<Vector2>(triangles, tUV, sUV), argTexWrap: TextureWrap.Loop);
            }
        }

        private static Material GenerateAtlasMat(Material targetMat, List<PropAndTexture2D> atlasTex, AtlasShaderSupportUtils shaderSupport, bool forceSetTexture)
        {
            var editableTMat = UnityEngine.Object.Instantiate(targetMat);

            editableTMat.SetTextures(atlasTex, forceSetTexture);
            var supporter = shaderSupport.GetAtlasShaderSupporter(editableTMat);

            foreach (var postProcess in supporter.AtlasMaterialPostProses) { postProcess.Proses(editableTMat); }

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
