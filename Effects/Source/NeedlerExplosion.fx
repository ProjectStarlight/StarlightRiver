sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float4 colorMod;
float progress;
float progress2;
texture noise;
sampler tent = sampler_state
{
    Texture = (noise);
};
float4 White(float2 coords : TEXCOORD0) : COLOR0
{
    float2 centerCoords = float2(0.5f,0.5f);
    centerCoords = centerCoords - coords;
    float distance = length(centerCoords);
    float2 distFromShockwave = pow((distance * 2) - progress - (tex2D(tent, coords).r / 8.0f), 2);

    float4 white = colorMod;
    float4 colortwo = white;
    float lerper = pow(distFromShockwave, 6);
    if (lerper > 1)
         colortwo = lerp(white,float4(0,0,0,0), lerper - 1);
    else
        colortwo = lerp(colorMod,white, lerper);
    float lerper2 = pow(distFromShockwave, 2);
    colortwo = lerp(float4(0,0,0,0), colortwo, lerper2);
    if (lerper > 2)
        colortwo *= 0;
     colortwo *= (2 - progress2);
    return colortwo;
}

technique BasicColorDrawing
{
    pass WhiteSprite
    {
        PixelShader = compile ps_2_0 White();
    }
};