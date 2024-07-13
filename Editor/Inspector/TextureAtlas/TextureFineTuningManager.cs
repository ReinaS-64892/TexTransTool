using UnityEngine;
using UnityEditor;
using System.Linq;
using net.rs64.TexTransTool.Editor;
using System.Collections.Generic;
using net.rs64.TexTransCore.Utils;
using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using net.rs64.TexTransTool.Preview;
using net.rs64.TexTransCore.BlendTexture;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;
using net.rs64.TexTransTool.Editor.OtherMenuItem;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
    internal class TextureFineTuningManager : EditorWindow
    {
        [SerializeField] AtlasTexture _fineTuningManageTarget;
        public AtlasTexture FineTuningManageTarget => _fineTuningManageTarget;

        AtlasTexture.AtlasData _previewedAtlasTexture;
        internal static void OpenAtlasTexture(AtlasTexture thisTarget)
        {
            var window = GetWindow<TextureFineTuningManager>();
            window.SetTarget(thisTarget);
        }

        private void SetTarget(AtlasTexture thisTarget) { _fineTuningManageTarget = thisTarget; InitializeGUI(); }
        private void SetTarget(ChangeEvent<UnityEngine.Object> evt) { SetTarget(evt.newValue as AtlasTexture); }

        public void InitializeGUI()
        {
            rootVisualElement.Clear();
            rootVisualElement.Add(GenerateTopBar());
            rootVisualElement.Add(AtlasCalculateAndCreateManagerUIElement(_fineTuningManageTarget));
        }

        private VisualElement GenerateTopBar()
        {
            var topBar = new VisualElement();
            topBar.style.flexShrink = 0;
            topBar.style.flexDirection = FlexDirection.Row;

            var targetView = new ObjectField("TuningManageTarget");
            targetView.style.flexGrow = 1;
            targetView.objectType = typeof(AtlasTexture);
            targetView.value = _fineTuningManageTarget;
            targetView.RegisterValueChangedCallback(SetTarget);
            topBar.hierarchy.Add(targetView);

            var refreshButton = new Button(InitializeGUI);
            refreshButton.text = "Refresh";
            topBar.hierarchy.Add(refreshButton);
            return topBar;
        }

        public void CreateGUI()
        {
            if (rootVisualElement.childCount == 0) { rootVisualElement.Add(GenerateTopBar()); }
        }

        VisualElement AtlasCalculateAndCreateManagerUIElement(AtlasTexture atlasTexture)
        {
            if (PreviewUtility.IsPreviewContains) { return null; }
            if (atlasTexture == null) { return null; }
            var atlasTextureSerializeObject = new SerializedObject(atlasTexture);

            var domainRoot = DomainMarkerFinder.FindMarker(atlasTexture.gameObject);
            if (domainRoot == null) { return null; }//TODO : ここ返す値何とかする
            _previewedAtlasTexture = GetAtlasTextureResult(atlasTexture, domainRoot);
            if (_previewedAtlasTexture == null) { return null; }

            AutoGenerateTextureIndividualTuning(atlasTexture, _previewedAtlasTexture.Textures.Keys);
            atlasTextureSerializeObject.Update();

            var viRoot = new ScrollView();
            var content = viRoot.Q<VisualElement>("unity-content-container");

            CreateManagerUIElement(content, atlasTexture, atlasTextureSerializeObject);

            return viRoot;
        }

        private void CreateManagerUIElement(VisualElement content, AtlasTexture atlasTexture, SerializedObject atlasTextureSerializeObject)
        {
            content.hierarchy.Clear();

            var atlasTexFineTuningTargets = FineTuning.TexFineTuningUtility.InitTexFineTuning(_previewedAtlasTexture.Textures);
            AtlasTexture.SetSizeDataMaxSize(atlasTexFineTuningTargets, _previewedAtlasTexture.SourceTextureMaxSize);
            foreach (var fineTuning in atlasTexture.AtlasSetting.TextureFineTuning)
            { fineTuning?.AddSetting(atlasTexFineTuningTargets); }

            var sAtlasSetting = atlasTextureSerializeObject.FindProperty("AtlasSetting");
            var individualTuningSerializedProperty = sAtlasSetting.FindPropertyRelative("TextureIndividualFineTuning");
            var targetPropName2SerializedProperty = GetProp2Dict(individualTuningSerializedProperty);



            foreach (var tuningHolderKV in atlasTexFineTuningTargets)
            {
                var wrapper = new IndividualTuningUIElementWrapper(tuningHolderKV.Key, tuningHolderKV.Value, targetPropName2SerializedProperty[tuningHolderKV.Key]);
                content.hierarchy.Add(wrapper.GetVisualElement);
            }
        }

        Dictionary<string, SerializedProperty> GetProp2Dict(SerializedProperty individualTuningSerializedProperty)
        {
            var dict = new Dictionary<string, SerializedProperty>();
            for (var index = 0; individualTuningSerializedProperty.arraySize > index; index += 1)
            {
                var individualTuningSerialized = individualTuningSerializedProperty.GetArrayElementAtIndex(index);
                var propertyName = individualTuningSerialized.FindPropertyRelative("TuningTarget").stringValue;

                if (dict.ContainsKey(propertyName)) { continue; }
                dict.Add(propertyName, individualTuningSerialized);
            }
            return dict;
        }

        internal static void AutoGenerateTextureIndividualTuning(AtlasTexture atlasTexture, IEnumerable<string> property)
        {
            Undo.RecordObject(atlasTexture, "Add-TextureIndividualTuning");

            var generateTarget = property.ToHashSet();
            foreach (var prop in atlasTexture.AtlasSetting.TextureIndividualFineTuning.Select(i => i.TuningTarget))
            { generateTarget.Remove(prop); }

            foreach (var propertyName in generateTarget)
            { atlasTexture.AtlasSetting.TextureIndividualFineTuning.Add(new TextureIndividualTuning() { TuningTarget = propertyName }); }
        }
        private AtlasTexture.AtlasData GetAtlasTextureResult(AtlasTexture atlasTexture, GameObject domainRoot)
        {
            using (var previewDomain = new NotWorkDomain(domainRoot.GetComponentsInChildren<Renderer>(true), new TextureManager(true)))
            {

                var result = atlasTexture.TryCompileAtlasTextures(
                    atlasTexture.GetTargetAllowedFilter(previewDomain.EnumerateRenderer()),
                    previewDomain,
                    out var atlasData
                    );

                if (result is false) { return null; }//TODO : ここ返す値何とかする


                foreach (var mesh in atlasData.Meshes) { UnityEngine.Object.DestroyImmediate(mesh.AtlasMesh); }
                return atlasData;
            }
        }
    }

    class IndividualTuningUIElementWrapper
    {
        VisualElement _viRoot;
        string _propertyName;
        Texture2D _previewTexture2D;
        FineTuning.TexFineTuningHolder _texFineTuningHolder;
        SerializedProperty _textureIndividualTuning;

        public IndividualTuningUIElementWrapper(string propertyName, FineTuning.TexFineTuningHolder texFineTuningHolder, SerializedProperty textureIndividualTuning)
        {
            _propertyName = propertyName;
            _previewTexture2D = texFineTuningHolder.Texture2D;
            _texFineTuningHolder = texFineTuningHolder;
            _textureIndividualTuning = textureIndividualTuning;

            _viRoot = new();
            GenerateVisualElements();
        }

        private void GenerateVisualElements()
        {
            _viRoot.hierarchy.Clear();

            _viRoot.style.flexDirection = FlexDirection.Row;
            _viRoot.style.unityTextAlign = TextAnchor.MiddleLeft;

            var previewImage = new Image { image = _previewTexture2D };
            previewImage.style.width = previewImage.style.height = 140;
            _viRoot.hierarchy.Add(previewImage);

            var overrideDescriptionsRoot = new VisualElement();
            overrideDescriptionsRoot.style.width = Length.Percent(80f);
            overrideDescriptionsRoot.style.justifyContent = Justify.SpaceBetween;
            overrideDescriptionsRoot.style.flexGrow = 1;


            var topHorizontal = CreateTopHorizontalElement();
            overrideDescriptionsRoot.hierarchy.Add(topHorizontal);

            var inheritStr = "(inherit)-";

            var pfOverrideResize = new PropertyField();
            pfOverrideResize.BindProperty(_textureIndividualTuning.FindPropertyRelative("TextureSize"));
            overrideDescriptionsRoot.hierarchy.Add(CreateFineTuningDataElement<SizeData>("TextureSize",
                _textureIndividualTuning.FindPropertyRelative("OverrideResize"), pfOverrideResize,
                d => inheritStr + (d?.TextureSize.ToString() ?? _texFineTuningHolder.Texture2D.width.ToString())));

            var pfCompressionData = new PropertyField();
            pfCompressionData.BindProperty(_textureIndividualTuning.FindPropertyRelative("CompressionData"));
            overrideDescriptionsRoot.hierarchy.Add(CreateFineTuningDataElement<TextureCompressionData>("CompressionFormat",
                _textureIndividualTuning.FindPropertyRelative("OverrideCompression"), pfCompressionData,
                d => inheritStr + (d?.FormatQualityValue.ToString() ?? "None")));

            var pfUseMipMap = new PropertyField();
            pfUseMipMap.BindProperty(_textureIndividualTuning.FindPropertyRelative("UseMipMap"));
            overrideDescriptionsRoot.hierarchy.Add(CreateFineTuningDataElement<MipMapData>("MipMap",
                _textureIndividualTuning.FindPropertyRelative("OverrideMipMapRemove"), pfUseMipMap,
                d => inheritStr + (d?.UseMipMap.ToString() ?? "None")));

            var pfLinear = new PropertyField();
            pfLinear.BindProperty(_textureIndividualTuning.FindPropertyRelative("Linear"));
            overrideDescriptionsRoot.hierarchy.Add(CreateFineTuningDataElement<ColorSpaceData>("ColorSpace",
                _textureIndividualTuning.FindPropertyRelative("OverrideColorSpace"), pfLinear,
                d => inheritStr + "is linier " + (d?.Linear.ToString() ?? (!_texFineTuningHolder.Texture2D.isDataSRGB).ToString())));

            _viRoot.hierarchy.Add(overrideDescriptionsRoot);
        }

        private VisualElement CreateTopHorizontalElement()
        {
            var topHorizontal = new VisualElement();
            topHorizontal.name = "TopHorizontalElement";
            topHorizontal.style.flexDirection = FlexDirection.Row;

            var propertyNameLabel = new Label(_propertyName);
            propertyNameLabel.style.fontSize = 18f;
            propertyNameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            propertyNameLabel.tooltip = "click to copy";
            propertyNameLabel.RegisterCallback<ClickEvent>(ce => GUIUtility.systemCopyBuffer = _propertyName);
            topHorizontal.hierarchy.Add(propertyNameLabel);

            var refCpData = _texFineTuningHolder.Find<ReferenceCopyData>();
            var refCopyInherit = refCpData?.CopySource;

            var refCpArrow = new Label(" <-- ");
            topHorizontal.hierarchy.Add(refCpArrow);

            var refCopyLabelStr = refCopyInherit is not null ? "(inherit)" + refCopyInherit : "";
            var refCopyLabel = new Label(refCopyLabelStr);
            topHorizontal.hierarchy.Add(refCopyLabel);

            var refCopyOverrideInput = new TextField();
            refCopyOverrideInput.BindProperty(_textureIndividualTuning.FindPropertyRelative("CopyReferenceSource"));
            refCopyOverrideInput.style.flexGrow = 1;
            topHorizontal.hierarchy.Add(refCopyOverrideInput);

            var padding = new VisualElement();
            padding.style.flexGrow = 1;
            topHorizontal.hierarchy.Add(padding);


            var refCopyOverrideButton = new Button();
            refCopyOverrideButton.text = "ReferenceCopyOverride";
            refCopyOverrideButton.style.width = 160f;
            topHorizontal.hierarchy.Add(refCopyOverrideButton);

            var refCpOverrideBoolSProperty = _textureIndividualTuning.FindPropertyRelative("OverrideAsReferenceCopy");
            refCopyOverrideButton.clicked += () =>
            {
                refCpOverrideBoolSProperty.serializedObject.Update();
                refCpOverrideBoolSProperty.boolValue = !refCpOverrideBoolSProperty.boolValue;
                refCpOverrideBoolSProperty.serializedObject.ApplyModifiedProperties();
                UpdateDisplay();
            };
            UpdateDisplay();
            void UpdateDisplay()
            {
                var nowOverrideUseValue = refCpOverrideBoolSProperty.boolValue;
                refCopyLabel.style.display = nowOverrideUseValue is false ? DisplayStyle.Flex : DisplayStyle.None;
                var finallyEnabledRefCopy = nowOverrideUseValue is false ? (refCopyInherit is not null ? true : false) : true;
                refCpArrow.style.display = finallyEnabledRefCopy ? DisplayStyle.Flex : DisplayStyle.None;
                refCopyOverrideInput.style.display = nowOverrideUseValue ? DisplayStyle.Flex : DisplayStyle.None;
                padding.style.display = nowOverrideUseValue ? DisplayStyle.None : DisplayStyle.Flex;
            }

            return topHorizontal;
        }

        private VisualElement CreateFineTuningDataElement<TuningData>(string dataName, SerializedProperty overrideBoolSProperty, VisualElement overrideInputElement, Func<TuningData, string> getInheritString) where TuningData : class, ITuningData, new()
        {
            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Row;
            root.style.justifyContent = Justify.SpaceBetween;

            var data = _texFineTuningHolder.Find<TuningData>();
            var dataNameLabel = new Label(dataName);
            dataNameLabel.style.width = 120;
            var inheritLabel = new Label(getInheritString(data));
            var overrideButton = new Button() { text = "Override" };
            root.hierarchy.Add(dataNameLabel);
            root.hierarchy.Add(inheritLabel);
            root.hierarchy.Add(overrideInputElement);
            root.hierarchy.Add(overrideButton);
            overrideInputElement.style.flexGrow = 1;

            overrideButton.clicked += () =>
            {
                overrideBoolSProperty.serializedObject.Update();
                overrideBoolSProperty.boolValue = !overrideBoolSProperty.boolValue;
                overrideBoolSProperty.serializedObject.ApplyModifiedProperties();
                UpdateDisplay();
            };

            UpdateDisplay();
            void UpdateDisplay()
            {
                var nowOverrideUseValue = overrideBoolSProperty.boolValue;
                inheritLabel.style.display = nowOverrideUseValue ? DisplayStyle.None : DisplayStyle.Flex;
                overrideInputElement.style.display = nowOverrideUseValue ? DisplayStyle.Flex : DisplayStyle.None;
            }

            return root;
        }

        public VisualElement GetVisualElement => _viRoot;
    }

    class NotWorkDomain : IDomain, IDisposable
    {
        IEnumerable<Renderer> _domainRenderers;
        HashSet<UnityEngine.Object> _transferredObject = new();
        protected readonly ITextureManager _textureManager;

        public NotWorkDomain(IEnumerable<Renderer> renderers, TextureManager textureManager)
        {
            _domainRenderers = renderers;
            _textureManager = textureManager;
        }

        public void AddTextureStack<BlendTex>(Texture dist, BlendTex setTex) where BlendTex : TextureBlend.IBlendTexturePair { }
        public IEnumerable<Renderer> EnumerateRenderer() { return _domainRenderers; }
        public ITextureManager GetTextureManager() { return _textureManager; }
        public bool IsPreview() { return true; }
        public bool OriginEqual(UnityEngine.Object l, UnityEngine.Object r) { return l == r; }
        public void RegisterReplace(UnityEngine.Object oldObject, UnityEngine.Object nowObject) { }
        public void ReplaceMaterials(Dictionary<Material, Material> mapping, bool one2one = true) { }
        public void SetMesh(Renderer renderer, Mesh mesh) { }
        public void TransferAsset(UnityEngine.Object asset) { _transferredObject.Add(asset); }
        public void Dispose() { foreach (var obj in _transferredObject) { UnityEngine.Object.DestroyImmediate(obj); } }
    }


}

