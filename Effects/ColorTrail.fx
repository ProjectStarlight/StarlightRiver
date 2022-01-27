float4 color;

matrix transformMatrix;

texture sampleTexture;
sampler2D samplerTex = sampler_state
{
	texture = <sampleTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

texture sampleTexture2;
sampler2D samplerTex2 = sampler_state
{
	texture = <sampleTexture2>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

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
	return color;
}

technique Technique1
{
	pass PrimitivesPass
	{
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
};