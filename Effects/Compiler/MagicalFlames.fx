texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture sampleTexture1;
sampler2D samplerTex1 = sampler_state { texture = <sampleTexture1>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture sampleTexture2;
sampler2D samplerTex2 = sampler_state { texture = <sampleTexture2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float uTime;

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float2 st = uv;

    float4 color = tex2D(samplerTex, st);

    float x2 = st.x + uTime * 0.4;
    float y2 = st.y - uTime * 0.4;
    float factor2 = (tex2D(samplerTex2, float2(x2, y2)).x * 0.25);
    float4 color2 = tex2D(samplerTex, st + float2(0, -1.0) * factor2);

    float x3 = st.x + uTime * 0.2;
    float y3 = st.y - uTime * 0.4;
    float factor3 = (tex2D(samplerTex2, float2(x3, y3)).x * 0.08);
    float4 color3 = tex2D(samplerTex, st + float2(0, -1.0) * factor3);

    float x = st.x + uTime * 0.6;
    float samplea = tex2D(samplerTex1, float2(x, st.y + sin(uTime + st.x * 10.0) * 0.05)).x;
    float finala = color.a;

    if (color2.a > 0.0 && color.a == 0.0)
    {
        color.b += 0.5 * color2.a;
        color.g += 0.2 * color2.a;
        color.r += 0.2 * color2.a;
        finala = samplea;
    }

    if (color3.a > 0.0 && color.a == 0.0)
    {
        color.g += 0.55 * color3.a;
        color.b += 0.8 * color3.a;
        color.r += 0.6 * color3.a;
        finala = samplea;
    }

    color.a = finala;

    return color;
}

technique Technique1
{
    pass MagicalFlamesPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}