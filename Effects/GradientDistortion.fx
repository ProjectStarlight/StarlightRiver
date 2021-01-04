sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;//scale
float uTime;//Time
float uIntensity;//Frequency
float uProgress;//Speed
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float strength = tex2D(uImage1, uv).r;
    float progress = uTime / uProgress;
    float sinTime = (strength + progress) * uIntensity * 6.28;
    return tex2D(uImage0, float2(sin(sinTime), sin(sinTime * 1.25168)) * 0.01 * strength * uOpacity);
}

technique Technique1
{
    pass ShaderPass
    {
        PixelShader = compile ps_2_0 main();
    }
}