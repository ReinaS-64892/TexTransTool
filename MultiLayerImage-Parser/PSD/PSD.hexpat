#pragma author Reina_Sakiria
#pragma description Photshop Document

#pragma endian big

import std.math;

enum ColorMode : u16 {

    Bitmap = 0,
    Grayscale = 1,
    Indexed = 2,
    RGB = 3,
    CMYK = 4,
    Multichannel = 7,
    Duotone = 8,
    Lab = 9,

};

struct Header {

    char Signature[4];
    u16 Version;
    u8 Resaved[6];
    u16 ChannelCount;
    u32 Height;
    u32 Width;
    u16 BitDepth;
    ColorMode ColoMode;

};


Header hader @ $;  

struct ColorModeData {

    u32 Length;

    u8 UnkownData[Length];

};


ColorModeData ColorModeData @ $;


struct ImageResource {

    char Signature[4];
    u16 ImageResourceID;

    u8 PascalStrigLength;

    char ImageResouseBloackname[PascalStrigLength];
    if(PascalStrigLength == 0 || ( PascalStrigLength % 2 ) == 0) { u8 nullChar[1]; }

    u32 ActualDataSizeFollows;

    u32 trueLength = (ActualDataSizeFollows % 2 ) == 0  ? ActualDataSizeFollows : ActualDataSizeFollows + 1 ;
    u8 FollowsUnkownData[trueLength];

};

struct ImageResources {

    u32 Length;
    u64 IR_End =  $ + Length;

    if(Length > 0)
    {
        //重くなるからなし！
        //ImageResource Resoucers[while ($ < IR_End)];
        padding[Length];
    }

};

ImageResources ImageResources @ $;

struct RectTangle {

    s32 Top;
    s32 Left;
    s32 Bottom;
    s32 Right;

};

enum  ChannelIDWithRGB : s16 {
    Red = 0,
    Green = 1,
    Blue = 2,
    Transparency = -1,
    UserLayerMask = -2,
    RealUserLayerMask = -3,
};

struct ChannelInformation {

    ChannelIDWithRGB channnelID;
    
    if (hader.Version == 1){ u32 CorrespondingChannelDataLength; }
    else if (hader.Version == 2){ u64 CorrespondingChannelDataLength; }
    
};

bitfield LayerFlag {

    TransparencyProtected: 1;
    NotVisible:1;
    Obsolete:1;
    UsefulInformation4Bit:1;
    NotDocPixelData:1;

};

bitfield MaskOrAdjustmentFlag {

    PosRelToLayer:1;
    MaskDisabled:1;
    InvertMask:1;
    UserMaskActuallyCame:1;
    UserOrVectorMasksHave:1;

};

bitfield MaskParametersFrag {

    UserMaskDensity:1;
    UserMaskFeather:1;
    VectorMaskDensity:1;
    VectorMaskFeather:1;

};

struct  MaskParametersFragAndFollows {

    MaskParametersFrag MaskParametersFrag;

    if(MaskParametersFrag.UserMaskDensity) { u8 UserMaskDensity; }
    if(MaskParametersFrag.UserMaskFeather) { double UserMaskFeather;}
    if(MaskParametersFrag.VectorMaskDensity) { u8 VectorMaskDensity; }
    if(MaskParametersFrag.VectorMaskFeather) { double VectorMaskFeather;}

};

struct LayerMaskAdjustmentLayerData {

    u32 SizeOfData;

    if (SizeOfData > 0)
    {

        RectTangle Rect;
        u8 DefaultColor;
        MaskOrAdjustmentFlag MaskOrAdjustmentFlag;

        if(MaskOrAdjustmentFlag.UserOrVectorMasksHave)
        {

        MaskParametersFragAndFollows MaskParametersFragAndFollows;

        }

        if (SizeOfData == 20 ) { padding[2]; }
        else
        {

            MaskOrAdjustmentFlag RealFlag;
            u8 ReadUserMaskBackground;
            RectTangle RealRectTangleLayerMask;

        }

    }

};

struct LayerBlendingRangeData {

    u8 CompositeGrayBlendSourceBlack1;
    u8 CompositeGrayBlendSourceBlack2;
    u8 CompositeGrayBlendSourceWhite1;
    u8 CompositeGrayBlendSourceWhite2;
    u32 CompositeGrayBlendDestinationRange;

};

struct LayerBlendingRangeDataArray {

    u32 Length;

    if (Length > 0) {

        u64 ArrayEnd = $ + Length;

        LayerBlendingRangeData LayerBlendingRangeData[while ($ < ArrayEnd)];

    }

};

struct PascalStringLayerName {

    u8 PascalStringLength;

    char LayerName[PascalStringLength];

    u8 readLength = PascalStringLength + 1;
    if ((readLength % 4) != 0)
    {
        padding[4 - (readLength % 4)];
    }

};

u8 IsCanvasType = 0;

struct AdditionalLayerInformation {

    char Signature[4];
    char Key[4];

    if (hader.Version == 1){ u32 Length; }
    else if (hader.Version == 2){

        match (Key)
        {
            ("LMsk"): u64 Length;
            ("Lr16"): u64 Length;
            ("Lr32"): u64 Length;
            ("Layr"): u64 Length;
            ("Mt16"): u64 Length;
            ("Mt32"): u64 Length;
            ("Mtrn"): u64 Length;
            ("Alph"): u64 Length;
            ("FMsk"): u64 Length;
            ("lnk2"): u64 Length;
            ("FEid"): u64 Length;
            ("FXid"): u64 Length;
            ("PxSD"): u64 Length;
            ("Lr32"): u64 Length;
            (_): u32 Length;

        }

    }

    u8 AdditionalData[Length];

    if (IsCanvasType == 1){
        if ((Length % 4) != 0)
        {
            padding[4 - (Length % 4)];
        }
    }
};
struct LayerRecode {

    RectTangle Rect;

    u16 LayerInChannelCount;
    ChannelInformation channelInformation[LayerInChannelCount];

    char Signature[4];
    char BlendModeKey[4];

    u8 Opacity;
    u8 Clipping;

    LayerFlag layerFlag;
    u8 Filler;

    u32 ExtraDataFieldLength;

    u64 EDF_Eed = $ + ExtraDataFieldLength;

    LayerMaskAdjustmentLayerData LayerMaskAdjustmentLayerData;
    LayerBlendingRangeDataArray LayerBlendingRangeDataArray;

    PascalStringLayerName PascalStringLayerName;

    AdditionalLayerInformation AdditionalLayerInformationArray[while ($ < EDF_Eed)];
    //padding[while ($ < EDF_Eed)];

};

enum Compression : u16 {

    RawData = 0,
    RLECompressed = 1,
    ZIPWithoutPrediction = 2,
    ZIPWithPrediction = 3,

};

/*
struct ChannelImageData {
Compression Compression;
u8 CompressedImageData[];
};
*/

struct LayerInfo {

    if (hader.Version == 1){ u32 Length; }
    else if (hader.Version == 2){ u64 Length; }
    
    if(Length > 0){

        u64 LayerInfo_End = $ + Length;
        s16 LayerCount;

        LayerRecode layerRecode[std::math::abs(LayerCount)];

        //パターンランゲージ何もわからんからあきらめた
        //u8 ChannelImageData[ LayerInfo_End - $ ];
        //重たくなっちゃうからスキップ
        padding[LayerInfo_End - $];
    }

};


struct GlobalLayerMaskInfo {

    u32 Length;
    u64 GlmI_End = $ + Length;

    if (Length > 0) 
    {
    
        u16 OverlayColorSpace;
        
        //Maybe
        u16 ColorR;
        u16 ColorG;
        u16 ColorB;
        u16 ColorA;
        
        u16 Opacity;
        u8 Kind;

        padding[ GlmI_End - $ ];
    }


};


struct LayerAndMaskInformation {

    if (hader.Version == 1){ u32 Length; }
    else if (hader.Version == 2){ u64 Length; }

    u64 LaM_Eed = $ + Length;

    if(Length > 0) {

        LayerInfo LayerInfo;
        
        GlobalLayerMaskInfo GlobalLayerMaskInfo;
        
        IsCanvasType = 1;

        AdditionalLayerInformation CanvasTypeAdditionalLayerInformation[while ($ < LaM_Eed)];
    }

};

LayerAndMaskInformation LayerAndMaskInformation @ $;

//重たくなっちゃうからスキップ
//u8 ImageData[while (!std::mem::eof())] @ $;
