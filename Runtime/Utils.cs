#if UNITY_EDITOR
using System.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rs64.TexTransTool.Decal;
using UnityEngine;
using UnityEditor;

namespace Rs64.TexTransTool
{
    public static class Utils
    {
        public static void ForEach2D(Vector2Int Reange, Action<int, int> action)
        {
            int countx = 0;
            int county = 0;
            while (true)
            {
                if (!(Reange.x > countx))
                {
                    countx = 0;
                    county += 1;
                }
                if (!(Reange.y > county))
                {
                    break;
                }

                action.Invoke(countx, county);

                countx += 1;
            }
        }
        public static List<Vector2Int> Reange2d(Vector2Int Reange)
        {
            var List = new List<Vector2Int>();
            ForEach2D(Reange, (x, y) => List.Add(new Vector2Int(x, y)));
            return List;
        }

        public static int TwoDToOneDIndex(Vector2Int TowDIndex, int Size)
        {
            return (TowDIndex.y * Size) + TowDIndex.x;
        }
        public static Vector2Int ConvertIndex2D(int Index1D, int withLengs)
        {
            return new Vector2Int(Index1D % withLengs, Index1D / withLengs);
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
                var newtex = new Texture2D(tex.width, tex.height, tex.format, CopySouse.mipmapCount > 1);
                newtex.SetPixels32(tex.GetPixels32());
                newtex.name = tex.name;
                tex = newtex;
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
            var Array = new T[Length];
            for (int i = 0; i < Array.Length; i++)
            {
                Array[i] = DefaultValue;
            }
            return Array;
        }
        public static List<TraiangleIndex> ToList(int[] triangleIndexs)
        {
            var TraianglesList = new List<TraiangleIndex>();
            int count = 0;
            while (triangleIndexs.Length > count)
            {
                TraianglesList.Add(new TraiangleIndex(triangleIndexs[count], triangleIndexs[count += 1], triangleIndexs[count += 1]));
                count += 1;
            }
            return TraianglesList;
        }
        public static List<List<TraiangleIndex>> GetSubTraiangel(this Mesh mesh)
        {
            var SubMeshCount = mesh.subMeshCount;
            List<List<TraiangleIndex>> SubTraiangles = new List<List<TraiangleIndex>>(SubMeshCount);

            for (int i = 0; i < SubMeshCount; i++)
            {
                SubTraiangles.Add(mesh.GetSubTraiangle(i));
            }
            return SubTraiangles;
        }
        public static List<TraiangleIndex> GetSubTraiangle(this Mesh mesh, int SubMesh)
        {
            return ToList(mesh.GetTriangles(SubMesh));
        }

        public static T[] TowDtoOneD<T>(T[,] SouseArry, Vector2Int Size)
        {
            T[] OneDArry = new T[Size.x * Size.y];
            foreach (var Index in Utils.Reange2d(Size))
            {
                OneDArry[TwoDToOneDIndex(Index, Size.x)] = SouseArry[Index.x, Index.y];
            }
            return OneDArry;
        }
        public static T[,] OneDToTowD<T>(T[] SouseArry, Vector2Int Size)
        {
            T[,] TowDArry = new T[Size.x, Size.y];
            int count = -1;
            foreach (var value in SouseArry)
            {
                count += 1;
                var Index = ConvertIndex2D(count, Size.x);
                TowDArry[Index.x, Index.y] = value;
            }
            return TowDArry;
        }
        public static List<Material> GetMaterials(IEnumerable<Renderer> Rendres)
        {
            List<Material> MatS = new List<Material>();
            foreach (var Rendera in Rendres)
            {
                MatS.AddRange(Rendera.sharedMaterials);
            }
            return MatS;
        }
        public static void SetMaterials(IEnumerable<Renderer> Rendres, IReadOnlyList<Material> Mat)
        {
            int StartOffset = 0;
            foreach (var Rendera in Rendres)
            {
                int TakeLengs = Rendera.sharedMaterials.Length;
                Rendera.sharedMaterials = Mat.Skip(StartOffset).Take(TakeLengs).ToArray();
                StartOffset += TakeLengs;
            }
        }
        public static void ChangeMaterialsRendereas(IEnumerable<Renderer> Rendres, IReadOnlyDictionary<Material, Material> MatPeas)
        {
            foreach (var Renderer in Rendres)
            {
                var Materials = Renderer.sharedMaterials;
                var IsEdit = false;
                foreach (var Index in Enumerable.Range(0, Materials.Length))
                {
                    var DistMat = Materials[Index];
                    if (MatPeas.ContainsKey(DistMat))
                    {
                        Materials[Index] = MatPeas[DistMat];
                        IsEdit = true;
                    }
                }
                if (IsEdit)
                {
                    Renderer.sharedMaterials = Materials;
                }
            }
        }
        public static void ChangeMaterialRendereas(IEnumerable<Renderer> Rendres, Material target, Material set)
        {
            foreach (var Renderer in Rendres)
            {
                var Materials = Renderer.sharedMaterials;
                var IsEdit = false;
                foreach (var Index in Enumerable.Range(0, Materials.Length))
                {
                    var DistMat = Materials[Index];
                    if (target == DistMat)
                    {
                        Materials[Index] = set;
                        IsEdit = true;
                    }
                }
                if (IsEdit)
                {
                    Renderer.sharedMaterials = Materials;
                }
            }
        }
        public static Dictionary<SerializedObject, SerializedProperty[]> SearchMaterialPropetys(GameObject targetRoot, Type[] IgnoreTypes = null)
        {
            var materialPropetysDict = new Dictionary<SerializedObject, SerializedProperty[]>();
            var allComponent = targetRoot.GetComponentsInChildren<Component>();
            IEnumerable<Component> componets;
            if (IgnoreTypes.Any())
            {
                var filtedComponents = new List<Component>(allComponent.Length);
                foreach (var comoponent in allComponent)
                {
                    var type = comoponent.GetType();
                    if (!IgnoreTypes.Any(J => J.IsAssignableFrom(type))) { filtedComponents.Add(comoponent); }
                }
                componets = filtedComponents;
            }
            else
            {
                componets = allComponent;
            }

            foreach (var component in componets)
            {
                var type = component.GetType();

                var serializeobj = new SerializedObject(component);
                var iter = serializeobj.GetIterator();
                var MaterialPropetys = new List<SerializedProperty>();
                while (iter.Next(true))
                {
                    var s_Obj = iter;
                    if (s_Obj.type == "PPtr<Material>" || s_Obj.type == "PPtr<$Material>")
                    {
                        MaterialPropetys.Add(s_Obj.Copy());
                    }
                }
                if (MaterialPropetys.Any())
                {
                    materialPropetysDict.Add(serializeobj, MaterialPropetys.ToArray());
                }
            }
            return materialPropetysDict;
        }
        public static void ChengeMateralSerialaizd(Dictionary<SerializedObject, SerializedProperty[]> MaterialPropetys, Material target, Material setMat)
        {
            foreach (var serializeObjectAndMatProp in MaterialPropetys)
            {
                serializeObjectAndMatProp.Key.Update();
                foreach (var MaterialPropety in serializeObjectAndMatProp.Value)
                {
                    if (MaterialPropety.objectReferenceValue == target)
                    {
                        MaterialPropety.objectReferenceValue = setMat;
                    }
                }
                serializeObjectAndMatProp.Key.ApplyModifiedProperties();
            }
        }
        public static List<Mesh> GetMeshes(IEnumerable<Renderer> renderers, bool NullInsertion = false)
        {
            List<Mesh> Meshs = new List<Mesh>();
            foreach (var Rendera in renderers)
            {
                var mesh = Rendera.GetMesh();
                if (mesh != null || NullInsertion) Meshs.Add(mesh);
            }
            return Meshs;
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

        public static void SetMeshs(IEnumerable<Renderer> renderers, IReadOnlyList<Mesh> DistMesh, IReadOnlyList<Mesh> SetMesh)
        {
            foreach (var Rendera in renderers)
            {
                switch (Rendera)
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
            var Count = XY.Count;
            if (Count != Z.Count) { throw new System.ArgumentException("XY.Count != Z.Count"); }

            List<Vector3> Result = new List<Vector3>(Count);

            for (int Index = 0; Index < Count; Index += 1)
            {
                Result.Add(new Vector3(XY[Index].x, XY[Index].y, Z[Index]));
            }

            return Result;
        }

        public static Dictionary<T, List<T2>> ZipToDictionaryOnList<T, T2>(IReadOnlyDictionary<T, List<T2>> Souse, IReadOnlyDictionary<T, List<T2>> Add)
        {
            var Result = ReadOnlyDictClone(Souse);
            foreach (var Key in Add.Keys)
            {
                if (Result.ContainsKey(Key))
                {
                    Result[Key].AddRange(Add[Key]);
                }
                else
                {
                    Result.Add(Key, Add[Key]);
                }
            }
            return Result;
        }

        public static Dictionary<T, T2> ReadOnlyDictClone<T, T2>(IReadOnlyDictionary<T, T2> Souse)
        {
            var Result = new Dictionary<T, T2>();
            foreach (var KeyValue in Souse)
            {
                Result.Add(KeyValue.Key, KeyValue.Value);
            }
            return Result;
        }

        public static Dictionary<T, List<T2>> ZipToDictionaryOnList<T, T2>(IReadOnlyList<Dictionary<T, List<T2>>> Target)
        {
            var Result = new Dictionary<T, List<T2>>();
            foreach (var ZiptargetDict in Target)
            {
                foreach (var Key in ZiptargetDict.Keys)
                {
                    if (Result.ContainsKey(Key))
                    {
                        Result[Key].AddRange(ZiptargetDict[Key]);
                    }
                    else
                    {
                        Result.Add(Key, ZiptargetDict[Key]);
                    }
                }
            }
            return Result;
        }

        public static List<Texture2D> GeneratTexturesList(IReadOnlyList<Material> SouseMaterials, IReadOnlyDictionary<Material, Texture2D> MatAndTexs)
        {
            List<Texture2D> Result = new List<Texture2D>();
            foreach (var Mat in SouseMaterials)
            {
                if (MatAndTexs.ContainsKey(Mat))
                {
                    Result.Add(MatAndTexs[Mat]);
                }
                else
                {
                    Result.Add(null);
                }
            }
            return Result;
        }

        public static Dictionary<T, T2> GeneretFromKvP<T, T2>(IReadOnlyList<KeyValuePair<T, T2>> KvPList)
        {
            Dictionary<T, T2> Result = new Dictionary<T, T2>();
            foreach (var KvP in KvPList)
            {
                Result.Add(KvP.Key, KvP.Value);
            }
            return Result;
        }
        public static bool InRange(float min, float max, float target)
        {
            return (min <= target && target <= max);
        }
        public static bool OutRenge(float min, float max, float target)
        {
            return (target < min || max < target);
        }


    }
    public static class IReadOnylUtility
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
        public static void DrowGizmoQuad(IEnumerable<List<Vector3>> Quads)
        {
            foreach (var Quad in Quads)
            {
                DrowQuad(Quad);
            }
        }

        public static void DrowQuad(IReadOnlyList<Vector3> Quad)
        {
            Gizmos.DrawLine(Quad[0], Quad[1]);
            Gizmos.DrawLine(Quad[0], Quad[2]);
            Gizmos.DrawLine(Quad[2], Quad[3]);
            Gizmos.DrawLine(Quad[1], Quad[3]);
        }

        public static void DrowGimzLine(List<Vector3> Line)
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
        public static List<Transform> GetChilds(this Transform Parent)
        {
            var List = new List<Transform>();
            foreach (Transform child in Parent)
            {
                List.Add(child);
            }
            return List;
        }

        public static List<int> AllIndexOf(this List<Mesh> DistMesh, Mesh Mesh)
        {
            List<int> Indexs = new List<int>();
            int I = 0;
            foreach (var FindatMesh in DistMesh)
            {
                if (FindatMesh == Mesh) Indexs.Add(I);
                I += 1;
            }

            return Indexs;
        }


    }

    [Serializable]
    public class OrderdHashSet<T> : IReadOnlyList<T>, IEnumerable<T>
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




        public OrderdHashSet(IEnumerable<T> enumreat)
        {
            List = new List<T>();
            List.AddRange(enumreat);
        }

        public OrderdHashSet()
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