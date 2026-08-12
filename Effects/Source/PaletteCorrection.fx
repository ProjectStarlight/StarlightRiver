// credits to https://github.com/OliHeamon/TidesOfTime/tree/master/Assets/Effects for the shader

sampler inputImage : register(s0);

float3 palette[16];

int colorCount;

float4 PixelShaderFunction(float2 uv: TEXCOORD0) : COLOR0
{
    float4 color = tex2D(inputImage, uv);

    if (any(color))
    {
        float3 closestColor = palette[0];
        float minDistance = distance(color.rgb, closestColor);

        for (int i = 1; i < colorCount; i++)
        {
            float3 paletteColor = palette[i];
            float distanceToColor = distance(color.rgb, paletteColor);
            if (distanceToColor < minDistance)
            {
                closestColor = paletteColor;
                minDistance = distanceToColor;
            }
        }

        return float4(closestColor, color.a);
    }

    return color;
}

technique Technique1
{
    pass PaletteCorrectionPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};