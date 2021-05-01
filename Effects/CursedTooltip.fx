sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float power;
float speed;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
	float2 off = float2(0, sin(uTime + uWorldPosition.x * speed) * power);

	return float4(tex2D(uImage0, coords).xyz, 1.0);
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}