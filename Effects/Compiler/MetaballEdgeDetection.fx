const float4 black = float4(0.0, 0.0, 0.0, 0.0);
float4 codedColor = float4(0.0, 1.0, 0.0, 1.0);

sampler2D SpriteTextureSampler;

float4 border = float4(1.0, 0.67, 0.2, 1.0);
float width;
float height;

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float2 scaleBack(float2 pos)
{
    return float2(pos.x / width, pos.y / height);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates);

    float nearby[4]; // right then clockwise

    float2 pos = float2(input.TextureCoordinates.x * width, input.TextureCoordinates.y * height);

    nearby[0] = tex2D(SpriteTextureSampler, scaleBack(pos + float2(0.5f, 0))).g;
    nearby[1] = tex2D(SpriteTextureSampler, scaleBack(pos + float2(0, 0.5f))).g;
    nearby[2] = tex2D(SpriteTextureSampler, scaleBack(pos + float2(-0.5f, 0))).g;
    nearby[3] = tex2D(SpriteTextureSampler, scaleBack(pos + float2(0, -0.5f))).g;

    if (nearby[0] == 1.0 && color.g < 1.0f)
    {
        return border;
    }
    else if (nearby[1] == 1.0 && color.g < 1.0f)
    {
        return border;
    }
    else if (nearby[2] == 1.0 && color.g < 1.0f)
    {
        return border;
    }
    else if (nearby[3] == 1.0 && color.g < 1.0f)
    {
        return border;
    }

    if (color.a == 0)
        return color;
    return codedColor;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile ps_2_0 MainPS();
    }
};
