sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
texture NPCTarget;

float2 screenSize;

float threshhold;

float time;
float min;
float max;

float random (float2 st) {
    return frac(sin(time + dot(st.xy,
                         float2(12.9898f,78.233f)))*
        43758.5453123f);
}

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

    float2 pixelSize = float2(2,2) / screenSize;
    float2 pixelCoords = floor(coords / pixelSize) * pixelSize;
    float ret = lerp(min, max, random(pixelCoords));
    return float4(ret,ret,ret,1);
}

technique BasicColorDrawing
{
    pass WhiteSprite
    {
        PixelShader = compile ps_2_0 White();
    }
};