#nullable enable
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransCore.UVIsland;
using Vector2Sys = System.Numerics.Vector2;
using net.rs64.TexTransCore;
using System.Runtime.InteropServices;
using Unity.Collections;
using net.rs64.TexTransCoreEngineForUnity;
using net.rs64.TexTransCore.AtlasTexture;

namespace net.rs64.TexTransTool.TextureAtlas
{
    /*
        memo
        マテリアルグループという概念
        衝突しないテクスチャーを持っているのであれば、同一扱いしないと無駄に VRAM を消費してしまう。(例えば Hair と Hair Transparent などや 同一テクスチャーを使用しているが _Color 違い(PropertyBake が無効なとき) など)

        AtlasSubData の取り回し (今の名前は AtlasSubMeshIndexID)
        同一のメッシュを複数のレンダラーが使用していたとき、同じメッシュ、同じのサブメッシュIndex 、同一のマテリアルグループの組み合わせになるのであればそれは同一の存在として扱うが、一部のスロットだけ違うマテリアルグループを使用している場合に、メッシュは別になるが同一の部分だけは同じものでという扱いをし、 Island をマージしないと VRAM にムダが生じる。
        Mesh + SubMeshIndex + MaterialGroupID の3つで同一性が判断でき、これを AtlasSubData と私は表記している。
        このユニークな AtlasSubData の部分だけ、 UVToIsland が行われる。同一メッシュ + 同一 SubMEshIndex であったとしてもマテリアルグループが違うと、同じ メッシュ の 同じ subMesh の Island が作られて別の領域が割り当てられるようになっていい感じ。

        Mesh の Normalize
        - サブメッシュを超えて同一頂点を使用している場合に別の頂点を使用させるように補正(頂点数の若干の増加は無視します。)
        - マテリアルスロットがサブメッシュより多いときに最後のサブメッシュをリピートする形で複製 (そうする他ないためポリゴン数の若干の増加は無視します。)

        Island の補正
        - 同一メッシュ内で  同じマテリアルグループ に属する場合
        - サブメッシュを超えて同一UV座標の頂点を持つ Island をマージする
        - (new) サブメッシュを超えていてもいなくても、領域が大きく重複している場合は Island をマージする


        AtlasSubData の Set
        AtlasSubData の並びとなり出力されるメッシュとっ対応する。
        だがここも雑にやってしまうと無意味にメッシュを増やす。
        マテリアルが
        - ABC
        - ABCDE
        などの場合、サブメッシュのほうが大きい分には描画されないだけであるため、その Set を大きい方 (今回は ABCDE) に寄せたほうが VRAM が若干安く済む。
        なので完全に同じ set や同一扱いできる場合は大きい方に寄せる必要がある。

        その set に Null が含まれうるのはすべてのマテリアルがターゲットになるわけではないから。
    */
    internal class AtlasContext : IDisposable
    {
        public MaterialGroupingContext MaterialGroupingCtx;
        public AtlasMeshSourceContext NormalizedMeshCtx;
        public AtlasSubMeshIndexIDSetContext AtlasSubMeshIndexSetCtx;
        public AtlasIslandContext AtlasIslandCtx;


        public IslandTransform[] SourceVirtualIslands;
        public Dictionary<IslandTransform, List<Island>> ReverseSourceVirtualIsland2OriginIslands;
        public Dictionary<IslandTransform, IslandReferences> SourceVirtualIsland2OriginRefaces;
        public Dictionary<IslandTransform, Texture?> SourceVirtualIslands2PrimaryTexture;
        public class AtlasContextOption
        {
            public string? PrimaryTexturePropertyOrMaximum = "_MainTex";
            public UVChannel AtlasTargetUVChannel = UVChannel.UV0;

            public AtlasIslandContextOption AtlasIslandContextOption = new();
        }
        public class AtlasIslandContextOption
        {
            public bool CrossSubMeshUsedIslandMerge = true;
            public bool OverCrossIslandMerge = true;
        }
        public AtlasContext(
            IRendererTargeting targeting
            , Renderer[] targetRenderers
            , HashSet<Material> targetMaterials
            , AtlasContextOption atlasContextOption
            )
        {
            MaterialGroupingCtx = new(targetMaterials, atlasContextOption.AtlasTargetUVChannel, atlasContextOption.PrimaryTexturePropertyOrMaximum);
            NormalizedMeshCtx = new(targeting, targetRenderers, atlasContextOption.AtlasTargetUVChannel);
            AtlasSubMeshIndexSetCtx = new(targeting, targetRenderers, targetMaterials, NormalizedMeshCtx, MaterialGroupingCtx);
            AtlasIslandCtx = new(AtlasSubMeshIndexSetCtx.AtlasSubMeshIndexIDHash, NormalizedMeshCtx.GetMeshDataFromMeshID, atlasContextOption.AtlasIslandContextOption);


            SourceVirtualIslands = AtlasIslandCtx.Origin2VirtualIsland.Values.Distinct().ToArray();


            ReverseSourceVirtualIsland2OriginIslands = new Dictionary<IslandTransform, List<Island>>();
            foreach (var kv in AtlasIslandCtx.Origin2VirtualIsland)
            {
                var origin = kv.Key;
                var virtualIsland = kv.Value;

                if (ReverseSourceVirtualIsland2OriginIslands.TryGetValue(virtualIsland, out var originIslands) is false)
                { originIslands = ReverseSourceVirtualIsland2OriginIslands[virtualIsland] = new(); }
                originIslands.Add(origin);
            }
            SourceVirtualIsland2OriginRefaces = ReverseSourceVirtualIsland2OriginIslands.ToDictionary(
                kv => kv.Key,
                kv =>
                {
                    var originIslands = kv.Value;
                    return new IslandReferences(
                        originIslands
                        , originIslands.SelectMany(i =>
                        {
                            var uvVertexes = NormalizedMeshCtx.GetMeshDataFromMeshID(AtlasIslandCtx.ReverseOriginDict[i].MeshID).VertexUV;
                            return i.Triangles.Select(i => i.ToTriangle2D(MemoryMarshal.Cast<Vector2, Vector2Sys>(uvVertexes.AsSpan())));
                        }).ToArray());
                }
            );

            SourceVirtualIslands2PrimaryTexture = new Dictionary<IslandTransform, Texture?>();
            foreach (var virtualIslands in SourceVirtualIslands)
            {
                var sourceMaterialID = ReverseSourceVirtualIsland2OriginIslands[virtualIslands]
                    .Select(i => AtlasIslandCtx.ReverseOriginDict[i])
                    .First();
                var primaryTex = MaterialGroupingCtx.GetPrimaryTexture(sourceMaterialID.MaterialGroupID);
                SourceVirtualIslands2PrimaryTexture[virtualIslands] = primaryTex;
            }
        }
        public void SourceVirtualIslandNormalize()
        {
            for (var i = 0; SourceVirtualIslands.Length > i; i += 1)
            {
                var vIsland = SourceVirtualIslands[i];
                var primaryTex = SourceVirtualIslands2PrimaryTexture[vIsland];
                if (primaryTex == null) { continue; }
                NormalizeIsland(vIsland, primaryTex.width, primaryTex.height);
            }
            static void NormalizeIsland(IslandTransform island, int width, int height)
            {
                var minPos = island.Position;
                var maxPos = island.GetNotRotatedMaxPos();
                island.Position = NormalizeMin(width, height, minPos);
                island.Size = NormalizeMax(width, height, maxPos) - island.Position;
            }
        }
        internal static Vector2Sys NormalizeMin(int width, int height, Vector2Sys vec)
        {
            vec.Y = Mathf.Min(vec.Y * height) / height;
            vec.X = Mathf.Min(vec.X * width) / width;
            return vec;
        }

        internal static Vector2Sys NormalizeMax(int width, int height, Vector2Sys vec)
        {
            vec.Y = Mathf.Max(vec.Y * height) / height;
            vec.X = Mathf.Max(vec.X * width) / width;
            return vec;
        }
        internal struct IslandReferences
        {
            public List<Island> origins;
            public Triangle2D[] mergedTriangles;

            public IslandReferences(List<Island> origins, Triangle2D[] mergedTriangles)
            {
                this.origins = origins;
                this.mergedTriangles = mergedTriangles;
            }
        }
        public void Dispose()
        {
            NormalizedMeshCtx.Dispose();
        }
        internal void PrimaryTextureSizeScaling(IslandTransform[] virtualIslandArray, Vector2Int atlasTargeSize)
        {
            for (var i = 0; virtualIslandArray.Length > i; i += 1)
            {
                var refTex = SourceVirtualIslands2PrimaryTexture[SourceVirtualIslands[i]];
                if (refTex != null)
                {
                    var scaling = (float)refTex.width / atlasTargeSize.x;
                    virtualIslandArray[i].Size *= scaling;

                    if (refTex.width != refTex.height)
                    {
                        var aspect = refTex.height / (float)refTex.width;
                        virtualIslandArray[i].Size.Y *= aspect;
                    }
                }
                else
                {
                    virtualIslandArray[i].Size *= 0.01f;
                }
            }
        }

        internal Mesh[] GenerateAtlasedMesh(
            AtlasSetting atlasSetting
            , Dictionary<IslandTransform, IslandTransform> source2MovedVirtualIsland
            , Vector2Int atlasedTextureSize
            )
        {
            // var normMeshes = atlasContext.Meshes.Select(m => atlasContext.NormalizeMeshes[m]).ToArray();
            var aspectScale = 1 / (atlasedTextureSize.y / (float)atlasedTextureSize.x);// 高さの比率を逆数にして 縦長だったら小さくなるような感じになる。
            var atlasSubSets = AtlasSubMeshIndexSetCtx.AtlasSubSets;
            var writeDefaultUVChannel = (int)atlasSetting.AtlasTargetUVChannel;
            var compiledMeshes = new Mesh[atlasSubSets.Count];

            if (atlasSetting.WriteOriginalUV)
                if (writeDefaultUVChannel == Math.Clamp(atlasSetting.OriginalUVWriteTargetChannel, 0, 7)) { TexTransCore.TTLog.Error("AtlasTexture:warn:OriginalUVWriteTargetForAtlasTargetUV"); }

            for (int subSetIndex = 0; compiledMeshes.Length > subSetIndex; subSetIndex += 1)
            {
                var subSet = atlasSubSets[subSetIndex];
                var firstID = subSet.First();
                var meshID = firstID.HasValue ? firstID.Value.MeshID : -1;

                var nmMesh = NormalizedMeshCtx.MeshID2Normalized[meshID];
                var distMesh = NormalizedMeshCtx.Normalized2OriginMesh[nmMesh];
                var meshData = NormalizedMeshCtx.Normalized2MeshData[nmMesh];
                var newMesh = UnityEngine.Object.Instantiate<Mesh>(nmMesh);
                newMesh.name = "AtlasMesh_" + subSetIndex + "_" + nmMesh.name;

                var moveTargetIslands = subSet.Where(i => i.HasValue)
                     .Cast<AtlasSubMeshIndexID>()
                     .SelectMany(i => AtlasIslandCtx.OriginIslandDict[i])
                     .Select(island => (island, AtlasIslandCtx.Origin2VirtualIsland[island])).ToArray();

                var moveTargetIndexes = atlasedTextureSize.x != atlasedTextureSize.y ? new HashSet<int>() : null;

                var originalUV = meshData.VertexUV;
                using var movedUVNativeArray = new NativeArray<Vector2>(originalUV, Allocator.TempJob);
                var movedUV = movedUVNativeArray.AsSpan();

                foreach (var (island, sourceVirtualIsland) in moveTargetIslands)
                {
                    var movedVirtualIsland = source2MovedVirtualIsland[sourceVirtualIsland];

                    var sourceSize = sourceVirtualIsland.Size;
                    var movedSize = movedVirtualIsland.Size;

                    var relativeScale = NotNormalToZero(new(movedSize.X / sourceSize.X, movedSize.Y / sourceSize.Y));
                    static Vector2 NotNormalToZero(Vector2 relativeScale)
                    {
                        relativeScale.x = float.IsNormal(relativeScale.x) ? relativeScale.x : 0;
                        relativeScale.y = float.IsNormal(relativeScale.y) ? relativeScale.y : 0;
                        return relativeScale;
                    }

                    foreach (var triangleIndex in island.Triangles)
                    {
                        for (var i = 0; 3 > i; i += 1)
                        {
                            var relativePosition = originalUV[triangleIndex[i]].ToSysNum() - sourceVirtualIsland.Position;
                            relativePosition.X *= relativeScale.x;
                            relativePosition.Y *= relativeScale.y;

                            var movedUVPosition = movedVirtualIsland.Position + IslandTransform.RotateVector(relativePosition, movedVirtualIsland.Rotation);
                            movedUV[triangleIndex[i]] = movedUVPosition.ToUnity();
                            moveTargetIndexes?.Add(triangleIndex[i]);
                        }
                    }

                }

                if (moveTargetIndexes is not null)
                {
                    foreach (var vi in moveTargetIndexes)
                    {
                        var uvPos = movedUV[vi];
                        uvPos.y *= aspectScale;
                        movedUV[vi] = uvPos;
                    }
                }
                newMesh.SetUVs(writeDefaultUVChannel, movedUVNativeArray);

                if (atlasSetting.WriteOriginalUV)
                {
                    var writeTarget = Math.Clamp(atlasSetting.OriginalUVWriteTargetChannel, 0, 7);
                    if (newMesh.HasUV(writeTarget)) { TexTransCore.TTLog.Info("AtlasTexture:warn:OriginalUVWriteTargetForAlreadyUV", writeTarget, distMesh); }
                    newMesh.SetUVs(writeTarget, meshData.VertexUV);
                }

                compiledMeshes[subSetIndex] = newMesh;
                newMesh.UploadMeshData(false);
            }
            return compiledMeshes;
        }
        public Dictionary<string, ITTRenderTexture> GenerateAtlasedTextures<TTT4U>(
            TTT4U engine
            , AtlasSetting atlasSetting
            , Vector2Int atlasedTextureSize
            , bool IsRectangleMove
            , Dictionary<IslandTransform, IslandTransform> source2MovedVirtualIsland
        ) where TTT4U : ITexTransToolForUnity
        {
            //アトラス化したテクスチャーを生成するフェーズ
            var compiledAtlasTextures = new Dictionary<string, ITTRenderTexture>();

            using var groupedTextures = GetGroupedDiskOrRenderTextures(engine);
            var containsProperty = MaterialGroupingCtx.GetContainsAllProperties();

            var loadedDiskTextures = new Dictionary<ITTDiskTexture, ITTRenderTexture>();
            try
            {
                foreach (var propName in containsProperty)
                {
                    var targetRT = engine.CreateRenderTexture(atlasedTextureSize.x, atlasedTextureSize.y);
                    engine.ColorFill(targetRT, atlasSetting.BackGroundColor.ToTTCore());

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
                        if (IsRectangleMove)
                        {
                            var findSubIDHash = AtlasSubMeshIndexSetCtx.AtlasSubMeshIndexIDHash
                                                    .Where(i => i.MaterialGroupID == findMaterialID).ToHashSet();

                            var drawTargetSourceVirtualIslandsHash = new HashSet<IslandTransform>();
                            foreach (var subID in findSubIDHash)
                            {
                                drawTargetSourceVirtualIslandsHash.UnionWith(
                                    AtlasIslandCtx.OriginIslandDict[subID]
                                        .Select(i => AtlasIslandCtx.Origin2VirtualIsland[i])
                                    );
                            }
                            var drawTargetSourceVirtualIslands = drawTargetSourceVirtualIslandsHash.ToArray();
                            var drawTargetMovedVirtualIslands = drawTargetSourceVirtualIslands.Select(i => source2MovedVirtualIsland[i]).ToArray();


                            AtlasingUtility.TransMoveRectangle(engine
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
                    compiledAtlasTextures.Add(propName, targetRT);
                }
            }
            finally
            {
                foreach (var dt in loadedDiskTextures.Values) { dt.Dispose(); }
            }
            return compiledAtlasTextures;
        }
        internal GroupedDiskOrRenderTextures GetGroupedDiskOrRenderTextures(ITexTransToolForUnity texTransToolForUnity)
        {
            var groupedTextures = new Dictionary<int, Dictionary<string, ITTTexture>>();
            var materialGroup = MaterialGroupingCtx.GroupMaterials;

            for (var i = 0; materialGroup.Length > i; i += 1)
            {
                groupedTextures[i] = materialGroup[i].GroupedTexture
                    .Where(i => i.Value != null)
                    .Cast<KeyValuePair<string, Texture>>()
                    .ToDictionary(
                        kv => kv.Key,
                        kv => texTransToolForUnity.WrappingOrUpload(kv.Value)
                    );
            }
            return new(groupedTextures);
        }

        internal class GroupedDiskOrRenderTextures : IDisposable
        {
            public Dictionary<int, Dictionary<string, ITTTexture>> GroupedTextures;

            public GroupedDiskOrRenderTextures(Dictionary<int, Dictionary<string, ITTTexture>> groupedTextures)
            {
                GroupedTextures = groupedTextures;
            }

            public void Dispose()
            {
                foreach (var g in GroupedTextures)
                    foreach (var texKV in g.Value)
                        texKV.Value.Dispose();

                GroupedTextures.Clear();
            }
        }
    }


}
