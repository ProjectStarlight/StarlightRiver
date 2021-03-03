float4x4 upscale;
float2 offset;
float time;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

struct VertexShaderInput
{
	float2 coord : TEXCOORD0;
	float4 Position : POSITION0;
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
	float2 coord : TEXCOORD0;
	float4 Position : POSITION0;
	float4 Color : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	output.Color = input.Color;
	output.coord = input.coord;
	output.Position = mul(input.Position, upscale);
	return output;
}

float4 Fragment(VertexShaderOutput input) : COLOR
{
	float value = tex2D(samplerTex, input.coord + float2(time, 0)).x;
	float4 color = input.Color * value * 2.6;
	return color;
}

technique Technique1
{
	pass Shade
	{
		VertexShader = compile vs_2_0 MainVS();
		PixelShader = compile ps_2_0 Fragment();
	}
}