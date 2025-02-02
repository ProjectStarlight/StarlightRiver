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

float4 barrierColor;

texture lightingTexture;
sampler lightingSampler = sampler_state
{
    Texture = (lightingTexture);
};

float4 White(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(uImage0, coords);

    float4 lightingColor = tex2D(lightingSampler, coords);
    float lightingOpacity = (lightingColor.r + lightingColor.g + lightingColor.b) / 3.0f;
    if (color.a > 0)
        return barrierColor * lightingOpacity;
    return float4(0,0,0,0);
}

technique Technique1
{
    pass NPCBarrier
    {
        PixelShader = compile ps_2_0 White();
    }
}