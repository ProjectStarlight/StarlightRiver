sampler uImage0 : register(s0); // The contents of the screen

float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float4 uShaderSpecificData;

float progresses[10];
float2 positions[10];
float intensity[10];
float numberOfPoints;

float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

float4 ColorSucker(float4 unused : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    
    float2 center = {0.5, 0.5};
    float2 normalizedCoords = (coords - center) * uScreenResolution / uScreenResolution.x + center;

    float dist = clamp(distance(normalizedCoords, center) * 20.0 - 20.0 + uIntensity * 20.0, 0.0, 1.0);
    
    float grey = (color.r + color.g + color.b) / 3.0;
    
    float4 final = grey * dist + color * (1.0 - dist);

    return final;
}

technique Technique1
{
    pass ColorSuckerPass
    {
        PixelShader = compile ps_2_0 ColorSucker();
    }
}