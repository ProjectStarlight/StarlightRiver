#include "Common.fxh"

sampler uImage0 : register(s0);

texture uImage1;
sampler2D uImage1Sampler = sampler_state
{
    texture = <uImage1>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

texture uImage2;
sampler2D uImage2Sampler = sampler_state
{
    texture = <uImage2>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float power;
float speed;
float opacity;

float uProgress;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float2 noiseCoords = coords + float2(uTime * 0.5, uTime * 0.1); 
   
    float4 noiseColor = tex2D(uImage2Sampler, noiseCoords);
    
    float2 distortionOffset = (noiseColor.rg - 0.5) * 0.1;
    
    float2 uv = coords + distortionOffset;
    
    float4 shapeColor = tex2D(uImage0, uv);
    
    float4 lightningColor = tex2D(uImage1Sampler, uv * 2.0);
    
    float4 finalColor = shapeColor * lightningColor;

    float dissolveValue = noiseColor.r;
    
    float lerper = 1 - uProgress;
    
    if (dissolveValue > lerper)
    {
        finalColor.rgb *= float3(0, 0, 0);
    }

    if (lightningColor.r > uProgress && finalColor.r > 0)
    {
        finalColor.rgb += float3(0.5, 0.5, 0.5);
    }
    
    if (finalColor.r > lerper)
    {
        finalColor.rgb += float3(1.0, 1.0, 1.0);
    }
    
    return finalColor * float4(uColor.r, uColor.g, uColor.b, uOpacity);
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}