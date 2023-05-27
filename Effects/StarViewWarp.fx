texture map;
sampler2D InputLayer0 = sampler_state
{
    texture = <map>;
};

texture distortionMap;
sampler2D InputLayer1 = sampler_state
{
    texture = <distortionMap>;
};

texture background;
sampler2D InputLayer2 = sampler_state
{
    texture = <background>;
};

float uIntensity;

float2 uTargetPosition;
float2 uResolution;

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float2 st = uv;
    float2 center = {0.5, 0.5};
    
    float2 rescale = (st - uTargetPosition / uResolution) / 2.0 + center;
    float distort = tex2D(InputLayer1, (rescale)).x;
    
    float mag = distort * (pow(uIntensity, 2.0) * 2.0);
    if (mag > 1.0) mag = 1.0;
    float2 distorted  = st - (st - uTargetPosition / uResolution) * mag;

    float strength = tex2D(InputLayer0, distorted);

    return tex2D(InputLayer2, st) * strength.r;
}

technique Technique1
{
    pass StarViewWarpPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}