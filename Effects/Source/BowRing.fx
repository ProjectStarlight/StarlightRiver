sampler uImage0 : register(s0); // The contents of the screen
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;
float cosine;

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float a = uTime;

    float2x2 rotate = float2x2(cosine, -sin(a), sin(a), cosine);

    float3 color = uColor * tex2D(uImage0, mul((uv + float2(-0.5, -0.5)), rotate) + float2(0.5, 0.5)).xyz;

    return float4(color, 1.0 * uOpacity - uv.x * 0.65);
}

technique Technique1
{
    pass BowRingPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}