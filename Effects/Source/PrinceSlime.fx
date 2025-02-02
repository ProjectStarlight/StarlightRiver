float time;
float3 colorIn;
float alpha;

texture sampleTexture;
sampler2D samplerTex = sampler_state
{
    texture = <sampleTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

float4 PixelShaderFunction(float4 st : TEXCOORD0) : COLOR0
{
    float4 sample = tex2D(samplerTex, st.xy);
    float3 color = sample.xyz;
    
    float scale = max(0.0, (color.r - 0.5) * 2.0);
    
    color *= float3(0.25, 0.1, 0.7) * colorIn
        + float3(scale, scale * 0.8, scale * 0.8)
     	+ float3(
        0.5 + sin(time + st.x) * 0.5,
        0.5 + sin(time + 20.0 + st.y) * 0.5,
        0.5 + sin(time + 10.0 + st.x + st.y) * 0.5) * 0.25;

    return float4(color, sample.a * alpha);
}

technique Technique1
{
    pass PrimitivesPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};