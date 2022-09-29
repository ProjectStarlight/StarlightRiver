sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;

float4 uShaderSpecificData;
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;

float hue2rgb(float p, float q, float t){
            if(t < 0) t += 1;
            if(t > 1) t -= 1;
            if(t < 0.166f) return p + (q - p) * 6.0f * t;
            if(t < 0.5f) return q;
            if(t < 0.66f) return p + (q - p) * (0.66f - t) * 6.0f;
            return p;
        }
float3 hslToRgb(float h, float s, float l){
    float r, g, b;
        float q = l < 0.5 ? l * (1 + s) : (l + s) - (l * s);
        float p = (2 * l) - q;
        r = hue2rgb(p, q, h + 0.33f); 
        g = hue2rgb(p, q, h);
        b = hue2rgb(p, q, h - 0.33f); 
	return float3(r,g,b);
}


float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float3 prismColor = hslToRgb((uTime + (uv.y * 20)) % 1, 1, 0.7f);
    float4 colour = tex2D(uImage0, uv);
        colour.rgb *= prismColor;

    return colour;
}

technique Technique1
{
    pass BasicPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}