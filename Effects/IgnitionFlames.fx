texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture sampleTexture1;
sampler2D samplerTex1 = sampler_state { texture = <sampleTexture1>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture sampleTexture2;
sampler2D samplerTex2 = sampler_state { texture = <sampleTexture2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture pallette;
sampler2D palletteSampler = sampler_state { texture = <pallette>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float uTime;

float stretch;
float stretch2;
float opacity;

matrix transformMatrix;

struct VertexShaderInput
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    output.Color = input.Color;
    output.TexCoords = input.TexCoords;
    output.Position = mul(input.Position, transformMatrix);

    return output;
}


float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 st = input.TexCoords;

    if (st.x < 0.03f || st.x > 0.97f)
        return float4(0,0,0,0);

    float4 color = tex2D(samplerTex, st);

    float x2 = st.x + uTime * 0.4;
    float y2 = st.y + uTime * 0.4;
    float factor2 = (tex2D(samplerTex2, float2(x2, y2)).x * stretch);
    float4 color2 = tex2D(samplerTex, st + float2(0, 1.0) * factor2);

    float x3 = st.x + uTime * 0.2;
    float y3 = st.y + uTime * 0.4;
    float factor3 = (tex2D(samplerTex2, float2(x3, y3)).x * stretch2);
    float4 color3 = tex2D(samplerTex, st + float2(0, 1.0) * factor3);

    float x = st.x + uTime * 0.6;

    //color2.a *= sin(st.x * 3.14);
    //color3.a *= sin(st.x * 3.14);

    float finala = 0;

    float column = 0;
    if (color.a == 0.0)
    {
        column += 0.45f * color2.a;
        finala = 1;
    }

    if (color.a == 0.0)
    {
        column += 0.3f * color3.a;
        finala = 1;
    }

    color = tex2D(palletteSampler, float2(column, 0.5f));
    color.a = finala;

    if (column == 0)
        return float4(0,0,0,0);

    color *= opacity;
    return color;
}

technique Technique1
{
    pass IgnitionFlamesPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}