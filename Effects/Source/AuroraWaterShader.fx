float time;
float2 screenSize;
float2 offset;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture sampleTexture2;
sampler2D samplerTex2 = sampler_state { texture = <sampleTexture2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float4 PixelShaderFunction(float4 screenSpace : TEXCOORD0) : COLOR0
{
    float2 st = screenSpace.xy;
    float2 off = float2(sin(time + st.y * 200.0 + offset.y * 100.0), sin(time + st.x * 200.0 + offset.x * 100.0)) / screenSize;

    float map = tex2D(samplerTex2, st * 2 + off).r;
    float4 color = tex2D(samplerTex, st + off);

    float progress = (st.x + st.y) * 10.0;

    float r = 40.0 * (1.0 + sin(time + progress * 0.2));
    float g = 46.0 * (1.0 + sin(1.57 + time + progress));
    float b = 72.0;

    float3 colorB = float3(r, g, b);
    float3 color2 = colorB * 0.015 * map * (color.r + color.b * 4.0);

    return float4(color2, color.a) * 0.5;
}

technique Technique1
{
    pass PrimitivesPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};