texture background;
sampler2D InputLayer0 = sampler_state
{
    texture = <background>;
    AddressU = clamp;
    AddressV = clamp;
};

texture noise;
sampler2D InputLayer1 = sampler_state
{
    texture = <noise>;
    AddressU = wrap; 
    AddressV = wrap;
};

float3 tint;
float time;
float freq;
float amp;

float GetLuminance(float3 color)
{
    return dot(color, float3(0.299, 0.587, 0.114));
}

float4 main(float2 uv : TEXCOORD0) : COLOR0
{
    float2 st = uv;
    float dist = distance(st, float2(0.5, 0.5));
    
    st += float2(sin(time + st.x * freq), cos(time + st.y * freq)) * amp * max(0.0, dist - 0.4);
    
    float3 color = tex2D(InputLayer0, st).xyz;
    float lum = GetLuminance(color);
    
    color = (tint * lum) * 0.5 + color * 0.5;
    color += (float3(0.3, 0.3, 0.3) + float3(sin(time + st.x * freq / 2.0), cos(time + st.y * freq / 2.0), sin(time + st.y * freq / 2.0))) * 0.5 * max(0.0, dist - 0.4);

    return float4(color, 1.0);
}

technique Technique1
{
    pass GradientDistortionPass
    {
        PixelShader = compile ps_3_0 main();
    }
}