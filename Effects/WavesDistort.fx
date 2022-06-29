sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
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

float2 offset;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
	float2 off = float2(0, sin(uTime + (coords.x + offset.x) * speed) * sin(1.57 + (coords.x + offset.x) * 0.37 * speed - uTime) * sin((coords.x + offset.x) * 0.21 * speed - uTime) * power);
    float4 color = tex2D(uImage0, coords + off);
    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}