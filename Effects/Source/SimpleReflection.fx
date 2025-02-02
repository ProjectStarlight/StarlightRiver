float4 uShaderSpecificData;
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


sampler normalMapSampler : register(s0); //corresponds to the normal map 
float2 normalMapPosition; //position of the normal map in screen uv coordinates 
float2 uImageSize0; //normalMap size

float offsetScale;
float2 flatOffset;

sampler reflectionTargetSampler : register(s1); //corresponds to the entities reflection target
float2 reflectionTargetSize;

float4 tintColor; //tints the output color by multiplying against this i.e. to dim the reflection set this as (255,255,255,100)


//this is the less intensive version of the reflection shader that only does direct angled specular reflection without any of the fancy stuff

float4 P1(float2 coords : TEXCOORD0) : COLOR0
{
    float4 reflectionNormalMapColor = tex2D(normalMapSampler, coords);
    float2 offset = reflectionNormalMapColor.xy * 2. - 1.;

    float2 sizeRatio = uImageSize0 / reflectionTargetSize;

    float2 sourceCoords = normalMapPosition + (coords * sizeRatio) + (offset * offsetScale) + flatOffset;

    float4 ReflectedPixel = tex2D(reflectionTargetSampler, sourceCoords);

    return reflectionNormalMapColor.a * ReflectedPixel * tintColor;
}

technique Technique1
{
    pass TileReflectionPass
    {
        PixelShader = compile ps_2_0 P1();
    }
}