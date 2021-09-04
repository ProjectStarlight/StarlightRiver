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

texture Map;
sampler mapSampler = sampler_state
{
    Texture = (Map);
};

texture PlayerMap;
sampler playerMapSampler = sampler_state
{
    Texture = (PlayerMap);
};

texture PlayerTarget;
sampler playerTargetSampler = sampler_state
{
    Texture = (PlayerTarget);
};

texture TileTarget;
sampler tileTargetSampler = sampler_state
{
    Texture = (TileTarget);
};

texture Noise;
sampler noiseSampler = sampler_state
{
    Texture = (Noise);
};

texture WallTarget;
sampler wallsampler = sampler_state
{
    Texture = (WallTarget);
};

float4 P1(float2 coords : TEXCOORD0) : COLOR0
{
    float4 colour = tex2D(uImage0, coords);
    float4 MapColor = tex2D(mapSampler, coords);
    float4 PlayerColor = tex2D(playerMapSampler, coords);
    float4 origPlayerColor = tex2D(playerTargetSampler, coords);
    float4 tileColor = tex2D(tileTargetSampler, coords);

    return colour + PlayerColor * MapColor.a * (1 - saturate(origPlayerColor.a)) * (1 - saturate(tileColor.a));
}

technique Technique1
{
    pass TileReflectionPass
    {
        PixelShader = compile ps_2_0 P1();
    }
}