float3 u_mouse;
float u_time;
float3 primary;
float3 primaryScaling;
float3 secondary;

texture sampleTexture;
sampler2D  u_tex0 = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture mapTexture;
sampler2D u_tex1 = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };


float4 PixelShaderFunction(float2 uv : TEXCOORD) : COLOR0
{
    float2 st = uv;
    st.y = 1.0 - st.y;
    
    float3 color = float3(0.0, 0.0, 0.0);
    
    float3 sample = tex2D(u_tex1, st * float2(1, 0.5) + float2(0, u_time)).xyz;
    
    color.r += max(0.0, (sample.r + (st.y - 1.0) * primaryScaling.r) * primary.r);
    color.g += max(0.0, (sample.r + (st.y - 1.0) * primaryScaling.g) * primary.g);
    color.b += max(0.0, (sample.r + (st.y - 1.0) * primaryScaling.b) * primary.b);
    
    float3 sample2 = tex2D(u_tex1, st * float2(1.0, 0.5) + float2(0.5, u_time * 0.5)).xyz;   
    
    color.r += max(0.0, (sample2.r + (st.y - 0.7)) * secondary.r);
    color.g += max(0.0, (sample2.r + (st.y - 0.7)) * secondary.g);
    color.b += max(0.0, (sample2.r + (st.y - 0.7)) * secondary.b);
    
    color *= tex2D(u_tex0, st).xyz;
    
    return float4(color, 1.0);
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};