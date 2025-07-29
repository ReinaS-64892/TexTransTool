using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using net.rs64.TexTransCore;
using net.rs64.TexTransCore.UVIsland;
using net.rs64.TexTransTool.TextureAtlas;
using net.rs64.TexTransTool.Utils;
using net.rs64.TexTransTool.UVIsland;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Color = UnityEngine.Color;

namespace net.rs64.TexTransTool.Editor
{
    internal class DomainTextureAnalyzer : EditorWindow
    {
        public GameObject DomainRoot;
        public bool IncludeDisableRenderers;
        [MenuItem("Tools/TexTransTool/DomainTextureAnalyzer (Experimental)")]
        internal static void OpenDomainTextureAnalyzer()
        {
            var selectedGameObject = Selection.activeGameObject;
            var window = GetWindow<DomainTextureAnalyzer>();
            window.DomainRoot = selectedGameObject;
            window.Analyzing();
        }
        VisualElement _analyzerElementContainer;
        public void CreateGUI()
        {
            rootVisualElement.Clear();
            var thisSObject = new SerializedObject(this);
            thisSObject.Update();

            var sDomainRoot = thisSObject.FindProperty(nameof(DomainRoot));

            var scrollView = new ScrollView();

            _analyzerElementContainer = scrollView.Q<VisualElement>("unity-content-container");

            var topBar = new VisualElement();
            topBar.style.flexDirection = FlexDirection.Row;
            topBar.style.flexShrink = 0;
            var domainRootField = new PropertyField();
            domainRootField.BindProperty(sDomainRoot);
            domainRootField.style.flexGrow = 1;
            domainRootField.RegisterValueChangeCallback(i => Analyzing());
            var analyzeButton = new Button(Analyzing);
            analyzeButton.text = "Analyze!";

            topBar.hierarchy.Add(domainRootField);
            topBar.hierarchy.Add(analyzeButton);
            rootVisualElement.Add(topBar);
            rootVisualElement.Add(scrollView);

        }

        void Analyzing()
        {
            _analyzerElementContainer.hierarchy.Clear();

            if (DomainRoot == null)
            {
                var label = new Label("please set DomainRoot!");
                _analyzerElementContainer.hierarchy.Add(label);
                return;
            }

            var domainRenderers = DomainRoot.GetComponentsInChildren<Renderer>(true);
            var nwDomain = new NotWorkDomain(domainRenderers, null);
            domainRenderers = domainRenderers
                .Where(i => i is SkinnedMeshRenderer || i is MeshRenderer)
                .Where(AtlasTexture.IsAtlasAllowedRenderer)
                .Where(i => AtlasTexture.CheckRendererActive(nwDomain, i, IncludeDisableRenderers))
                .ToArray();

            if (domainRenderers.Length == 0)
            {
                var label = new Label("No analysis target exists");
                _analyzerElementContainer.hierarchy.Add(label);
                return;
            }

            var domainContainedMaterials = new HashSet<Material>();
            foreach (var renderer in domainRenderers)
            {
                var subMeshCount = renderer.GetMesh().subMeshCount;
                var sharedMaterials = renderer.sharedMaterials;

                for (var i = 0; subMeshCount > i; i += 1)
                {
                    if (sharedMaterials.Length <= i) { break; }
                    domainContainedMaterials.Add(sharedMaterials[i]);
                }
            }

            domainContainedMaterials = domainContainedMaterials.Where(m => m.GetTexture("_MainTex") != null).ToHashSet();

            var groupingMaterials = new MaterialGroupingContext(domainContainedMaterials, UVChannel.UV0, null);

            var islandUsedRecodeList = new List<UVUsageRecode>();
            foreach (var matG in groupingMaterials.GroupMaterials)
            {
                var record = new UVUsageRecode();

                record.MainTexture = matG.First().GetTexture("_MainTex");
                record.Materials = matG.ToList();
                record.Islands = new();

                foreach (var r in domainRenderers)
                {
                    var mesh = r.GetMesh();
                    var subMeshCount = mesh.subMeshCount;
                    var sharedMaterials = r.sharedMaterials;

                    for (var i = 0; subMeshCount > i; i += 1)
                    {
                        if (sharedMaterials.Length <= i) { break; }
                        if (matG.Contains(sharedMaterials[i]) is false) { continue; }
                        record.Islands.AddRange(UnityIslandUtility.UVtoIsland(MemoryMarshal.Cast<int, TriangleVertexIndices>(mesh.GetTriangles(i).AsSpan()), mesh.uv.AsSpan()));
                    }
                }

                record.TotalArea = record.Islands.Sum(i => i.Transform.Size.X * i.Transform.Size.Y);
                islandUsedRecodeList.Add(record);
            }
            islandUsedRecodeList = islandUsedRecodeList.OrderBy(i => i.MainTexture.name).ToList();

            var col = Color.gray;
            col.a = 0.7f;
            foreach (var usedRecord in islandUsedRecodeList)
            {
                var rowRoot = new VisualElement();
                rowRoot.style.flexDirection = FlexDirection.Row;
                rowRoot.style.justifyContent = Justify.FlexStart;
                rowRoot.style.marginTop = rowRoot.style.marginBottom = 1f;
                var imageSize = 256;
                rowRoot.style.height = imageSize;
                _analyzerElementContainer.hierarchy.Add(rowRoot);

                var aspect = (float)usedRecord.MainTexture.height / usedRecord.MainTexture.width;
                var previewImage = new Image { image = usedRecord.MainTexture };
                previewImage.style.width = imageSize;
                previewImage.style.height = imageSize * aspect;
                previewImage.style.marginTop = Length.Auto();
                previewImage.style.backgroundColor = Color.white;
                rowRoot.hierarchy.Add(previewImage);
                var rectScaler = new VisualElement();
                previewImage.hierarchy.Add(rectScaler);

                rectScaler.style.scale = new StyleScale(new Vector2(1, aspect));
                rectScaler.style.width = previewImage.style.width;
                rectScaler.style.height = previewImage.style.width;
                rectScaler.style.position = Position.Absolute;
                rectScaler.style.bottom = imageSize * -0.5f;
                rectScaler.style.left = 0;

                foreach (var island in usedRecord.Islands)
                {
                    var rect = new VisualElement();

                    rect.style.position = Position.Absolute;
                    rect.style.backgroundColor = col;

                    rect.style.left = island.Transform.Position.X * imageSize;
                    rect.style.bottom = (island.Transform.Position.Y * imageSize) + (imageSize * 0.5f);

                    rect.style.width = island.Transform.Size.X * imageSize;
                    rect.style.height = island.Transform.Size.Y * imageSize;

                    rectScaler.hierarchy.Add(rect);
                }

                var columElement = new VisualElement();
                columElement.style.flexGrow = 1;
                columElement.style.justifyContent = Justify.SpaceBetween;
                rowRoot.hierarchy.Add(columElement);

                var texNameLabel = new Label(usedRecord.MainTexture.name);
                texNameLabel.style.fontSize = 26f;
                columElement.hierarchy.Add(texNameLabel);

                var usageLabelRow = new VisualElement();
                usageLabelRow.style.flexDirection = FlexDirection.Row;
                var usageParent = Mathf.RoundToInt(usedRecord.TotalArea * 100);
                var usageLabel = new Label("Texture usage :");
                var usageParentLabel = new Label(usageParent + "%");
                usageParentLabel.style.fontSize = usageLabel.style.fontSize = 18f;
                usageLabelRow.hierarchy.Add(usageLabel);
                usageLabelRow.hierarchy.Add(usageParentLabel);
                columElement.hierarchy.Add(usageLabelRow);
                usageParentLabel.style.color = usageParent > 70 ? Color.white : (usageParent > 40 ? Color.yellow : Color.red);


                var groupMaterialElementRoot = new VisualElement();
                var groupedText = new Label("GroupMaterials");
                groupMaterialElementRoot.hierarchy.Add(groupedText);
                columElement.hierarchy.Add(groupMaterialElementRoot);

                var matRow = new ScrollView();
                groupMaterialElementRoot.hierarchy.Add(matRow);
                var matRowContainer = matRow.Q<VisualElement>("unity-content-container");
                matRowContainer.style.flexDirection = FlexDirection.Row;

                foreach (var mat in usedRecord.Materials)
                {
                    var obElement = new ObjectField();
                    obElement.SetEnabled(false);
                    obElement.objectType = typeof(Material);
                    obElement.value = mat;

                    var previewMatImage = new Image();
                    previewMatImage.image = AssetPreview.GetAssetPreview(mat);

                    var container = new VisualElement();
                    container.style.flexDirection = FlexDirection.Column;
                    container.style.width = container.style.height = 84f;

                    container.hierarchy.Add(previewMatImage);
                    container.hierarchy.Add(obElement);
                    matRowContainer.hierarchy.Add(container);

                }


            }


        }

        record UVUsageRecode
        {
            public Texture MainTexture;
            public List<Island> Islands;
            public float TotalArea;
            public List<Material> Materials;
        }


    }
}


