#include "Common.fxh"

Texture2D body;
sampler2D tex0 = sampler_state { texture = <body>; };

Texture2D mask;
sampler2D tex1 = sampler_state { texture = <mask>; };

float2 u_resolution;
float u_time;
float u_amplitude;
float u_offset;
float u_alpha;

float4 main(float2 st : TEXCOORD0) : COLOR0
{ 
    st -= fmod(st, 2.0 / u_resolution);
    st += 1.0 / u_resolution;
    
    float2 sample1 = st;
    sample1.y += sin((st.x + u_offset) * TWO_PI) * u_amplitude / u_resolution.y;
    sample1.x = frac(st.x * u_resolution / 1400.0 - u_time);

    //sample1 -= fmod(sample1, 2.0 / u_resolution);
    //sample1 += 1.0 / u_resolution;
    
    float2 sample2 = st;
    sample2.y += sin((st.x + u_offset) * TWO_PI) * (u_amplitude * 0.6) / u_resolution.y;
    sample2.x = frac(st.x * u_resolution / 1400.0 - u_time * 1.2);

    //sample2 -= fmod(sample2, 2.0 / u_resolution);
    //sample2 += 1.0 / u_resolution;
    
    float2 sample3 = st;
    sample3.y += sin((st.x + u_offset) * TWO_PI) * (u_amplitude * 0.6) / u_resolution.y;
    sample3.x = frac(st.x * u_resolution / 1400.0 - u_time * 0.7);

    //sample3 -= fmod(sample3, 2.0 / u_resolution);
    //sample3 += 1.0 / u_resolution;
    
    float3 color = tex2D(tex0, sample1).xyz;
    float3 color2 = tex2D(tex1, sample2).xyz;
    float3 color3 = tex2D(tex1, sample3).xyz;
    
    float3 final = float3(0.0, 0.0, 0.0);
    
    float3 pcolor1 = color * color2;
    pcolor1.g = GetLuminance(pcolor1) * 0.6;
    pcolor1.r = pow(GetLuminance(pcolor1) * 0.6f, 2.0);
    final += pcolor1;
    
    float3 pcolor2 = color * color3;
    pcolor2.r = GetLuminance(pcolor2) * 0.05;
    pcolor2.g = GetLuminance(pcolor2) * 0.4;
    final += pcolor2;
    
    float4 mulled = float4(final * 3.0, (final.b * 3.0));
    mulled -= fmod(mulled, 0.2);
    mulled.rgb += 0.1;
    
    return u_alpha * mulled;
}

technique Technique1
{
    pass mainPass
    {
        PixelShader = compile ps_3_0 main();
    }
};
