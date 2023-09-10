#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace net.rs64.TexTransTool.Utils
{
    public static class RendererUtility
    {
        /// <summary>
        /// マテリアルをとりあえず集めてくる。同一物を消したりなどしない。
        /// </summary>
        /// <param name="Renderers"></param>
        /// <returns></returns>
        public static List<Material> GetMaterials(IEnumerable<Renderer> Renderers)
        {
            List<Material> matList = new List<Material>();
            foreach (var renderer in Renderers)
            {
                matList.AddRange(renderer.sharedMaterials);
            }
            return matList;
        }
        public static List<Material> GetFilteredMaterials(IEnumerable<Renderer> Renderers)
        {
            return GetMaterials(Renderers).Distinct().Where(I => I != null).ToList();
        }

        /// <summary>
        /// レンダラーを捜索して、ターゲットのテクスチャをSetに差し替えたマテリアルを生成する。
        /// </summary>
        /// <param name="Renderers"></param>
        /// <param name="Target"></param>
        /// <param name="SetTex"></param>
        /// <returns>差し替え元と差し替え先のペア</returns>
        public static List<MatPair> SetTexture(IEnumerable<Renderer> Renderers, Texture2D Target, Texture2D SetTex)
        {
            var mats = GetFilteredMaterials(Renderers);
            var targetAndSet = new List<MatPair>();
            foreach (var mat in mats)
            {
                var Textures = MaterialUtility.FiltalingUnused(MaterialUtility.GetPropAndTextures(mat), mat);

                if (Textures.ContainsValue(Target))
                {
                    var NewMat = UnityEngine.Object.Instantiate<Material>(mat);

                    foreach (var KVP in Textures)
                    {
                        if (KVP.Value == Target)
                        {
                            NewMat.SetTexture(KVP.Key, SetTex);
                        }
                    }

                    targetAndSet.Add(new MatPair(mat, NewMat));
                }
            }

            return targetAndSet;
        }
        /// <summary>
        /// マテリアルをとりあえず差し替える上のGetMaterialsと合わせて使うこと推奨。
        /// </summary>
        /// <param name="Renderers"></param>
        /// <param name="Mat"></param>
        public static void SetMaterials(IEnumerable<Renderer> Renderers, List<Material> Mat)
        {
            int startOffset = 0;
            foreach (var renderer in Renderers)
            {
                int takeLength = renderer.sharedMaterials.Length;
                renderer.sharedMaterials = Mat.Skip(startOffset).Take(takeLength).ToArray();
                startOffset += takeLength;
            }
        }
        public static void ChangeMaterialForRenderers(IEnumerable<Renderer> Renderer, Dictionary<Material, Material> MatPairs)
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
        public static void ChangeMaterialForSerializedProperty(Dictionary<Material, Material> MatMapping, GameObject targetRoot, Type[] IgnoreTypes = null)
        {
            var allComponent = targetRoot.GetComponentsInChildren<Component>();
            IEnumerable<Component> components;
            if (IgnoreTypes.Any())
            {
                var filteredComponents = new List<Component>(allComponent.Length);
                foreach (var component in allComponent)
                {
                    if (component == null) { continue; }
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


        /// <summary>
        /// メッシュをとりあえず集めてくる。同一のものを消したりはしない。
        /// </summary>
        /// <param name="renderers"></param>
        /// <param name="NullInsertion"></param>
        /// <returns></returns>
        public static List<Mesh> GetMeshes(IEnumerable<Renderer> renderers)
        {
            List<Mesh> meshes = new List<Mesh>();
            foreach (var renderer in renderers)
            {
                var mesh = renderer.GetMesh();
                meshes.Add(mesh);
            }
            return meshes;
        }
        /// <summary>
        /// メッシュをとりあえず、セットする。上のGetMeshesと合わせて使うこと推奨。
        /// </summary>
        /// <param name="renderers"></param>
        /// <param name="meshes"></param>
        public static void SetMeshes(IEnumerable<Renderer> renderers, List<Mesh> meshes)
        {
            int Index = 0;
            foreach (var renderer in renderers)
            {
                renderer.SetMesh(meshes[Index]);
                Index += 1;
            }
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

        public static void SetMeshes(IEnumerable<Renderer> renderers, List<Mesh> DistMesh, List<Mesh> SetMesh)
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



    }
}
#endif
