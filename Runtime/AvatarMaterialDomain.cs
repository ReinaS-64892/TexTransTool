#if UNITY_EDITOR
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Rs64.TexTransTool
{
    [RequireComponent(typeof(TexTransGroup))]
    public class AvatarMaterialDomain : MonoBehaviour
    {
        public GameObject Avatar;
        [SerializeField] public TexTransGroup TexTransGroup;
        [SerializeField] protected MaterialDomain CacheDomain;

        public virtual MaterialDomain GetDomain()
        {
            return new MaterialDomain(Avatar.GetComponentsInChildren<Renderer>(true).ToList());
        }
        protected void Reset()
        {
            TexTransGroup = GetComponent<TexTransGroup>();
        }
        public virtual void Appry()
        {
            if (CacheDomain != null) return;
            if (TexTransGroup == null) Reset();
            if (Avatar == null) return;
            CacheDomain = GetDomain();
            TexTransGroup.Appry(CacheDomain);
        }

        public virtual void Revart()
        {
            if (CacheDomain == null) return;
            if (TexTransGroup == null) Reset();
            TexTransGroup.Revart(CacheDomain);
            CacheDomain = null;
        }
    }
    [System.Serializable]
    public class MaterialDomain
    {
        public MaterialDomain(List<Renderer> Renderers)
        {
            _Renderers = Renderers;
            _initialMaterials = Utils.GetMaterials(Renderers);
        }
        [SerializeField] List<Renderer> _Renderers;
        [SerializeField] List<Material> _initialMaterials;
        public MaterialDomain GetBackUp()
        {
            return new MaterialDomain(_Renderers);
        }
        public void SetMaterial(Material Target, Material SetMat)
        {
            foreach (var Renderer in _Renderers)
            {
                var Materials = Renderer.sharedMaterials;
                var IsEdit = false;
                foreach (var Index in Enumerable.Range(0, Materials.Length))
                {
                    if (Materials[Index] == Target)
                    {
                        Materials[Index] = SetMat;
                        IsEdit = true;
                    }
                }
                if (IsEdit)
                {
                    Renderer.sharedMaterials = Materials;
                }
            }
        }

        public void SetMaterials(List<Material> Target, List<Material> SetMat)
        {
            foreach (var index in Enumerable.Range(0, Target.Count))
            {
                SetMaterial(Target[index], SetMat[index]);
            }
        }
        public void SetMaterials(IEnumerable<Material> Target, IEnumerable<Material> SetMat)
        {
            SetMaterials(Target.ToList(), SetMat.ToList());
        }
        public void SetMaterials(Dictionary<Material, Material> TargetAndSet)
        {
            foreach (var KVP in TargetAndSet)
            {
                SetMaterial(KVP.Key, KVP.Value);
            }
        }
        public void SetMaterials(List<Material> TargetMat, Material SetMat)
        {
            foreach (var Target in TargetMat)
            {
                SetMaterial(Target, SetMat);
            }
        }

        public void ResetMaterial()
        {
            Utils.SetMaterials(_Renderers, _initialMaterials);
        }
    }
}
#endif