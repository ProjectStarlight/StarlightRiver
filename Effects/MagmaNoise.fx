float4 codedColor = float4(0.0, 1.0, 0.0, 1.0);
float4 newColor;
float4 redColor;

float2 noiseScale;
float2 offset;

sampler2D SpriteTextureSampler;

texture distort;
sampler2D distortS = sampler_state { texture = <distort>; AddressU = wrap; AddressV = wrap; };

texture red;
sampler2D redS = sampler_state { texture = <red>; AddressU = wrap; AddressV = wrap; };

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates);

    float lerper = tex2D(distortS, (input.TextureCoordinates * noiseScale) + offset).r;
    
    float lerper2 = tex2D(redS, input.TextureCoordinates).r;
    
    if (color.g == codedColor.g)
    {
        float4 retColor = lerp(float4(1.0, 0.75, 0.12, 1.0), newColor, lerper);
        return retColor;
    }
    
    return color;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile ps_2_0 MainPS();
    }
};
