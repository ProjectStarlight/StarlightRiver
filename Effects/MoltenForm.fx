﻿texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture sampleTexture2;
sampler2D samplerTex2 = sampler_state { texture = <sampleTexture2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float uTime;
float4 sourceFrame;
float2 texSize;

float Mod(float x, float y)
{
    return x - y * floor(x / y);
}

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float2 st = (uv - float2(sourceFrame.xy / texSize)) * float2(texSize.x / sourceFrame.z, texSize.y / sourceFrame.w);
    float4 color = tex2D(samplerTex, uv);
    color.a = 0.0;
    float value = Mod(-tex2D(samplerTex2, (1 - st)).x + uTime - (1 - st.y), 2.0);

    color.a += value > uTime ? 0.0 : 1.0;
    color.r += max(1.2 - abs(uTime - st.y), 0.0) * (1.0 - value);
    color.g += (1.0 - (uTime - (1 - st.y))) * max(1.2 - abs(uTime - (1 - st.y)), 0.0) * (1.0 - value);
    color.b += (1.0 - (uTime - (1 - st.y))) * 0.2 * max(1.2 - abs(uTime -  (1 - st.y)), 0.0) * (1.0 - value);

    color.a *= tex2D(samplerTex, uv).a * (color.r * 1.5);

    return color;
}

technique Technique1
{
    pass MoltenFormPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}