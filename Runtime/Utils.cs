#if UNITY_EDITOR
using System.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Decal;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace net.rs64.TexTransTool
{
    public static class Utils
    {
        public const int ThiSaveDataVersion = 0;
        public static void ForEach2D(Vector2Int Range, Action<int, int> action)
        {
            int countX = 0;
            int countyY = 0;
            while (true)
            {
                if (!(Range.x > countX))
                {
                    countX = 0;
                    countyY += 1;
                }
                if (!(Range.y > countyY))
                {
                    break;
                }

                action.Invoke(countX, countyY);

                countX += 1;
            }
        }
        public static List<Vector2Int> Range2d(Vector2Int Range)
        {
            var list = new List<Vector2Int>();
            ForEach2D(Range, (x, y) => list.Add(new Vector2Int(x, y)));
            return list;
        }

        public static int TwoDToOneDIndex(Vector2Int TowDIndex, int Size)
        {
            return (TowDIndex.y * Size) + TowDIndex.x;
        }
        public static Vector2Int ConvertIndex2D(int Index1D, int width)
        {
            return new Vector2Int(Index1D % width, Index1D / width);
        }
        public static Texture2D CreateFillTexture(int Size, Color FillColor)
        {
            return CreateFillTexture(new Vector2Int(Size, Size), FillColor);
        }
        public static Texture2D CreateFillTexture(Vector2Int Size, Color FillColor)
        {
            var TestTex = new Texture2D(Size.x, Size.y);
            TestTex.SetPixels(FilledArray(FillColor, Size.x * Size.y));
            return TestTex;
        }
        /// <summary>
        /// いろいろな設定をコピーしたような感じにする。
        /// ただしリサイズだけは行わない。
        /// 戻り値はクローンになる可能性があるため注意。
        /// ならない場合もあるため注意。
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="CopySouse"></param>
        /// <returns></returns>
        public static Texture2D CopySetting(this Texture2D tex, Texture2D CopySouse, SortedList<int, Color[]> MipMap = null)
        {
            var TextureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(CopySouse)) as TextureImporter;
            if (TextureImporter != null && TextureImporter.textureType == TextureImporterType.NormalMap) tex = tex.ConvertNormalMap();
            if (tex.mipmapCount > 1 != CopySouse.mipmapCount > 1)
            {
                var newTex = new Texture2D(tex.width, tex.height, tex.format, CopySouse.mipmapCount > 1);
                newTex.SetPixels32(tex.GetPixels32());
                newTex.name = tex.name;
                tex = newTex;
            }
            tex.filterMode = CopySouse.filterMode;
            tex.anisoLevel = CopySouse.anisoLevel;
            tex.alphaIsTransparency = CopySouse.alphaIsTransparency;
            tex.requestedMipmapLevel = CopySouse.requestedMipmapLevel;
            tex.mipMapBias = CopySouse.mipMapBias;
            tex.wrapModeU = CopySouse.wrapModeU;
            tex.wrapModeV = CopySouse.wrapModeV;
            tex.wrapMode = CopySouse.wrapMode;
            if (tex.mipmapCount > 1)
            {
                if (MipMap != null) { tex.ApplyMip(MipMap); }
                else { tex.Apply(true); }
            }
            EditorUtility.CompressTexture(tex, CopySouse.format, TextureImporter == null ? 50 : TextureImporter.compressionQuality);

            return tex;
        }
        public static Texture2D ConvertNormalMap(this Texture2D tex)
        {
            throw new NotImplementedException();
        }
        public static T[] FilledArray<T>(T DefaultValue, int Length)
        {
            var array = new T[Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = DefaultValue;
            }
            return array;
        }
        public static List<TriangleIndex> ToList(int[] triangleIndexes)
        {
            var trianglesList = new List<TriangleIndex>();
            int count = 0;
            while (triangleIndexes.Length > count)
            {
                trianglesList.Add(new TriangleIndex(triangleIndexes[count], triangleIndexes[count += 1], triangleIndexes[count += 1]));
                count += 1;
            }
            return trianglesList;
        }
        public static List<List<TriangleIndex>> GetSubTriangle(this Mesh mesh)
        {
            var subMeshCount = mesh.subMeshCount;
            List<List<TriangleIndex>> subTriangles = new List<List<TriangleIndex>>(subMeshCount);

            for (int i = 0; i < subMeshCount; i++)
            {
                subTriangles.Add(mesh.GetSubTriangle(i));
            }
            return subTriangles;
        }
        public static List<TriangleIndex> GetSubTriangle(this Mesh mesh, int SubMesh)
        {
            return ToList(mesh.GetTriangles(SubMesh));
        }

        public static T[] TowDtoOneD<T>(T[,] SouseArray, Vector2Int Size)
        {
            T[] oneDArray = new T[Size.x * Size.y];
            foreach (var index in Utils.Range2d(Size))
            {
                oneDArray[TwoDToOneDIndex(index, Size.x)] = SouseArray[index.x, index.y];
            }
            return oneDArray;
        }
        public static T[,] OneDToTowD<T>(T[] SouseArray, Vector2Int Size)
        {
            T[,] towDArray = new T[Size.x, Size.y];
            int count = -1;
            foreach (var value in SouseArray)
            {
                count += 1;
                var index = ConvertIndex2D(count, Size.x);
                towDArray[index.x, index.y] = value;
            }
            return towDArray;
        }
        public static List<Material> GetMaterials(IEnumerable<Renderer> Renderers)
        {
            List<Material> matList = new List<Material>();
            foreach (var renderer in Renderers)
            {
                matList.AddRange(renderer.sharedMaterials);
            }
            return matList;
        }
        public static void SetMaterials(IEnumerable<Renderer> Renderers, IReadOnlyList<Material> Mat)
        {
            int startOffset = 0;
            foreach (var renderer in Renderers)
            {
                int takeLength = renderer.sharedMaterials.Length;
                renderer.sharedMaterials = Mat.Skip(startOffset).Take(takeLength).ToArray();
                startOffset += takeLength;
            }
        }
        public static void ChangeMaterialForRenderers(IEnumerable<Renderer> Renderer, IReadOnlyDictionary<Material, Material> MatPairs)
        {
            foreach (var renderer in Renderer)
            {
                var materials = renderer.sharedMaterials;
                var isEdit = false;
                foreach (var Index in Enumerable.Range(0, materials.Length))
                {
                    var distMat = materials[Index];
                    if (MatPairs.ContainsKey(distMat))
                    {
                        materials[Index] = MatPairs[distMat];
                        isEdit = true;
                    }
                }
                if (isEdit)
                {
                    renderer.sharedMaterials = materials;
                }
            }
        }
        public static void ChangeMaterialForRenderers(IEnumerable<Renderer> Renderers, Material target, Material set)
        {
            foreach (var renderer in Renderers)
            {
                var materials = renderer.sharedMaterials;
                var isEdit = false;
                foreach (var index in Enumerable.Range(0, materials.Length))
                {
                    var distMat = materials[index];
                    if (target == distMat)
                    {
                        materials[index] = set;
                        isEdit = true;
                    }
                }
                if (isEdit)
                {
                    renderer.sharedMaterials = materials;
                }
            }
        }
        public static void ChangeMaterialForSerializedProperty(IReadOnlyDictionary<Material, Material> MatMapping, GameObject targetRoot, Type[] IgnoreTypes = null)
        {
            var allComponent = targetRoot.GetComponentsInChildren<Component>();
            IEnumerable<Component> components;
            if (IgnoreTypes.Any())
            {
                var filteredComponents = new List<Component>(allComponent.Length);
                foreach (var component in allComponent)
                {
                    var type = component.GetType();
                    if (!IgnoreTypes.Any(J => J.IsAssignableFrom(type))) { filteredComponents.Add(component); }
                }
                components = filteredComponents;
            }
            else
            {
                components = allComponent;
            }

            foreach (var component in components)
            {
                var type = component.GetType();

                var serializeObj = new SerializedObject(component);
                var iter = serializeObj.GetIterator();
                while (iter.Next(true))
                {
                    var s_Obj = iter;
                    if (s_Obj.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (s_Obj.objectReferenceValue is Material mat && MatMapping.ContainsKey(mat))
                        {
                            s_Obj.objectReferenceValue = MatMapping[mat];
                            serializeObj.ApplyModifiedProperties();
                        }
                    }
                }
            }
        }
        public static List<Mesh> GetMeshes(IEnumerable<Renderer> renderers, bool NullInsertion = false)
        {
            List<Mesh> meshes = new List<Mesh>();
            foreach (var renderer in renderers)
            {
                var mesh = renderer.GetMesh();
                if (mesh != null || NullInsertion) meshes.Add(mesh);
            }
            return meshes;
        }
        public static Mesh GetMesh(this Renderer Target)
        {
            Mesh mesh = null;
            switch (Target)
            {
                case SkinnedMeshRenderer SMR:
                    {
                        mesh = SMR.sharedMesh;
                        break;
                    }
                case MeshRenderer MR:
                    {
                        mesh = MR.GetComponent<MeshFilter>().sharedMesh;
                        break;
                    }
                default:
                    break;
            }
            return mesh;
        }
        public static void SetMesh(this Renderer Target, Mesh SetTarget)
        {
            switch (Target)
            {
                case SkinnedMeshRenderer SMR:
                    {
                        SMR.sharedMesh = SetTarget;
                        break;
                    }
                case MeshRenderer MR:
                    {
                        MR.GetComponent<MeshFilter>().sharedMesh = SetTarget;
                        break;
                    }
                default:
                    break;
            }
        }

        public static void SetMeshes(IEnumerable<Renderer> renderers, IReadOnlyList<Mesh> DistMesh, IReadOnlyList<Mesh> SetMesh)
        {
            foreach (var renderer in renderers)
            {
                switch (renderer)
                {
                    case SkinnedMeshRenderer SMR:
                        {
                            if (DistMesh.Contains(SMR.sharedMesh))
                            {
                                SMR.sharedMesh = SetMesh[DistMesh.IndexOf(SMR.sharedMesh)];
                            }
                            break;
                        }
                    case MeshRenderer MR:
                        {
                            var MF = MR.GetComponent<MeshFilter>();
                            if (DistMesh.Contains(MF.sharedMesh))
                            {
                                MF.sharedMesh = SetMesh[DistMesh.IndexOf(MF.sharedMesh)];
                            }
                            break;
                        }
                    default:
                        continue;
                }
            }
        }


        public static List<Vector3> ZipListVector3(IReadOnlyList<Vector2> XY, IReadOnlyList<float> Z)
        {
            var count = XY.Count;
            if (count != Z.Count) { throw new System.ArgumentException("XY.Count != Z.Count"); }

            List<Vector3> result = new List<Vector3>(count);

            for (int index = 0; index < count; index += 1)
            {
                result.Add(new Vector3(XY[index].x, XY[index].y, Z[index]));
            }

            return result;
        }

        public static Dictionary<T, List<T2>> ZipToDictionaryOnList<T, T2>(IReadOnlyDictionary<T, List<T2>> Souse, IReadOnlyDictionary<T, List<T2>> Add)
        {
            var result = ReadOnlyDictClone(Souse);
            foreach (var key in Add.Keys)
            {
                if (result.ContainsKey(key))
                {
                    result[key].AddRange(Add[key]);
                }
                else
                {
                    result.Add(key, Add[key]);
                }
            }
            return result;
        }

        public static Dictionary<T, T2> ReadOnlyDictClone<T, T2>(IReadOnlyDictionary<T, T2> Souse)
        {
            var result = new Dictionary<T, T2>();
            foreach (var keyValue in Souse)
            {
                result.Add(keyValue.Key, keyValue.Value);
            }
            return result;
        }

        public static Dictionary<T, List<T2>> ZipToDictionaryOnList<T, T2>(IReadOnlyList<Dictionary<T, List<T2>>> Target)
        {
            var result = new Dictionary<T, List<T2>>();
            foreach (var zipTargetDict in Target)
            {
                foreach (var key in zipTargetDict.Keys)
                {
                    if (result.ContainsKey(key))
                    {
                        result[key].AddRange(zipTargetDict[key]);
                    }
                    else
                    {
                        result.Add(key, zipTargetDict[key]);
                    }
                }
            }
            return result;
        }

        public static List<Texture2D> GenerateTexturesList(IReadOnlyList<Material> SouseMaterials, IReadOnlyDictionary<Material, Texture2D> MatAndTexDict)
        {
            List<Texture2D> result = new List<Texture2D>();
            foreach (var mat in SouseMaterials)
            {
                if (MatAndTexDict.ContainsKey(mat))
                {
                    result.Add(MatAndTexDict[mat]);
                }
                else
                {
                    result.Add(null);
                }
            }
            return result;
        }

        public static Dictionary<T, T2> GenerateFromKvP<T, T2>(IReadOnlyList<KeyValuePair<T, T2>> KvPList)
        {
            Dictionary<T, T2> result = new Dictionary<T, T2>();
            foreach (var KvP in KvPList)
            {
                result.Add(KvP.Key, KvP.Value);
            }
            return result;
        }
        public static bool InRange(float min, float max, float target)
        {
            return (min <= target && target <= max);
        }
        public static bool OutRange(float min, float max, float target)
        {
            return (target < min || max < target);
        }


    }
    public static class IReadOnlyUtility
    {
        public static int IndexOf<T>(this IReadOnlyList<T> ROList, T item)
        {
            var Count = ROList.Count;
            for (int Index = 0; Index < Count; Index += 1)
            {
                if (ROList[Index].Equals(item))
                {
                    return Index;
                }
            }

            return -1;
        }
    }
    public static class GizmosUtility
    {
        public static void DrawGizmoQuad(IEnumerable<List<Vector3>> Quads)
        {
            foreach (var Quad in Quads)
            {
                DrawQuad(Quad);
            }
        }

        public static void DrawQuad(IReadOnlyList<Vector3> Quad)
        {
            Gizmos.DrawLine(Quad[0], Quad[1]);
            Gizmos.DrawLine(Quad[0], Quad[2]);
            Gizmos.DrawLine(Quad[2], Quad[3]);
            Gizmos.DrawLine(Quad[1], Quad[3]);
        }

        public static void DrawGizmoLine(List<Vector3> Line)
        {
            var LineCount = Line.Count;
            if (LineCount < 1) return;
            int Count = 1;
            while (LineCount > Count)
            {

                var FromPos = Line[Count - 1];
                var ToPos = Line[Count];
                Gizmos.DrawLine(FromPos, ToPos);

                Count += 1;

            }
        }
        public static List<Transform> GetChildren(this Transform Parent)
        {
            var list = new List<Transform>();
            foreach (Transform child in Parent)
            {
                list.Add(child);
            }
            return list;
        }

        public static List<int> AllIndexOf(this List<Mesh> Meshes, Mesh Mesh)
        {
            List<int> indexes = new List<int>();
            int I = 0;
            foreach (var findTargetMesh in Meshes)
            {
                if (findTargetMesh == Mesh) indexes.Add(I);
                I += 1;
            }

            return indexes;
        }


    }

    [Serializable]
    public class OrderedHashSet<T> : IReadOnlyList<T>, IEnumerable<T>
    {
        [SerializeField] List<T> List;
        public T this[int index] => List[index];
        public int Count => List.Count;

        public IEnumerator<T> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return List.GetEnumerator();
        }

        public void Add(T item)
        {
            var Index = IndexOf(item);
            if (Index == -1)
            {
                List.Add(item);
            }
        }
        public int AddAndIndexOf(T item)
        {
            var Index = List.IndexOf(item);
            if (Index == -1)
            {
                List.Add(item);
                return List.Count - 1;
            }
            return Index;
        }

        public int IndexOf(T item)
        {
            return List.IndexOf(item);
        }

        public void AddRange(IEnumerable<T> Items)
        {
            foreach (var item in Items)
            {
                Add(item);
            }
        }




        public OrderedHashSet(IEnumerable<T> enumreat)
        {
            List = new List<T>();
            List.AddRange(enumreat);
        }

        public OrderedHashSet()
        {
            List = new List<T>();
        }

        public List<T> ToList(bool DeepClone = false)
        {
            if (DeepClone)
            {
                return new List<T>(List);
            }
            else
            {
                return List;
            }
        }
    }

    public static class MaterialUtil
    {
        public static void SetTextures(this Material TargetMat, List<PropAndTexture2D> PropAndTextures, bool FocuseSetTexture = false)
        {
            foreach (var propAndTexture in PropAndTextures)
            {
                if (FocuseSetTexture || TargetMat.GetTexture(propAndTexture.PropertyName) is Texture2D)
                {
                    TargetMat.SetTexture(propAndTexture.PropertyName, propAndTexture.Texture2D);
                }
            }
        }

        //MIT License
        //Copyright (c) 2020-2021 lilxyzw
        //https://github.com/lilxyzw/lilToon/blob/master/Assets/lilToon/Editor/lilMaterialUtils.cs
        //
        //https://light11.hatenadiary.com/entry/2018/12/04/224253
        public static void RemoveUnusedProperties(this Material material)
        {
            var so = new SerializedObject(material);
            so.Update();
            var savedProps = so.FindProperty("m_SavedProperties");

            var texs = savedProps.FindPropertyRelative("m_TexEnvs");
            DeleteUnused(ref texs, material);

            var floats = savedProps.FindPropertyRelative("m_Floats");
            DeleteUnused(ref floats, material);

            var colors = savedProps.FindPropertyRelative("m_Colors");
            DeleteUnused(ref colors, material);

            so.ApplyModifiedProperties();
        }

        public static void DeleteUnused(ref SerializedProperty props, Material material)
        {
            for (int i = props.arraySize - 1; i >= 0; i--)
            {
                if (!material.HasProperty(props.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue))
                {
                    props.DeleteArrayElementAtIndex(i);
                }
            }
        }

        public static Dictionary<string, Texture2D> GetPropAndTextures(Material material)
        {
            var so = new SerializedObject(material);
            so.Update();
            var savedProps = so.FindProperty("m_SavedProperties");

            var texs = savedProps.FindPropertyRelative("m_TexEnvs");

            Dictionary<string, Texture2D> PropAndTextures = new Dictionary<string, Texture2D>();

            for (int i = 0; i < texs.arraySize; i++)
            {
                var prop = texs.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue;
                var tex = texs.GetArrayElementAtIndex(i).FindPropertyRelative("second.m_Texture").objectReferenceValue as Texture2D;
                PropAndTextures.Add(prop, tex);
            }

            return PropAndTextures;
        }

        public static Dictionary<string, Texture2D> FiltalingUnused(Dictionary<string, Texture2D> PropAndTextures, Material material)
        {
            Dictionary<string, Texture2D> FiltalingPropAndTextures = new Dictionary<string, Texture2D>();
            foreach (var kvp in PropAndTextures)
            {
                if (material.HasProperty(kvp.Key))
                {
                    FiltalingPropAndTextures.Add(kvp.Key, kvp.Value);
                }
            }
            return FiltalingPropAndTextures;
        }

    }
}
#endif
