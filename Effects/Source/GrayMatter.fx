#include "Common.fxh"

texture background;
sampler2D InputLayer0 = sampler_state
{
    texture = <background>;
};

texture map;
sampler2D InputLayer1 = sampler_state
{
    texture = <map>;
};

texture noise;
sampler2D InputLayer2 = sampler_state
{
    texture = <noise>;
    AddressU = wrap; 
    AddressV = wrap;
};

texture over;
sampler2D InputLayer3 = sampler_state
{
    texture = <over>;
};

float time;
float2 screensize;
float2 fullScreensize;
float2 screenpos;
float distortionpow;
float chromepow;

float4 main(float2 uv : TEXCOORD0) : COLOR0
{
    float2 noiseSize = (screensize * fullScreensize);

    float2 st = uv;
    float2 ns = (st - screenpos) / screensize;
    float2 pix_st = st - fmod(st, 2.0 / fullScreensize);
    float2 pix_ns = ns - fmod(ns, 2.0 / noiseSize);
 
    float3 color = tex2D(InputLayer0,st).xyz;
    float distort = sin( min(1.0, tex2D(InputLayer1, pix_st).x) * PI ) * 0.1;
    float noise = tex2D(InputLayer2, (pix_ns +  time * 0.1)).x;
    
    float3 sample = tex2D(InputLayer0, st).xyz;
    float light = min(1.0, (sample.r + sample.g + sample.b) / 3.0 * 10.0);
    
    float push = noise * distort;
    pix_st += push * distortionpow * light;
    float3 final = tex2D(InputLayer0, st + (pix_st - st) * min(1.0, push * 100.0)).xyz;
    
    float gray = GetLuminance(final);
    float power = min(1.0, tex2D(InputLayer1,pix_st).x * 1.5 + push * 1.5);
    float3 grayscale = float3(gray, gray, gray) * power + final * (1.0 - power);
    final = grayscale;
    
    float2 sam1 = (pix_ns + time * 0.1);
    sam1 -= fmod(sam1, 2.0 / noiseSize);
    float2 sam2 = (pix_ns + time * -0.2);
    sam2 -= fmod(sam2, 2.0 / noiseSize);
    float2 sam3 = (pix_ns + time * 0.3);
    sam3 -= fmod(sam3, 2.0 / noiseSize);

    float3 chrome = float3(tex2D(InputLayer2, sam1).x, tex2D(InputLayer2, sam2).x, tex2D(InputLayer2, sam3).x);
    chrome -= fmod(chrome, 0.1);

    final += light * (push * chrome * chromepow);
    final += light * push;
    
    float4 overlay = tex2D(InputLayer3,uv);
    overlay.a *= power;
    final.rgb = overlay.rgb * overlay.a + final.rgb * (1.0 - overlay.a);
    
    //return float4(color * 0.1, 0.5) + float4(distort * 5.0, 0.0, 0.0, 1.0);
    return float4(final,1.0);
}

technique Technique1
{
    pass GrayMatterPass
    {
        PixelShader = compile ps_3_0 main();
    }
}