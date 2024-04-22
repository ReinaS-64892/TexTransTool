using net.rs64.TexTransTool.Decal;
using net.rs64.TexTransTool.IslandSelector;
using net.rs64.TexTransTool.MatAndTexUtils;
using net.rs64.TexTransTool.MultiLayerImage;
using net.rs64.TexTransTool.TextureAtlas;
using UnityEditor;
using UnityEngine;
using M = UnityEditor.MenuItem;

namespace net.rs64.TexTransTool.Editor.OtherMenuItem
{
    internal class NewGameObjectAndAddTTTComponent
    {
        static void C<TTB>() where TTB : MonoBehaviour
        {
            var parent = Selection.activeGameObject;
            var newGameObj = new GameObject(typeof(TTB).Name);
            newGameObj.transform.SetParent(parent?.transform);
            newGameObj.AddComponent<TTB>();
        }
        const string GOPath = "GameObject";
        const string BP = GOPath + "/" + TexTransBehavior.TTTName + "/";

        [M(BP + AtlasTexture.MenuPath)] static void AT() => C<AtlasTexture>();
        [M(BP + SimpleDecal.MenuPath)] static void SD() => C<SimpleDecal>();

        [M(BP + MultiLayerImageCanvas.MenuPath)] static void MLIC() => C<MultiLayerImageCanvas>();
        [M(BP + LayerFolder.MenuPath)] static void LF() => C<LayerFolder>();
        [M(BP + RasterLayer.MenuPath)] static void RL() => C<RasterLayer>();
        [M(BP + RasterImportedLayer.MenuPath)] static void RIL() => C<RasterImportedLayer>();
        [M(BP + SolidColorLayer.MenuPath)] static void SCL() => C<SolidColorLayer>();
        [M(BP + HSLAdjustmentLayer.MenuPath)] static void HAL() => C<HSLAdjustmentLayer>();
        [M(BP + LevelAdjustmentLayer.MenuPath)] static void LAL() => C<LevelAdjustmentLayer>();
        [M(BP + SelectiveColoringAdjustmentLayer.MenuPath)] static void SCAL() => C<SelectiveColoringAdjustmentLayer>();

        [M(BP + TexTransGroup.MenuPath)] static void TTG() => C<TexTransGroup>();
        [M(BP + PhaseDefinition.PDMenuPath)] static void PD() => C<PhaseDefinition>();

        [M(BP + PreviewGroup.MenuPath)] static void PG() => C<PreviewGroup>();

        [M(BP + MatAndTexAbsoluteSeparator.MenuPath)] static void MATAS() => C<MatAndTexAbsoluteSeparator>();
        [M(BP + MatAndTexRelativeSeparator.MenuPath)] static void MATRS() => C<MatAndTexRelativeSeparator>();
        [M(BP + MaterialModifier.MenuPath)] static void MM() => C<MaterialModifier>();

        [M(BP + BoxIslandSelector.MenuPath)] static void BIS() => C<BoxIslandSelector>();
        [M(BP + SphereIslandSelector.MenuPath)] static void SIS() => C<SphereIslandSelector>();
        [M(BP + RayCastIslandSelector.MenuPath)] static void RCIS() => C<RayCastIslandSelector>();
        [M(BP + RendererIslandSelector.MenuPath)] static void RIS() => C<RendererIslandSelector>();
        [M(BP + SubMeshIslandSelector.MenuPath)] static void SMIS() => C<SubMeshIslandSelector>();
        [M(BP + IslandSelectorOR.MenuPath)] static void ISOR() => C<IslandSelectorOR>();
        [M(BP + IslandSelectorAND.MenuPath)] static void ISAND() => C<IslandSelectorAND>();
        [M(BP + IslandSelectorNOT.MenuPath)] static void ISNOT() => C<IslandSelectorNOT>();
        [M(BP + IslandSelectorXOR.MenuPath)] static void ISXOR() => C<IslandSelectorXOR>();
        [M(BP + IslandSelectorRelay.MenuPath)] static void ISR() => C<IslandSelectorRelay>();


    }
}
