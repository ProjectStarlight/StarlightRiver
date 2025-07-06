#include "Common.fxh"

texture texture0;
sampler uImage0 = sampler_state { texture = <texture0>; };
matrix World;
matrix View;
matrix Projection;

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
    output.Position = mul(input.Position, mul(World, mul(View, Projection)));

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoords;
    float4 uColor = input.Color;
    float alpha = max(max(uColor.r, uColor.g), uColor.b);
    
    float4 sam = tex2D(uImage0, coords);
	float4 color = uColor * sam;
    float lum = GetLuminance(sam.xyz);
    lum = (alpha - 0.5) * 2.0 * pow(lum, 2.0);
    color.xyz = color.xyz * (1.0 - lum) + float3(1.0, 1.0, 1.0) * lum;
    color.a = 0.0;

	return color;
}

technique Technique1
{
	pass GlowingDustPass
	{
        VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}