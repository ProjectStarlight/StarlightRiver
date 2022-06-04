sampler2D SpriteTextureSampler;

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float offset;

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 pos = input.TextureCoordinates - float2(0.5, 0.5);

    float angle = atan2(pos.y, pos.x);

    float delta = sin((angle + offset * 1.2) * 4.6) * 2;
    delta -= sin(1.57 + (angle - offset * 1.8) * 6.8) * 1.2f;

    pos *= 1 + delta * 0.025;

    float4 color = tex2D(SpriteTextureSampler, pos + float2(0.5, 0.5));

    return color;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile ps_2_0 MainPS();
    }
};
