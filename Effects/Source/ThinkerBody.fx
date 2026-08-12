float3 u_color;
float3 u_fade;
float2 u_resolution;
float u_time;

texture mainbody_t;
sampler2D mainbody = sampler_state { texture = <mainbody_t>; AddressU = wrap; AddressV = wrap; };

texture linemap_t;
sampler2D linemap = sampler_state { texture = <linemap_t>; AddressU = wrap; AddressV = wrap; };

texture noisemap_t;
sampler2D noisemap = sampler_state { texture = <noisemap_t>; AddressU = wrap; AddressV = wrap; };

texture overlay_t;
sampler2D overlay = sampler_state { texture = <overlay_t>; AddressU = wrap; AddressV = wrap; };

texture normal_t;
sampler2D normal = sampler_state { texture = <normal_t>; AddressU = wrap; AddressV = wrap; };

texture mask_t;
sampler2D maskTex = sampler_state { texture = <mask_t>; AddressU = wrap; AddressV = wrap; };

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
     
    float3 outline = tex2D(mainbody, st + float2(2.0 / u_resolution.x, 0.0)).xyz;
    outline += tex2D(mainbody, st + float2(-2.0 / u_resolution.x, 0.0)).xyz;
    outline += tex2D(mainbody, st + float2(0.0, -2.0 / u_resolution.y)).xyz;
    outline += tex2D(mainbody, st + float2(0.0, 2.0 / u_resolution.y)).xyz;
    outline = min(outline, 1.0);
    
    float3 over = tex2D(overlay, st + float2(0.0, sin(u_time * 4.0) * 0.02)).xyz;
    
    outline *= 1.0 - tex2D(mainbody, st).xyz;
    outline *= rainbow(st, u_time) * 1.5;
    
    st -= fmod(st, 2.0 / u_resolution);
    st += 1.0 / u_resolution;
    float3 norm = tex2D(normal, st).xyz;

    st.y += (norm.g - 0.5) / (norm.b - 0.5) * 0.15;
    st.x += (norm.r - 0.5) / (norm.b - 0.5) * 0.15;
    
    float3 noise = tex2D(linemap, st + float2(u_time * 0.6, u_time * 0.6)).xyz;
    noise += tex2D(linemap, st + float2(u_time * 1.7, u_time * 1.7)).xyz;
    noise += tex2D(linemap, st + float2(u_time * 0.2, u_time * 0.2)).xyz;
    
    noise += tex2D(noisemap, (st + float2(u_time * 0.6, u_time * 0.6)) / 2.0).xyz * 0.5;
     
    noise += float3(0.325, 0.25, 0.1);
    
    st.y -= (norm.g - 0.5) / (norm.b - 0.5) * 0.15;
    st.x -= (norm.r - 0.5) / (norm.b - 0.5) * 0.15;
    
    noise *= rainbow(st, u_time - 0.3);
    float mask = tex2D(mainbody, st).x;
    noise *= mask;
     
    float3 color = (outline + noise);
    color = color - fmod(color, 0.2);
    color += over * rainbow(st, u_time);
    
    return float4(color, length(color)) * tex2D(maskTex, uv).r;
}

technique Technique1
{
    pass VitricReplicaItemPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}