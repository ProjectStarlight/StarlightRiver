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

struct VertexShaderInput
{
	float2 coord : TEXCOORD0;
	float4 color : COLOR0;
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float2 coord : TEXCOORD0;
	float4 color : COLOR0;
	float4 Position : POSITION0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	return output;
}

float4 Fragment(VertexShaderOutput input) : COLOR
{
	float2 coords = input.coord;
	float2 off = float2(sin(uTime + coords.y * 6.28 * 10.0) * power * 0.1, sin(uTime + coords.x * 6.28 * 20.0) * power * 0.5);

	float4 colorr = tex2D(uImage0, coords + off) * float4(input.color.xyz, 1.0);

	colorr *= 1.0 + sin(uTime + coords.x * 6.28 * 20.0) * 0.1;
	colorr.x *= 1.0 + sin(uTime + coords.y * 6.28 * 10.0) * 0.1;

	return colorr * input.color.a;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 Fragment();
	}
}