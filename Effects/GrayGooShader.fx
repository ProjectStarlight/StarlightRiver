sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
texture NPCTarget;

float threshhold;
sampler tent = sampler_state
{
    Texture = (NPCTarget);
};
float4 White(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    float4 npcColor = tex2D(tent, coords * 2);

    if (color.r < threshhold && npcColor.a < 0.1f)
        return float4(0,0,0,0);

    if (color.r < 1 - threshhold)
        return float4(0,0,0,0);
    return float4(1,1,1,1);
}

technique BasicColorDrawing
{
    pass WhiteSprite
    {
        PixelShader = compile ps_2_0 White();
    }
};