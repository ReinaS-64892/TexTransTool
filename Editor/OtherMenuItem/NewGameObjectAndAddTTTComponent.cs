using System;
using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.IslandSelector;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.TextureAtlas;
using UnityEditor;
using UnityEngine;
using M = UnityEditor.MenuItem;

namespace net.rs64.TexTransTool.Editor.OtherMenuItem
{
    internal class NewGameObjectAndAddTTTComponent
    {
        static TTB C<TTB>(bool firstSibling = false) where TTB : MonoBehaviour
        {
            var parent = Selection.activeGameObject?.transform;
            if (parent == null) return null;
            var component = C<TTB>(parent, typeof(TTB).Name, firstSibling);
            Undo.RegisterCreatedObjectUndo(component.gameObject, "Create " + typeof(TTB).Name);
            return component;
        }

        static TTB C<TTB>(Transform parent, string name, bool firstSibling = false) where TTB : MonoBehaviour
        {
            var newGameObj = new GameObject(name);
            newGameObj.transform.SetParent(parent, false);
            var newComponent = newGameObj.AddComponent<TTB>();
            if (firstSibling) newGameObj.transform.SetAsFirstSibling();
            Selection.activeGameObject = newGameObj;
            EditorGUIUtility.PingObject(newGameObj);
            return newComponent;
        }

        const string GOPath = "GameObject";
        const string BP = GOPath + "/" + TexTransBehavior.TTTName + "/";

        [M(BP + AtlasTexture.MenuPath)] static void AT() => C<AtlasTexture>();
        [M(BP + SimpleDecal.MenuPath)] static void SD() => C<SimpleDecal>();

        [M(BP + MultiLayerImageCanvas.MenuPath)] static void MLIC() => C<MultiLayerImageCanvas>();
        [M(BP + LayerFolder.MenuPath)] static void LF() => C<LayerFolder>(true);
        [M(BP + RasterLayer.MenuPath)] static void RL() => C<RasterLayer>(true);
        [M(BP + RasterImportedLayer.MenuPath)] static void RIL() => C<RasterImportedLayer>(true);
        [M(BP + SolidColorLayer.MenuPath)] static void SCL() => C<SolidColorLayer>(true);
        [M(BP + HSLAdjustmentLayer.MenuPath)] static void HSLAL() => C<HSLAdjustmentLayer>(true);
        [M(BP + HSVAdjustmentLayer.MenuPath)] static void HSVAL() => C<HSVAdjustmentLayer>(true);
        [M(BP + LevelAdjustmentLayer.MenuPath)] static void LAL() => C<LevelAdjustmentLayer>(true);
        [M(BP + SelectiveColoringAdjustmentLayer.MenuPath)] static void SCAL() => C<SelectiveColoringAdjustmentLayer>(true);
        [M(BP + UnityGradationMapLayer.MenuPath)] static void UGML() => C<UnityGradationMapLayer>(true);
        [M(BP + YAxisFixedGradientLayer.MenuPath)] static void YAFGL() => C<YAxisFixedGradientLayer>(true);
        [M(BP + ColorizeLayer.MenuPath)] static void CL() => C<ColorizeLayer>(true);
        [M(BP + PhotoshopGradationMapLayer.MenuPath)] static void PGML() => C<PhotoshopGradationMapLayer>(true);

        [M(BP + PhaseDefinition.PDMenuPath)] static void PD() => C<PhaseDefinition>();

        [M(BP + PreviewGroup.MenuPath)] static void PG() => C<PreviewGroup>();

        [M(BP + BoxIslandSelector.MenuPath)] static void BIS() => C<BoxIslandSelector>();
        [M(BP + SphereIslandSelector.MenuPath)] static void SIS() => C<SphereIslandSelector>();
        [M(BP + PinIslandSelector.MenuPath)] static void PIS() => C<PinIslandSelector>();
        [M(BP + AimIslandSelector.MenuPath)] static void AIS() => C<AimIslandSelector>();
        [M(BP + RendererIslandSelector.MenuPath)] static void RIS() => C<RendererIslandSelector>();
        [M(BP + MaterialIslandSelector.MenuPath)] static void MIS() => C<MaterialIslandSelector>();
        [M(BP + SubMeshIndexIslandSelector.MenuPath)] static void SMIIS() => C<SubMeshIndexIslandSelector>();
        [M(BP + IslandSelectorOR.MenuPath)] static void ISOR() => C<IslandSelectorOR>();
        [M(BP + IslandSelectorAND.MenuPath)] static void ISAND() => C<IslandSelectorAND>();
        [M(BP + IslandSelectorNOT.MenuPath)] static void ISNOT() => C<IslandSelectorNOT>();
        [M(BP + IslandSelectorXOR.MenuPath)] static void ISXOR() => C<IslandSelectorXOR>();
        [M(BP + RendererIslandSelectorLink.MenuPath)] static void RISL() => C<RendererIslandSelectorLink>();
        [M(BP + MaterialIslandSelectorLink.MenuPath)] static void MISL() => C<MaterialIslandSelectorLink>();
        [M(BP + SubMeshIndexIslandSelectorLink.MenuPath)] static void SMIISL() => C<SubMeshIndexIslandSelectorLink>();
        [M(BP + SubMeshIslandSelectorLink.MenuPath)] static void SMISL() => C<SubMeshIslandSelectorLink>();

        [M(BP + SingleGradationDecal.MenuPath)] static void SGD() => C<SingleGradationDecal>();
        [M(BP + DistanceGradationDecal.MenuPath)] static void DGD() => C<DistanceGradationDecal>();

        [M(BP + TextureConfigurator.MenuPath)] static void TC() => C<TextureConfigurator>();
        [M(BP + TextureBlender.MenuPath)] static void TB() => C<TextureBlender>();
        [M(BP + MaterialOverrideTransfer.MenuPath)] static void MOT() => C<MaterialOverrideTransfer>();
        [M(BP + MaterialModifier.MenuPath)] static void MC() => C<MaterialModifier>();
        [M(BP + ColorDifferenceChanger.MenuPath)] static void CDC() => C<ColorDifferenceChanger>();

        [M(BP + NearTransTexture.MenuPath)] static void NTT() => C<NearTransTexture>();
        [M(BP + UVCopy.MenuPath)] static void UC() => C<UVCopy>();
        [M(BP + TileAtlasBreaker.MenuPath)] static void TAB() => C<TileAtlasBreaker>();



        [M(BP + ParallelProjectionWithLilToonDecal.MenuPath)]
        static void PPWLD()
        {
            var ppwld = C<ParallelProjectionWithLilToonDecal>();
            var ais = C<AimIslandSelector>();
            Undo.RecordObject(ais.transform, "move parent and scaling");
            ais.transform.SetParent(ppwld.transform, false);
            ais.transform.localScale = new Vector3(4f, 4f, 4f);
            Undo.RecordObject(ppwld, "set island selector");
            ppwld.IslandSelector = ais;
        }

        static void CM<TTB>(MenuCommand menuCommand, Action<TTB, Material> action = null) where TTB : MonoBehaviour
        {
            var material = menuCommand.context as Material;
            var transform = Selection.activeGameObject?.transform;
            if (transform == null) return;
            var parent = transform.parent == null ? transform : transform.parent;
            var component = C<TTB>(parent, material.name);
            action?.Invoke(component, material);
            Undo.RegisterCreatedObjectUndo(component.gameObject, "Create " + typeof(TTB).Name);
        }

        const string CPath = "CONTEXT";
        // 四つ超えたら、 TexTransTool としてまとめてもよいかも
        const string MRP = CPath + "/" + nameof(Material) + "/";
        const int PRIORITY = 200;
        [M(MRP + MaterialOverrideTransfer.ComponentName, false, PRIORITY)] static void MOTM(MenuCommand mc) => CM<MaterialOverrideTransfer>(mc, (c, m) => c.TargetMaterial = m);
        [M(MRP + MaterialModifier.ComponentName, false, PRIORITY)] static void MCM(MenuCommand mc) => CM<MaterialModifier>(mc, (c, m) => c.TargetMaterial = m);

    }
}
