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
    
    float4 sam = tex2D(uImage0, coords);
	float4 color = uColor * sam;
    float lum = GetLuminance(sam.xyz);
    float added = clamp(lum - 0.5, 0.0, 0.5);
    
    color.rgb += float3(added, added, added) * uColor.a;
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