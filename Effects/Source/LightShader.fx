float2 screenSize;
float2 fullBufferSize;
float2 offset;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = mirror; AddressV = mirror; };

struct VertexShaderInput
{
	float2 coord : TEXCOORD0;
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float2 coord : TEXCOORD0;
	float4 Position : SV_POSITION;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	output.coord = input.coord + offset;
	output.Position = input.Position;
	return output;
}

float4 Fragment(VertexShaderOutput input) : COLOR
{
	float2 uv = input.coord * screenSize / fullBufferSize;

	float3 color = tex2D(samplerTex, uv).rgb;

	return float4(color, 1);
}

technique Technique1
{
	pass Shade
	{
		VertexShader = compile vs_2_0 MainVS();
		PixelShader = compile ps_2_0 Fragment();
	}
}