sampler uImage0 : register(s0);

float4x4 MATRIX;

struct VertexShaderInput
{
    float2 TextureCoordinates : TEXCOORD0;
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float2 TextureCoordinates : TEXCOORD0;
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};

VertexShaderOutput MainVS(float4 position : SV_POSITION, float4 color : COLOR0, float2 texCoord : TEXCOORD0)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(position, MATRIX);
    output.TextureCoordinates = texCoord;

    return output;
}

float convertToTruePrecision(float4 c)
{
    float combine = c.g + c.r;
    return c.g - c.r + (1 - sign(floor(abs(combine * 255.0f)))) * (c.b - 0.5f) * (2.0f / 255.0f);
}

float4 maptogas(VertexShaderOutput input) : COLOR0
{
    float4 map = tex2D(uImage0, input.TextureCoordinates);
    float c = convertToTruePrecision(map);
    return float4(c, c, c, c);
}

technique Technique1
{
    pass projectmap
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_2_0 maptogas();
    }
}