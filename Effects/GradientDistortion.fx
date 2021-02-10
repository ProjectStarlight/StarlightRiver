sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);

texture draw;
sampler2D InputLayer1 = sampler_state { texture = <draw>; };

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

float4 main(float2 uv : TEXCOORD0) : COLOR0
{
    float strength = max(tex2D(uImage1, uv).r - tex2D(uImage1, uv).b, 0.0);
    float progress = uTime / uProgress;
    float sinTime = (strength + progress) * uIntensity * 6.28;
	float2 off = float2(sin(sinTime), sin(sinTime * 1.25168)) * 0.1 * (strength / 255.0) * uOpacity;
    return tex2D(uImage0, uv + off);
}

technique Technique1
{
    pass GradientDistortionPass
    {
        PixelShader = compile ps_2_0 main();
    }
}