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
        static void C<TTB>() where TTB : MonoBehaviour
        {
            var parent = Selection.activeGameObject;
            var newGameObj = new GameObject(typeof(TTB).Name);
            newGameObj.transform.SetParent(parent?.transform, false);
            newGameObj.AddComponent<TTB>();
            Undo.RegisterCreatedObjectUndo(newGameObj, "Create " + typeof(TTB).Name);
            Selection.activeGameObject = newGameObj;
            EditorGUIUtility.PingObject(newGameObj);
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
        [M(BP + UnityGradationMapLayer.MenuPath)] static void UGML() => C<UnityGradationMapLayer>();
        [M(BP + YAsixFixedGradientLayer.MenuPath)] static void YAFGL() => C<YAsixFixedGradientLayer>();

        [M(BP + TexTransGroup.MenuPath)] static void TTG() => C<TexTransGroup>();
        [M(BP + PhaseDefinition.PDMenuPath)] static void PD() => C<PhaseDefinition>();

        [M(BP + PreviewGroup.MenuPath)] static void PG() => C<PreviewGroup>();

        [M(BP + BoxIslandSelector.MenuPath)] static void BIS() => C<BoxIslandSelector>();
        [M(BP + SphereIslandSelector.MenuPath)] static void SIS() => C<SphereIslandSelector>();
        [M(BP + RayCastIslandSelector.MenuPath)] static void RCIS() => C<RayCastIslandSelector>();
        [M(BP + RendererIslandSelector.MenuPath)] static void RIS() => C<RendererIslandSelector>();
        [M(BP + SubMeshIslandSelector.MenuPath)] static void SMIS() => C<SubMeshIslandSelector>();
        [M(BP + IslandSelectorOR.MenuPath)] static void ISOR() => C<IslandSelectorOR>();
        [M(BP + IslandSelectorAND.MenuPath)] static void ISAND() => C<IslandSelectorAND>();
        [M(BP + IslandSelectorNOT.MenuPath)] static void ISNOT() => C<IslandSelectorNOT>();
        [M(BP + IslandSelectorXOR.MenuPath)] static void ISXOR() => C<IslandSelectorXOR>();

        [M(BP + SingleGradationDecal.MenuPath)] static void SGD() => C<SingleGradationDecal>();
        [M(BP + TextureConfigurator.MenuPath)] static void TC() => C<TextureConfigurator>();
        [M(BP + TextureBlender.MenuPath)] static void TB() => C<TextureBlender>();
        [M(BP + MaterialOverrideTransfer.MenuPath)] static void MOT() => C<MaterialOverrideTransfer>();


    }
}
