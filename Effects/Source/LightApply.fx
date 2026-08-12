float4 drawColor;
float4x4 zoom;
float4x4 sampleTrans;

texture targetTexture;
sampler2D targetTex = sampler_state { texture = <targetTexture>; };
texture sampleTexture;
sampler2D sampleTex = sampler_state { texture = <sampleTexture>; };


struct VertexShaderInput
{
	float2 coord : TEXCOORD0;
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float2 coord : TEXCOORD0;
    float4 PositionPass : TEXCOORD1;
	float4 Position : SV_POSITION;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	output.coord = input.coord;
	output.Position = mul(input.Position, zoom);
    output.PositionPass = mul(input.Position, zoom);
	return output;
}

float4 Fragment(VertexShaderOutput input) : COLOR
{
    float2 st = input.PositionPass;
    st = mul(float4(st.x, st.y, 0, 1), sampleTrans).xy;

	float3 color = tex2D(sampleTex, st).xyz * tex2D(targetTex, input.coord).xyz;

	return float4(color, tex2D(targetTex, input.coord).w) * drawColor;
}

technique Technique1
{
	pass Shade
	{
		VertexShader = compile vs_2_0 MainVS();
		PixelShader = compile ps_2_0 Fragment();
	}
}