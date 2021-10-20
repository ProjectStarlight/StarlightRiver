texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture sampleTexture2;
sampler2D samplerTex2 = sampler_state { texture = <sampleTexture2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float uTime;
float4 sourceFrame;
float2 texSize;

float mod(float x, float y)
{
    return x - y * floor(x / y);
}

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float2 st = (uv - float2(sourceFrame.xy / texSize)) * float2(texSize.x / sourceFrame.z, texSize.y / sourceFrame.w);

    float4 color = tex2D(samplerTex, uv);

    float bright = (color.x + color.y + color.z) / 3.0;

    if (bright == 0.0);
    else if (bright < 0.15) color.xyz = float3(57.0, 78.0, 103.0) / 255.0;
    else if (bright < 0.3) color.xyz = float3(81.0, 131.0, 142.0) / 255.0;
    else if (bright < 0.45) color.xyz = float3(115.0, 181.0, 158.0) / 255.0;
    else if (bright < 0.6) color.xyz = float3(170.0, 228.0, 167.0) / 255.0;
    else if (bright < 0.75) color.xyz = float3(248.0, 254.0, 211.0) / 255.0;
    else color.xyz = float3(1.0, 1.0, 1.0);

    color.a = 0;

    float value = mod(-tex2D(samplerTex2, (1 - st)).x + uTime - (1 - st.y), 4.0);

    float rAdd = max(1.2 - abs(uTime - st.y), 0.0) * (1.0 - value);

    color.a += value > uTime ? 0.0 : 1.0;
    color.r += rAdd;
    color.g += (1.0 - (uTime - (1 - st.y))) * max(1.2 - abs(uTime - (1 - st.y)), 0.0) * (1.0 - value);
    color.b += (1.0 - (uTime - (1 - st.y))) * 0.2 * max(1.2 - abs(uTime - (1 - st.y)), 0.0) * (1.0 - value);

    float value2 = tex2D(samplerTex2, st);
    float opacity = (0.5 + sin((uTime - 2.0) / 2.0 * 6.28 + st.y * 10.0) * 0.5);

    color.r += value2 * opacity * 1.8;
    color.g += value2 * opacity * 0.8;

    color.a *= tex2D(samplerTex, uv).a * ((bright + rAdd) * 1.5);

    return color;
}

technique Technique1
{
    pass MoltenFormPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}