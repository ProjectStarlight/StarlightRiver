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

float time;
float stretch;

float3 topColor;
float3 bottomColor;

float dilation = 1.37f;
float falloff = 6;

float rotation = 0;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = clamp; AddressV = clamp; };

texture noiseTexture;
sampler2D noiseSampler = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = clamp; AddressV = clamp; };


float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float angle = atan2(coords.x - 0.5f, coords.y - 0.5f) + rotation;
	float2 uv = float2(angle / 6.28f, 0.5f + length(coords - float2(0.5f,0.5f)));
    float2 stretchedcoords = float2(uv.x * stretch, uv.y);

	float offset = tex2D(noiseSampler, float2(stretchedcoords.x, ((time * 0.4f) + uv.y) % 1.0f));
	float2 newcoords = float2 (stretchedcoords.x, lerp(uv.y, 0.5f + (sign(uv.y - 0.5f) * 0.25f), pow(offset / 2, dilation)));
	float3 Color = tex2D(samplerTex, uv).rgb; 
	float3 Color2 = tex2D(samplerTex, float2(0.5f, tex2D(noiseSampler, float2((time - abs(uv.y - 0.5f)) % 1.0f, newcoords.y)).r)).rgb * pow(sin(uv.y * 3.14f), falloff);

    float3 totalColor = Color + Color2;

    float3 origColor = lerp(topColor, bottomColor, coords.y);
    return float4(totalColor * origColor, totalColor.x);
}

technique Technique1
{
    pass BloomPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};