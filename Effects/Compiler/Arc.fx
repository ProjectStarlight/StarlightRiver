float2 screenSize;
float2 texSize;
float4 drawColor;
float4x4 zoom;

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
	output.coord = input.coord;
	output.Position = mul(input.Position, zoom);
	output.Position += float4(0.5, 0.5, 0, 0);
	return output;
}

float4 Fragment(VertexShaderOutput input) : COLOR
{
	float2 st = input.coord * texSize;
	float2 st2 = float2(st.x, st.y - (sqrt(1.0 - st.x * st.x) - 0.5));

	float3 color = tex2D(samplerTex, st2).xyz;

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