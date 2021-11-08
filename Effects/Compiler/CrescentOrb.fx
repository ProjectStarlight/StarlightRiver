texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture sampleTexture2;
sampler2D samplerTex2 = sampler_state { texture = <sampleTexture2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float uTime;

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float2 st = uv;
    float4 color = tex2D(samplerTex, st);

    float2 st2 = st + float2(uTime, uTime);

    float2 st3 = st - float2(uTime, uTime);

    color.g -= tex2D(samplerTex2, st2).x * 0.5;
    color.g += tex2D(samplerTex2, st3).x;

    color.r -= tex2D(samplerTex2, st2).x;
    color.r += tex2D(samplerTex2, st3).x * 0.5;

    return color;
}

technique Technique1
{
    pass CrescentOrbPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}