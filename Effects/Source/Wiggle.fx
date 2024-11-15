texture toDraw;
sampler2D InputLayer0 = sampler_state
{
    texture = <toDraw>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

float time;

float freq1;
float speed1;
float amp1;

float freq2;
float speed2;
float amp2;

float4 colorIn;

float4 main(float2 uv : TEXCOORD0) : COLOR0
{
    float2 coord = uv;

    uv.y += sin(uv.x * freq1 + time * speed1) * amp1;
    uv.y += sin(uv.x * freq2 + time * speed2) * amp2;
       
    float4 color = tex2D(InputLayer0, uv);
    
    return color * colorIn;
}

technique Technique1
{
    pass GradientDistortionPass
    {
        PixelShader = compile ps_2_0 main();
    }
}