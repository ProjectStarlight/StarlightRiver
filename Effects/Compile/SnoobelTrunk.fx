float alpha = 0;
matrix transformMatrix;

bool flip;

float totalLength;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture sampleTextureEnd;
sampler2D samplerTexEnd = sampler_state { texture = <sampleTextureEnd>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

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
    float2 uv = input.TexCoords;
    if (flip)
        uv.y = 1 - uv.y;

    float xLength = uv.x * totalLength;

    float4 color = float4(0,0,0,0);

    float sampleX = 0;

    if (xLength > totalLength - 12)
    {
        sampleX = (xLength + 12 - totalLength) / 12.0f;
        color = tex2D(samplerTexEnd, float2(sampleX, uv.y));
    }
    else
    {
        float innerLength = totalLength - 12;
        float segmentLength = 38;
        sampleX = clamp((xLength % segmentLength) / segmentLength, 0.01f, 0.99f);
        color = tex2D(samplerTex, float2(sampleX, uv.y));
    }
    return color * input.Color;
}

technique Technique1
{
    pass PrimitivesPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};