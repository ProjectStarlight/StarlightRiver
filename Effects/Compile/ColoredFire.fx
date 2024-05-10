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

    float4 color = float4(0.0, 0.0, 0.0, 0.0);
    
    float3 sample = tex2D(u_tex1, st * float2(1, 0.5) + float2(0, -u_time)).xyz;
    
    color.r += max(0.0, (sample.r - st.y * primaryScaling.r) * primary.r);
    color.g += max(0.0, (sample.r - st.y * primaryScaling.g) * primary.g);
    color.b += max(0.0, (sample.r - st.y * primaryScaling.b) * primary.b);
    
    float3 sample2 = tex2D(u_tex1, st * float2(1, 0.5) + float2(0.5, -u_time * 0.5)).xyz;   
    
    color.r += max(0.0, (sample2.r - st.y) * secondary.r);
    color.g += max(0.0, (sample2.r - st.y) * secondary.g);
    color.b += max(0.0, (sample2.r - st.y) * secondary.b);
   
    color.rgb *= tex2D(u_tex0, st).xyz;

    color.r = color.r - fmod(color.a, 0.2);
    color.g = color.g - fmod(color.a, 0.2);
    color.b = color.b - fmod(color.a, 0.2);
   
    color.a = color.a - fmod(color.a, 0.25);
    
    return color;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};