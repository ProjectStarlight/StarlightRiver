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

float4 main(float2 uv : TEXCOORD0) : COLOR0
{
    float2 coord = uv;

    float strength = tex2D(InputLayer0, uv);
    return tex2D(InputLayer1, coord) * strength.r;
}

technique Technique1
{
    pass GradientDistortionPass
    {
        PixelShader = compile ps_2_0 main();
    }
}