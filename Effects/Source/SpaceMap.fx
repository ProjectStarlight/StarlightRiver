﻿sampler uImage0 : register(s0); // The contents of the screen
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);

texture sampleTexture2;
sampler2D samplerTex2 = sampler_state { texture = <sampleTexture2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

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
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, uv);
    float4 map = tex2D(samplerTex2, uv);

    if (color.b == 1.0 && color.r > 59.0 / 255.0)
    {
        color.b = lerp(1.0, 0.3, uOpacity);
        color.g *= lerp(1.0, 0.05, uOpacity);
        color.r *= lerp(1.0, 0.3, uOpacity) + (0.5 + sin(uTime + uv.x * 6.28) * 0.5) * map.r * uOpacity + (0.5 + sin(uTime - uv.x * 6.28 * 2.0) * 0.5) * map.b * uOpacity;
    }

    else
    {
        color *= lerp(1.0, 0.5, uOpacity);
        color.b *= lerp(1.0, 1.1, uOpacity);
        color.r *= lerp(1.0, 0.9, uOpacity);
    }

    return color;  
}

technique Technique1
{
    pass SpaceMapPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}