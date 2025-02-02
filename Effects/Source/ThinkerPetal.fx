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

texture shading_t;
sampler2D shading = sampler_state { texture = <shading_t>; AddressU = wrap; AddressV = wrap; };

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
    
    st.y += sin(st.x * 6.0 + u_time * -5.0) * 24.0 / u_resolution.y * st.x;
     
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
    
    float3 noise = tex2D(linemap, st + float2(sin(u_time + st.y * 2.0) * -0.6, sin(u_time * 1.5 + st.x * 1.0) * 0.6)).xyz;
    noise += tex2D(linemap, st + float2(sin(u_time + st.y * 1.0) * 0.7, sin(u_time * 1.5 + st.x * 1.5) * 0.7)).xyz;
    noise += tex2D(linemap, st + float2(sin(u_time + st.y * 1.5) * 0.2, sin(u_time * 1.5 + st.x * 3.0) * -0.5)).xyz;
    
    noise += tex2D(noisemap, (st + float2(u_time * 0.6, u_time * 0.6)) / 2.0).xyz * 0.5;
    noise += u_color * 0.35;

    noise *= rainbow(st, u_time - 0.5);
    float mask = tex2D(mainbody, st).x;
    noise *= mask;
     
    float3 color = (outline + noise);
    color = color - fmod(color, 0.2);
    //color += over * rainbow(st, u_time);
    
    return float4(color, length(color));
}

technique Technique1
{
    pass VitricReplicaItemPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}