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

float4 White(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
	float1 pixW = 2.0 / uImageSize0.x;
	float1 pixH = 2.0 / uImageSize0.y;
	float4 color = tex2D(uImage0, coords);
    float2 squareWidth = float2(pixW, pixH);
    float4 red = float4(1, 0, 0, 1);
    if (color.a != 0)
        return color;

    float2 opposite = squareWidth * float2(1, -1);
    if (tex2D(uImage0, coords + squareWidth).a != 0)
        return red;
    if (tex2D(uImage0, coords - squareWidth).a != 0)
        return red;
    if (tex2D(uImage0, coords + opposite).a != 0)
        return red;
    if (tex2D(uImage0, coords - opposite).a != 0)
        return red;
    return float4(0,0,0,0);
}

technique Technique1
{
    pass BreacherScan
    {
        PixelShader = compile ps_2_0 White();
    }
}