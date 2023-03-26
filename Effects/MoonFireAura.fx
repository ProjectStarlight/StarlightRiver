sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

Texture2D fnoise2;
sampler fnoise2Sampler = sampler_state
{
    Texture = (fnoise2);
};

Texture2D fnoise;
sampler fnoiseSampler = sampler_state
{
    Texture = (fnoise);
};

Texture2D vnoise;
sampler vnoiseSampler = sampler_state
{
    Texture = (vnoise);
};

float time;
float fireHeight;
float curvature = 17;
float skew = 7.69f;
float colorseed = 0.22f;

float4 ColorOne = float4(0.305f, 0.34f, 0.75f, 1);
float4 ColorTwo = float4(0.905f, 0.89f, 1, 1);

float4 One(float2 uv : TEXCOORD0) : COLOR0
{
    float4 originalColor = tex2D(uImage0, uv);

    //uv = floor(uv * 70) / 70;
    if (originalColor.r > 0)
        return float4(0, 0, 0, 0);
    float pixelMult = tex2D(fnoise2Sampler, (uv + float2(time, 0)) % 1).r + tex2D(fnoiseSampler, (uv + float2(0, time)) % 1).r;
    float pixelSize = fireHeight * pixelMult;

    float curveVal = tex2D(fnoiseSampler, (uv + float2(time, time)) % 1) - 0.5f;

    float curveVal2 = tex2D(vnoiseSampler, uv).r - 0.5f;
    float2 squareWidth = float2(pixelSize, pixelSize) * 1.2f;
    float2 opposite = pixelSize * float2(1, -1);

    uv.y -= pixelSize * curveVal2 * curvature * curveVal;

    if (tex2D(uImage0, uv + squareWidth).r > 0)
        return ColorOne;
    if (tex2D(uImage0, uv - squareWidth).r > 0)
        return ColorOne;
    if (tex2D(uImage0, uv + opposite).r > 0)
        return ColorOne;
    if (tex2D(uImage0, uv - opposite).r > 0)
        return ColorOne;
    return float4(0, 0, 0, 0);
}

float4 Two(float2 uv : TEXCOORD0) : COLOR0
{
    float4 originalColor = tex2D(uImage0, uv);

    //uv = floor(uv * 70) / 70;
    if (originalColor.r > 0)
        return originalColor;
    float pixelMult = tex2D(fnoise2Sampler, (uv + float2(time, 0)) % 1).r + tex2D(fnoiseSampler, (uv + float2(0, time)) % 1).r;
    float pixelSize = fireHeight * pixelMult;

    float curveVal = tex2D(fnoiseSampler, (uv + float2(time, time)) % 1) - 0.5f;

    float curveVal2 = tex2D(vnoiseSampler, uv).r - 0.5f;
    float2 squareWidth = float2(pixelSize, pixelSize) / skew;
    float2 opposite = pixelSize * float2(1, -1);

    float increment = 0.3f + (colorseed * (tex2D(vnoiseSampler, (uv + float2(time, time)) % 1) - 0.5f));

    if (tex2D(uImage0, uv + squareWidth).r > 0)
        return ColorTwo;
    if (tex2D(uImage0, uv - squareWidth).r > 0)
        return ColorTwo;
    if (tex2D(uImage0, uv + opposite).r > 0)
        return ColorTwo;
    if (tex2D(uImage0, uv - opposite).r > 0)
        return ColorTwo;

    return float4(0,0,0,0);
}

technique BasicColorDrawing
{
    pass FirstLayer
    {
        PixelShader = compile ps_2_0 One();
    }
    pass SecondLayer
    {
        PixelShader = compile ps_2_0 Two();
    }
};