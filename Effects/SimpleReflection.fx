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


sampler reflectionTargetSampler : register(s0); //corresponds to the entities reflection target, forced into linear wrap by tml (possible TODO: break out this entire shader from the tml's shader structs and implement manually)

float2 uImageSize0; //eflection target size

float offsetScale;
float2 flatOffset;

sampler normalMapSampler : register(s1); //corresponds to the normal map
float2 normalMapSize;

//this is the less intensive version of the reflection shader that only does direct angled specular reflection without any of the fancy stuff

float4 P1(float2 coords : TEXCOORD0) : COLOR0
{
    float4 reflectionNormalMapColor = tex2D(normalMapSampler, coords);
    float2 offset = reflectionNormalMapColor.xy * 2. - 1.;

    float2 sizeRatio = uImageSize0 / normalMapSize;

    float2 sourceCoords = coords + (flatOffset + offset * offsetScale) * sizeRatio;

    float4 ReflectedPixel = tex2D(reflectionTargetSampler, sourceCoords);

    return reflectionNormalMapColor.a * ReflectedPixel;
}

technique Technique1
{
    pass TileReflectionPass
    {
        PixelShader = compile ps_2_0 P1();
    }
}