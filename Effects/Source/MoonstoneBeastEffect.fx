sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

texture baseTexture;
sampler2D baseSampler = sampler_state
{
    texture = <baseTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

texture distortTexture;
sampler2D distortSampler = sampler_state
{
    texture = <distortTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

float2 size;
float time;
float opacity;
float2 noiseSampleSize;
float noisePower;

float4 Main(float2 uv : TEXCOORD) : COLOR
{
    float noise0 = tex2D(distortSampler, uv / size * noiseSampleSize + time * 0.05).r;
    float noise1 = tex2D(distortSampler, uv / size * noiseSampleSize - time * 0.1).r;
    
    uv += (noise0 * 0.025 + noise1 * 0.025) / size * noisePower;
    
    float4 color = tex2D(baseSampler, uv);
        color.a *= 2.5
        - tex2D(baseSampler, uv + 2.0 / size.x).a
        - tex2D(baseSampler, uv - 2.0 / size.x).a;
    
    color.r *= color.a;
    color.g *= color.a * 0.9;
    color.b *= color.a * 2.0;
    
    color += color.a * 0.2;
    
    color.a *= 0.25 + noise0 * noise1;
    
    return color * opacity;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile ps_3_0 Main();
    }
};