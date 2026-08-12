float3 uColor;
float opacity;
texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float2 offset;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
	float alpha = 1.0 - tex2D(samplerTex, coords).a;

    return float4(0.0, 0.0, 0.0, alpha * opacity);
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}