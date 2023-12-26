texture map;
sampler2D InputLayer0 = sampler_state
{
    texture = <map>;
};

texture background;
sampler2D InputLayer1 = sampler_state
{
    texture = <background>;
};

float2 screenSize;
float rad;

float4 main(float2 uv : TEXCOORD0) : COLOR0
{
    float2 coord = uv;

    float strength = tex2D(InputLayer0, uv).r;

    float blur = strength +
    tex2D(InputLayer0, uv + float2(0.0, rad) / screenSize) +
    tex2D(InputLayer0, uv + float2(0.0, -rad) / screenSize) +
    tex2D(InputLayer0, uv + float2(rad, 0.0) / screenSize) +
    tex2D(InputLayer0, uv + float2(-rad, 0.0) / screenSize) +
    tex2D(InputLayer0, uv + float2(rad, rad) / screenSize) +
    tex2D(InputLayer0, uv + float2(-rad, rad) / screenSize) +
    tex2D(InputLayer0, uv + float2(rad, -rad) / screenSize) +
    tex2D(InputLayer0, uv + float2(-rad, -rad) / screenSize);

    return tex2D(InputLayer1, coord) * blur / 9.0;
}

technique Technique1
{
    pass GradientDistortionPass
    {
        PixelShader = compile ps_2_0 main();
    }
}