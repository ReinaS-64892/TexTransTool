using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using net.rs64.TexTransCore.TransTextureCore.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.MatAndTexUtils
{
    [AddComponentMenu(TexTransBehavior.TTTName + "/" + MenuPath)]
    public class MaterialModifier : TexTransRuntimeBehavior
    {
        internal const string ComponentName = "TTT MaterialModifier";
        internal const string MenuPath = MatAndTexAbsoluteSeparator.FoldoutName + "/" + ComponentName;
        public List<Renderer> TargetRenderers = new List<Renderer> { null };
        public bool MultiRendererMode = false;
        internal override List<Renderer> GetRenderers => TargetRenderers;
        internal override bool IsPossibleApply => TargetRenderers.Any(i => i != null);
        public List<Material> ModifiedTarget = new List<Material>();

        public List<MatMod> ChangeList = new List<MatMod>();
        [Serializable]
        public class MatMod
        {
            public ModTypeEnum ModType;
            public enum ModTypeEnum
            {
                Float,
                Texture,
                Color,
            }

            public string Float_PropertyName;
            public float Float_Value;


            public PropertyName Texture_PropertyName = PropertyName.DefaultValue;
            public Texture Texture_Value;


            public string Color_PropertyName;
            public Color Color_Value;

            public void Modified(Material material)
            {
                switch (ModType)
                {
                    case ModTypeEnum.Float:
                        {
                            if (!material.HasProperty(Float_PropertyName)) { break; }
                            material.SetFloat(Float_PropertyName, Float_Value);
                            break;
                        }
                    case ModTypeEnum.Texture:
                        {
                            if (!material.HasProperty(Texture_PropertyName)) { break; }
                            material.SetTexture(Texture_PropertyName, Texture_Value);
                            break;
                        }
                    case ModTypeEnum.Color:
                        {
                            if (!material.HasProperty(Color_PropertyName)) { break; }
                            material.SetColor(Color_PropertyName, Color_Value);
                            break;
                        }
                }
            }

        }
        internal override TexTransPhase PhaseDefine => TexTransPhase.UnDefined;

        internal override void Apply([NotNull] IDomain domain)
        {
            if (!IsPossibleApply) { throw new TTTNotExecutable(); }

            var modMatList = new Dictionary<Material, Material>();

            var hashSet = new HashSet<Material>(RendererUtility.GetMaterials(GetRenderers));
            var containedModTarget = hashSet.Where(i => ModifiedTarget.Any(m => domain.OriginEqual(m, i))).ToList();

            foreach (var modTarget in containedModTarget)
            {
                var newMat = Instantiate(modTarget);
                modMatList.Add(modTarget, newMat);
                foreach (var Modified in ChangeList)
                {
                    Modified.Modified(newMat);
                }
            }
            domain.ReplaceMaterials(modMatList);
        }

        internal override IEnumerable<UnityEngine.Object> GetDependency()
        {
            foreach (var i in ModifiedTarget) { yield return i; }
            foreach (var i in TargetRenderers) { yield return i; }
        }
    }
}
