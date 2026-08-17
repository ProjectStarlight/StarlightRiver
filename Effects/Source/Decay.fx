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
   
    float4 noiseColor = tex2D(uImage1Sampler, noiseCoords);
    
    float2 distortionOffset = (noiseColor.rg - 0.5) * 0.1;
    
    float2 uv = coords + distortionOffset;
    
    float4 shapeColor = tex2D(uImage0, uv);

    float4 finalColor = shapeColor * float4(uColor.r, uColor.g, uColor.b, uOpacity);

    float dissolveValue = noiseColor.r;
    
    float lerper = 1 - uProgress;
    
    float dist = length(coords - 0.5);
    
    // decay from the inside out based on the noise map provided
    
    if (dissolveValue > lerp(dist, 1, lerper))
	{
		finalColor.rgb *= float3(0, 0, 0);
	}

    return finalColor;
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}