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
float projAngle;
float zRotation;
bool flipped;
float2 origin;
float curvature = 17;
float skew = 7.69f;
float colorseed = 0.22f;

float4 ColorOne = float4(0.305f, 0.34f, 0.75f, 1);
float4 ColorTwo = float4(0.905f, 0.89f, 1, 1);

float2 rotate2D(float2 vec, float angle)
{
    float2x2 rotationMatrix= {{sin(3.14159265 / 2 - angle), -sin(angle)}, {sin(angle), sin(3.14159265 / 2 - angle)}};
    return mul(rotationMatrix, vec);
}

float2 AdjustRotation(float2 uv)
{
    uv.y -= origin.y;
    uv.x -= origin.x;

    uv = rotate2D(uv, 3.14159265 / 4 + projAngle);

    if (!flipped) {
        if (sin(1.570796325 - zRotation) > 0)
            uv.x /= sin(1.570796325 - zRotation);
        else if (sin(1.570796325 - zRotation) < 0)
            uv.x /= -sin(1.570796325 - zRotation);
        else
            uv.x = 100;
    }
    else {
        if (sin(1.570796325 - zRotation) > 0)
            uv.y /= sin(1.570796325 - zRotation);
        else if (sin(1.570796325 - zRotation) < 0)
            uv.y /= -sin(1.570796325 - zRotation);
        else
            uv.y = 100;
    }
       
    uv = rotate2D(uv, -3.14159265 / 4 - projAngle);

    uv.y += origin.y;
    uv.x += origin.x;
    return uv;
}

float4 One(float2 uv : TEXCOORD0) : COLOR0
{
    float4 originalColor = tex2D(uImage0, AdjustRotation(uv));

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

    if (tex2D(uImage0, AdjustRotation(uv + squareWidth)).r > 0)
        return ColorOne;
    if (tex2D(uImage0, AdjustRotation(uv - squareWidth)).r > 0)
        return ColorOne;
    if (tex2D(uImage0, AdjustRotation(uv + opposite)).r > 0)
        return ColorOne;
    if (tex2D(uImage0, AdjustRotation(uv - opposite)).r > 0)
        return ColorOne;
    return float4(0, 0, 0, 0);
}

float4 Two(float2 uv : TEXCOORD0) : COLOR0
{
    float4 originalColor = tex2D(uImage0, AdjustRotation(uv));

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

    if (tex2D(uImage0, AdjustRotation(uv + squareWidth)).r > 0)
        return ColorTwo;
    if (tex2D(uImage0, AdjustRotation(uv - squareWidth)).r > 0)
        return ColorTwo;
    if (tex2D(uImage0, AdjustRotation(uv + opposite)).r > 0)
        return ColorTwo;
    if (tex2D(uImage0, AdjustRotation(uv - opposite)).r > 0)
        return ColorTwo;

    return float4(0,0,0,0);
}

float4 Three(float2 uv : TEXCOORD0) : COLOR0
{
    uv = AdjustRotation(uv);

    if (uv.x > 1 || uv.x < 0 || uv.y > 1 || uv.y < 0)
        return float4(0,0,0,0);

    return tex2D(uImage0, uv);
}

technique BasicColorDrawing
{
    pass FirstLayer
    {
        PixelShader = compile ps_3_0 One();
    }
    pass SecondLayer
    {
        PixelShader = compile ps_3_0 Two();
    }
    pass ThirdLayer
    {
        PixelShader = compile ps_3_0 Three();
    }
};