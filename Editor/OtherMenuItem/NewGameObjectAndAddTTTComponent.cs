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
        static TTB C<TTB>() where TTB : MonoBehaviour
        {
            var parent = Selection.activeGameObject?.transform;
            if (parent == null) return null;
            var component = C<TTB>(parent, typeof(TTB).Name);
            Undo.RegisterCreatedObjectUndo(component.gameObject, "Create " + typeof(TTB).Name);
            return component;
        }

        static TTB C<TTB>(Transform parent, string name) where TTB : MonoBehaviour
        {
            var newGameObj = new GameObject(name);
            newGameObj.transform.SetParent(parent, false);
            var newComponent = newGameObj.AddComponent<TTB>();
            Selection.activeGameObject = newGameObj;
            EditorGUIUtility.PingObject(newGameObj);
            return newComponent;
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
        [M(BP + HSLAdjustmentLayer.MenuPath)] static void HSLAL() => C<HSLAdjustmentLayer>();
        [M(BP + HSVAdjustmentLayer.MenuPath)] static void HSVAL() => C<HSVAdjustmentLayer>();
        [M(BP + LevelAdjustmentLayer.MenuPath)] static void LAL() => C<LevelAdjustmentLayer>();
        [M(BP + SelectiveColoringAdjustmentLayer.MenuPath)] static void SCAL() => C<SelectiveColoringAdjustmentLayer>();
        [M(BP + UnityGradationMapLayer.MenuPath)] static void UGML() => C<UnityGradationMapLayer>();
        [M(BP + YAsixFixedGradientLayer.MenuPath)] static void YAFGL() => C<YAsixFixedGradientLayer>();
        [M(BP + ColorizeLayer.MenuPath)] static void CL() => C<ColorizeLayer>();

        [M(BP + TexTransGroup.MenuPath)] static void TTG() => C<TexTransGroup>();
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
        [M(BP + TextureConfigurator.MenuPath)] static void TC() => C<TextureConfigurator>();
        [M(BP + TextureBlender.MenuPath)] static void TB() => C<TextureBlender>();
        [M(BP + MaterialOverrideTransfer.MenuPath)] static void MOT() => C<MaterialOverrideTransfer>();
        [M(BP + MaterialConfigurator.MenuPath)] static void MC() => C<MaterialConfigurator>();

    }
}
