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
s
float power;
float speed;
float opacity;

float2 offset;

float4 drawColor;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float sin1 = sin(uTime + (coords.x + offset.x) * speed);
    float sin2 = sin(1.57 + (coords.x + offset.x) * 0.37 * speed - uTime);
    float sin3 = sin((coords.x + offset.x) * 0.21 * speed - uTime);

	float2 off = float2(0, sin1 * sin2 * sin3 * power);
    float4 color = tex2D(uImage0, coords + off);
    color *= drawColor;
    return color * opacity;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}