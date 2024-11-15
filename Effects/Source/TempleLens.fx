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

texture LensTarget;
sampler lens = sampler_state
{
    Texture = (LensTarget);
};

float4 White(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
	float4 color2 = tex2D(lens, coords);
    if (tex2D(uImage0, coords).a == 0)
        return float4(0,0,0,0);
    return color2;
}

technique Technique1
{
    pass BreacherScan
    {
        PixelShader = compile ps_2_0 White();
    }
}