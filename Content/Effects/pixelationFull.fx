float pixelation;
float2 resolution;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = mirror; AddressV = mirror; };
texture targetTexture;
sampler2D targetTex = sampler_state { texture = <targetTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = mirror; AddressV = mirror; };

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
	output.Position = input.Position;
	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 scaledRes = resolution / pixelation;
	float2 modUV = (round(input.coord * scaledRes) / scaledRes);
	
	return tex2D(samplerTex, modUV); 
}

technique Technique1
{
	pass Shade
	{
		VertexShader = compile vs_2_0 MainVS();
		PixelShader = compile ps_2_0 MainPS();
	}
}