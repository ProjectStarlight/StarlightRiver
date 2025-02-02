float2 u_resolution;
float u_time;
float u_alpha;

texture mainbody_t;
sampler2D mainbody = sampler_state
{
    texture = <mainbody_t>;
    AddressU = wrap;
    AddressV = wrap;
};

texture noisemap_t;
sampler2D noisemap = sampler_state
{
    texture = <noisemap_t>;
    AddressU = wrap;
    AddressV = wrap;
};

float3 rainbow(float2 st, float time)
{
    float3 col = float3(0.7 + 0.3 * abs(sin(st.x + time)), 0.3 + 0.5 * abs(sin(st.y + time + 2.0)), 0.3 + 0.3 * abs(sin(st.x + st.y + time + 4.0)));
    return col;
}

float dist(float2 st)
{
    float2 normed = st - float2(0.5, 0.5);
    float2 abso = float2(abs(normed.x), abs(normed.y));
    float least = max(abso.x, abso.y);
    return least;
}

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float2 st = uv;
     
    float noise = tex2D(noisemap, st).x * 6.28;
    noise = 0.8 + 0.6 * sin(u_time * 3.0 + noise);
    
    st += (st - float2(0.5, 0.5)) * 0.3 * noise * (0.5 - dist(st));
    float3 color = tex2D(mainbody, st).xyz;

    
    float alpha = tex2D(mainbody, st).r;
    
    color *= rainbow(st, u_time);
    
    return float4(color, alpha) * u_alpha;
}

technique Technique1
{
    pass mainPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}