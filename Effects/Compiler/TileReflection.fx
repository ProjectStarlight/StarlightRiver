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

float offsetScale;
float2 flatOffset;

texture ReflectionTarget;
sampler reflectionTargetSampler = sampler_state
{
    Texture = (ReflectionTarget);
};

float4 P1(float2 coords : TEXCOORD0) : COLOR0
{
    float4 reflectionNormalMapColor = tex2D(uImage0, coords);
    float2 offset = reflectionNormalMapColor.xy * 2. - 1.;

    float4 ReflectedPixel = tex2D(reflectionTargetSampler, (coords + flatOffset + offset * offsetScale));

    return reflectionNormalMapColor.a * ReflectedPixel;
}

technique Technique1
{
    pass TileReflectionPass
    {
        PixelShader = compile ps_2_0 P1();
    }
}