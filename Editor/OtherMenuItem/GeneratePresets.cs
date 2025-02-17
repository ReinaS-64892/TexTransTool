using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using M = UnityEditor.MenuItem;
using net.rs64.TexTransTool.Utils;

namespace net.rs64.TexTransTool.Editor.OtherMenuItem
{
    internal class GeneratePresets
    {
        const string GOPath = "GameObject";
        const string GPath = "Generate";
        const string BP = GOPath + "/" + TexTransBehavior.TTTName + "/" + GPath + "/";

        private static void GeneratePresetsFor<T, TComponent>(Func<Renderer[], IEnumerable<T>> getTargets, Action<TComponent, T> setTarget) where T : UnityEngine.Object where TComponent : Component
        {
            var selected = Selection.activeGameObject;
            if (selected == null) return;

            var renderers = selected.GetComponentsInChildren<Renderer>(true)
                .Where(r => r is SkinnedMeshRenderer or MeshRenderer).ToArray();
            var targets = getTargets(renderers);

            var root = new GameObject(typeof(TComponent).Name);
            root.transform.SetParent(selected.transform, false);

            foreach (var target in targets)
            {
                var newGameObj = new GameObject(target.name);
                var component = newGameObj.AddComponent<TComponent>();
                newGameObj.SetActive(false);
                setTarget(component, target);
                newGameObj.transform.SetParent(root.transform, false);
            }
            Undo.RegisterCreatedObjectUndo(root, "Create" + typeof(TComponent).Name);
            EditorGUIUtility.PingObject(root);
        }


        [M(BP + MaterialModifier.ComponentName)]
        static void GenerateMaterialModifiers()
        {
            GeneratePresetsFor<Material, MaterialModifier>(
                renderers => RendererUtility.GetFilteredMaterials(renderers),
                (component, material) => component.TargetMaterial = material
            );
        }


        [M(BP + TextureConfigurator.ComponentName)]
        static void GenerateTextureConfigurators()
        {
            GeneratePresetsFor<Texture2D, TextureConfigurator>(
                renderers => RendererUtility.GetAllTexture<Texture2D>(renderers),
                (component, texture) => component.TargetTexture = new TextureSelector() { SelectTexture = texture }
            );
        }
    }
}
