float2 offset;
float2 screenSize;
float2 texSize;
float2 zoom;


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

float2 ScaleCoordByZoom(float2 Coord)
{
	return float2((Coord - 0.5) / zoom) + 0.5;
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	output.coord = ScaleCoordByZoom(input.coord);
	output.Position = float4((input.Position - 0.5) / zoom.x) + 0.5;
	return output;
}

float4 FragmentPS(VertexShaderOutput input) : COLOR
{
	float2 st = ScaleCoordByZoom(input.coord * texSize / screenSize + offset);
	float2 uv = ScaleCoordByZoom(input.coord);

	float3 color = tex2D(samplerTex, st).rgb * tex2D(targetTex, uv).rgb;

	return float4(color, tex2D(targetTex, uv).w);
}

technique Technique1
{
	pass Shade
	{
		VertexShader = compile vs_2_0 MainVS();
		PixelShader = compile ps_2_0 FragmentPS();
	}
}