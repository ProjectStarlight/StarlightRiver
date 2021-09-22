sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
texture TileTarget;
sampler tent = sampler_state
{
    Texture = (TileTarget);
};
float4 White(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    float4 tileColor = tex2D(tent, coords);
    color *= tileColor.a;
    return color;
}

technique BasicColorDrawing
{
    pass WhiteSprite
    {
        PixelShader = compile ps_2_0 White();
    }
};