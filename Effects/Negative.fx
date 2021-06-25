float3 uColor;
float opacity;
texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float2 offset;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
	float alpha = tex2D(samplerTex, coords).a;

	if(alpha == 0.0)
		return float4(uColor, opacity);

	return float4(0.0, 0.0, 0.0, 0.0);
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}