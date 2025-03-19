texture mainbody_t;
sampler2D u_tex0 = sampler_state { texture = <mainbody_t>; AddressU = wrap; AddressV = wrap; };

texture noise_t;
sampler2D u_tex1 = sampler_state { texture = <noise_t>; AddressU = wrap; AddressV = wrap; };

texture light_t;
sampler2D u_tex2 = sampler_state { texture = <light_t>; AddressU = wrap; AddressV = wrap; };

float2 u_screenSize;
float2 u_resolution;
float u_time;
float3 u_color;

float2 u_sampleTopLeft;

#define PI 3.1415926538

float dist(float2 pointz) {
    return distance(pointz, float2(0.5, 0.5)) / 0.707;
}

float angle(float2 pointz) {
float2 fromCenter = pointz - float2(0.5, 0.5);
    return (PI + atan2(fromCenter.x, fromCenter.y)) / (PI * 2.0);
}

float4 PixelShaderFunction(float2 st : TEXCOORD0) : COLOR
{
    float4 color = tex2D(u_tex0, st) * float4(u_color, 1.0);
    
    float2 coord = u_sampleTopLeft / u_screenSize + (u_resolution / u_screenSize) * st;
    color.rgb *= tex2D(u_tex2, coord).rgb;
    
    float2 pixel = st - fmod(st, 2.0 / u_resolution);
    float2 radial = float2(dist(pixel), frac(angle(pixel) * 10.0));
    
    float adjusted = dist(st) * 0.85 + tex2D(u_tex1, radial).r * 0.15;
    float relDist = 1.0 - min(1.0, abs(u_time - adjusted) * 10.0);
    
    float3 add = tex2D(u_tex0, st).rgb * float3(1.0, 0.5 - u_time, 0.0) * relDist * (2.0 - u_time * 2.0);
    color.rgb += add;
    color *= (adjusted < u_time) ? 1.0 : 0.0;
    
    return color;
}

technique Technique1
{
    pass MainPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
