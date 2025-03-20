using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using net.rs64.TexTransTool.TextureAtlas.FineTuning;
using net.rs64.TexTransTool.Editor.OtherMenuItem;
using net.rs64.TexTransCore;

namespace net.rs64.TexTransTool.TextureAtlas.Editor
{
//     internal class TextureFineTuningManager : EditorWindow
//     {
//         [SerializeField] AtlasTexture _fineTuningManageTarget;
//         public AtlasTexture FineTuningManageTarget => _fineTuningManageTarget;

//         AtlasTexture.AtlasData _previewedAtlasTexture;
//         internal static void OpenAtlasTexture(AtlasTexture thisTarget)
//         {
//             var window = GetWindow<TextureFineTuningManager>();
//             window.SetTarget(thisTarget);
//         }

//         private void SetTarget(AtlasTexture thisTarget) { _fineTuningManageTarget = thisTarget; InitializeGUI(); }
//         private void SetTarget(ChangeEvent<UnityEngine.Object> evt) { SetTarget(evt.newValue as AtlasTexture); }

//         public void InitializeGUI()
//         {
//             rootVisualElement.Clear();
//             rootVisualElement.Add(GenerateTopBar());
//             rootVisualElement.Add(AtlasCalculateAndCreateManagerUIElement(_fineTuningManageTarget));
//         }

//         private VisualElement GenerateTopBar()
//         {
//             var topBar = new VisualElement();
//             topBar.style.flexShrink = 0;
//             topBar.style.flexDirection = FlexDirection.Row;

//             var targetView = new ObjectField("TuningManageTarget");
//             targetView.style.flexGrow = 1;
//             targetView.objectType = typeof(AtlasTexture);
//             targetView.value = _fineTuningManageTarget;
//             targetView.RegisterValueChangedCallback(SetTarget);
//             topBar.hierarchy.Add(targetView);

//             var refreshButton = new Button(InitializeGUI);
//             refreshButton.text = "Refresh";
//             topBar.hierarchy.Add(refreshButton);
//             return topBar;
//         }

//         public void CreateGUI()
//         {
//             if (rootVisualElement.childCount == 0) { rootVisualElement.Add(GenerateTopBar()); }
//         }

//         VisualElement AtlasCalculateAndCreateManagerUIElement(AtlasTexture atlasTexture)
//         {
//             if (PreviewUtility.IsPreviewContains) { return null; }
//             if (atlasTexture == null) { return null; }
//             var atlasTextureSerializeObject = new SerializedObject(atlasTexture);

//             var domainRoot = DomainMarkerFinder.FindMarker(atlasTexture.gameObject);
//             if (domainRoot == null) { return null; }//TODO : ここ返す値何とかする
//             _previewedAtlasTexture = GetAtlasTextureResult(atlasTexture, domainRoot);
//             if (_previewedAtlasTexture == null) { return null; }

//             AutoGenerateTextureIndividualTuning(atlasTexture, _previewedAtlasTexture.Textures.Keys);
//             atlasTextureSerializeObject.Update();

//             var viRoot = new ScrollView();
//             var content = viRoot.Q<VisualElement>("unity-content-container");

//             CreateManagerUIElement(content, atlasTexture, atlasTextureSerializeObject);

//             return viRoot;
//         }

//         private void CreateManagerUIElement(VisualElement content, AtlasTexture atlasTexture, SerializedObject atlasTextureSerializeObject)
//         {
//             content.hierarchy.Clear();

//             var atlasTexFineTuningTargets = FineTuning.TexFineTuningUtility.InitTexFineTuning(_previewedAtlasTexture.Textures);
//             AtlasTexture.SetSizeDataMaxSize(atlasTexFineTuningTargets, _previewedAtlasTexture.SourceTextureMaxSize);
//             AtlasTexture.DefaultMargeTextureDictTuning(atlasTexFineTuningTargets, _previewedAtlasTexture.MargeTextureDict);
//             AtlasTexture.DefaultRefCopyTuning(atlasTexFineTuningTargets, _previewedAtlasTexture.ReferenceCopyDict);
//             foreach (var fineTuning in atlasTexture.AtlasSetting.TextureFineTuning)
//             { fineTuning?.AddSetting(atlasTexFineTuningTargets); }

//             var sAtlasSetting = atlasTextureSerializeObject.FindProperty("AtlasSetting");
//             var individualTuningSerializedProperty = sAtlasSetting.FindPropertyRelative("TextureIndividualFineTuning");
//             var targetPropName2SerializedProperty = GetProp2Dict(individualTuningSerializedProperty);



//             foreach (var tuningHolderKV in atlasTexFineTuningTargets)
//             {
//                 var wrapper = new IndividualTuningUIElementWrapper(tuningHolderKV.Key, tuningHolderKV.Value, targetPropName2SerializedProperty[tuningHolderKV.Key]);
//                 content.hierarchy.Add(wrapper.GetVisualElement);
//             }
//         }

//         Dictionary<string, SerializedProperty> GetProp2Dict(SerializedProperty individualTuningSerializedProperty)
//         {
//             var dict = new Dictionary<string, SerializedProperty>();
//             for (var index = 0; individualTuningSerializedProperty.arraySize > index; index += 1)
//             {
//                 var individualTuningSerialized = individualTuningSerializedProperty.GetArrayElementAtIndex(index);
//                 var propertyName = individualTuningSerialized.FindPropertyRelative("TuningTarget").stringValue;

//                 if (dict.ContainsKey(propertyName)) { continue; }
//                 dict.Add(propertyName, individualTuningSerialized);
//             }
//             return dict;
//         }

//         internal static void AutoGenerateTextureIndividualTuning(AtlasTexture atlasTexture, IEnumerable<string> property)
//         {
//             Undo.RecordObject(atlasTexture, "Add-TextureIndividualTuning");

//             var generateTarget = property.ToHashSet();
//             foreach (var prop in atlasTexture.AtlasSetting.TextureIndividualFineTuning.Select(i => i.TuningTarget))
//             { generateTarget.Remove(prop); }

//             foreach (var propertyName in generateTarget)
//             { atlasTexture.AtlasSetting.TextureIndividualFineTuning.Add(new TextureIndividualTuning() { TuningTarget = propertyName }); }
//         }
//         private AtlasTexture.AtlasData GetAtlasTextureResult(AtlasTexture atlasTexture, GameObject domainRoot)
//         {
//             var texManage = new TextureManager(true);
//             using var previewDomain = new NotWorkDomain(domainRoot.GetComponentsInChildren<Renderer>(true), texManage, new TTCEUnityWithTTT4Unity(new UnityDiskUtil(texManage)));


//             var nowRenderers = atlasTexture.GetTargetAllowedFilter(previewDomain.EnumerateRenderer());
//             var targetMaterials = atlasTexture.GetTargetMaterials(previewDomain, nowRenderers);
//             if (targetMaterials.Any() is false) { return null; }

//             var result = atlasTexture.TryCompileAtlasTextures(
//                 atlasTexture.GetTargetAllowedFilter(previewDomain.EnumerateRenderer()), targetMaterials,
//                 previewDomain,
//                 out var atlasData
//                 );

//             if (result is false) { return null; }//TODO : ここ返す値何とかする


//             foreach (var mesh in atlasData.Meshes) { UnityEngine.Object.DestroyImmediate(mesh.AtlasMesh); }
//             return atlasData;

//         }
//     }

//     class IndividualTuningUIElementWrapper
//     {
//         VisualElement _viRoot;
//         string _propertyName;
//         Texture2D _previewTexture2D;
//         FineTuning.TexFineTuningHolder _texFineTuningHolder;
//         SerializedProperty _textureIndividualTuning;

//         public IndividualTuningUIElementWrapper(string propertyName, FineTuning.TexFineTuningHolder texFineTuningHolder, SerializedProperty textureIndividualTuning)
//         {
//             _propertyName = propertyName;
//             _previewTexture2D = texFineTuningHolder.Texture2D;
//             _texFineTuningHolder = texFineTuningHolder;
//             _textureIndividualTuning = textureIndividualTuning;

//             _viRoot = new();
//             GenerateVisualElements();
//         }

//         private void GenerateVisualElements()
//         {
//             _viRoot.hierarchy.Clear();

//             _viRoot.style.flexDirection = FlexDirection.Row;
//             _viRoot.style.unityTextAlign = TextAnchor.MiddleLeft;

//             var previewImage = new Image { image = _previewTexture2D };
//             previewImage.style.width = previewImage.style.height = 140;
//             _viRoot.hierarchy.Add(previewImage);

//             var overrideDescriptionsRoot = new VisualElement();
//             overrideDescriptionsRoot.style.width = Length.Percent(80f);
//             overrideDescriptionsRoot.style.justifyContent = Justify.SpaceBetween;
//             overrideDescriptionsRoot.style.flexGrow = 1;


//             var topHorizontal = CreateTopHorizontalElement();
//             overrideDescriptionsRoot.hierarchy.Add(topHorizontal);

//             var inheritStr = "(inherit)-";

//             var pfRemove = new PropertyField();
//             pfRemove.BindProperty(_textureIndividualTuning.FindPropertyRelative("IsRemove"));
//             overrideDescriptionsRoot.hierarchy.Add(CreateFineTuningDataElement<RemoveData>("Remove",
//                 _textureIndividualTuning.FindPropertyRelative("OverrideRemove"), pfRemove,
//                 d => inheritStr + (d?.IsRemove.ToString() ?? false.ToString())));

//             var pfOverrideResize = new PropertyField();
//             pfOverrideResize.BindProperty(_textureIndividualTuning.FindPropertyRelative("TextureSize"));
//             overrideDescriptionsRoot.hierarchy.Add(CreateFineTuningDataElement<SizeData>("TextureSize",
//                 _textureIndividualTuning.FindPropertyRelative("OverrideResize"), pfOverrideResize,
//                 d => inheritStr + (d?.TextureSize.ToString() ?? _texFineTuningHolder.Texture2D.width.ToString())));

//             var pfCompressionData = new PropertyField();
//             pfCompressionData.BindProperty(_textureIndividualTuning.FindPropertyRelative("CompressionData"));
//             overrideDescriptionsRoot.hierarchy.Add(CreateFineTuningDataElement<TextureCompressionData>("CompressionFormat",
//                 _textureIndividualTuning.FindPropertyRelative("OverrideCompression"), pfCompressionData,
//                 d => inheritStr + (d is not null ? (d.UseOverride ? "UseOverride-" + d.OverrideTextureFormat.ToString() : d.FormatQualityValue.ToString()) : "None")));

//             var pfUseMipMap = new PropertyField();
//             pfUseMipMap.BindProperty(_textureIndividualTuning.FindPropertyRelative("UseMipMap"));
//             overrideDescriptionsRoot.hierarchy.Add(CreateFineTuningDataElement<MipMapData>("MipMap",
//                 _textureIndividualTuning.FindPropertyRelative("OverrideMipMapRemove"), pfUseMipMap,
//                 d => inheritStr + (d?.UseMipMap.ToString() ?? "None")));

//             var pfLinear = new PropertyField();
//             pfLinear.BindProperty(_textureIndividualTuning.FindPropertyRelative("Linear"));
//             overrideDescriptionsRoot.hierarchy.Add(CreateFineTuningDataElement<ColorSpaceData>("ColorSpace",
//                 _textureIndividualTuning.FindPropertyRelative("OverrideColorSpace"), pfLinear,
//                 d => inheritStr + "Linear-is-" + (d?.Linear.ToString() ?? (!_texFineTuningHolder.Texture2D.isDataSRGB).ToString())));

//             _viRoot.hierarchy.Add(overrideDescriptionsRoot);
//         }

//         private VisualElement CreateTopHorizontalElement()
//         {
//             var topHorizontal = new VisualElement();
//             topHorizontal.name = "TopHorizontalElement";
//             topHorizontal.style.flexDirection = FlexDirection.Row;

//             var propertyNameLabel = new Label(_propertyName);
//             propertyNameLabel.style.fontSize = 18f;
//             propertyNameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
//             propertyNameLabel.tooltip = "click to copy";
//             propertyNameLabel.RegisterCallback<ClickEvent>(ce => GUIUtility.systemCopyBuffer = _propertyName);
//             topHorizontal.hierarchy.Add(propertyNameLabel);

//             var topHorizontalCol = new VisualElement();
//             topHorizontalCol.style.flexDirection = FlexDirection.Column;
//             topHorizontalCol.style.flexGrow = 1;
//             topHorizontal.hierarchy.Add(topHorizontalCol);



//             var refCpData = _texFineTuningHolder.Find<ReferenceCopyData>();
//             var refCopyInherit = refCpData?.CopySource;

//             var sCopyReferenceSource = _textureIndividualTuning.FindPropertyRelative("CopyReferenceSource");
//             var refCpOverrideBoolSProperty = _textureIndividualTuning.FindPropertyRelative("OverrideReferenceCopy");

//             topHorizontalCol.hierarchy.Add(NewMethod(" <-- Copy-from ", refCopyInherit, "ReferenceCopyOverride", sCopyReferenceSource, refCpOverrideBoolSProperty));


//             var mergeTexData = _texFineTuningHolder.Find<MergeTextureData>();
//             var mergeTexInherit = mergeTexData?.MargeParent;

//             var sMargeRootProperty = _textureIndividualTuning.FindPropertyRelative("MargeRootProperty");
//             var sOverrideAsMargeTexture = _textureIndividualTuning.FindPropertyRelative("OverrideMargeTexture");

//             topHorizontalCol.hierarchy.Add(NewMethod(" --> MergeParent-as ", mergeTexInherit, "MergeTextureOverride", sMargeRootProperty, sOverrideAsMargeTexture));





//             return topHorizontal;
//         }

//         private VisualElement NewMethod(string arrowLabelStr, string inheritValueStr, string overrideButtonText, SerializedProperty strInputSerializedProperty, SerializedProperty boolOverrideSerializedProperty)
//         {
//             var topHorizontalRefCopRow = new VisualElement();
//             topHorizontalRefCopRow.style.flexDirection = FlexDirection.Row;

//             var refCpArrow = new Label(arrowLabelStr);
//             topHorizontalRefCopRow.hierarchy.Add(refCpArrow);

//             var inheritLabelStr = inheritValueStr is not null ? "(inherit)" + inheritValueStr : "";
//             var inheritLabel = new Label(inheritLabelStr);
//             topHorizontalRefCopRow.hierarchy.Add(inheritLabel);

//             var overrideInput = new TextField();
//             overrideInput.BindProperty(strInputSerializedProperty);
//             overrideInput.style.flexGrow = 1;
//             topHorizontalRefCopRow.hierarchy.Add(overrideInput);

//             var padding = new VisualElement();
//             padding.style.flexGrow = 1;
//             topHorizontalRefCopRow.hierarchy.Add(padding);


//             var refCopyOverrideButton = new Button();
//             refCopyOverrideButton.text = overrideButtonText;
//             refCopyOverrideButton.style.width = 160f;
//             topHorizontalRefCopRow.hierarchy.Add(refCopyOverrideButton);

//             refCopyOverrideButton.clicked += () =>
//             {
//                 boolOverrideSerializedProperty.serializedObject.Update();
//                 boolOverrideSerializedProperty.boolValue = !boolOverrideSerializedProperty.boolValue;
//                 boolOverrideSerializedProperty.serializedObject.ApplyModifiedProperties();
//                 UpdateDisplayRefCp();
//             };
//             UpdateDisplayRefCp();
//             void UpdateDisplayRefCp()
//             {
//                 var nowOverrideUseValue = boolOverrideSerializedProperty.boolValue;
//                 inheritLabel.style.display = nowOverrideUseValue is false ? DisplayStyle.Flex : DisplayStyle.None;
//                 var finallyEnabledRefCopy = nowOverrideUseValue is false ? (inheritValueStr is not null ? true : false) : true;
//                 refCpArrow.style.display = finallyEnabledRefCopy ? DisplayStyle.Flex : DisplayStyle.None;
//                 overrideInput.style.display = nowOverrideUseValue ? DisplayStyle.Flex : DisplayStyle.None;
//                 padding.style.display = nowOverrideUseValue ? DisplayStyle.None : DisplayStyle.Flex;
//             }

//             return topHorizontalRefCopRow;
//         }

//         private VisualElement CreateFineTuningDataElement<TuningData>(string dataName, SerializedProperty overrideBoolSProperty, VisualElement overrideInputElement, Func<TuningData, string> getInheritString) where TuningData : class, ITuningData, new()
//         {
//             var root = new VisualElement();
//             root.style.flexDirection = FlexDirection.Row;
//             root.style.justifyContent = Justify.SpaceBetween;

//             var data = _texFineTuningHolder.Find<TuningData>();
//             var dataNameLabel = new Label(dataName);
//             dataNameLabel.style.width = 120;
//             var inheritLabel = new Label(getInheritString(data));
//             var overrideButton = new Button() { text = "Override" };
//             root.hierarchy.Add(dataNameLabel);
//             root.hierarchy.Add(inheritLabel);
//             root.hierarchy.Add(overrideInputElement);
//             root.hierarchy.Add(overrideButton);
//             overrideInputElement.style.flexGrow = 1;

//             overrideButton.clicked += () =>
//             {
//                 overrideBoolSProperty.serializedObject.Update();
//                 overrideBoolSProperty.boolValue = !overrideBoolSProperty.boolValue;
//                 overrideBoolSProperty.serializedObject.ApplyModifiedProperties();
//                 UpdateDisplay();
//             };

//             UpdateDisplay();
//             void UpdateDisplay()
//             {
//                 var nowOverrideUseValue = overrideBoolSProperty.boolValue;
//                 inheritLabel.style.display = nowOverrideUseValue ? DisplayStyle.None : DisplayStyle.Flex;
//                 overrideInputElement.style.display = nowOverrideUseValue ? DisplayStyle.Flex : DisplayStyle.None;
//             }

//             return root;
//         }

//         public VisualElement GetVisualElement => _viRoot;
//     }

    internal class NotWorkDomain : IDomain, IDisposable
    {
        IEnumerable<Renderer> _domainRenderers;
        HashSet<UnityEngine.Object> _transferredObject = new();
        protected readonly ITextureManager _textureManager;
        private readonly ITexTransToolForUnity _ttce4U;

        public NotWorkDomain(IEnumerable<Renderer> renderers, TextureManager textureManager, ITexTransToolForUnity iTexTransToolForUnity)
        {
            _domainRenderers = renderers;
            _textureManager = textureManager;
            _ttce4U = iTexTransToolForUnity;
        }

        public void AddTextureStack(Texture dist, ITTRenderTexture addTex, ITTBlendKey blendKey) { }
        public IEnumerable<Renderer> EnumerateRenderer() { return _domainRenderers; }
        public ITextureManager GetTextureManager() { return _textureManager; }
        public bool IsPreview() { return true; }
        public bool OriginEqual(UnityEngine.Object l, UnityEngine.Object r) { return l == r; }
        public void RegisterReplace(UnityEngine.Object oldObject, UnityEngine.Object nowObject) { }
        public void ReplaceMaterials(Dictionary<Material, Material> mapping, bool one2one = true) { }
        public void SetMesh(Renderer renderer, Mesh mesh) { }
        public void TransferAsset(UnityEngine.Object asset) { _transferredObject.Add(asset); }
        public void Dispose() { foreach (var obj in _transferredObject) { UnityEngine.Object.DestroyImmediate(obj); } _textureManager.Dispose(); }
        public ITexTransToolForUnity GetTexTransCoreEngineForUnity() => _ttce4U;

    }


}

