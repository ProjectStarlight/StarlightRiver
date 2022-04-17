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
float4 uShaderSpecificData;

float offsetScale;
float2 flatOffset;
float blurSize; // controls how much blur based on how far away pixels are sampled from the target

texture ReflectionTarget;
sampler reflectionTargetSampler = sampler_state
{
    Texture = (ReflectionTarget);
};

float4 P1(float4 unused : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 reflectionNormalMapColor = tex2D(uImage0, coords);
    float2 offset = reflectionNormalMapColor.xy * 2. - 1.;

    float tau = 6.28318530718; // Pi*2

    float2 Radius = blurSize / uImageSize0;

    // GAUSSIAN BLUR SETTINGS
    float blurDirections = 16.0; // BLUR blurDirections (Default 16.0 - More is better but slower)
    float blurQuality = 8.0; // number of subsamples for each direction up to the blur radius

    float2 sourceCoords = coords + flatOffset + offset * offsetScale;

    float4 ReflectedPixel = tex2D(reflectionTargetSampler, sourceCoords);

    // Blur calculations
    for (float angle = 0.0; angle < tau; angle += tau / blurDirections)
    {
        for (float distance = 1.0 / blurQuality; distance <= 1.0; distance += 1.0 / blurQuality)
        {
            ReflectedPixel += tex2D(reflectionTargetSampler, sourceCoords + float2(cos(angle), sin(angle)) * Radius * distance);
        }
    }

    return reflectionNormalMapColor.a * (ReflectedPixel / (blurQuality * blurDirections));
}

technique Technique1
{
    pass TileReflectionPass
    {
        PixelShader = compile ps_3_0 P1();
    }
}