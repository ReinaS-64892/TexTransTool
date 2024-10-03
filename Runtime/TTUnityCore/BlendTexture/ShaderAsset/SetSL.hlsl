
float GetLum(float3 rgbCol)
{
    return (0.3 * rgbCol.r) + (0.59 * rgbCol.g) + (0.11 * rgbCol.b);
}

float3 ClipColor(float3 rgbCol)
{
    float l = GetLum(rgbCol);
    float n = min(rgbCol.r,min(rgbCol.g,rgbCol.b));
    float x = max(rgbCol.r,max(rgbCol.g,rgbCol.b));

    if(n < 0.0)
    {
        rgbCol.r = l + (((rgbCol.r - l) * l) / (l - n));
        rgbCol.g = l + (((rgbCol.g - l) * l) / (l - n));
        rgbCol.b = l + (((rgbCol.b - l) * l) / (l - n));
    }

    if(x > 1.0)
    {
        rgbCol.r = l + (((rgbCol.r - l) * (1 - l)) / (x - l));
        rgbCol.g = l + (((rgbCol.g - l) * (1 - l)) / (x - l));
        rgbCol.b = l + (((rgbCol.b - l) * (1 - l)) / (x - l));
    }

    return rgbCol;
}

float3 SetLum(float3 rgbCol,float l)
{
    float d = l - GetLum(rgbCol);

    rgbCol.r += d;
    rgbCol.g += d;
    rgbCol.b += d;

    return ClipColor(rgbCol);
}


float GetSat(float3 rgbCol)//双円錐モデル
{
    return max(max(rgbCol.r,rgbCol.g),rgbCol.b) -  min(min(rgbCol.r,rgbCol.g),rgbCol.b) ;
}

float SetSatChannel(float col,float maxVal,float minVal,float medVal,float sat,float notSat)
{
    float thisMax = col == maxVal;
    float thisMin = col == minVal;
    float thisMed = col == medVal;
    float avoidNan = notSat > 0.5 ? 2.4414e-4 : 0 ;

    float med = (((col - minVal) * sat) / (maxVal - minVal + avoidNan)) * thisMed;
    return ((sat * thisMax) + med) *( 1 - notSat );
}

float3 SetSat(float3 rgbCol,float s)
{
    float maxVal = max(max(rgbCol.r,rgbCol.g),rgbCol.b);
    float minVal = min(min(rgbCol.r,rgbCol.g),rgbCol.b);
    float medVal = min(max(rgbCol.r,rgbCol.g),rgbCol.b);

    float notSat = maxVal == minVal;

    rgbCol.r = SetSatChannel(rgbCol.r , maxVal, minVal, medVal, s ,notSat);
    rgbCol.g = SetSatChannel(rgbCol.g , maxVal, minVal, medVal, s ,notSat);
    rgbCol.b = SetSatChannel(rgbCol.b , maxVal, minVal, medVal, s ,notSat);

    return rgbCol;
}
