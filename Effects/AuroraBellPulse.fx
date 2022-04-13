sampler uImage0 : register(s0); // The contents of the screen

float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float4 uShaderSpecificData;

float progresses[10];
float2 positions[10];
float intensity[10];
float numberOfBells;


float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

// Returns Saturation, Brightness, BrightestComponent, DarkestComponent
float4 GetSaturationAndBrightness(float3 color)
{
    float brightestComponent = max(color.r, max(color.g, color.b));
    float darkestComponent = min(color.r, min(color.g, color.b));
    
    float saturation = brightestComponent - darkestComponent;
    float brightness = (brightestComponent + darkestComponent) / 2.0;

    return float4(saturation, brightness, brightestComponent, darkestComponent);
}

float4 BellShader(float4 unused : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 position = uScreenResolution * coords + uScreenPosition;
    float4 textureColor = tex2D(uImage0, coords);

    [loop]for (int i = 0; i < 10; i++)
    {
        float2 diffPosition = position - positions[i];
        float distanceToPixel =  length(diffPosition);
        float radius = progresses[i] * 200.0 * intensity[i] * 0.07f;

        float modifiedDistance = -(distanceToPixel - radius);

        float waveIntensity = min(5.0, abs(50.0 / modifiedDistance)) * intensity[i];

        diffPosition /= distanceToPixel;
        diffPosition *= distanceToPixel + sin(distanceToPixel * 0.2 * (1.0 - progresses[i]) + -5.0 * uTime) * waveIntensity;

        float2 offsetCoords1 = (diffPosition + positions[i] - uScreenPosition) / uScreenResolution;
        float2 offsetCoords2 = (offsetCoords1 - coords) * 2.25 + coords;
        coords = (offsetCoords1 - coords) * 3.5 + coords;
        float blueDiff = tex2D(uImage0, offsetCoords1).b - textureColor.b;
        float greenDiff = tex2D(uImage0, offsetCoords2).g - textureColor.g;

        float3 color = textureColor.rgb;
        float4 data = GetSaturationAndBrightness(color);
        float brightness = data.y;
        if (brightness < 0.5) {
            color = lerp(float3(0.0, 0.0, 0.0), uColor, brightness * 2.0);
        } else {
            color = lerp(uColor, float3(1.0, 1.0, 1.0), 2.0 * brightness - 1.0);
        }

        if (i < numberOfBells)
        {
            textureColor.rgb = lerp(textureColor.rgb, color * textureColor.a, (max(radius - distanceToPixel, 0) / (radius * 0.2f)) * uOpacity);
            textureColor.b += blueDiff;
            textureColor.g += greenDiff;
        }
    }
    return textureColor;
}

technique Technique1
{
    pass AuroraBellPulsePass
    {
        PixelShader = compile ps_3_0 BellShader();
    }
}