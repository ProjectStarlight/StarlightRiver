﻿
float time;
float repeats;

matrix transformMatrix;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture sampleTexture2;
sampler2D samplerTex2 = sampler_state { texture = <sampleTexture2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

struct VertexShaderInput
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    output.Color = input.Color;
    output.TexCoords = input.TexCoords;
    output.Position = mul(input.Position, transformMatrix);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 st = float2(input.TexCoords.x * repeats, 0.25 + input.TexCoords.y * 0.5);

	float3 color = tex2D(samplerTex, st + float2(time, 0)).xyz;
	float3 color2 = tex2D(samplerTex, st + float2(-time * 1.5, 0)).xyz * 0.5;

    float sam = tex2D(samplerTex2, float2(st.y, time)).x;
    float mult = tex2D(samplerTex2, st).x + (input.Color.g * -2.0);

    float4 output = float4((color + color2) * input.Color * (1.0 + color.x * 2.0), color.x * input.Color.w);

    //if((input.Color.g) < mult)
        output *= sam + input.Color.g;

    return output;
}

technique Technique1
{
    pass PrimitivesPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
};