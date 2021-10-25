texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture sampleTexture2;
sampler2D samplerTex2 = sampler_state { texture = <sampleTexture2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture sampleTexture3;
sampler2D samplerTex3 = sampler_state { texture = <sampleTexture3>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float uTime;
float4 sourceFrame;
float4 drawColor;
float2 texSize;

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float2 st = (uv - float2(sourceFrame.xy / texSize)) * float2(texSize.x / sourceFrame.z, texSize.y / sourceFrame.w);

    float4 color = tex2D(samplerTex, uv) * drawColor;
    float dist = tex2D(samplerTex3, st).x;
    float factor = min(1.0, (uTime - dist) * 4.0);

    if (uTime > dist)
    {
        color.r += pow(tex2D(samplerTex2, st).x * 1.4, 2.0) * factor;
        color.g += pow(tex2D(samplerTex2, st).x * 1.4, 5.0) * 0.5 * factor;
    }

    return color;
}

technique Technique1
{
    pass MagmaCracksPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}