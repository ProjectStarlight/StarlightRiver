float3 u_color;
float3 u_fade;
float2 u_resolution;
float u_time;

texture tex0_t;
sampler2D tex0 = sampler_state { texture = <tex0_t>; AddressU = wrap; AddressV = wrap; };


float3 rainbow(float2 st, float time)
{
    float3 col = float3(u_color.r + u_fade.r * abs(sin(st.x + time)),
                        u_color.g + u_fade.g * abs(sin(st.y + time + 2.0)),
                        u_color.b + u_fade.b * abs(sin(st.x + st.y + time + 4.0)));
    return col;
}

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float2 st = uv;

    st += float2(sin(st.y * 12.0 + u_time) * (1.0 / u_resolution.x), cos(st.x * 12.0 + u_time) * (1.0 / u_resolution.y));
    return tex2D(tex0, st) * float4(rainbow(st, u_time), 1) * 0.6;
}

technique Technique1
{
    pass MainPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}