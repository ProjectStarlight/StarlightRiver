sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
texture TileTarget;

float width;
float height;
float tileScale;

float4 border;
float4 oldBorder;

float transparency;
sampler tent = sampler_state
{
    Texture = (TileTarget);
};

float2 scaleBack(float2 pos)
{
    return float2(pos.x / width, pos.y / height);
}

float4 White(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    if (color.a < 1)
        return float4(0,0,0,0);
        

    float4 tileColor = tex2D(tent, coords * tileScale);

    if (tileColor.a > 0)
        return color * transparency;

    if (color.g == oldBorder.g)
        return color;

    float nearby[8]; // right then clockwise

    float2 pos = float2(coords.x * width, coords.y * height);

    nearby[0] = tex2D(tent, (scaleBack(pos + float2(1, 0))) * tileScale).a;
    nearby[1] = tex2D(tent, (scaleBack(pos + float2(0, 1))) * tileScale).a;
    nearby[2] = tex2D(tent, (scaleBack(pos + float2(-1, 0))) * tileScale).a;
    nearby[3] = tex2D(tent, (scaleBack(pos + float2(0, -1))) * tileScale).a;
    nearby[4] = tex2D(tent, (scaleBack(pos + float2(0.5f, 0))) * tileScale).a;
    nearby[5] = tex2D(tent, (scaleBack(pos + float2(0, 0.5f))) * tileScale).a;
    nearby[6] = tex2D(tent, (scaleBack(pos + float2(-0.5f, 0))) * tileScale).a;
    nearby[7] = tex2D(tent, (scaleBack(pos + float2(0, -0.5f))) * tileScale).a;

    if (nearby[0] > 0)
        return border;
    if (nearby[1] > 0)
        return border;
    if (nearby[2] > 0)
        return border;
    if (nearby[3] > 0)
        return border;
    if (nearby[4] > 0)
        return border;
    if (nearby[5] > 0)
        return border;
    if (nearby[6] > 0)
        return border;
    if (nearby[7] > 0)
        return border;

    return color;
}

technique BasicColorDrawing
{
    pass WhiteSprite
    {
        PixelShader = compile ps_2_0 White();
    }
};